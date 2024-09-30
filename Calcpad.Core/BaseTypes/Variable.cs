using System;

namespace Calcpad.Core
{
    internal class Variable
    {
        public IValue Value;
        internal ref IValue ValueByRef() => ref Value;
        internal event Action OnChange;
        internal bool IsInitialized => _isIntialised;
        private bool _isIntialised;

        internal Variable(in IValue value)
        {
            Value = value;
            _isIntialised = true;
        }

        internal Variable(in Complex number) : this(new Value(number)) { }
        public Variable() { }
        internal void SetNumber(in Complex number)
        {
            ref var value = ref ValueByRef();
            if (value is Value v)
                Value = new Value(number, v.Units);
            else
                Value = new Value(number);

        }
        internal void SetNumber(double number)
        {
            ref var value = ref ValueByRef();
            if (value is Value v)
                Value = new Value(number, v.Units);
            else
                Value = new Value(number);
        }

        internal void SetUnits(Unit units)
        {
            ref var value = ref ValueByRef();
            if (value is Value v)
                Value = new Value(v.Re, v.Im, units);
            else
                ((Vector)Value).SetUnits(units);

        }
        internal void SetValue(Unit units) => Value = new Value(units);
        internal void SetValue(double number, Unit units)
        {
            Value = new Value(number, units);
            _isIntialised = true;
        }

        internal void SetValue(in IValue value)
        {
            Value = value;
            _isIntialised = true;
        }

        internal void Assign(in IValue value)
        {
            Value = value;
            _isIntialised = true;
            OnChange?.Invoke();
        }
    }
}
