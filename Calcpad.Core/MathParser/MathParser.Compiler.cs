using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class Compiler
        {
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Calculator _calc;

            internal Compiler(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
            }

            internal Func<Value> Compile(Token[] rpn)
            {
                if (rpn.Length < 1)
#if BG
                    throw new MathParserException("Изразът е празен.");
#else
                    throw new MathParserException("Expression is empty.");
#endif
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
                                if (!stackBuffer.Any())
#if BG
                                    throw new MathParserException("Липсва операнд.");
#else
                                    throw new MathParserException("Missing operand.");
#endif

                                var b = stackBuffer.Pop();
                                if (!stackBuffer.TryPop(out var a))
                                {
                                    if (t.Content != "-")
#if BG
                                        throw new MathParserException("Липсва операнд.");
#else
                                        throw new MathParserException("Missing operand.");
#endif
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
#if BG
                                throw new MathParserException($"Невалидна функция: \"{t.Content}\".");
#else
                                throw new MathParserException($"Invalid function: \"{t.Content}\".");
#endif
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
#if BG
                            throw new MathParserException($"Не мога да изчисля \"{t.Content}\" като \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#else
                            throw new MathParserException($"Cannot evaluate \"{t.Content}\" as \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#endif
                    }
                }
                if (!stackBuffer.Any())
#if BG
                    throw new MathParserException("Неосвободена памет в стека. Невалиден израз.");
#else
                    throw new MathParserException("Stack memory leak. Invalid expression.");
#endif
                return stackBuffer.Pop();
            }

            private static Expression ParseToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    if (t is ValueToken vt)
                        return Expression.Constant(vt.Value);

                    return Expression.Constant(((VariableToken)t).Variable.Value);
                }

                if (t.Type == TokenTypes.Variable)
                    return ParseVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
#if BG
                    throw new MathParserException("Недефинирано поле за въвеждане.");
#else
                    throw new MathParserException("Undefined input field.");
#endif
                return Expression.Constant(((ValueToken)t).Value);
            }

            private static Expression ParseVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized)
                {
                    if (t.Index < 0)
                        return Expression.Constant(v.Value);

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
#if BG
                    throw new MathParserException($"Недефинирана променлива или мерни единици: \"{t.Content}\".");
#else
                    throw new MathParserException($"Undefined variable or units: \"{t.Content}\".");
#endif
                }
            }

            private Expression ParseToken(Token t, Expression a)
            {
                if (t.Type != TokenTypes.Function && t.Content != NegateString)
#if BG
                    throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция.");
#else
                    throw new MathParserException($"Error evaluating \"{t.Content}\" as function.");
#endif
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
                    {
                        Expression e = ((MemberExpression)a).Expression;
                        ConstantExpression c = (ConstantExpression)e;
                        Variable v = (Variable)c.Value;
                        Type type = typeof(Variable);
                        MethodInfo method = type.GetMethod(
                            "SetValue",
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            Type.DefaultBinder,
                            new[] { typeof(Value).MakeByRefType() },
                            null
                            );
                        return Expression.Block(
                            Expression.Call(Expression.Constant(v), method, b),
                            Expression.Field(Expression.Constant(v), "Value")
                            );
                    }

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
                        _ => Expression.Invoke(Expression.Constant(_calc.GetOperator(t.Index)), a, b),
                    };
                }

                if (t.Type != TokenTypes.Function2)
#if BG
                    throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция или оператор.");
#else
                    throw new MathParserException($"Error evaluating \"{t.Content}\" as function or operator.");
#endif

                return Expression.Invoke(Expression.Constant(_calc.GetFunction2(t.Index)), a, b);
            }

            private static Value EvaluateConstantExpression(Expression a)
            {
                var lambdaExpression = Expression.Lambda<Func<Value>>(a);
                var lambda = lambdaExpression.Compile();
                return lambda.Invoke();
            }

            private Expression EvaluateConstantExpressionToken(Token t, Expression a, Expression b)
            {
                var va = EvaluateConstantExpression(a);
                var vb = EvaluateConstantExpression(b);
                Value vc;
                if (t.Type == TokenTypes.Operator)
                {
                    vc =_calc.GetOperator(t.Index)(va, vb);
                }
                else if (t.Type == TokenTypes.Function2)
                {
                    vc = _calc.GetFunction2(t.Index)(va, vb);
                }
                else
#if BG
                    throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция или оператор.");
#else
                    throw new MathParserException($"Error evaluating \"{t.Content}\" as function or operator.");
#endif
                return Expression.Constant(vc);
            }

            private static Expression ParseIf(Expression condition, Expression valueIfTrue, Expression valueIfFalse)
            {
                var method = typeof(Evaluator).GetMethod("EvaluateIf", BindingFlags.Static | BindingFlags.NonPublic);
                return Expression.Call(method, condition, valueIfTrue, valueIfFalse);
            }

            private Expression ParseSolver(Token t)
            {
                var solveBlock = _solveBlocks[t.Index];
                Expression instance = Expression.Constant(solveBlock);
                var method = typeof(SolveBlock).GetMethod("Calculate", BindingFlags.Instance | BindingFlags.NonPublic);
                return Expression.Call(instance, method);
            }

            private Expression ParseFunction(CustomFunction cf, Expression[] parameters)
            {
                if (AreConstantParameters(parameters))
                    return Expression.Constant(
                        _parser._evaluator.EvaluateFunction(
                            cf, EvaluateConstantParameters(parameters)
                            )
                        );

                Expression instance = Expression.Constant(_parser._evaluator);
                var method = typeof(Evaluator).GetMethod("EvaluateFunction", BindingFlags.NonPublic | BindingFlags.Instance);
                Expression args = Expression.NewArrayInit(typeof(Value), parameters);
                return Expression.Call(instance, method, Expression.Constant(cf), args);
            }

            private Expression ParseMultiFunction(int Index, Expression[] parameters)
            {
                if (AreConstantParameters(parameters))
                    return Expression.Constant(
                        _calc.GetMultiFunction(Index)(
                            EvaluateConstantParameters(parameters)
                            )
                        );

                var method = Expression.Constant(_calc.GetMultiFunction(Index));
                Expression args = Expression.NewArrayInit(typeof(Value), parameters);
                return Expression.Invoke(method, args);
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
                    values[i]= EvaluateConstantExpression(parameters[i]);   

                return values;  
            }
        }
    }
}