using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private partial class Compiler
        {
            private readonly Expression _evaluatorInstance;
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Calculator _calc;
            private readonly VectorCalculator _vectorCalc;
            private readonly MatrixCalculator _matrixCalc;
            private  bool _allowAssignment;

            internal Compiler(MathParser parser)
            {
                _parser = parser;
                _evaluatorInstance = Expression.Constant(_parser._evaluator);
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
                _vectorCalc = parser._vectorCalc;
                _matrixCalc = parser._matrixCalc;
            }

            internal Func<IValue> Compile(Token[] rpn, bool allowAssignment = false)
            {
                if (rpn.Length < 1)
                    Throw.ExpressionEmptyException();
                _allowAssignment = allowAssignment;
                var expression = Expression.Convert(Parse(rpn), typeof(IValue));
                var lambda = Expression.Lambda<Func<IValue>>(expression);
                return lambda.Compile();
            }

            private Expression Parse(Token[] rpn)
            {
                var stackBuffer = new Stack<Expression>();
                var len = rpn.Length;
                if (_allowAssignment && 
                    rpn[len - 1].Content == "=")
                {
                    var rpn0 = rpn[0];
                    if (rpn0.Type == TokenTypes.Variable && 
                        rpn0 is  VariableToken vt)
                    {
                        var v = vt.Variable;
                        if (!v.IsInitialized)
                            v.Assign(Value.Zero);
                    }
                }
                for (int i = 0; i < len; ++i)
                {
                    var t = rpn[i];
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                            stackBuffer.Push(
                                Expression.Constant(((ValueToken)t).Value,
                                typeof(IValue)));
                            continue;
                        case TokenTypes.Unit:
                        case TokenTypes.Variable:
                        case TokenTypes.Input:
                            stackBuffer.Push(ParseToken(t));
                            continue;
                        case TokenTypes.Operator when t.Content == NegateString:
                        case TokenTypes.Function:
                        case TokenTypes.VectorFunction:
                        case TokenTypes.MatrixFunction:
                            if (stackBuffer.Count == 0)
                                Throw.MissingOperandException();

                            var a1 = stackBuffer.Pop();
                            stackBuffer.Push(ParseToken(t, a1));
                            continue;
                        case TokenTypes.Operator:
                        case TokenTypes.Function2:
                        case TokenTypes.VectorFunction2:
                        case TokenTypes.MatrixFunction2:
                            if (stackBuffer.Count == 0)
                                Throw.MissingOperandException();

                            var b2 = stackBuffer.Pop();
                            if (!stackBuffer.TryPop(out var a2))
                                Throw.MissingOperandException();
                            else
                                stackBuffer.Push(ParseToken(t, a2, b2));

                            continue;
                        case TokenTypes.Function3:
                        case TokenTypes.VectorFunction3:
                        case TokenTypes.MatrixFunction3:
                            Expression c3 = stackBuffer.Pop(),
                                b3 = stackBuffer.Pop(),
                                a3 = stackBuffer.Pop();
                            stackBuffer.Push(ParseFunction3Token(t, a3, b3, c3));
                            continue;
                        case TokenTypes.MatrixFunction4:
                            Expression d4 = stackBuffer.Pop(),
                                c4 = stackBuffer.Pop(),
                                b4 = stackBuffer.Pop(),
                                a4 = stackBuffer.Pop();
                            stackBuffer.Push(ParseFunction4Token(t, a4, b4, c4, d4));
                            continue;
                        case TokenTypes.MatrixFunction5:
                            Expression e5 = stackBuffer.Pop(),
                                d5 = stackBuffer.Pop(),
                                c5 = stackBuffer.Pop(),
                                b5 = stackBuffer.Pop(),
                                a5 = stackBuffer.Pop();
                            stackBuffer.Push(ParseFunction5Token(t, a5, b5, c5, d5, e5));
                            continue;
                        case TokenTypes.Interpolation:
                            var iValueCount = ((FunctionToken)t).ParameterCount;
                            var iValues = PopValues(stackBuffer, iValueCount);
                            stackBuffer.Push(ParseInterpolationToken(t, iValues));
                            continue;
                        case TokenTypes.MultiFunction:
                        case TokenTypes.VectorMultiFunction:
                        case TokenTypes.MatrixMultiFunction:
                            var mfValueCount = ((FunctionToken)t).ParameterCount;
                            var mfValues = PopValues(stackBuffer, mfValueCount);
                            stackBuffer.Push(ParseMultiFunctionToken(t, mfValues));
                            continue;
                        case TokenTypes.CustomFunction:
                            if (t.Index < 0)
                                Throw.InvalidFunctionException(t.Content);

                            var cf = _functions[t.Index];
                            var cfValues = PopValues(stackBuffer, cf.ParameterCount);
                            stackBuffer.Push(ParseFunction(cf, cfValues));
                            continue;
                        case TokenTypes.Vector:
                        case TokenTypes.RowDivisor:
                            if (t is VectorToken)
                            {
                                var values = PopValues(stackBuffer, (int)t.Index);
                                stackBuffer.Push(ParseVectorToken(values));
                            }
                            else
                                stackBuffer.Push(ParseToken(t));
                            continue;
                        case TokenTypes.Matrix:
                            if (t is MatrixToken)
                            {
                                var count = (int)(t.Index / Vector.MaxLength);
                                var values = PopValues(stackBuffer, count);
                                stackBuffer.Push(ParseMatrixToken(values));
                            }
                            else
                                stackBuffer.Push(ParseToken(t));
                            continue;
                        case TokenTypes.VectorIndex:
                            var index = stackBuffer.Pop();
                            var vtarget = stackBuffer.Pop();
                            stackBuffer.Push(ParseVectorIndexToken(vtarget, index));
                            continue;
                        case TokenTypes.MatrixIndex:
                            var index2 = stackBuffer.Pop();
                            var index1 = stackBuffer.Pop();
                            var mtarget = stackBuffer.Pop();
                            stackBuffer.Push(ParseMatrixIndexToken(mtarget, index1, index2));
                            continue;
                        case TokenTypes.Solver:
                            stackBuffer.Push(ParseSolverToken(t));
                            continue;
                        default:
                            Throw.CannotEvaluateAsTypeException(t.Content, t.Type.GetType().GetEnumName(t.Type));
                            break;
                    }
                }
                if (stackBuffer.Count == 0)
                    Throw.StackLeakException();

                return stackBuffer.Pop();
            }

            private static Expression[] PopValues(Stack<Expression> stackBuffer, int count)
            {
                var values = new Expression[count];
                for (int i = count - 1; i >= 0; --i)
                    values[i] = stackBuffer.Pop();

                return values;
            }

            private Expression ParseToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    var v = t is ValueToken vt ?
                        vt.Value :
                        (Value)((VariableToken)t).Variable.ValueByRef();

                    return Expression.Constant(v, typeof(IValue));
                }

                if (t.Type == TokenTypes.Variable)
                    return ParseVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
                    Throw.UndefinedInputFieldException();

                if (t.Type == TokenTypes.Matrix)
                {
                    if (t is MatrixToken mt)
                        return Expression.Constant(mt.Matrix, typeof(IValue));

                    return ParseVariableToken((VariableToken)t);
                }
                if (t.Type == TokenTypes.Vector)
                {
                    if (t is VectorToken vt)
                        return Expression.Constant(vt.Vector, typeof(IValue));

                    return ParseVariableToken((VariableToken)t);
                }
                return Expression.Constant(((ValueToken)t).Value, typeof(IValue));
            }

            private Expression ParseVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized || t.Type == TokenTypes.Vector || t.Type == TokenTypes.Matrix)
                {
                    if (t.Index < 0 && !_allowAssignment)
                        return Expression.Constant(v.Value, typeof(IValue));

                    return Expression.Field(Expression.Constant(v), "Value");
                }
                try
                {
                    var u = Unit.Get(t.Content);
                    t.Type = TokenTypes.Unit;
                    v.SetValue(u);
                    return Expression.Constant(v.Value, typeof(IValue));
                }
                catch
                {
                    Throw.UndefinedVariableOrUnitsException(t.Content);
                    return null;
                }
            }

            private Expression ParseToken(Token t, Expression a)
            {
                if (a.NodeType == ExpressionType.Constant)
                {
                    var s = t.Content.ToLowerInvariant();
                    if (s != "vector" &&
                        s != "identity" &&
                        s != "utriang" &&
                        s != "ltriang" &&
                        s != "symmetric")
                        return EvaluateConstantExpressionToken(t, a);
                }

                if (t.Content == NegateString)
                {
                    if (_parser._settings.IsComplex)
                        return Expression.Call(
                            EvaluateFunctionMethod,
                        Expression.Constant(_matrixCalc),
                        Expression.Constant(t.Index), a);

                    return Expression.Negate(a);
                }

                if (t.Type == TokenTypes.Function)
                    return Expression.Call(
                        EvaluateFunctionMethod,
                        Expression.Constant(_matrixCalc),
                        Expression.Constant(t.Index), a);

                if (t.Type == TokenTypes.VectorFunction)
                    return Expression.Call(
                        Expression.Constant(_vectorCalc),
                        EvaluateVectorFunctionMethod,
                        Expression.Constant(t.Index), a);

                //TokenTypes.MatrixFunction
                return Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixFunctionMethod,
                    Expression.Constant(t.Index), a);
            }

            private Expression ParseToken(Token t, Expression a, Expression b)
            {
                if (a.NodeType == ExpressionType.Constant &&
                    b.NodeType == ExpressionType.Constant)
                {
                    var s = t.Content.ToLowerInvariant();
                    if (s != "matrix" &&
                        s != "diagonal" &&
                        s != "column")
                        return EvaluateConstantExpressionToken(t, a, b);
                }

                if (t.Type == TokenTypes.Operator)
                {
                    if (t.Content[0] == '=')
                    {
                        if (a.NodeType == ExpressionType.Block)
                        {
                            b = Expression.Convert(b, typeof(Value));
                            BlockExpression c = (BlockExpression)a;
                            a = c.Expressions[1];
                            if (a.NodeType == ExpressionType.Convert)
                                a = ((UnaryExpression)a).Operand;

                            return Expression.Block(
                                c.Expressions[0],
                                Expression.Assign(a, b));
                        }
                        return Expression.Assign(a, b);
                    }
                    if (_parser._settings.IsComplex)
                        return Expression.Call(
                            EvaluateOperatorMethod,
                            Expression.Constant(_matrixCalc),
                            Expression.Constant(t.Index), a, b);

                    return t.Content[0] switch
                    {
                        '+' => Expression.Add(a, b),
                        '-' => Expression.Subtract(a, b),
                        '/' or '÷' => Expression.Divide(a, b),
                        '*' => Expression.Multiply(a, b),
                        '<' => Expression.LessThan(a, b),
                        '>' => Expression.GreaterThan(a, b),
                        '≤' => Expression.LessThanOrEqual(a, b),
                        '≥' => Expression.GreaterThanOrEqual(a, b),
                        '≡' => Expression.Call(EvaluateEqual, a, b),
                        '≠' => Expression.Call(EvaluateNotEqual, a, b),
                        '∧' => Expression.And(a, b),
                        '∨' => Expression.Or(a, b),
                        '⊕' => Expression.ExclusiveOr(a, b),
                        _ => Expression.Call(EvaluateOperatorMethod,
                            Expression.Constant(_matrixCalc),
                            Expression.Constant(t.Index), a, b),
                    };
                }
                if (t.Type == TokenTypes.Function2)
                    return Expression.Call(
                        EvaluateFunction2Method,
                        Expression.Constant(_matrixCalc),
                        Expression.Constant(t.Index),
                        a, b);

                if (t.Type == TokenTypes.VectorFunction2)
                    return Expression.Call(
                        Expression.Constant(_vectorCalc),
                        EvaluateVectorFunction2Method,
                        Expression.Constant(t.Index), a, b);


                //TokenTypes.MatrixFunction2
                return Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixFunction2Method,
                    Expression.Constant(t.Index), a, b);
            }

            private static IValue EvaluateConstantExpression(Expression a) =>
                (IValue)((ConstantExpression)a).Value;

            private ConstantExpression EvaluateConstantExpressionToken(Token t, Expression a)
            {
                IValue vb;
                if (t.Content == NegateString)
                {
                    vb = EvaluateConstantExpression(a);
                    if (_parser._settings.IsComplex && vb is Value v)
                        vb = ComplexCalculator.Negate(v);
                    else
                        vb = -vb;

                    return Expression.Constant(vb, typeof(IValue));
                }
                if (t.Type == TokenTypes.Function)
                    vb = IValue.EvaluateFunction(_matrixCalc, t.Index, EvaluateConstantExpression(a));
                else if (t.Type == TokenTypes.VectorFunction)
                    vb = _vectorCalc.EvaluateVectorFunction(t.Index, EvaluateConstantExpression(a));
                else //TokenTypes.MatrixFunction
                    vb = _matrixCalc.EvaluateMatrixFunction(t.Index, EvaluateConstantExpression(a));

                _parser._evaluator.NormalizeUnits(ref vb);
                return Expression.Constant(vb, typeof(IValue));
            }

            private ConstantExpression EvaluateConstantExpressionToken(Token t, Expression a, Expression b)
            {
                var va = EvaluateConstantExpression(a);
                var vb = EvaluateConstantExpression(b);
                IValue vc;
                if (t.Type == TokenTypes.Operator)
                    vc = IValue.EvaluateOperator(_matrixCalc, t.Index, va, vb);
                else if (t.Type == TokenTypes.Function2)
                    vc = IValue.EvaluateFunction2(_matrixCalc, t.Index, va, vb);
                else if (t.Type == TokenTypes.VectorFunction2)
                    vc = _vectorCalc.EvaluateVectorFunction2(t.Index, va, vb);
                else //TokenTypes.MatrixFunction2
                    vc = _matrixCalc.EvaluateMatrixFunction2(t.Index, va, vb);

                _parser._evaluator.NormalizeUnits(ref vc);
                return Expression.Constant(vc, typeof(IValue));
            }

            private Expression ParseFunction3Token(Token t, Expression a, Expression b, Expression c)
            {
                if (t.Type == TokenTypes.Function3)
                    return Expression.Invoke(
                        Expression.Constant(_calc.GetFunction3(t.Index)), a, b, c);

                if (t.Type == TokenTypes.VectorFunction3)
                    return Expression.Call(
                        Expression.Constant(_vectorCalc),
                        EvaluateVectorFunction3Method,
                        Expression.Constant(t.Index), a, b, c);

                //TokenTypes.MatrixFunction3
                return Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixFunction3Method,
                    Expression.Constant(t.Index), a, b, c);
            }

            private MethodCallExpression ParseFunction4Token(Token t, Expression a, Expression b, Expression c, Expression d) =>
                Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixFunction4Method,
                    Expression.Constant(t.Index), a, b, c, d);

            private MethodCallExpression ParseFunction5Token(Token t, Expression a, Expression b, Expression c, Expression d, Expression e) =>
                Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixFunction5Method,
                    Expression.Constant(t.Index), a, b, c, d, e);

            private Expression ParseFunction(CustomFunction cf, Expression[] arguments)
            {
                if (cf.IsRecursion)
                    return Expression.Constant(Value.NaN, typeof(IValue));

                if (AreConstantParameters(arguments))
                {
                    var parameters = EvaluateConstantParameters(arguments).Cast<IValue>().ToArray();
                    return cf.ParameterCount switch
                    {
                        1 => Expression.Constant(
                            _parser._evaluator.EvaluateFunction(
                                (CustomFunction1)cf, parameters[0]),
                            typeof(IValue)),
                        2 => Expression.Constant(
                            _parser._evaluator.EvaluateFunction(
                                (CustomFunction2)cf, parameters[0], parameters[1]),
                            typeof(IValue)),
                        3 => Expression.Constant(
                            _parser._evaluator.EvaluateFunction(
                                (CustomFunction3)cf, parameters[0], parameters[1], parameters[2]),
                            typeof(IValue)),
                        _ => Expression.Constant(
                            _parser._evaluator.EvaluateFunction(
                                (CustomFunctionN)cf, parameters),
                            typeof(IValue)),
                    };
                }
                var n = arguments.Length;
                if (n == 1)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction1Method,
                        Expression.Constant((CustomFunction1)cf),
                        arguments[0]
                    );

                if (n == 2)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction2Method,
                        Expression.Constant((CustomFunction2)cf),
                        arguments[0],
                        arguments[1]
                    );

                if (n == 3)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction3Method,
                        Expression.Constant((CustomFunction3)cf),
                        arguments[0],
                        arguments[1],
                        arguments[2]
                    );

                return Expression.Call(
                    _evaluatorInstance,
                    EvaluateCustomFunctionNMethod,
                    Expression.Constant((CustomFunctionN)cf),
                    Expression.NewArrayInit(typeof(IValue), arguments)
                );
            }

            private Expression ParseInterpolationToken(Token t, Expression[] arguments)
            {
                if (AreConstantParameters(arguments))
                    return EvaluateInterpolation(t, arguments);

                var argsExpression = Expression.NewArrayInit(typeof(IValue), arguments);
                return Expression.Call(
                    EvaluateInterpolationMethod,
                    Expression.Constant(_matrixCalc),
                    Expression.Constant(t.Index),
                    argsExpression);
            }

            private Expression ParseMultiFunctionToken(Token t, Expression[] arguments)
            {
                if (AreConstantParameters(arguments))
                    return EvaluateMultiFunction(t, arguments);

                var argsExpression = Expression.NewArrayInit(typeof(IValue), arguments);
                if (t.Type == TokenTypes.MultiFunction)
                    return Expression.Call(
                        EvaluateMultiFunctionMethod,
                        Expression.Constant(_matrixCalc),
                        Expression.Constant(t.Index),
                        argsExpression);

                if (t.Type == TokenTypes.VectorMultiFunction)
                    return Expression.Call(
                        Expression.Constant(_vectorCalc),
                        EvaluateVectorMultiFunctionMethod,
                        Expression.Constant(t.Index), argsExpression);

                //TokenTypes.MatrixMultiFunction
                return Expression.Call(
                    Expression.Constant(_matrixCalc),
                    EvaluateMatrixMultiFunctionMethod,
                    Expression.Constant(t.Index), argsExpression);
            }

            private static bool AreConstantParameters(Expression[] parameters)
            {
                for (int i = 0, len = parameters.Length; i < len; ++i)
                    if (parameters[i].NodeType != ExpressionType.Constant)
                        return false;

                return true;
            }

            private static IValue[] EvaluateConstantParameters(Expression[] parameters)
            {
                var len = parameters.Length;
                var values = new IValue[len];
                for (int i = 0; i < len; ++i)
                    values[i] = EvaluateConstantExpression(parameters[i]);

                return values;
            }

            private ConstantExpression EvaluateInterpolation(Token t, Expression[] arguments)
            {
                var iParams = EvaluateConstantParameters(arguments);
                return Expression.Constant(
                    IValue.EvaluateInterpolation(_matrixCalc, t.Index, iParams));
            }

            private ConstantExpression EvaluateMultiFunction(Token t, Expression[] arguments)
            {
                var mfParams = EvaluateConstantParameters(arguments);

                if (t.Type == TokenTypes.MultiFunction)
                    return Expression.Constant(
                        IValue.EvaluateMultiFunction(_matrixCalc, t.Index, mfParams));

                if (t.Type == TokenTypes.VectorMultiFunction)
                    return Expression.Constant(
                        _vectorCalc.EvaluateVectorMultiFunction(t.Index, mfParams));

                //MatrixMultiFunction
                return Expression.Constant(
                    _matrixCalc.EvaluateMatrixMultiFunction(t.Index, mfParams));
            }

            private static NewExpression ParseVectorToken(Expression[] values)
            {
                var args = Expression.NewArrayInit(typeof(IValue), values);
                return Expression.New(VectorConstructor, Expression.Call(ExpandValuesMethod, args));
            }

            private static MethodCallExpression ParseMatrixToken(Expression[] values)
            {
                var args = Expression.NewArrayInit(typeof(IValue), values);
                return Expression.Call(JoinRowsMethod, args);
            }

            private static readonly ConstantExpression IndexTargetItem =
                Expression.Constant(Throw.Items.IndexTarget);
            private static readonly ConstantExpression IndexItem =
                Expression.Constant(Throw.Items.Index);

            private static BlockExpression ParseVectorIndexToken(Expression vector, Expression value)
            {
                var i = Expression.Convert(
                    Expression.Field(
                        Expression.Call(AsValueMethod, value, IndexItem),
                        "Re"),
                    typeof(int));

                var vec = Expression.Call(AsVectorMethod, vector, IndexTargetItem);
                var checkBounds = Expression.IfThen(
                    Expression.OrElse(
                        Expression.LessThan(i, Expression.Constant(1)),
                        Expression.GreaterThan(i, Expression.Property(vec, "Length"))),
                    Expression.Call(ThrowIndexOutOfRangeException, Expression.Call(i, "ToString", Type.EmptyTypes)));
                var itemProperty = Expression.Convert(
                        Expression.Property(
                            vec, "item",
                        Expression.Subtract(i, Expression.Constant(1))),
                    typeof(IValue));
                return Expression.Block(checkBounds, itemProperty);
            }

            private static BlockExpression ParseMatrixIndexToken(Expression matrix, Expression ivalue, Expression jvalue)
            {
                var i = Expression.Convert(
                    Expression.Field(
                        Expression.Call(AsValueMethod, ivalue, IndexItem),
                        "Re"),
                    typeof(int));
                var j = Expression.Convert(
                    Expression.Field(
                        Expression.Call(AsValueMethod, jvalue, IndexItem),
                        "Re"),
                    typeof(int));
                var m = Expression.Call(AsMatrixMethod, matrix, IndexTargetItem);
                var checkRowBounds = Expression.OrElse(
                    Expression.LessThan(i, Expression.Constant(1)),
                    Expression.GreaterThan(i, Expression.Property(m, "RowCount")));
                var checkColBounds = Expression.OrElse(
                    Expression.LessThan(j, Expression.Constant(1)),
                    Expression.GreaterThan(j, Expression.Property(m, "ColCount")));
                var checkBounds = Expression.IfThen(
                    Expression.OrElse(checkRowBounds, checkColBounds),
                    Expression.Call(ThrowIndexOutOfRangeException, Expression.Call(i, "ToString", Type.EmptyTypes)));
                var itemProperty = Expression.Convert(
                        Expression.Property(
                            m, "item",
                        [Expression.Subtract(i, Expression.Constant(1)),
                        Expression.Subtract(j, Expression.Constant(1))]),
                    typeof(IValue));
                return Expression.Block(checkBounds, itemProperty);
            }

            private UnaryExpression ParseSolverToken(Token t)
            {
                var solveBlock = _solveBlocks[(int)t.Index];
                return Expression.Convert(
                    Expression.Call(
                        Expression.Constant(solveBlock),
                        CalculateMethod),
                    typeof(IValue));
            }
        }
    }
}