using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private readonly Stack<Loop> _loops = new();
        private abstract class Loop
        {
            internal const int MaxCount = 100000000;
            protected readonly int _startLine;
            protected int _iteration;
            internal int Id { get; }
            internal int Iteration => _iteration;
            private protected Loop(int startLine, double count, int id)
            {
                _startLine = startLine;
                if (count < 0 || count > MaxCount)
                    count = MaxCount;

                _iteration = (int)count;
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
            internal RepeatLoop(int startLine, double count, int id) :
                base(startLine, count, id)
            { }
        }

        private sealed class ForLoop : Loop
        {
            private double _counterRe;
            private double _counterIm;
            private readonly double _incRe;
            private readonly double _incIm;
            private readonly Unit _incUnits;
            private readonly Variable _counter;

            internal ForLoop(int startLine, IScalarValue start, IScalarValue end, Variable counter, int id) :
                base(startLine, Math.Abs((start - end).Re) + 1, id)
            {
                var inc = -(start - end); //Reversed because of units comversion
                _incRe = Math.Sign(inc.Re);
                _incIm = Math.Sign(inc.Im);
                _incUnits = inc.Units;
                _counter = counter;
                _counterRe = start.Re;
                _counterIm = start.Im;
                _counter.Assign(start);
            }

            internal void IncrementCounter()
            {
                _counterRe += _incRe;
                if (_incIm == 0d)
                    _counter.Assign(new RealValue(_counterRe, _incUnits));
                else
                {
                    _counterIm += _incIm;
                    _counter.Assign(new ComplexValue(_counterRe, _counterIm, _incUnits));
                }
            }
        }

        private sealed class WhileLoop : Loop
        {
            private readonly string _condition;
            internal WhileLoop(int startLine, string condition, int id) :
                base(startLine, MaxCount, id)
            {
                _condition = condition;
            }
            internal string Condition => _condition;
        }
    }
}