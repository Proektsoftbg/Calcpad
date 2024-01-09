using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class Evaluator
        {
            private int _tos;
            private readonly Value[] _stackBuffer = new Value[100];
            internal readonly HashSet<string> DefinedVariables = new(StringComparer.Ordinal);
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Calculator _calc;
            private readonly Dictionary<string, Unit> _units;

            internal Evaluator(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
                _units = parser._units;
            }

            internal void Reset() => _tos = 0;

            internal Value Evaluate(Token[] rpn, bool isVisible = false)
            {
                var tos = _tos;
                var rpnLength = rpn.Length;
                if (rpnLength < 1)
                    Throw.ExpressionEmptyException();

                var i0 = 0;
                if (rpn[0].Type == TokenTypes.Variable && rpn[rpnLength - 1].Content == "=")
                    i0 = 1;

                _parser._backupVariable = new(null, Value.Zero);
                for (int i = i0; i < rpnLength; ++i)
                {
                    if (_tos < tos)
                        Throw.StackEmptyException();

                    var t = rpn[i];
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                            _stackBuffer[++_tos] = ((ValueToken)t).Value;
                            continue;
                        case TokenTypes.Unit:
                        case TokenTypes.Variable:
                        case TokenTypes.Input:
                            _stackBuffer[++_tos] = EvaluateToken(t);
                            continue;
                        case TokenTypes.Operator:
                        case TokenTypes.Function:
                        case TokenTypes.Function2:
                            Value c;
                            if (t.Type == TokenTypes.Function || t.Content == NegateString)
                            {
                                var a = _stackBuffer[_tos--];
                                c = EvaluateToken(t, a);
                            }
                            else
                            {
                                if (_tos == tos)
                                    Throw.MissingOperandException();

                                var b = _stackBuffer[_tos--];
                                if (t.Content == "=")
                                {
                                    _parser.Units = ApplyUnits(ref b, _parser._targetUnits);
                                    if (isVisible && b.Units is not null)
                                    {
                                        var d = b.Units.Normalize();
                                        if (d != 1)
                                        {
                                            if (_parser._settings.IsComplex)
                                                b = new(b.Complex * d, b.Units);
                                            else
                                                b *= d;
                                        }
                                    }
                                    if (rpn[0].Type == TokenTypes.Variable && rpn[0] is VariableToken ta)
                                    {
                                        _parser._backupVariable = new(ta.Content, ta.Variable.Value);
                                        ta.Variable.Assign(b);
                                    }
                                    else if (rpn[0].Type == TokenTypes.Unit && rpn[0] is ValueToken tc)
                                    {
                                        if (tc.Value.Units is not null)
                                            Throw.CannotRewiriteUnitsException(tc.Value.Units.Text);

                                        _parser.SetUnit(tc.Content, b);
                                        tc.Value = new(_parser.GetUnit(tc.Content));
                                    }
                                    return b;
                                }

                                if (_tos == tos)
                                {
                                    if (t.Content != "-")
                                        Throw.MissingOperandException();

                                    c = new Value(-b.Re, -b.Im, b.Units);
                                }
                                else
                                {
                                    var a = _stackBuffer[_tos--];
                                    c = EvaluateToken(t, a, b);
                                }
                            }
                            _stackBuffer[++_tos] = c;
                            continue;
                        case TokenTypes.If:
                            Value vFalse = _stackBuffer[_tos--],
                                vTrue = _stackBuffer[_tos--],
                                vCond = _stackBuffer[_tos--];
                            _stackBuffer[++_tos] = EvaluateIf(vCond, vTrue, vFalse);
                            continue;
                        case TokenTypes.MultiFunction:
                            var mfParamCount = ((FunctionToken)t).ParameterCount;
                            var mfParams = new Value[mfParamCount];
                            for (int j = mfParamCount - 1; j >= 0; --j)
                                mfParams[j] = _stackBuffer[_tos--];

                            _stackBuffer[++_tos] = _calc.EvaluateMultiFunction(t.Index, mfParams);
                            continue;
                        case TokenTypes.CustomFunction:
                            var cf = _functions[t.Index];
                            var cfParamCount = cf.ParameterCount;
                            if (cf.IsRecursion)
                            {
                                tos -= cfParamCount;
                                _stackBuffer[++_tos] = Value.NaN;
                            }
                            else
                            {
                                switch (cf.ParameterCount)
                                {
                                    case 1:
                                        var x = _stackBuffer[_tos--];
                                        _stackBuffer[++_tos] = EvaluateFunction((CustomFunction1)cf, x);
                                        continue;
                                    case 2:
                                        var y = _stackBuffer[_tos--];
                                        x = _stackBuffer[_tos--];
                                        _stackBuffer[++_tos] = EvaluateFunction((CustomFunction2)cf, x, y);
                                        continue;
                                    case 3:
                                        var z = _stackBuffer[_tos--];
                                        y = _stackBuffer[_tos--];
                                        x = _stackBuffer[_tos--];
                                        _stackBuffer[++_tos] = EvaluateFunction((CustomFunction3)cf, x, y, z);
                                        continue;
                                    default:
                                        var cfParams = new Value[cfParamCount];
                                        for (int j = cfParamCount - 1; j >= 0; --j)
                                            cfParams[j] = _stackBuffer[_tos--];

                                        _stackBuffer[++_tos] = EvaluateFunction((CustomFunctionN)cf, cfParams);
                                        continue;
                                }
                            }
                            continue;
                        case TokenTypes.Solver:
                            _stackBuffer[++_tos] = EvaluateSolver(t);
                            continue;
                        default:
                            Throw.CannotEvaluateAsTypeException(t.Content, t.Type.GetType().GetEnumName(t.Type));
                            break;
                    }
                }
                if (_tos > tos)
                {
                    var v = _stackBuffer[_tos--];
                    _parser.Units = ApplyUnits(ref v, _parser._targetUnits);
                    return v;
                }
                if (_tos > tos)
                    Throw.StackLeakException();

                _parser.Units = null;
                return new Value(0.0);
            }

            private static Unit ApplyUnits(ref Value v, Unit u)
            {
                var vu = v.Units;
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
                    v = v.IsReal ?
                        new Value(v.Re * c, u) :
                        new Value(v.Complex * c, u);

                    return v.Units;
                }
                var cu = u.Text[0];
                if ((cu == '%' || cu == '‰') && vu is null)
                {
                    v = new Value(v.Complex * (cu == '%' ? 100d : 1000d), u);
                    return u;
                }
                if (!Unit.IsConsistent(vu, u))
                    Throw.InconsistentTargetUnitsException(Unit.GetText(vu), Unit.GetText(u));

                var d = vu.ConvertTo(u);
                if (u.IsTemp)
                {
                    if (v.IsReal)
                    {
                        var re = v.Re * d + GetTempUnitsDelta(vu.Text, u.Text);
                        v = new Value(re, u);

                    }
                    else
                    {
                        var c = v.Complex * d + GetTempUnitsDelta(vu.Text, u.Text);
                        v = new Value(c, u);
                    }
                }
                else
                    v = v.IsReal ?
                        new Value(v.Re * d, u) :
                        new Value(v.Complex * d, u);

                return v.Units;
            }

            private static double GetTempUnitsDelta(string src, string tgt) =>
                src switch
                {
                    "°C" => tgt switch
                    {
                        "K" => 273.15,
                        "°F" => 32.0,
                        "°R" => 491.67,
                        _ => 0
                    },
                    "K" => tgt switch
                    {
                        "°C" => -273.15,
                        "°F" => -459.67,
                        "°R" => 0,
                        _ => 0
                    },
                    "°F" => tgt switch
                    {
                        "°C" => -17.7777777777777778,
                        "K" => 255.3722222222222222,
                        "°R" => 459.67,
                        _ => 0
                    },
                    "°R" => tgt switch
                    {
                        "°C" => -273.15,
                        "°F" => -459.67,
                        "K" => 0,
                        _ => 0
                    },
                    _ => 0,
                };

            private Value EvaluateSolver(Token t)
            {
                var solveBlock = _solveBlocks[t.Index];
                solveBlock.Calculate();
                return solveBlock.Result;
            }

            internal Value EvaluateFunction(CustomFunction1 cf, in Value x)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal Value EvaluateFunction(CustomFunction2 cf, in Value x, in Value y)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x, y);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal Value EvaluateFunction(CustomFunction3 cf, in Value x, in Value y, in Value z)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(x, y, z);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            internal Value EvaluateFunction(CustomFunctionN cf, Value[] arguments)
            {
                cf.Function ??= _parser.CompileRpn(cf.Rpn);
                var result = cf.Calculate(arguments);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            private Value EvaluateToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    return EvaluatePercent(t is ValueToken vt ?
                        vt.Value :
                        ((VariableToken)t).Variable.ValueByRef()
                        );
                }
                if (t.Type == TokenTypes.Variable)
                    return EvaluateVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
                    Throw.UndefinedInputFieldException();

                return ((ValueToken)t).Value;
            }

            private Value EvaluateVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized)
                {
                    if (_parser._isSolver == 0)
                        _parser._hasVariables = true;

                    return EvaluatePercent(v.ValueByRef());
                }
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
                    Throw.UndefinedVariableOrUnitsException(t.Content);
                    return default;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Value EvaluatePercent(in Value v)
            {
                if (v.Units is not null && v.Units.IsDimensionless)
                {
                    return v.Units.Text[0] switch
                    {
                        '%' => new Value(v.Complex * 0.01),
                        '‰' => new Value(v.Complex * 0.001),
                        _ => v
                    };
                }
                return v;
            }

            private Value EvaluateToken(Token t, in Value a)
            {
                if (t.Type != TokenTypes.Function && t.Content != NegateString)
                    Throw.ErrorEvaluatingAsFunctionException(t.Content);

                return _calc.EvaluateFunction(t.Index, a);
            }

            private Value EvaluateToken(Token t, in Value a, in Value b)
            {
                if (t.Type == TokenTypes.Operator)
                    return _calc.EvaluateOperator(t.Index, a, b);

                if (t.Type != TokenTypes.Function2)
                    Throw.ErrorEvaluatingAsFunctionOrOperatorException(t.Content);

                return _calc.EvaluateFunction2(t.Index, a, b);
            }

            private static Value EvaluateIf(in Value condition, in Value valueIfTrue, in Value valueIfFalse)
            {
                if (Math.Abs(condition.Re) < 1e-12)
                    return valueIfFalse;

                return valueIfTrue;
            }
        }
    }
}