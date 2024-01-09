using System.Collections.Generic;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class SyntaxAnalyser
        {
            private readonly struct MultiFunctionStackItem
            {
                internal readonly Token Token;
                internal readonly int CountOfBrackets;
                internal readonly int CountOfDivisors;
                internal MultiFunctionStackItem(Token token, int countOfBrackets, int countOfDivisors)
                {
                    Token = token;
                    CountOfBrackets = countOfBrackets;
                    CountOfDivisors = countOfDivisors;
                }
            }
            private static readonly int[] OrderIndex =
                //N  C  V  U  I  O  F  2  M  I  C  L  R  D  S  E
                [0, 1, 1, 1, 1, 2, 3, 3, 3, 3, 3, 4, 5, 6, 1, 0];
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
                if (input.Count == 0)
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
                                Throw.RecursionNotAllowedException(t.Content);
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
                            Throw.MissingLeftBracketException();

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
                                Throw.AssignmentPrecededException();
                            else if (countOfOperators != 1)
                                Throw.AssignmentNotFirstException();
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
                        Throw.InvalidSyntaxException(pt.Content, t.Content);

                    pt = t;
                }

                if (pt.Type == TokenTypes.Operator ||
                    pt.Type == TokenTypes.Function && pt.Content != "!" ||
                    pt.Type == TokenTypes.Function2 ||
                    pt.Type == TokenTypes.MultiFunction ||
                    pt.Type == TokenTypes.If ||
                    pt.Type == TokenTypes.CustomFunction ||
                    pt.Type == TokenTypes.BracketLeft)
                    Throw.IncompleteExpressionException();

                if (firstToken.Type == TokenTypes.CustomFunction && firstToken.Index < 0 && !isFunctionDefinition)
                    Throw.InvalidFunctionException(firstToken.Content);

                if (countOfBrackets > 0)
                    Throw.MissingRightBracketException();

                if (countOfDivisors > 0)
                    Throw.UnexpectedDelimiterException();

                if (countOfDivisors < 0)
                    Throw.InvalidNumberOfArgumentsException();
            }
        }
    }
}