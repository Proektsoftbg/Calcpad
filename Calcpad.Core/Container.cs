using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    internal class Container<T>
    {
        internal int Count { get; private set; }
        internal string LastName { get; private set; }
        private readonly Dictionary<string, int> _index = new(StringComparer.Ordinal);
        private readonly T[] _values = new T[1000];

        internal int Add(string name, T value)
        {
            if (_index.TryGetValue(name, out var i))
                _values[i] = value;
            else
            {
                _index.Add(name, Count);
                _values[Count] = value;
                i = Count++;
            }
            LastName = name;
            return i;
        }

        internal int IndexOf(string name)
        {
            if (_index.TryGetValue(name, out var n))
                return n;

            return -1;
        }

        internal T this[long index] => _values[index];
    }
}
