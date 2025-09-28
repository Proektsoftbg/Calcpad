using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed class Evaluator
        {
            private int _tos;
            private int _stackUBound = 99;
            private IValue[] _stackBuffer = new IValue[100];
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Calculator _calc;
            private readonly VectorCalculator _vectorCalc;
            private readonly MatrixCalculator _matrixCalc;
            private readonly Dictionary<string, Unit> _units;

            internal Evaluator(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
                _vectorCalc = parser._vectorCalc;
                _matrixCalc = parser._matrixCalc;
                _units = parser._units;
            }

            internal void Reset() => _tos = 0;
            private void StackPush(IValue v)
            {
                if (_tos >= _stackUBound)
                {
                    _stackUBound *= 2;
                    Array.Resize(ref _stackBuffer, _stackUBound + 1);
                }
                _stackBuffer[++_tos] = v;
            }
            private IValue StackPop() => _stackBuffer[_tos--];

            internal IValue Evaluate(Token[] rpn, bool isVisible = false)
            {
                var tos = _tos;
                var rpnLength = rpn.Length;
                if (rpnLength < 1)
                    throw Exceptions.ExpressionEmpty();

                var i0 = 0;
                if (rpn[0].Type == TokenTypes.Variable && rpn[rpnLength - 1].Content == "=")
                    i0 = 1;

                _parser._backupVariable = new(null, RealValue.Zero);
                for (int i = i0; i < rpnLength; ++i)
                {
                    if (_tos < tos)
                        throw Exceptions.StackEmpty();

                    var t = rpn[i];
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                            var value = ((ValueToken)t).Value;
                            StackPush(value);
                            continue;
                        case TokenTypes.Unit:
                        case TokenTypes.Variable:
                        case TokenTypes.Input:
                            StackPush(EvaluateToken(t));
                            continue;
                        case TokenTypes.Operator when t.Content == NegateString:
                        case TokenTypes.Function:
                        case TokenTypes.VectorFunction:
                        case TokenTypes.MatrixFunction:
                        case TokenTypes.MatrixOptionalFunction:
                            if (_tos == tos)
                                throw Exceptions.MissingOperand();

                            var a = StackPop();
                            StackPush(EvaluateToken(t, a));
                            continue;
                        case TokenTypes.Operator:
                        case TokenTypes.Function2:
                        case TokenTypes.VectorFunction2:
                        case TokenTypes.MatrixFunction2:
                            if (_tos == tos)
                                throw Exceptions.MissingOperand();

                            var b = StackPop();
                            if (t.Content == "=")
                                return EvaluateAssignment(b, rpn, isVisible);

                            if (_tos == tos)
                                throw Exceptions.MissingOperand();

                            var c = StackPop();
                            StackPush(EvaluateToken(t, c, b));
                            continue;
                        case TokenTypes.Function3:
                        case TokenTypes.VectorFunction3:
                        case TokenTypes.MatrixFunction3:
                            StackPush(EvaluateFunction3Token(t));
                            continue;
                        case TokenTypes.MatrixFunction4:
                            StackPush(EvaluateFunction4Token(t));
                            continue;
                        case TokenTypes.MatrixFunction5:
                            StackPush(EvaluateFunction5Token(t));
                            continue;
                        case TokenTypes.Interpolation:
                            StackPush(EvaluateInterpolationToken(t));
                            continue;
                        case TokenTypes.MultiFunction:
                        case TokenTypes.VectorMultiFunction:
                        case TokenTypes.MatrixMultiFunction:
                            StackPush(EvaluateMultiFunctionToken(t));
                            continue;
                        case TokenTypes.CustomFunction:
                            StackPush(EvaluateFunctionToken(t, ref tos));
                            continue;
                        case TokenTypes.Vector:
                        case TokenTypes.RowDivisor:
                            StackPush(EvaluateVectorToken(t));
                            continue;
                        case TokenTypes.Matrix:
                            StackPush(EvaluateMatrixToken(t));
                            continue;
                        case TokenTypes.VectorIndex:
                        case TokenTypes.MatrixIndex:
                            StackPush(EvaluateIndexToken(t));
                            continue;
                        case TokenTypes.Solver:
                            StackPush(EvaluateSolverToken(t));
                            continue;
                        default:
                            throw Exceptions.CannotEvaluateAsType(t.Content, t.Type.GetType().GetEnumName(t.Type));
                    }
                }
                if (_tos > tos)
                {
                    var v = StackPop();
                    _parser.Units = ApplyUnits(ref v, _parser._targetUnits);
                    return v;
                }
                if (_tos > tos)
                    throw Exceptions.StackLeak();

                _parser.Units = null;
                return RealValue.Zero;
            }

            private IValue EvaluateToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    if (t is ValueToken vt)
                        return vt.Value;

                    return ((VariableToken)t).Variable.ValueByRef();
                }
                if (t.Type == TokenTypes.Variable)
                    return EvaluateVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
                    throw Exceptions.UndefinedInputField();

                if (t.Type == TokenTypes.Vector)
                {
                    if (t is VectorToken vt)
                        return vt.Vector;

                    return EvaluateVariableToken(((VariableToken)t));
                }
                if (t.Type == TokenTypes.Matrix)
                {
                    if (t is MatrixToken mt)
                        return mt.Matrix;

                    return EvaluateVariableToken(((VariableToken)t));
                }
                return ((ValueToken)t).Value;
            }


            private IValue EvaluateToken(Token t, in IValue a)
            {
                if (t.Content == NegateString)
                    return -a;

                if (t.Type == TokenTypes.Function)
                    return IValue.EvaluateFunction(_matrixCalc, t.Index, a);

                if (t.Type == TokenTypes.VectorFunction)
                    return _vectorCalc.EvaluateVectorFunction(t.Index, a);

                if (t.Type == TokenTypes.MatrixOptionalFunction)
                    return _matrixCalc.EvaluateMatrixFunction2(t.Index, a, RealValue.Zero);

                //MatrixFunction
                var result = _matrixCalc.EvaluateMatrixFunction(t.Index, a);
                if (t.Index == MatrixCalculator.FunctionIndex["lu"])
                {
                    ref var indexes = ref CollectionsMarshal.GetValueRefOrAddDefault(_parser._variables, "ind", out var _);
                    indexes = new(_matrixCalc.Indexes);
                }
                return result;
            }

            private IValue EvaluateToken(Token t, in IValue a, in IValue b)
            {
                if (t.Type == TokenTypes.Operator)
                    return IValue.EvaluateOperator(_matrixCalc, t.Index, a, b);

                if (t.Type == TokenTypes.Function2)
                    return IValue.EvaluateFunction2(_matrixCalc, t.Index, a, b);

                if (t.Type == TokenTypes.VectorFunction2)
                    return _vectorCalc.EvaluateVectorFunction2(t.Index, a, b);

                //MatrixFunction2
                return _matrixCalc.EvaluateMatrixFunction2(t.Index, a, b);
            }


            private IValue EvaluateVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized ||
                    t.Type == TokenTypes.Vector ||
                    t.Type == TokenTypes.Matrix)
                    return v.Value;

                try
                {
                    if (!_units.TryGetValue(t.Content, out var u))
                        u = Unit.Get(t.Content);

                    t.Type = TokenTypes.Unit;
                    v.SetValue(u);
                    return v.Value;
                }
                catch
                {
                    throw Exceptions.UndefinedVariableOrUnits(t.Content);
                }
            }
            private IValue EvaluateAssignment(IValue b, Token[] rpn, bool isVisible)
            {
                _parser.Units = ApplyUnits(ref b, _parser._targetUnits);
                if (isVisible)
                    NormalizeUnits(ref b);

                var t0 = rpn[0];
                if (t0 is VariableToken ta)
                {
                    if (t0.Type == TokenTypes.Vector ||
                        t0.Type == TokenTypes.Matrix)
                    {
                        int i = -1, j = -1;
                        for (int k = 0; k < rpn.Length; ++k)
                        {
                            var t = rpn[k];
                            if (t.Type == TokenTypes.VectorIndex && t.Content == t0.Content)
                            {
                                i = (int)t.Index;
                                break;
                            }
                            if (t.Type == TokenTypes.MatrixIndex && t.Content == t0.Content)
                            {
                                i = (int)(t.Index / Vector.MaxLength);
                                j = (int)(t.Index % Vector.MaxLength);
                                break;
                            }
                        }
                        var variable = ta.Variable;
                        if (t0.Type == TokenTypes.Vector)
                        {
                            var vector = (Vector)variable.Value;
                            if (i < 1 || i > vector.Length)
                                throw Exceptions.IndexOutOfRange(i.ToString());

                            if (b is RealValue rb)
                            {
                                _parser._backupVariable = new(ta.Content + '.' + i, vector[i - 1]);
                                vector[i - 1] = rb;
                            }
                            else
                                throw Exceptions.CannotAssignVectorToScalar();
                        }
                        else
                        {
                            var matrix = (Matrix)variable.Value;
                            if (i < 1 || i > matrix.RowCount ||
                                j < 1 || j > matrix.ColCount)
                                throw Exceptions.IndexOutOfRange($"{i}, {j}");

                            if (b is RealValue rb)
                            {
                                _parser._backupVariable = new(ta.Content + '.' + i + '.' + j, matrix[i - 1, j - 1]);
                                matrix[i - 1, j - 1] = rb;
                            }
                            else
                                throw Exceptions.CannotAssignVectorToScalar();
                        }
                        variable.Change();
                    }
                    else if (t0.Type == TokenTypes.Variable)
                    {
                        _parser._backupVariable = new(ta.Content, ta.Variable.Value);
                        ta.Variable.Assign(b);
                    }
                }
                else if (t0.Type == TokenTypes.Unit &&
                         t0 is ValueToken tc && b is IScalarValue iScalar)
                {
                    if (tc.Value.Units is not null)
                        throw Exceptions.CannotRewriteUnits(tc.Value.Units.Text);

                    _parser.SetUnit(tc.Content, iScalar.AsComplex());
                    tc.Value = new RealValue(_parser.GetUnit(tc.Content));
                }
                return b;
            }

            private IValue EvaluateFunction3Token(Token t)
            {
                IValue c = StackPop(),
                       b = StackPop(),
                       a = StackPop();
                return t.Type switch
                {
                    TokenTypes.Function3 => _calc.EvaluateFunction3(t.Index, a, b, c),
                    TokenTypes.VectorFunction3 => _vectorCalc.EvaluateVectorFunction3(t.Index, a, b, c),
                    _ => _matrixCalc.EvaluateMatrixFunction3(t.Index, a, b, c)
                };
            }

            private IValue EvaluateFunction4Token(Token t)
            {
                IValue d = StackPop(),
                       c = StackPop(),
                       b = StackPop(),
                       a = StackPop();
                return _matrixCalc.EvaluateMatrixFunction4(t.Index, a, b, c, d);
            }

            private IValue EvaluateFunction5Token(Token t)
            {
                IValue e = StackPop(),
                       d = StackPop(),
                       c = StackPop(),
                       b = StackPop(),
                       a = StackPop();
                return _matrixCalc.EvaluateMatrixFunction5(t.Index, a, b, c, d, e);
            }

            private IValue EvaluateInterpolationToken(Token t)
            {
                var paramCount = ((FunctionToken)t).ParameterCount;
                if (paramCount == 2 && _stackBuffer[_tos] is Vector vec1)
                {
                    --_tos;
                    var ival = StackPop();
                    if (ival is RealValue x)
                        return _vectorCalc.EvaluateInterpolation(t.Index, x, vec1);

                    throw Exceptions.CannotInterpolateWithNonScalarValue();
                }
                if (paramCount == 3 && _stackBuffer[_tos] is Matrix matrix)
                {
                    --_tos;
                    var ival = StackPop();
                    if (ival is RealValue x)
                    {
                        ival = StackPop();
                        if (ival is RealValue y)
                            return _matrixCalc.EvaluateInterpolation(t.Index, x, y, matrix);
                    }
                    throw Exceptions.CannotInterpolateWithNonScalarValue();
                }
                var mfParams = StackPopAndExpandValues(paramCount);
                return _calc.EvaluateInterpolation(t.Index, mfParams);
            }

            private IValue EvaluateMultiFunctionToken(Token t)
            {
                var paramCount = ((FunctionToken)t).ParameterCount;
                if (t.Type == TokenTypes.MultiFunction)
                {
                    if (paramCount == 1)
                    {
                        if (_stackBuffer[_tos] is Vector vec2)
                        {
                            --_tos;
                            return _vectorCalc.EvaluateMultiFunction(t.Index, vec2);
                        }
                        if (_stackBuffer[_tos] is Matrix matrix)
                        {
                            --_tos;
                            return _matrixCalc.EvaluateMultiFunction(t.Index, matrix);
                        }
                    }
                    var mfParams = StackPopAndExpandValues(paramCount);
                    return _calc.EvaluateMultiFunction(t.Index, mfParams);
                }
                var vmParams = StackPopValues(paramCount);
                if (t.Type == TokenTypes.VectorMultiFunction)
                    return _vectorCalc.EvaluateVectorMultiFunction(t.Index, vmParams);

                //MatrixMultiFunction
                return _matrixCalc.EvaluateMatrixMultiFunction(t.Index, vmParams);
            }

            private IValue EvaluateFunctionToken(Token t, ref int tos)
            {
                var cf = _functions[t.Index];
                var cfParamCount = cf.ParameterCount;
                if (cf.IsRecursion)
                {
                    tos -= cfParamCount;
                    return RealValue.NaN;
                }
                switch (cf.ParameterCount)
                {
                    case 1:
                        var x = StackPop();
                        return EvaluateFunction((CustomFunction1)cf, x);
                    case 2:
                        var y = StackPop();
                        x = StackPop();
                        return EvaluateFunction((CustomFunction2)cf, x, y);
                    case 3:
                        var z = StackPop();
                        y = StackPop();
                        x = StackPop();
                        return EvaluateFunction((CustomFunction3)cf, x, y, z);
                    default:
                        var cfParams = StackPopValues(cfParamCount);
                        return EvaluateFunction((CustomFunctionN)cf, cfParams);
                }
            }

            internal IValue EvaluateFunction(CustomFunction1 cf, in IValue x)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal IValue EvaluateFunction(CustomFunction2 cf, in IValue x, in IValue y)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x, y);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal IValue EvaluateFunction(CustomFunction3 cf, in IValue x, in IValue y, in IValue z)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x, y, z);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal IValue EvaluateFunction(CustomFunctionN cf, IValue[] arguments)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(arguments);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            private IValue EvaluateMatrixToken(Token t)
            {
                if (t is MatrixToken mt)
                {
                    var count = (int)(t.Index / Vector.MaxLength);
                    mt.Matrix = MatrixCalculator.JoinRows(StackPopValues(count));
                    return mt.Matrix;
                }
                else
                    return EvaluateToken(t);
            }

            private IValue EvaluateVectorToken(Token t)
            {
                if (t is VectorToken vt)
                {
                    var count = (int)t.Index;
                    vt.Vector = new(StackPopAndExpandRealValues(count));
                    return vt.Vector;
                }
                else
                    return EvaluateToken(t);
            }

            private IValue EvaluateIndexToken(Token t)
            {
                var value = IValue.AsValue(StackPop(), Exceptions.Items.Index);
                var i = (int)value.Re;
                if (t.Type == TokenTypes.VectorIndex)
                {
                    var vector = IValue.AsVector(StackPop(), Exceptions.Items.IndexTarget);
                    if (i < 1 || i > vector.Length)
                        throw Exceptions.IndexOutOfRange(i.ToString());

                    t.Index = i;
                    return vector[i - 1];
                }
                value = IValue.AsValue(StackPop(), Exceptions.Items.Index);
                var matrix = IValue.AsMatrix(StackPop(), Exceptions.Items.IndexTarget);
                var j = i;
                i = (int)value.Re;
                if (i < 1 || i > matrix.RowCount ||
                    j < 1 || j > matrix.ColCount)
                    throw Exceptions.IndexOutOfRange($"{i}, {j}");

                t.Index = i * Vector.MaxLength + j;
                return matrix[i - 1, j - 1];
            }

            private IValue EvaluateSolverToken(Token t)
            {
                var solveBlock = _solveBlocks[(int)t.Index];
                solveBlock.Calculate();
                return solveBlock.Result;
            }

            private IValue[] StackPopValues(int count)
            {
                var values = new IValue[count];
                for (int i = count - 1; i >= 0; --i)
                    values[i] = StackPop();

                return values;
            }

            private RealValue[] StackPopAndExpandRealValues(int count)
            {
                var list = StackPopAndExpand(count);
                var n = list.Count;
                var values = new RealValue[n];
                --n;
                for (int i = n; i >= 0; --i)
                    values[n - i] = (RealValue)list[i];

                return values;
            }

            private IScalarValue[] StackPopAndExpandValues(int count)
            {
                var list = StackPopAndExpand(count);
                var n = list.Count;
                var values = new IScalarValue[n];
                --n;
                for (int i = n; i >= 0; --i)
                    values[n - i] = list[i];

                return values;
            }

            private List<IScalarValue> StackPopAndExpand(int count)
            {
                var values = new List<IScalarValue>(count);
                for (int k = count - 1; k >= 0; --k)
                {
                    IValue ival = StackPop();
                    if (ival is IScalarValue scalar)
                        values.Add(scalar);
                    else if (ival is Vector vector)
                    {
                        for (int j = vector.Length - 1; j >= 0; --j)
                            values.Add(vector[j]);
                    }
                    else if (ival is Matrix matrix)
                    {
                        var colCount = matrix.ColCount;
                        for (int i = matrix.RowCount - 1; i >= 0; --i)
                            for (int j = colCount - 1; j >= 0; --j)
                                values.Add(matrix[i, j]);
                    }
                }
                return values;
            }

            internal void NormalizeUnits(ref IValue ival)
            {
                if (ival is RealValue real)
                {
                    if (real.Units is not null)
                    {
                        NormalizeUnits(ref real);
                        ival = real;
                    }
                }
                else if (ival is ComplexValue complex)
                {
                    if (complex.Units is not null)
                    {
                        NormalizeUnits(ref complex);
                        ival = complex;
                    }
                }
                else if (ival is Vector vector)
                    NormalizeUnits(vector);
                else if (ival is Matrix matrix)
                    NormalizeUnits(matrix);
            }

            private void NormalizeUnits(Matrix M)
            {
                if (M is HpMatrix hpM)
                {
                    if (hpM.Units is not null)
                    {
                        var d = hpM.Units.Normalize();
                        if (d != 1d)
                            hpM.Scale(d);
                    }
                }
                else
                    for (int i = 0, n = M.Rows.Length; i < n; ++i)
                        NormalizeUnits(M.Rows[i]);
            }

            private void NormalizeUnits(Vector vector)
            {
                if (vector is HpVector hpv)
                {
                    if (hpv.Units is not null)
                    {
                        var d = hpv.Units.Normalize();
                        if (d != 1d)
                            hpv.Scale(d);
                    }
                }
                else
                    for (int i = 0, n = vector.Size; i < n; ++i)
                    {
                        ref var value = ref vector.ValueByRef(i);
                        if (value.Units is not null)
                            NormalizeUnits(ref value);
                    }
            }

            private void NormalizeUnits(ref ComplexValue value)
            {
                var d = value.Units.Normalize();
                if (d != 1)
                {
                    if (_parser._settings.IsComplex)
                        value = new(value.Complex * d, value.Units);
                    else
                        value *= d;
                }
            }

            private void NormalizeUnits(ref RealValue value)
            {
                var d = value.Units.Normalize();
                if (d != 1)
                {
                    if (_parser._settings.IsComplex)
                        value = new(value.D * d, value.Units);
                    else
                        value *= d;
                }
            }

            internal static Unit ApplyUnits(ref IValue ival, Unit u)
            {
                if (ival is RealValue real)
                {
                    var result = ApplyUnits(ref real, u);
                    ival = real;
                    return result;
                }
                if (ival is ComplexValue complex)
                {
                    var result = ApplyUnits(ref complex, u);
                    ival = complex;
                    return result;
                }
                if (ival is Vector vector)
                    return ApplyUnits(vector, u);

                if (ival is Matrix matrix)
                    return ApplyUnits(matrix, u);

                return null;
            }

            private static Unit ApplyUnits(Matrix M, Unit u)
            {
                Unit result;
                if (M is HpMatrix hpM)
                {
                    RealValue value = new(1d, hpM.Units);
                    result = ApplyUnits(ref value, u);
                    var d = value.D;
                    if (d != 1d)
                        hpM.Scale(d);

                    hpM.Units = result;
                }
                else
                {
                    result = ApplyUnits(M.Rows[0], u);
                    for (int i = 1, n = M.Rows.Length; i < n; ++i)
                        ApplyUnits(M.Rows[i], u);
                }
                return result;
            }

            private static Unit ApplyUnits(Vector vector, Unit u)
            {
                if (vector.Size == 0)
                    return u;

                if (vector is HpVector hpv)
                {
                    RealValue value = new(1d, hpv.Units);
                    var result = ApplyUnits(ref value, u);
                    var d = value.D;
                    if (d != 1d)
                    {
                        var row = hpv.Raw;
                        for (int i = vector.Size - 1; i >= 0; --i)
                            row[i] *= d;
                    }
                    hpv.Units = result;
                    return result;
                }
                for (int i = vector.Size - 1; i >= 0; --i)
                    ApplyUnits(ref vector.ValueByRef(i), u);

                return ApplyUnits(ref vector.ValueByRef(0), u);
            }

            private static Unit ApplyUnits(ref ComplexValue value, Unit u)
            {
                var vu = value.Units;
                if (u is null)
                {
                    if (vu is null)
                        return null;

                    var field = vu.GetField();
                    if (field == Unit.Field.Mechanical)
                        u = Unit.GetForceUnit(vu);
                    else if (field == Unit.Field.Electrical)
                        u = Unit.GetElectricalUnit(vu);
                    else
                        return vu;

                    if (ReferenceEquals(u, vu))
                        return vu;

                    var c = vu.ConvertTo(u);
                    value = new(value.Complex * c, u);
                    return u;
                }
                if (u.IsDimensionless)
                {
                    if (vu is null)
                    {
                        value = new(value.Complex / u.GetDimensionlessFactor(), u);
                        return u;
                    }
                    var format = u.FormatString;
                    if (format is not null)
                    {
                        vu = new(vu) { Text = vu.Text + ':' + format };
                        value = new(value.Complex, vu);
                        return vu;
                    }
                }
                if (!Unit.IsConsistent(vu, u))
                    throw Exceptions.InconsistentTargetUnits(Unit.GetText(vu), Unit.GetText(u));

                var d = vu.ConvertTo(u);
                if (u.IsTemp)
                {
                    var c = value.Complex * d + Unit.GetTempUnitsDelta(vu.Text, u.Text);
                    value = new(c, u);
                }
                else
                    value = new(value.Complex * d, u);

                return value.Units;
            }

            private static Unit ApplyUnits(ref RealValue value, Unit u)
            {
                var vu = value.Units;
                if (u is null)
                {
                    if (vu is null)
                        return null;

                    var field = vu.GetField();
                    if (field == Unit.Field.Mechanical)
                        u = Unit.GetForceUnit(vu);
                    else if (field == Unit.Field.Electrical)
                        u = Unit.GetElectricalUnit(vu);
                    else
                        return vu;

                    if (ReferenceEquals(u, vu))
                        return vu;

                    var c = vu.ConvertTo(u);
                    value = new(value.D * c, u);
                    return u;
                }
                if (u.IsDimensionless)
                {
                    if (vu is null)
                    {
                        value = new(value.D / u.GetDimensionlessFactor(), u);
                        return u;
                    }
                    var format = u.FormatString;
                    if (format is not null)
                    {
                        vu = new(vu) { Text = vu.Text + ':' + format };
                        value = new(value.D, vu);
                        return vu;
                    }
                }
                if (!Unit.IsConsistent(vu, u))
                    throw Exceptions.InconsistentTargetUnits(Unit.GetText(vu), Unit.GetText(u));

                var d = vu.ConvertTo(u);
                if (u.IsTemp)
                {
                    var re = value.D * d + Unit.GetTempUnitsDelta(vu.Text, u.Text);
                    value = new(re, u);
                }
                else
                    value = new(value.D * d, u);

                return value.Units;
            }
        }
    }
}