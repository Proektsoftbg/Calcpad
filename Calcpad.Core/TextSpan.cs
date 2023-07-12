using System;
using System.Linq;

namespace Calcpad.Core
{
    public ref struct TextSpan
    {
        private int _start;
        private int _end;
        private ReadOnlySpan<char> _contents;

        public TextSpan(ReadOnlySpan<char> contents)
        {
            _contents = contents;
            _start = 0;
            _end = 0;
        }

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

        public readonly ReadOnlySpan<char> Cut() => _contents[_start.._end];

        public override readonly string ToString() => _contents[_start.._end].ToString();

        public readonly bool StartsWith(char c) => _contents[_start] == c;

        public readonly bool StartsWithAny(char[] chars) => chars.Contains(_contents[_start]);

        public readonly bool Equals(string s) => _contents[_start.._end].SequenceEqual(s.AsSpan());
    }
}
