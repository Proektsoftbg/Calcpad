using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class SyntaxAnalyser
        {
            private struct MultiFunctionStackItem
            {
                internal Token Token;
                internal int CountOfBrackets;
                internal int CountOfDivisors;
                internal MultiFunctionStackItem(Token token, int countOfBrackets, int countOfDivisors)
                {
                    Token = token;
                    CountOfBrackets = countOfBrackets;
                    CountOfDivisors = countOfDivisors;
                }
            }
            private static readonly int[] OrderIndex = 
                //N  C  V  U  I  O  F  2  M  I  C  L  R  D  S  E
                { 0, 1, 1, 1, 1, 2, 3, 3, 3, 3, 3, 4, 5, 6, 1, 0 };
            //[N]one
            //[C]onstant
            //[V]ariable
            //[U]nit
            //[I]nput
            //[O]perator
            //[F]unction
            //Function[2]
            //[M]ultiFunction
            //[I]f
            //[C]ustomFunction
            //Bracket[L]eft
            //Bracket[R]ight
            //[D]ivisor
            //[S]olver
            //[E]rror
            private static readonly bool[,] CorrectOrder =
            {
                {true, true, true, true, true, true, true},
                {true, false, true, false, false, true, true},
                {true, true, false, true, true, false, false},
                {true, true, false, false, true, false, false},
                {true, true, false, true, true, false, false},
                {true, false, true, false, false, true, true},
                {true, true, false, true, true, false, false}
            };

            private readonly Container<CustomFunction> _functions;

            internal SyntaxAnalyser(Container<CustomFunction> functions)
            {
                _functions = functions;
            }

            internal void Check(Queue<Token> input, out bool isFunctionDefinition)
            {
                isFunctionDefinition = false;
                if (!input.Any())
                    return;

                var countOfBrackets = 0;
                var countOfOperators = 0;
                var countOfDivisors = 0;
                var multiFunctionStack = new Stack<MultiFunctionStackItem>();
                var pt = new Token(string.Empty, TokenTypes.None);
                var firstToken = input.Peek();
                foreach (var t in input)
                {
                    if (t.Type == TokenTypes.Function2)
                        --countOfDivisors;
                    else if (t.Type == TokenTypes.If)
                        countOfDivisors -= 2;
                    else if (t.Type == TokenTypes.MultiFunction)
                        multiFunctionStack.Push(new MultiFunctionStackItem(t, countOfBrackets, countOfDivisors));
                    else if (t.Type == TokenTypes.CustomFunction)
                    {
                        if (t.Index < 0)
                        {
                            if (isFunctionDefinition && t.Content == firstToken.Content)
#if BG
                                throw new MathParserException($"Не e разрешена рекурсия в дефиницията на функция:\"{t.Content}\".");
#else
                                throw new MathParserException($"Recursion is not allowed in function definition:\"{t.Content}\".");
#endif
                        }
                        else
                            countOfDivisors = countOfDivisors - _functions[t.Index].ParameterCount + 1;
                    }
                    else if (t.Type == TokenTypes.BracketLeft)
                        ++countOfBrackets;
                    else if (t.Type == TokenTypes.BracketRight)
                    {
                        --countOfBrackets;
                        if (countOfBrackets < 0)
#if BG
                            throw new MathParserException("Липсва лява скоба '('.");
#else
                            throw new MathParserException("Missing left bracket '('.");
#endif
                        if (multiFunctionStack.TryPeek(out var mfsItem))
                        {
                            if (countOfBrackets == mfsItem.CountOfBrackets)
                            {
                                multiFunctionStack.Pop();
                                FunctionToken ft = (FunctionToken)mfsItem.Token;
                                ft.ParameterCount = countOfDivisors - mfsItem.CountOfDivisors + 1;
                                countOfDivisors = mfsItem.CountOfDivisors;
                            }
                        }
                    }
                    else if (t.Type == TokenTypes.Divisor)
                        ++countOfDivisors;
                    else if (t.Type == TokenTypes.Operator)
                    {
                        ++countOfOperators;
                        if (t.Content == "=")
                        {
                            if (firstToken.Type == TokenTypes.CustomFunction)
                            {
                                countOfDivisors = 0;
                                isFunctionDefinition = true;
                            }
                            else if (pt.Type != TokenTypes.Variable && pt.Type != TokenTypes.Unit)
                            {
#if BG
                                throw new MathParserException("Преди оператора за присвояване '=' трябва да има функция или променлива.");
#else
                                throw new MathParserException("The assignment '=' must be preceded by custom function or variable.");
#endif
                            }
                            else if (countOfOperators != 1)
                            {
#if BG
                                throw new MathParserException("Преди оператора за присвояване '=' не може да има други оператори.");
#else
                                throw new MathParserException("Assignment '=' must be the first operator in the expression.");
#endif
                            }
                        }
                    }
                    bool correctOrder;
                    if (pt.Content == NegateString)
                        correctOrder = CorrectOrder[OrderIndex[(int)TokenTypes.Operator], OrderIndex[(int)t.Type]];
                    else if (pt.Content == "!")
                        correctOrder = CorrectOrder[OrderIndex[(int)TokenTypes.Constant], OrderIndex[(int)t.Type]];
                    else if (t.Content == "!")
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)TokenTypes.Operator]];
                    else if (t.Content == NegateString)
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)TokenTypes.Function]];
                    else
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)t.Type]];

                    if (!correctOrder)
#if BG
                        throw new MathParserException($"Невалиден синтаксис: \"{pt.Content} {t.Content}\".");
#else
                        throw new MathParserException($"Invalid syntax: \"{pt.Content} {t.Content}\".");
#endif
                    pt = t;
                }

                if (pt.Type == TokenTypes.Operator ||
                    pt.Type == TokenTypes.Function && pt.Content != "!" ||
                    pt.Type == TokenTypes.Function2 ||
                    pt.Type == TokenTypes.MultiFunction ||
                    pt.Type == TokenTypes.If ||
                    pt.Type == TokenTypes.CustomFunction ||
                    pt.Type == TokenTypes.BracketLeft)
#if BG
                    throw new MathParserException("Непълен израз.");
#else
                    throw new MathParserException("Incomplete expression.");
#endif
                if (firstToken.Type == TokenTypes.CustomFunction && firstToken.Index < 0 && !isFunctionDefinition)
#if BG
                    throw new MathParserException($"Невалидна функция: \"{firstToken.Content}\".");
#else
                    throw new MathParserException($"Invalid function: \"{firstToken.Content}\".");
#endif
                if (countOfBrackets > 0)
#if BG
                    throw new MathParserException("Липсва дясна скоба ')'.");
#else
                    throw new MathParserException("Missing right bracket ')'.");
#endif

                if (countOfDivisors > 0)
#if BG
                    throw new MathParserException("Неочакван символ за разделител ';'.");
#else
                    throw new MathParserException("Unexpected delimiter ';'.");
#endif
                if (countOfDivisors < 0)
#if BG
                    throw new MathParserException("Невалиден брой аргументи на функция.");
#else
                    throw new MathParserException("Invalid number of function arguments.");
#endif
            }
        }
    }
}