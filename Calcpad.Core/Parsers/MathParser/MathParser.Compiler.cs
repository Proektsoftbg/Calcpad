using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed partial class Compiler
        {
            private readonly Expression _evaluatorInstance;
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolverBlock> _solveBlocks;
            private readonly Calculator _calc;
            private readonly MatrixCalculator _matrixCalc;
            private bool _allowAssignment;

            internal Compiler(MathParser parser)
            {
                _parser = parser;
                _evaluatorInstance = Expression.Constant(_parser._evaluator);
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
                _matrixCalc = parser._matrixCalc;
            }

            internal Func<IValue> Compile(Token[] rpn, bool allowAssignment)
            {
                if (rpn.Length < 1)
                    throw Exceptions.ExpressionEmpty();

                _allowAssignment = allowAssignment;
                var expression = Expression.Convert(Parse(rpn), typeof(IValue));
                var lambda = Expression.Lambda<Func<IValue>>(expression);
                return lambda.Compile();
            }

            internal Expression RpnToExpressionTree(Token[] rpn, bool allowAssignment)
            {
                if (rpn.Length < 1)
                    throw Exceptions.ExpressionEmpty();

                _allowAssignment = allowAssignment;
                return Expression.Convert(Parse(rpn), typeof(IValue));
            }

            private Expression Parse(Token[] rpn)
            {
                var stackBuffer = new Stack<Expression>();
                var len = rpn.Length;
                if (_allowAssignment && (rpn[len - 1].Content == "="))
                { 
                    var rpn0 = rpn[0];
                    if (rpn0.Type == TokenTypes.Variable &&
                        rpn0 is VariableToken vt)
                    {
                        var v = vt.Variable;
                        if (!v.IsInitialized)
                            v.Assign(RealValue.Zero);
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
                        case TokenTypes.MatrixOptionalFunction:
                            if (stackBuffer.Count == 0)
                                throw Exceptions.MissingOperand();

                            var a1 = stackBuffer.Pop();
                            stackBuffer.Push(ParseToken(t, a1));
                            continue;
                        case TokenTypes.Operator:
                        case TokenTypes.Function2:
                        case TokenTypes.VectorFunction2:
                        case TokenTypes.MatrixFunction2:
                        case TokenTypes.MatrixIterativeFunction:
                            if (stackBuffer.Count == 0)
                                throw Exceptions.MissingOperand();

                            var b2 = stackBuffer.Pop();
                            if (!stackBuffer.TryPop(out var a2))
                                throw Exceptions.MissingOperand();
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
                                throw Exceptions.InvalidFunction(t.Content);

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
                            var vTarget = stackBuffer.Pop();
                            stackBuffer.Push(ParseVectorIndexToken(vTarget, index));
                            continue;
                        case TokenTypes.MatrixIndex:
                            var index2 = stackBuffer.Pop();
                            var index1 = stackBuffer.Pop();
                            var mTarget = stackBuffer.Pop();
                            stackBuffer.Push(ParseMatrixIndexToken(mTarget, index1, index2));
                            continue;
                        case TokenTypes.Solver:
                            stackBuffer.Push(ParseSolverToken(t));
                            continue;
                        default:
                            throw Exceptions.CannotEvaluateAsType(t.Content, t.Type.GetType().GetEnumName(t.Type));
                    }
                }
                if (stackBuffer.Count == 0)
                    throw Exceptions.StackLeak();

                return stackBuffer.Pop();
            }

            private static Expression[] PopValues(Stack<Expression> stackBuffer, int count)
            {
                var values = new Expression[count];
                for (int i = count - 1; i >= 0; --i)
                    values[i] = stackBuffer.Pop();

                return values;
            }

            private static Expression ParseToken(Token t) =>
                t.Type switch
                {
                    TokenTypes.Unit =>
                        Expression.Constant(
                            t is ValueToken vt ?
                            (RealValue)vt.Value :
                            (RealValue)((VariableToken)t).Variable.ValueByRef()
                            , typeof(IValue)),
                    TokenTypes.Variable =>
                        ParseVariableToken((VariableToken)t),
                    TokenTypes.Matrix =>
                        t is MatrixToken mt ?
                            Expression.Constant(mt.Matrix, typeof(IValue)):
                            ParseVariableToken((VariableToken)t),
                    TokenTypes.Vector =>
                        t is VectorToken vt ?
                            Expression.Constant(vt.Vector, typeof(IValue)):
                            ParseVariableToken((VariableToken)t),
                    TokenTypes.Input when t.Content == "?" =>
                        throw Exceptions.UndefinedInputField(),
                    _ =>
                        Expression.Constant(((ValueToken)t).Value, typeof(IValue))
                };

            private Expression ParseToken(Token t, Expression a)
            {
                if (a.NodeType == ExpressionType.Constant)
                {
                    var s = t.Content.ToLowerInvariant().AsSpan();
                    if (!(s.SequenceEqual("random") ||
                         s.StartsWith("range") ||
                         s.StartsWith("vector") ||
                         s.StartsWith("identity") ||
                         s.StartsWith("utriang") ||
                         s.StartsWith("ltriang") ||
                         s.StartsWith("symmetric")))
                        return EvaluateConstantExpressionToken(t, a);
                }
                return t.Type switch
                {
                    TokenTypes.Operator =>
                        Expression.Negate(a),
                    TokenTypes.Function =>
                        Expression.Call(
                            EvaluateFunctionMethod,
                            Expression.Constant(_matrixCalc),
                            Expression.Constant(t.Index), a),
                    TokenTypes.VectorFunction =>
                        Expression.Invoke(
                            Expression.Constant(VectorCalculator.GetFunction(t.Index)), a),
                    TokenTypes.MatrixOptionalFunction =>
                        Expression.Invoke(
                            Expression.Constant(MatrixCalculator.GetIterativeFunction(t.Index)), a,
                            Expression.Constant(RealValue.Zero, typeof(IValue)),
                            Expression.Property(Expression.Constant(_parser), typeof(MathParser), nameof(Tol))),
                    _ => //TokenTypes.MatrixFunction
                        t.Index == MatrixCalculator.LuIndex ?
                            ParseLuDecomposition(a) :
                            Expression.Invoke(
                                Expression.Constant(MatrixCalculator.GetFunction(t.Index)), a)
                };
            }

            private Expression ParseToken(Token t, Expression a, Expression b)
            {
                var tc = t.Content;
                if (a.NodeType == ExpressionType.Constant &&
                    b.NodeType == ExpressionType.Constant)
                {
                    var s = tc.ToLowerInvariant().AsSpan();
                    if (!(s.StartsWith("matrix") ||
                        s.StartsWith("diagonal") ||
                        s.StartsWith("column")))
                        return EvaluateConstantExpressionToken(t, a, b);
                }
                return t.Type switch
                {
                    TokenTypes.Operator =>
                        ParseOperatorToken(t, a, b),
                    TokenTypes.Function2 =>
                        Expression.Call(
                            EvaluateFunction2Method,
                            Expression.Constant(_matrixCalc),
                            Expression.Constant(t.Index),
                            a, b),
                    TokenTypes.VectorFunction2 =>
                        Expression.Invoke(
                            Expression.Constant(VectorCalculator.GetFunction2(t.Index)), a, b),
                    TokenTypes.MatrixIterativeFunction =>
                        Expression.Invoke(
                            Expression.Constant(MatrixCalculator.GetIterativeFunction(t.Index)), a, b,
                                Expression.Property(Expression.Constant(_parser), typeof(MathParser), nameof(Tol))),
                    _ => //TokenTypes.MatrixFunction2
                        Expression.Invoke(
                            Expression.Constant(MatrixCalculator.GetFunction2(t.Index)), a, b)
                };
            }

            private Expression ParseOperatorToken(Token t, Expression a, Expression b)
            {
                var tc0 = t.Content[0];
                if (tc0 == '=' || tc0 == '←')
                {
                    if (a is MethodCallExpression mce)
                    {
                        var method = mce.Method;
                        var args = mce.Arguments;
                        if (method == GetVectorElementMethod)
                            return Expression.Call(SetVectorElementMethod, args[0], args[1], b);

                        if (method == GetMatrixElementMethod)
                            return Expression.Call(SetMatrixElementMethod, args[0], args[1], args[2], b);
                    }
                    if (a is MemberExpression ma)
                    {
                        a = ma.Expression;
                        return Expression.Block(
                            Expression.Call(a, AssignVariableMethod, b),
                            ma);
                    }
                    return Expression.Assign(a, b);
                }
                return tc0 switch
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
                    '⦼' => Expression.Modulo(a, b),
                    '∠' => Expression.Call(
                            EvaluatePhasorMethod,
                            Expression.Constant(a),
                            Expression.Constant(b)),
                    _ => Expression.Call(EvaluateOperatorMethod,
                            Expression.Constant(_matrixCalc),
                            Expression.Constant(t.Index), a, b),
                };
            }

            private MethodCallExpression ParseLuDecomposition(Expression a)
            {
                ref var indVariable = ref CollectionsMarshal.GetValueRefOrAddDefault(_parser._variables, "ind", out var _);
                if (indVariable?.Value is not HpVector vector)
                {
                    vector = new HpVector(0, 0, null);
                    indVariable = new(vector);
                }
                var b = Expression.Constant(vector);
                return Expression.Call(LuDecompositionMethod, a, b);
            }

            private static Expression ParseVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized || t.Type == TokenTypes.Vector || t.Type == TokenTypes.Matrix)
                    return Expression.Property(Expression.Constant(v), typeof(Variable), nameof(Variable.Value));
                try
                {
                    var u = Unit.Get(t.Content);
                    t.Type = TokenTypes.Unit;
                    v.SetValue(u);
                    return Expression.Constant(v.Value, typeof(IValue));
                }
                catch
                {
                    throw Exceptions.UndefinedVariableOrUnits(t.Content);
                }
            }

            private static IValue EvaluateConstantExpression(Expression a) =>
                (IValue)((ConstantExpression)a).Value;

            private ConstantExpression EvaluateConstantExpressionToken(Token t, Expression a)
            {
                IValue vb;
                if (t.Content == NegateString)
                {
                    vb = -EvaluateConstantExpression(a);
                    return Expression.Constant(vb, typeof(IValue));
                }
                var va = EvaluateConstantExpression(a);
                vb = t.Type switch
                {
                    TokenTypes.Function =>
                        IValue.EvaluateFunction(_matrixCalc, t.Index, va),
                    TokenTypes.VectorFunction =>
                        VectorCalculator.EvaluateVectorFunction(t.Index, va),
                    TokenTypes.MatrixOptionalFunction =>
                        MatrixCalculator.EvaluateMatrixIterativeFunction(t.Index, va, RealValue.Zero, _parser.Tol),
                    _ => //TokenTypes.MatrixFunction
                        t.Index == MatrixCalculator.LuIndex ?
                            Evaluator.EvaluateLuDecomposition(va, _parser._variables) :
                            MatrixCalculator.EvaluateMatrixFunction(t.Index, va)
                };
                _parser._evaluator.NormalizeUnits(ref vb);
                return Expression.Constant(vb, typeof(IValue));
            }

            private ConstantExpression EvaluateConstantExpressionToken(Token t, Expression a, Expression b)
            {
                var va = EvaluateConstantExpression(a);
                var vb = EvaluateConstantExpression(b);
                var vc = t.Type switch
                {
                    TokenTypes.Operator =>
                        IValue.EvaluateOperator(_matrixCalc, t.Index, va, vb),
                    TokenTypes.Function2 =>
                        IValue.EvaluateFunction2(_matrixCalc, t.Index, va, vb),
                    TokenTypes.VectorFunction2 =>
                        VectorCalculator.EvaluateVectorFunction2(t.Index, va, vb),
                    TokenTypes.MatrixIterativeFunction =>
                        MatrixCalculator.EvaluateMatrixIterativeFunction(t.Index, va, vb, _parser.Tol),
                    _ => //TokenTypes.MatrixFunction2
                        MatrixCalculator.EvaluateMatrixFunction2(t.Index, va, vb),
                };
                _parser._evaluator.NormalizeUnits(ref vc);
                return Expression.Constant(vc, typeof(IValue));
            }

            private Expression ParseFunction3Token(Token t, Expression a, Expression b, Expression c) =>
                t.Type switch
                {
                    TokenTypes.Function3 =>
                        t.Index == Calculator.IfFunctionIndex ?
                            Expression.Condition(
                                ToNegativeConditionExpression(a), c, b, typeof(IValue)) :
                            Expression.Invoke(
                                Expression.Constant(_calc.GetFunction3(t.Index)), a, b, c),
                    TokenTypes.VectorFunction3 =>
                        Expression.Invoke(
                            Expression.Constant(VectorCalculator.GetFunction3(t.Index)), a, b, c),
                    _ => //TokenTypes.MatrixFunction3
                        Expression.Invoke(
                            Expression.Constant(MatrixCalculator.GetFunction3(t.Index)), AsMatrix(a), b, c)
                };

            internal static BinaryExpression ToNegativeConditionExpression(Expression c) =>
                Expression.LessThan(
                    Expression.Property(
                        Expression.Call(
                            AsValueMethod,
                            c,
                            Expression.Constant(Exceptions.Items.Argument)
                        ),
                        typeof(IScalarValue),
                        nameof(IScalarValue.Re)
                    ),
                    Expression.Constant(ComplexValue.LogicalZero));

            private static InvocationExpression ParseFunction4Token(Token t, Expression a, Expression b, Expression c, Expression d) =>
                Expression.Invoke(
                    Expression.Constant(MatrixCalculator.GetFunction4(t.Index)), AsMatrix(a), b, c, d);

            private static InvocationExpression ParseFunction5Token(Token t, Expression a, Expression b, Expression c, Expression d, Expression e) =>
                Expression.Invoke(
                    Expression.Constant(MatrixCalculator.GetFunction5(t.Index)), AsMatrix(a), b, c, d, e);

            private static MethodCallExpression AsMatrix(Expression a) =>
                 Expression.Call(AsMatrixMethod, a, Expression.Constant(Exceptions.Items.Argument));

            private Expression ParseFunction(CustomFunction cf, Expression[] arguments)
            {
                if (cf.IsRecursion)
                    return Expression.Constant(RealValue.NaN, typeof(IValue));

                return arguments.Length switch
                {
                    1 => Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction1Method,
                        Expression.Constant((CustomFunction1)cf),
                        arguments[0]),
                    2 => Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction2Method,
                        Expression.Constant((CustomFunction2)cf),
                        arguments[0],
                        arguments[1]),
                    3 => Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunction3Method,
                        Expression.Constant((CustomFunction3)cf),
                        arguments[0],
                        arguments[1],
                        arguments[2]),
                    _ => Expression.Call(
                        _evaluatorInstance,
                        EvaluateCustomFunctionNMethod,
                        Expression.Constant((CustomFunctionN)cf),
                        Expression.NewArrayInit(typeof(IValue), arguments))
                };
            }

            private Expression ParseInterpolationToken(Token t, Expression[] arguments)
            {
                if (AreConstantParameters(arguments))
                    return EvaluateInterpolation(t, arguments);

                return Expression.Call(
                    EvaluateInterpolationMethod,
                    Expression.Constant(_matrixCalc),
                    Expression.Constant(t.Index),
                    Expression.NewArrayInit(typeof(IValue), arguments));
            }

            private Expression ParseMultiFunctionToken(Token t, Expression[] arguments)
            {
                if (AreConstantParameters(arguments))
                    return EvaluateMultiFunction(t, arguments);

                if(t.Index == Calculator.SwitchIndex)
                    return ParseSwitchFunctionToken(arguments, 0);

                var argsExpression = Expression.NewArrayInit(typeof(IValue), arguments);
                if (t.Type == TokenTypes.MultiFunction)
                    return Expression.Call(
                        EvaluateMultiFunctionMethod,
                        Expression.Constant(_matrixCalc),
                        Expression.Constant(t.Index),
                        argsExpression);

                if (t.Type == TokenTypes.VectorMultiFunction)
                    return Expression.Invoke(
                        Expression.Constant(
                            VectorCalculator.GetMultiFunction(t.Index)), 
                            argsExpression);

                //TokenTypes.MatrixMultiFunction
                return Expression.Invoke(
                    Expression.Constant(
                        MatrixCalculator.GetMultiFunction(t.Index)), 
                        argsExpression);
            }

            private static Expression ParseSwitchFunctionToken(Expression[] arguments, int start)
            {
                var len = arguments.Length - start;
                if (len == 0)
                    return Expression.Constant(RealValue.NaN, typeof(IValue));

                if (len == 1)
                    return arguments[start];

                var a = ToNegativeConditionExpression(arguments[start]);
                var b = arguments[start + 1];
                var c = ParseSwitchFunctionToken(arguments, start + 2);
                return Expression.Condition(a, c, b, typeof(IValue));
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

            private ConstantExpression EvaluateInterpolation(Token t, Expression[] arguments) =>
                Expression.Constant(
                    IValue.EvaluateInterpolation(
                        _matrixCalc, 
                        t.Index, 
                        EvaluateConstantParameters(arguments)), 
                        typeof(IValue));

            private ConstantExpression EvaluateMultiFunction(Token t, Expression[] arguments)
            {
                var mfParams = EvaluateConstantParameters(arguments);
                return t.Type switch
                {
                    TokenTypes.MultiFunction =>
                        Expression.Constant(
                            IValue.EvaluateMultiFunction(_matrixCalc, t.Index, mfParams), typeof(IValue)),
                    TokenTypes.VectorMultiFunction =>
                        Expression.Constant(
                            VectorCalculator.EvaluateVectorMultiFunction(t.Index, mfParams), typeof(IValue)),
                    _ => //TokenTypes.MatrixMultiFunction
                        Expression.Constant(
                            MatrixCalculator.EvaluateMatrixMultiFunction(t.Index, mfParams), typeof(IValue)),
                };
            }

            private static NewExpression ParseVectorToken(Expression[] values) =>
                Expression.New(
                    VectorConstructor, 
                    Expression.Call(
                        ExpandRealValuesMethod, 
                        Expression.NewArrayInit(typeof(IValue), 
                        values)));

            private static MethodCallExpression ParseMatrixToken(Expression[] values) =>
                Expression.Call(
                    JoinRowsMethod, 
                    Expression.NewArrayInit(typeof(IValue), values));

            private static MethodCallExpression ParseVectorIndexToken(Expression vector, Expression ii) =>
                Expression.Call(GetVectorElementMethod, vector, ii);

            private static MethodCallExpression ParseMatrixIndexToken(Expression matrix, Expression ii, Expression jj) =>
                Expression.Call(GetMatrixElementMethod, matrix, ii, jj);

            private UnaryExpression ParseSolverToken(Token t) =>
                Expression.Convert(
                    Expression.Call(
                        Expression.Constant(_solveBlocks[(int)t.Index]),
                        CalculateMethod),
                    typeof(IValue));

            internal static Func<IValue> CompileWhileBLock(List<Expression> expressions)
            {
                if (expressions.Count < 2)
                    throw Exceptions.InvalidNumberOfArguments();

                var condition = expressions[0];
                var resultVariable = Expression.Variable(typeof(IValue), "result");
                var counterVariable = Expression.Variable(typeof(int), "counter");
                var breakLabel = Expression.Label(typeof(IValue), "breakLabel");
                var checkCondition = Expression.IfThen(
                    ToNegativeConditionExpression(condition),
                    Expression.Break(breakLabel, resultVariable)
                );
                expressions[0] = checkCondition;
                var n = expressions.Count - 1;
                var result = expressions[n];
                expressions[n] = Expression.Assign(resultVariable, result);
                var maxCountExpression = Expression.Constant(1000000000);
                expressions.Add(Expression.PreIncrementAssign(counterVariable));
                expressions.Add(
                    Expression.IfThen(
                        Expression.GreaterThan(counterVariable, maxCountExpression),
                        Expression.Throw(Expression.Call(ThrowInfiniteLoopMethod, maxCountExpression))
                    )
                );
                var loopBody = Expression.Block(expressions);
                var whileLoop = Expression.Block(
                     [resultVariable, counterVariable],
                     Expression.Assign(counterVariable, Expression.Constant(0)),
                     Expression.Assign(resultVariable, Expression.Constant(RealValue.Zero, typeof(IValue))),
                     Expression.Loop(loopBody, breakLabel)
                );
                var lambda = Expression.Lambda<Func<IValue>>(whileLoop);
                return lambda.Compile();
            }

        }
    }
}