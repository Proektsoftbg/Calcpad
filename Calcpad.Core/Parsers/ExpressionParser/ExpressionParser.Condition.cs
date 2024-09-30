using System;

namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private Condition _condition;
        private class Condition
        {
            internal const int RemoveConditionKeyword = Keyword.EndIf - Keyword.If;
            private enum Types
            {
                None,
                If,
                ElseIf,
                Else,
                EndIf,
                While
            }
            private readonly struct Item
            {
                internal bool Value { get; }
                internal Types Type { get; }
                internal Item(bool value, Types type)
                {
                    Type = type;
                    Value = value;
                }
            }

            private int _count;
            private string _keyword;
            private int _keywordLength;
            private readonly Item[] _conditions = new Item[20];
            private Types Type => _conditions[Id].Type;
            internal int Id { get; private set; }
            internal bool IsUnchecked { get; private set; }
            internal bool IsSatisfied => _conditions[_count].Value;
            internal bool IsFound { get; private set; }
            internal int KeywordLength => _keywordLength;
            internal bool IsLoop => _conditions[_count].Type == Types.While;

            internal Condition()
            {
                _conditions[0] = new Item(true, Types.None);
                _keyword = string.Empty;
            }
            private void Add(bool value)
            {
                ++Id;
                _conditions[Id] = new Item(value, Types.If);
                if (IsSatisfied)
                {
                    ++_count;
                    IsFound = false;
                }
            }

            private void Remove()
            {
                --Id;
                if (_count > Id)
                {
                    --_count;
                    IsFound = true;
                }
            }

            private void Change(bool value, Types type)
            {
                _conditions[Id] = new Item(value, type);
            }

            internal void SetCondition(int index)
            {
                if (index < 0 || index >= (int)Types.While)
                {
                    if (_keywordLength > 0)
                    {
                        _keywordLength = 0;
                        _keyword = string.Empty;
                    }
                    return;
                }

                var type = (Types)(index + 1);
                _keywordLength = GetKeywordLength(type);
                _keyword = GetKeyword(type);
                IsUnchecked = type == Types.If || type == Types.ElseIf;
                if (type > Types.If && type < Types.While && _count == 0)
                    Throw.ConditionNotInitializedException();

                if (Type == Types.Else)
                {
                    if (type == Types.Else)
                        Throw.DuplicateElseException();

                    if (type == Types.ElseIf)
                        Throw.ElseIfAfterElseException();
                }
                switch (type)
                {
                    case Types.If:
                        Add(true);
                        break;
                    case Types.While:
                        _conditions[++Id] = new Item(true, type);
                        ++_count;
                        break;
                    case Types.ElseIf:
                        Change(true, Types.If);
                        break;
                    case Types.Else:
                        Change(!IsFound, type);
                        break;
                    case Types.EndIf:
                        Remove();
                        break;
                }
            }

            internal void Check(Complex value)
            {
                if (!value.IsReal)
                    Throw.ConditionComplexException();

                var d = value.Re;
                if (double.IsNaN(d) || double.IsInfinity(d))
                    Throw.ConditionResultInvalidException(d.ToString());

                var result = Math.Abs(d) > 1e-12;
                if (result)
                    IsFound = true;
                Change(result, Type);
                IsUnchecked = false;
            }

            internal void Check() => IsUnchecked = false;

            public override string ToString() => _keyword;

            internal string ToHtml()
            {
                if (string.IsNullOrEmpty(_keyword))
                    return _keyword;
                return "<span class=\"cond\">" + _keyword + "</span>";
            }

            private static int GetKeywordLength(Types type)
            {
                return type switch
                {
                    Types.If => 3,
                    Types.Else => 5,
                    Types.While => 6,
                    Types.EndIf => 7,
                    Types.ElseIf => 8,
                    _ => 0,
                };
            }

            private static string GetKeyword(Types type)
            {
                return type switch
                {
                    Types.If => "#if ",
                    Types.ElseIf => "#else if ",
                    Types.Else => "#else",
                    Types.EndIf => "#end if",
                    Types.While => "#while ",
                    _ => string.Empty,
                };
            }
        }
    }
}