using System.Collections.Frozen;
using System.Collections.Generic;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed class SyntaxAnalyser
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

            private static readonly FrozenDictionary<TokenTypes, int> OrderIndex =
            new Dictionary<TokenTypes, int>()
            {
                {TokenTypes.None, 0 },
                {TokenTypes.Constant, 1 },
                {TokenTypes.Variable, 1 },
                {TokenTypes.Unit, 1 },
                {TokenTypes.Input, 1 },
                {TokenTypes.Vector, 7 },
                {TokenTypes.Matrix, 7 },
                {TokenTypes.VectorIndex, 8 },
                {TokenTypes.MatrixIndex, 8 },
                {TokenTypes.Operator, 2 },
                {TokenTypes.Function, 3 },
                {TokenTypes.Function2, 3 },
                {TokenTypes.Function3, 3 },
                {TokenTypes.MultiFunction, 3 },
                {TokenTypes.Interpolation, 3 },
                {TokenTypes.VectorFunction, 3 },
                {TokenTypes.VectorFunction2, 3 },
                {TokenTypes.VectorFunction3, 3 },
                {TokenTypes.VectorMultiFunction, 3 },
                {TokenTypes.MatrixFunction, 3 },
                {TokenTypes.MatrixFunction2, 3 },
                {TokenTypes.MatrixFunction3, 3 },
                {TokenTypes.MatrixFunction4, 3 },
                {TokenTypes.MatrixFunction5, 3 },
                {TokenTypes.MatrixMultiFunction, 3 },
                {TokenTypes.CustomFunction, 3 },
                {TokenTypes.BracketLeft, 4 },
                {TokenTypes.BracketRight, 5 },
                {TokenTypes.SquareBracketLeft, 4 },
                {TokenTypes.SquareBracketRight, 5 },
                {TokenTypes.Divisor, 6 },
                {TokenTypes.RowDivisor, 6 },
                {TokenTypes.Solver, 1 },
                {TokenTypes.Error, 0 }
            }.ToFrozenDictionary();

            private static readonly bool[,] CorrectOrder =
            {
               //None   Const  Oper   Func   Left   Right  Div    Vec    Index
                {true,  true,  true,  true,  true,  true,  true,  true,  false}, //None
                {true,  false, true,  false, false, true,  true,  false, false}, //Constant
                {true,  true,  false, true,  true,  false, false, true,  false}, //Operator
                {true,  false, false, false, true,  false, false, false, false}, //Function
                {true,  true,  false, true,  true,  false, false, true,  false}, //Left Bracket
                {true,  false, true,  false, false, true,  true,  false, true},  //Right Bracket
                {true,  true,  false, true,  true,  false, false, true,  false}, //Divisor
                {true,  false, true,  false, false, true,  true,  false, true},  //Vector                                             //V
                {false, true,  false, true,  true,  false, false, true,  false}, //Index                                             //I
            };

            private readonly Container<CustomFunction> _functions;

            internal SyntaxAnalyser(Container<CustomFunction> functions)
            {
                _functions = functions;
            }

            internal void Check(Queue<Token> input, bool isCalculating, out bool isFunctionDefinition)
            {
                isFunctionDefinition = false;
                if (input.Count == 0)
                    return;

                var countOfBrackets = 0;
                var countOfOperators = 0;
                var countOfDivisors = 0;
                var isIndex = 0;
                var multiFunctionStack = new Stack<MultiFunctionStackItem>();
                var vectorStack = new Stack<MultiFunctionStackItem>();
                var indexStack = new Stack<MultiFunctionStackItem>();
                var pt = new Token(string.Empty, TokenTypes.None);
                var firstToken = input.Peek();
                foreach (var t in input)
                {
                    switch (t.Type)
                    {
                        case TokenTypes.Function2:
                        case TokenTypes.VectorFunction2:
                        case TokenTypes.MatrixFunction2:
                            --countOfDivisors;
                            break;
                        case TokenTypes.Function3:
                        case TokenTypes.VectorFunction3:
                        case TokenTypes.MatrixFunction3:
                            countOfDivisors -= 2;
                            break;
                        case TokenTypes.MatrixFunction4:
                            countOfDivisors -= 3;
                            break;
                        case TokenTypes.MatrixFunction5:
                            countOfDivisors -= 4;
                            break;
                        case TokenTypes.MultiFunction:
                        case TokenTypes.Interpolation:
                        case TokenTypes.VectorMultiFunction:
                        case TokenTypes.MatrixMultiFunction:
                            multiFunctionStack.Push(new MultiFunctionStackItem(t, countOfBrackets, countOfDivisors));
                            break;
                        case TokenTypes.CustomFunction:
                            if (t.Index < 0)
                            {
                                if (isFunctionDefinition && t.Content == firstToken.Content)
                                    Throw.RecursionNotAllowedException(t.Content);

                                if (t is FunctionToken)
                                    multiFunctionStack.Push(new MultiFunctionStackItem(t, countOfBrackets, countOfDivisors));
                            }
                            else
                                countOfDivisors += 1 - _functions[t.Index].ParameterCount;
                            break;
                        case TokenTypes.BracketLeft:
                            if (isIndex != 0)
                            {
                                indexStack.Push(new MultiFunctionStackItem(pt, countOfBrackets, countOfDivisors));
                                countOfDivisors += 1 - isIndex;
                                isIndex = 0;
                            }
                            ++countOfBrackets;
                            break;
                        case TokenTypes.BracketRight:
                            --countOfBrackets;
                            if (countOfBrackets < 0)
                                Throw.MissingLeftBracketException();

                            if (multiFunctionStack.TryPeek(out var mfStackItem) &&
                                countOfBrackets == mfStackItem.CountOfBrackets)
                            {
                                multiFunctionStack.Pop();
                                if (mfStackItem.Token is FunctionToken ft)
                                    ft.ParameterCount = countOfDivisors - mfStackItem.CountOfDivisors + 1;
                                else
                                    Throw.InvalidFunctionException(mfStackItem.Token.Content);
                                
                                countOfDivisors = mfStackItem.CountOfDivisors;
                            }
                            else if (indexStack.TryPeek(out var indStackItem) &&
                                countOfBrackets == indStackItem.CountOfBrackets)
                            {
                                if (!isCalculating)
                                    countOfDivisors = mfStackItem.CountOfDivisors;

                                indexStack.Pop();
                            }

                            break;
                        case TokenTypes.SquareBracketLeft:
                            vectorStack.Push(new MultiFunctionStackItem(t, countOfBrackets, countOfDivisors));
                            break;
                        case TokenTypes.SquareBracketRight:

                            if (!vectorStack.TryPop(out var vecstItem))
                                Throw.MissingVectorOpeningBracketException();

                            if (countOfBrackets != vecstItem.CountOfBrackets)
                                Throw.BracketMismatchException();

                            countOfDivisors = vecstItem.CountOfDivisors;
                            break;
                        case TokenTypes.Divisor:
                            ++countOfDivisors;
                            break;
                        case TokenTypes.Operator:
                            if (indexStack.Count == 0)
                                ++countOfOperators;
                            if (t.Content == "=")
                            {
                                if (firstToken.Type == TokenTypes.CustomFunction)
                                {
                                    countOfDivisors = 0;
                                    isFunctionDefinition = true;
                                }
                                else if (isCalculating)
                                {
                                    if (
                                       pt.Type != TokenTypes.Variable &&
                                       pt.Type != TokenTypes.Unit &&
                                       firstToken.Type != TokenTypes.Vector &&
                                       firstToken.Type != TokenTypes.Matrix
                                    )
                                       Throw.AssignmentPrecededException();
                                    else if (countOfOperators != 1)
                                        Throw.AssignmentNotFirstException();
                                }
                            }
                            break;
                        case TokenTypes.VectorIndex:
                            isIndex = 1;
                            break;
                        case TokenTypes.MatrixIndex:
                            isIndex = 2;
                            break;
                    }
                    CheckOrder(pt, t);
                    pt = t;
                }
                if (pt.Type != TokenTypes.None &&
                    pt.Type != TokenTypes.Constant &&
                    pt.Type != TokenTypes.Variable &&
                    pt.Type != TokenTypes.Unit &&
                    pt.Type != TokenTypes.Input &&
                    pt.Type != TokenTypes.Vector &&
                    pt.Type != TokenTypes.Matrix &&
                    pt.Type != TokenTypes.BracketRight &&
                    pt.Type != TokenTypes.SquareBracketRight &&
                    pt.Type != TokenTypes.Solver &&
                    pt.Type != TokenTypes.Error &&
                    pt.Content != "!")
                    Throw.IncompleteExpressionException();

                if (firstToken.Type == TokenTypes.CustomFunction && 
                    firstToken.Index < 0 && 
                    !isFunctionDefinition &&
                    isCalculating)
                    Throw.InvalidFunctionException(firstToken.Content);

                if (countOfBrackets > 0)
                    Throw.MissingRightBracketException();

                if (vectorStack.Count > 0)
                    Throw.MissingVectorClosingBracketException();

                if (countOfDivisors > 0)
                    Throw.UnexpectedDelimiterException();

                if (countOfDivisors < 0)
                    Throw.InvalidNumberOfArgumentsException();
            }

            private static void CheckOrder(Token pt, Token ct)
            {
                var ptt = pt.Type;
                var ctt = ct.Type;
                if (pt.Content == NegateString)
                    ptt = TokenTypes.Operator;
                else if (pt.Content == "!")
                    ptt = TokenTypes.Constant;
                else if (ct.Content == "!")
                    ctt = TokenTypes.Operator;
                else if (ct.Content == NegateString)
                    ctt = TokenTypes.Function;

                var correctOrder = CorrectOrder[OrderIndex[ptt], OrderIndex[ctt]];
                if (!correctOrder)
                    Throw.InvalidSyntaxException(pt.Content, ct.Content);
            }
        }
    }
}