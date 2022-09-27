using System;

namespace Calcpad.Core
{
    internal class Variable
    {
        internal delegate void ChangeEvent();
        internal event Action OnChange;
        public Value Value;
        internal bool IsInitialized => _isIntialised;
        protected bool _isIntialised;
        internal Variable(in Value value)
        {
            Value = value;
            _isIntialised = true;
        }
        internal Variable(in Complex number) : this(new Value(number)) { }
        internal Variable() { }
        internal void SetNumber(in Complex number) => Value = new Value(number, Value.Units);
        internal void SetNumber(double number) => Value = new Value(number, Value.Units);
        internal void SetUnits(Unit units)
        {
            Value = new Value(Value.Re, Value.Im, units);
            _isIntialised = true;
        }

        internal void SetValue(Unit units) => Value = new Value(units);

        internal void SetValue(in Value value)
        {
            Value = value;
            _isIntialised = true;
            OnChange?.Invoke();
        }

        internal void SetValue(in Complex number, Unit units)
        {
            Value = new Value(number, units);
            _isIntialised = true;
        }

        internal void SetValue(double number, Unit units)
        {
            Value = new Value(number, units);
            _isIntialised = true;
        }
    }

    internal class Parameter : Variable
    {
        internal string Name;
        internal Parameter(in string name)
        {
            Name = string.Intern(name);
            _isIntialised = true;
        }
    }
}
