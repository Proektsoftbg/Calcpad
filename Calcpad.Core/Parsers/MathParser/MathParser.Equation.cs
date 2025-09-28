using System;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed class Equation
        {
            internal Token[] Rpn;
            internal Unit TargetUnits;
            internal Func<IValue> Function;
            internal Equation(Token[] rpn, Unit targetUnits, Func<IValue> function)
            {
                Rpn = rpn;
                TargetUnits = targetUnits;
                Function = function;
            }
        }
    }
}
