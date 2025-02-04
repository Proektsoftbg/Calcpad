using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private readonly Stack<Loop> _loops = new();
        private abstract class Loop
        {
            protected const int _maxCount = 1000000;
            protected readonly int _startLine;
            protected int _iteration;
            internal int Id { get; }
            internal int Iteration => _iteration;
            private protected Loop(int startLine, int count, int id)
            {
                _startLine = startLine;
                if (count < 0 || count > _maxCount)
                    count = _maxCount;

                _iteration = count;
                Id = id;
            }
            internal bool Iterate(ref int currentLine)
            {
                if (_iteration <= 1)
                    return false;

                currentLine = _startLine;
                --_iteration;

                return true;
            }
            internal void Break() => _iteration = 0;
            internal bool IsBroken => _iteration == 0;
        }

        private sealed class RepeatLoop : Loop
        {
            internal RepeatLoop(int startLine, int count, int id) :
                base(startLine, count, id)
            { }
        }

        private sealed class ForLoop : Loop
        {
            private readonly IScalarValue _start;
            private readonly IScalarValue _end;
            private readonly string _varName;

            internal ForLoop(int startLine, IScalarValue start, IScalarValue end, string varName, int id) :
                base(startLine, Math.Abs((int)(end - start).Re) + 1, id)
            {
                _start = start;
                _end = end;
                _varName = varName;
            }
            internal IScalarValue Start => _start;
            internal IScalarValue End => _end;
            internal string VarName => _varName;
        }

        private sealed class WhileLoop : Loop
        {
            private readonly string _condition;
            internal WhileLoop(int startLine, string condition, int id) :
                base(startLine, _maxCount, id)
            {
                _condition = condition;
            }
            internal string Condition => _condition;
        }
    }
}