using System;

namespace Calcpad.OpenXml
{
    public ref struct SplitEnumerator(ReadOnlySpan<char> span, char delimiter)
    {
        private ReadOnlySpan<char> _span = span;
        private readonly char _delimiter = delimiter;

        public readonly SplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_span.IsEmpty)
                return false;

            var i = _span.IndexOf(_delimiter);
            if (i < 0)
            {
                Current = _span;
                _span = [];
                return true;
            }
            Current = _span[..i];
            _span = _span[(i + 1)..];
            return true;
        }
        public ReadOnlySpan<char> Current { get; private set; } = default;
    }
}