using System;
using System.Reflection;

namespace Calcpad.Core
{
    public ref struct TextSpan
    {
        private int _start;
        private int _end;
        private ReadOnlySpan<char> _contents;

        public TextSpan(ReadOnlySpan<char> contents) => _contents = contents;
        public bool IsEmpty => _end == _start;
        public int Length => _end - _start;

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

        public ReadOnlySpan<char> Cut() => _contents[_start.._end];

        public override string ToString() => _contents[_start.._end].ToString();

        public bool StartsWith(char c) => _contents[_start] == c;

        public bool Equals(string s) => _contents[_start.._end] == s;
    }
}
