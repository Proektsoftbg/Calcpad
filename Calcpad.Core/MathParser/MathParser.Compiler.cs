using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class Compiler
        {
            private static readonly MethodInfo EvaluateIfMethod =
                typeof(Evaluator).GetMethod(
                "EvaluateIf",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            private static readonly MethodInfo CalculateMethod =
                typeof(SolveBlock).GetMethod(
                "Calculate",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            private static readonly MethodInfo EvaluateFunctionMethod1 =
                typeof(Evaluator).GetMethod(
                "EvaluateFunction",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[]
                {
                    typeof(CustomFunction1),
                    typeof(Value).MakeByRefType()
                },
                null
            );

            private static readonly MethodInfo EvaluateFunctionMethod2 =
                typeof(Evaluator).GetMethod(
                "EvaluateFunction",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[]
                {
                    typeof(CustomFunction2),
                    typeof(Value).MakeByRefType(),
                    typeof(Value).MakeByRefType()
                },
                null
            );

            private static readonly MethodInfo EvaluateFunctionMethod3 =
                typeof(Evaluator).GetMethod(
                "EvaluateFunction",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[]
                {
                    typeof(CustomFunction3),
                    typeof(Value).MakeByRefType(),
                    typeof(Value).MakeByRefType(),
                    typeof(Value).MakeByRefType()
                },
                null
            );

            private static readonly MethodInfo EvaluateFunctionMethod =
                typeof(Evaluator).GetMethod(
                "EvaluateFunction",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[]
                {
                    typeof(CustomFunctionN),
                    typeof(Value[])
                },
                null
            );

            private readonly Expression _evaluatorInstance;
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Calculator _calc;

            internal Compiler(MathParser parser)
            {
                _parser = parser;
                _evaluatorInstance = Expression.Constant(_parser._evaluator);
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
            }

            internal Func<Value> Compile(Token[] rpn)
            {
                if (rpn.Length < 1)
                    Throw.ExpressionEmptyException();

                var expression = Parse(rpn);
                var lambda = Expression.Lambda<Func<Value>>(expression);
                return lambda.Compile();
            }

            private Expression Parse(Token[] rpn)
            {
                var stackBuffer = new Stack<Expression>();
                for (int i = 0, len = rpn.Length; i < len; ++i)
                {
                    var t = rpn[i];
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                            stackBuffer.Push(Expression.Constant(((ValueToken)t).Value));
                            continue;
                        case TokenTypes.Unit:
                        case TokenTypes.Input:
                        case TokenTypes.Variable:
                            stackBuffer.Push(ParseToken(t));
                            continue;
                        case TokenTypes.Operator:
                        case TokenTypes.Function:
                        case TokenTypes.Function2:
                            Expression c;
                            if (t.Type == TokenTypes.Function || t.Content == NegateString)
                            {
                                var a = stackBuffer.Pop();
                                c = ParseToken(t, a);
                            }
                            else
                            {
                                if (stackBuffer.Count == 0)
                                    Throw.MissingOperandException();

                                var b = stackBuffer.Pop();
                                if (!stackBuffer.TryPop(out var a))
                                {
                                    if (t.Content != "-")
                                        Throw.MissingOperandException();

                                    if (b.NodeType == ExpressionType.Constant)
                                        c = Expression.Constant(-EvaluateConstantExpression(b));
                                    else
                                        c = Expression.Negate(b);
                                }
                                else
                                    c = ParseToken(t, a, b);
                            }
                            stackBuffer.Push(c);
                            continue;
                        case TokenTypes.If:
                            Expression vFalse = stackBuffer.Pop(),
                                vTrue = stackBuffer.Pop(),
                                vCond = stackBuffer.Pop();
                            stackBuffer.Push(ParseIf(vCond, vTrue, vFalse));
                            continue;
                        case TokenTypes.MultiFunction:
                            var mfValueCount = ((FunctionToken)t).ParameterCount;
                            var mfValues = new Expression[mfValueCount];
                            for (int j = mfValueCount - 1; j >= 0; --j)
                                mfValues[j] = stackBuffer.Pop();

                            stackBuffer.Push(ParseMultiFunction(t.Index, mfValues));
                            continue;
                        case TokenTypes.CustomFunction:
                            if (t.Index < 0)
                                Throw.InvalidFunctionException(t.Content);

                            var cf = _functions[t.Index];
                            var cfValueCount = cf.ParameterCount;
                            var cfValues = new Expression[cfValueCount];
                            for (int j = cfValueCount - 1; j >= 0; --j)
                                cfValues[j] = stackBuffer.Pop();

                            stackBuffer.Push(ParseFunction(cf, cfValues));
                            continue;
                        case TokenTypes.Solver:
                            stackBuffer.Push(ParseSolver(t));
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

            private static Expression ParseToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    if (t is ValueToken vt)
                        return Expression.Constant(Evaluator.EvaluatePercent(vt.Value));

                    return Expression.Constant(((VariableToken)t).Variable.Value);
                }

                if (t.Type == TokenTypes.Variable)
                    return ParseVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
                    Throw.UndefinedInputFieldException();

                return Expression.Constant(((ValueToken)t).Value);
            }

            private static Expression ParseVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized)
                {
                    if (t.Index < 0)
                        return Expression.Constant(Evaluator.EvaluatePercent(v.Value));

                    return Expression.Field(Expression.Constant(v), "Value");
                }
                try
                {
                    var u = Unit.Get(t.Content);
                    t.Type = TokenTypes.Unit;
                    v.SetValue(u);
                    return Expression.Constant(v.Value);
                }
                catch
                {
                    Throw.UndefinedVariableOrUnitsException(t.Content);
                    return null;
                }
            }

            private Expression ParseToken(Token t, Expression a)
            {
                if (t.Type != TokenTypes.Function && t.Content != NegateString)
                    Throw.ErrorEvaluatingAsFunctionException(t.Content);

                if (a.NodeType == ExpressionType.Constant)
                    return Expression.Constant(_calc.GetFunction(t.Index)(EvaluateConstantExpression(a)));

                return Expression.Invoke(Expression.Constant(_calc.GetFunction(t.Index)), a);
            }

            private Expression ParseToken(Token t, Expression a, Expression b)
            {
                if (a.NodeType == ExpressionType.Constant &&
                    b.NodeType == ExpressionType.Constant)
                    return EvaluateConstantExpressionToken(t, a, b);

                if (t.Type == TokenTypes.Operator)
                {
                    if (t.Content == "=")
                        return Expression.Assign(a, b);

                    if (_parser._settings.IsComplex)
                        return Expression.Invoke(Expression.Constant(_calc.GetOperator(t.Index)), a, b);

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
                        '≡' => Expression.Equal(a, b),
                        '≠' => Expression.NotEqual(a, b),
                        '∧' => Expression.And(a, b),
                        '∨' => Expression.Or(a, b),
                        '⊕' => Expression.ExclusiveOr(a, b),
                        _ => Expression.Invoke(Expression.Constant(_calc.GetOperator(t.Index)), a, b),
                    };
                }

                if (t.Type != TokenTypes.Function2)
                    Throw.ErrorEvaluatingAsFunctionOrOperatorException(t.Content);

                return Expression.Invoke(Expression.Constant(_calc.GetFunction2(t.Index)), a, b);
            }

            private static Value EvaluateConstantExpression(Expression a) =>
                (Value)((ConstantExpression)a).Value;

            private ConstantExpression EvaluateConstantExpressionToken(Token t, Expression a, Expression b)
            {
                var va = EvaluateConstantExpression(a);
                var vb = EvaluateConstantExpression(b);
                Value vc;
                if (t.Type == TokenTypes.Operator)
                    vc = _calc.GetOperator(t.Index)(va, vb);
                else if (t.Type == TokenTypes.Function2)
                    vc = _calc.GetFunction2(t.Index)(va, vb);
                else
                {
                    Throw.ErrorEvaluatingAsFunctionOrOperatorException(t.Content);
                    return null;
                }
                return Expression.Constant(vc);
            }

            private static MethodCallExpression ParseIf(Expression condition, Expression valueIfTrue, Expression valueIfFalse)
            {
                return Expression.Call(EvaluateIfMethod, condition, valueIfTrue, valueIfFalse);
            }

            private MethodCallExpression ParseSolver(Token t)
            {
                var solveBlock = _solveBlocks[t.Index];
                Expression instance = Expression.Constant(solveBlock);
                return Expression.Call(instance, CalculateMethod);
            }

            private Expression ParseFunction(CustomFunction cf, Expression[] arguments)
            {
                if (cf.IsRecursion)
                    return Expression.Constant(Value.NaN);

                if (AreConstantParameters(arguments))
                {
                    var parameters = EvaluateConstantParameters(arguments);
                    return cf.ParameterCount switch
                    {
                        1 => Expression.Constant(_parser._evaluator.EvaluateFunction((CustomFunction1)cf, parameters[0])),
                        2 => Expression.Constant(_parser._evaluator.EvaluateFunction((CustomFunction2)cf, parameters[0], parameters[1])),
                        3 => Expression.Constant(_parser._evaluator.EvaluateFunction((CustomFunction3)cf, parameters[0], parameters[1], parameters[2])),
                        _ => Expression.Constant(_parser._evaluator.EvaluateFunction((CustomFunctionN)cf, parameters)),
                    };
                }
                var n = arguments.Length;
                if (n == 1)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateFunctionMethod1,
                        Expression.Constant((CustomFunction1)cf),
                        arguments[0]
                    );

                if (n == 2)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateFunctionMethod2,
                        Expression.Constant((CustomFunction2)cf),
                        arguments[0],
                        arguments[1]
                    );

                if (n == 3)
                    return Expression.Call(
                        _evaluatorInstance,
                        EvaluateFunctionMethod3,
                        Expression.Constant((CustomFunction3)cf),
                        arguments[0],
                        arguments[1],
                        arguments[2]
                    );

                return Expression.Call(
                    _evaluatorInstance,
                    EvaluateFunctionMethod,
                    Expression.Constant((CustomFunctionN)cf),
                    Expression.NewArrayInit(typeof(Value), arguments)
                );
            }

            private Expression ParseMultiFunction(int index, Expression[] arguments)
            {
                if (AreConstantParameters(arguments))
                    return Expression.Constant(
                        _calc.GetMultiFunction(index)(
                            EvaluateConstantParameters(arguments)
                            )
                        );

                var method = Expression.Constant(_calc.GetMultiFunction(index));
                Expression argsExpression = Expression.NewArrayInit(typeof(Value), arguments);
                return Expression.Invoke(method, argsExpression);
            }

            private static bool AreConstantParameters(Expression[] parameters)
            {
                for (int i = 0, len = parameters.Length; i < len; ++i)
                    if (parameters[i].NodeType != ExpressionType.Constant)
                        return false;

                return true;
            }

            private static Value[] EvaluateConstantParameters(Expression[] parameters)
            {
                var len = parameters.Length;
                var values = new Value[len];
                for (int i = 0; i < len; ++i)
                    values[i] = EvaluateConstantExpression(parameters[i]);

                return values;
            }
        }
    }
}