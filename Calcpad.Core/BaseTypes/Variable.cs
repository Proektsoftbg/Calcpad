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

        internal Variable(in Complex number) : this(new ComplexValue(number)) { }
        public Variable() { }
        internal void SetNumber(in Complex number)
        {
            ref var ival = ref ValueByRef();
            if (ival is RealValue real)
                Value = new ComplexValue(number, real.Units);
            else if (ival is ComplexValue complex)
                Value = new ComplexValue(number, complex.Units);
            else
                Value = new ComplexValue(number);

        }
        internal void SetNumber(double number)
        {
            ref var ival = ref ValueByRef();
            if (ival is RealValue real)
                Value = new RealValue(number, real.Units);
            else if (ival is ComplexValue complex)
                Value = new RealValue(number, complex.Units);
            else
                Value = new ComplexValue(number);
        }

        internal void SetUnits(Unit units)
        {
            ref var value = ref ValueByRef();
            if (value is RealValue real)
                Value = new RealValue(real.D, units);
            else if (value is ComplexValue complex)
                Value = new ComplexValue(complex.A, complex.B, units);
            else
                ((Vector)Value).SetUnits(units);

        }
        internal void SetValue(Unit units) => Value = new RealValue(units);
        internal void SetValue(double number, Unit units)
        {
            Value = new RealValue(number, units);
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
