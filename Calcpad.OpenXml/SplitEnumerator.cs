using System;

namespace Calcpad.OpenXml
{
    public ref struct SplitEnumerator
    {
        private ReadOnlySpan<char> _span;
        private readonly char _delimiter;

        public SplitEnumerator(ReadOnlySpan<char> span, char delimiter)
        {
            _span = span;
            _delimiter = delimiter;
            Current = default;
        }
        public readonly SplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_span.IsEmpty)
                return false;

            var i = _span.IndexOf(_delimiter);
            if (i < 0)
            {
                Current = _span;
                _span = ReadOnlySpan<char>.Empty;
                return true;
            }
            Current = _span[..i];
            _span = _span[(i + 1)..];
            return true;
        }
        public ReadOnlySpan<char> Current { get; private set; }
        //public readonly bool IsEmpty => _span.IsEmpty;
    }
}