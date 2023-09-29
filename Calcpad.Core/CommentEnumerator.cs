using System;

namespace Calcpad.Core
{
    public ref struct CommentEnumerator
    {
        private ReadOnlySpan<char> _span;
        private char _commentChar = '\0';

        public CommentEnumerator(ReadOnlySpan<char> span)
        {
            _span = span;
            Current = default;
        }
        public readonly CommentEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_span.IsEmpty)
                return false;

            int i;
            var isComment = _commentChar != '\0';
            if (isComment)
            {
                i = _span[1..].IndexOf(_commentChar) + 2;
                if (i == 1)
                    i = -1;

                _commentChar = '\0';
            }
            else
            {
                i = _span.IndexOfAny('\'', '"');
                _commentChar = i > -1 ? _span[i] : '\0';
            }
            var j = _span.IndexOf('\n');
            if (j > -1 && j < i)
                i = j;

            if (i < 0)
            {
                Current = _span;
                _span = ReadOnlySpan<char>.Empty;
                return true;
            }
            if (i == 0)
                Current = default;
            else
            {
                if (i == j)
                    ++i;

                Current = _span[..i];
                _span = _span[i..];
            }
            return true;
        }
        public ReadOnlySpan<char> Current { get; private set; }
        //public readonly bool IsEmpty => _span.IsEmpty;
    }
}