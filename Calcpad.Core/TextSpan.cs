using System;

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

        public void Start(int index)
        {
            _start = index;
            _end = index;
        }

        public void Expand() => ++_end;

        public ReadOnlySpan<char> Cut() => _contents[_start.._end];

        public override string ToString() => _contents[_start.._end].ToString();

        public bool StartsWith(char c) => _contents[_start] == c;

        public bool Equals(string s) => _contents[_start.._end] == s;
    }
}
