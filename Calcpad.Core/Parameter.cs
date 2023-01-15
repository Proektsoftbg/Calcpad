using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal readonly struct Parameter
    {
        internal readonly string Name;
        internal readonly Variable Variable;
        internal Parameter(in string name)
        {
            Name = name;
            Variable = new Variable(Value.Zero);
        }
    }
}
