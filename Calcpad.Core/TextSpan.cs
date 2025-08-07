using System;
using System.Linq;

namespace Calcpad.Core
{
    public ref struct TextSpan(ReadOnlySpan<char> contents)
    {
        private int _start = 0;
        private int _end = 0;
        private ReadOnlySpan<char> _contents = contents;

        public readonly bool IsEmpty => _end == _start;
        public readonly int Length => _end - _start;

        public void Reset(int index)
        {
            _start = index;
            _end = index;
        }

        public void Restart(ReadOnlySpan<char> contents)
        {
            _contents = contents;
            _start = 0;
            _end = 0;
        }

        public void Expand() => ++_end;

        public void ExpandBy(int count) => _end += count;
        public void ExpandTo(int index) => _end = index;

        public readonly ReadOnlySpan<char> Cut() => _contents[_start.._end];

        public readonly override string ToString() => _contents[_start.._end].ToString();

        public readonly bool StartsWith(char c) => _contents[_start] == c;

        public readonly bool StartsWithAny(char[] chars) => chars.Contains(_contents[_start]);

        public readonly bool Equals(ReadOnlySpan<char> s) => _contents[_start.._end].SequenceEqual(s);
    }
}
