using System;
using System.Collections.Generic;

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

            internal Evaluator(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _calc = parser._calc;
            }

            internal Value Evaluate(Token[] rpn)
            {
                var tos = _tos;
                var rpnLength = rpn.Length;
                if (rpnLength < 1)
#if BG
                    throw new MathParserException("Празен израз.");
#else
                    throw new MathParserException("Expression is empty.");
#endif

                var i0 = 0;
                if (rpn[0].Type == TokenTypes.Variable && rpn[rpnLength - 1].Content == "=")
                    i0 = 1;

                for (int i = i0; i < rpnLength; ++i)
                {
                    if (_tos < tos)
#if BG
                        throw new MathParserException("Стекът е празен. Невалиден израз.");
#else
                        throw new MathParserException("Stack empty. Invalid expression.");
#endif
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
#if BG
                                    throw new MathParserException("Липсва операнд.");
#else
                                    throw new MathParserException("Missing operand.");
#endif

                                var b = _stackBuffer[_tos--];
                                if (t.Content == "=")
                                {
                                    var ta = (VariableToken)rpn[0];
                                    _parser.Units = ApplyUnits(ref b, _parser._targetUnits);
                                    _parser._backupVariable = new(ta.Content, ta.Variable.Value);
                                    ta.Variable.SetValue(b);
                                    return b;
                                }

                                if (_tos == tos)
                                {
                                    if (t.Content != "-")
#if BG
                                        throw new MathParserException("Липсва операнд.");
#else
                                        throw new MathParserException("Missing operand.");
#endif

                                    c = new Value(-b.Number, b.Units);
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
                            var cfParams = new Value[cfParamCount];
                            for (int j = cfParamCount - 1; j >= 0; --j)
                                cfParams[j] = _stackBuffer[_tos--];

                            _stackBuffer[++_tos] = EvaluateFunction(cf, cfParams);
                            continue;
                        case TokenTypes.Solver:
                            _stackBuffer[++_tos] = EvaluateSolver(t);
                            continue;
                        default:
#if BG
                            throw new MathParserException($"Не мога да изчисля \"{t.Content}\" като \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#else
                            throw new MathParserException($"Cannot evaluate \"{t.Content}\" as \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#endif
                    }
                }
                if (_tos > tos)
                {
                    var v = _stackBuffer[_tos--];
                    _parser.Units = ApplyUnits(ref v, _parser._targetUnits);
                    return v;
                }
                if (_tos > tos)
#if BG
                    throw new MathParserException("Неосвободена памет в стека. Невалиден израз.");
#else
                    throw new MathParserException("Stack memory leak. Invalid expression.");
#endif
                _parser.Units = null;
                return new Value(0.0);
            }

            private static Unit ApplyUnits(ref Value v, Unit u)
            {
                var vu = v.Units;
                if (u is null)
                {
                    if (vu is null || !vu.IsForce)
                        return v.Units;

                    u = Unit.GetForceUnit(vu);
                    if (u is null)
                        return v.Units;

                    v = new Value(v.Number * vu.ConvertTo(u), u);
                    return v.Units;
                }
                if (vu is null && u.IsEmpty)
                {
                    v = new Value(v.Number * Calculator.Rad2deg, u);
                    return v.Units;
                }

                if (!Unit.IsConsistent(vu, u))
#if BG
                    throw new MathParserException($"Получените мерни единици \"{Unit.GetText(vu)}\" не съответстват на отправните \"{Unit.GetText(u)}\".");
#else
                    throw new MathParserException($"The calculated units \"{Unit.GetText(vu)}\" are inconsistent with the target units \"{Unit.GetText(u)}\".");
#endif
                var number = v.Number * vu.ConvertTo(u);
                if (u.IsTemp)
                    number += GetTempUnitsDelta(vu.Text, u.Text);

                v = new Value(number, u);
                return v.Units;
            }

            private static double GetTempUnitsDelta(string src, string tgt) =>
                src switch
                {
                    "°C" => tgt switch
                    {
                        "K" => 273.15,
                        "°F" => 32.0,
                        "R" => 491.67,
                        _ => 0
                    },
                    "K" => tgt switch
                    {
                        "°C" => -273.15,
                        "°F" => -459.67,
                        "R" => 0,
                        _ => 0
                    },
                    "°F" => tgt switch
                    {
                        "°C" => -17.0,
                        "K" => 255.372222222222,
                        "R" => 459.67,
                        _ => 0
                    },
                    "R" => tgt switch
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

            private Value EvaluateFunction(CustomFunction cf, Value[] parameters)
            {
                if (cf.IsRecursion)
                    return new Value(double.NaN);

                if (cf.Function is null)
                    cf.Function = _parser.CompileRpn(cf.Rpn);

                if (_parser.IsCanceled)
#if BG
                    throw new MathParserException("Прекъсване от потребителя.");
#else
                    throw new MathParserException("Interrupted by user.");
#endif
                var result = cf.Calculate(parameters);
                _parser.Units = ApplyUnits(ref result, cf.Units);
                return result;
            }

            private Value EvaluateToken(Token t)
            {
                if (t.Type == TokenTypes.Unit)
                {
                    if (t is ValueToken vt)
                        return vt.Value;

                    return ((VariableToken)t).Variable.Value;
                }

                else if (t.Type == TokenTypes.Variable)
                    return EvaluateVariableToken((VariableToken)t);

                if (t.Type == TokenTypes.Input && t.Content == "?")
#if BG
                    throw new MathParserException("Недефинирано поле за въвеждане.");
#else
                    throw new MathParserException("Undefined input field.");
#endif
                return ((ValueToken)t).Value;
            }

            private Value EvaluateVariableToken(VariableToken t)
            {
                var v = t.Variable;
                if (v.IsInitialized)
                {
                    if (!_parser._isSolver)
                        _parser._hasVariables = true;

                    return v.Value;
                }
                try
                {
                    var u = new Unit(t.Content);
                    t.Type = TokenTypes.Unit;
                    v.SetValue(u);
                    return v.Value;
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

            private Value EvaluateToken(Token t, Value a)
            {
                if (t.Type != TokenTypes.Function && t.Content != NegateString)
#if BG
                    throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция.");
#else
                    throw new MathParserException($"Error evaluating \"{t.Content}\" as function.");
#endif

                return _calc.EvaluateFunction(t.Index, a);
            }

            private Value EvaluateToken(Token t, Value a, Value b)
            {
                if (t.Type == TokenTypes.Operator)
                    return _calc.EvaluateOperator(t.Index, a, b);

                if (t.Type != TokenTypes.Function2)
#if BG
                    throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция или оператор.");
#else
                    throw new MathParserException($"Error evaluating \"{t.Content}\" as function or operator.");
#endif

                return _calc.EvaluateFunction2(t.Index, a, b);
            }

            private static Value EvaluateIf(in Value condition, in Value valueIfTrue, in Value valueIfFalse)
            {
                if (Math.Abs(condition.Number.Re) < 1e-12)
                    return valueIfFalse;

                return valueIfTrue;
            }
        }
    }
}