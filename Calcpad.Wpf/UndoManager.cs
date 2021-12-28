using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Wpf
{
    internal class UndoManager
    {
        private readonly struct Step
        {
            internal string Text { get; }
            internal int Pointer { get; }

            internal string[] Values { get; }
            internal Step(string text, int pointer, string[] values)
            {
                Text = text;
                Pointer = pointer;
                Values = values;
            }
        }
        private readonly List<Step> _undoStack = new();
        private readonly List<Step> _redoStack = new();

        internal string RestoreText { get; private set; }
        internal int RestorePointer { get; private set; }
        internal string[] RestoreValues { get; private set; }

        internal int UndoLimit;
        internal UndoManager() { UndoLimit = 20; }
        internal void Reset() { _undoStack.Clear(); }
        internal void Record(string text, int pointer, string[] values)
        {
            Add(_undoStack, new Step(text, pointer, values));
            _redoStack.Clear();
        }

        internal bool Undo()
        {
            if (_undoStack.Count > 1)
            {
                var last = _undoStack.Last();
                Add(_redoStack, last);
                RestorePointer = last.Pointer;
                _undoStack.RemoveAt(_undoStack.Count - 1);
                last = _undoStack.Last();
                RestoreText = last.Text;
                RestoreValues = last.Values;
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
                RestorePointer = last.Pointer;
                RestoreValues = last.Values;
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
