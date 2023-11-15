using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Wpf
{
    internal class UndoManager
    {
        private readonly struct Step
        {
            internal string Text { get; }
            internal int Line { get; }
            internal int Offset { get; }
            internal Step(string text, int line, int offset)
            {
                Text = text;
                Line = line;
                Offset = offset;
            }
        }
        private readonly List<Step> _undoStack = [];
        private readonly List<Step> _redoStack = [];
        internal string RestoreText { get; private set; }
        internal int RestoreLine { get; private set; }
        internal int RestoreOffset { get; private set; }
        internal int UndoLimit;
        internal UndoManager() { UndoLimit = 20; }
        internal void Reset() { _undoStack.Clear(); }
        internal void Record(string text, int line, int offset)
        {
            Add(_undoStack, new Step(text, line, offset));
            _redoStack.Clear();
        }

        internal bool Undo()
        {
            if (_undoStack.Count > 1)
            {
                var last = _undoStack.Last();
                Add(_redoStack, last);
                RestoreLine = last.Line;
                RestoreOffset = last.Offset;
                _undoStack.RemoveAt(_undoStack.Count - 1);
                last = _undoStack.Last();
                RestoreText = last.Text;
                return true;
            }
            return false;
        }

        internal bool Redo()
        {
            if (_redoStack.Count > 0)
            {
                var last = _redoStack.Last();
                RestoreText = last.Text;
                RestoreLine = last.Line;
                RestoreOffset = last.Offset;
                Add(_undoStack, last);
                _redoStack.RemoveAt(_redoStack.Count - 1);
                return true;
            }
            return false;
        }

        private void Add(List<Step> toList, Step value)
        {
            toList.Add(value);
            if (toList.Count > UndoLimit)
                toList.RemoveAt(0);
        }
    }
}
