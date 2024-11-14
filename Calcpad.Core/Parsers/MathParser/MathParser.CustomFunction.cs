using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private abstract class CustomFunction
        {
            protected const string InvalidNumberOfParameters = "Invalid number of parameters.";
            protected const int MaxCacheSize = 1000;
            internal event Action OnChange;
            internal Token[] Rpn;
            internal Unit Units;
            internal int ParameterCount { get; set; }
            internal bool IsRecursion;
            internal Func<IValue> Function;

            internal abstract void AddParameters(List<string> parameters);
            internal abstract void ClearCache();
            internal abstract void PurgeCache();
            internal abstract ReadOnlySpan<Parameter> Parameters { get; }
            internal abstract string ParameterName(int index);

            internal void SubscribeCache(MathParser parser)
            {
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t is VariableToken vt)
                        vt.Variable.OnChange += Change;
                    else if (t.Type == TokenTypes.Solver)
                        parser._solveBlocks[(int)t.Index].OnChange += Change;
                    else if (t.Type == TokenTypes.CustomFunction)
                        parser._functions[t.Index].OnChange += Change;
                }
            }

            internal bool CheckRecursion(CustomFunction f, Container<CustomFunction> functions)
            {
                if (ReferenceEquals(f, this))
                    return true;

                f ??= this;
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t.Type == TokenTypes.CustomFunction)
                    {
                        var cf = functions[t.Index];
                        try
                        {
                            if (cf.CheckRecursion(f, functions))
                                return true;
                        }
                        catch
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            internal void Change()
            {
                ClearCache();
                OnChange?.Invoke();
            }
        }

        private sealed class CustomFunction1 : CustomFunction
        {
            private readonly Dictionary<Value, IValue> _cache = [];
            private Parameter _x;

            internal override void AddParameters(List<string> parameters)
            {
                ParameterCount = 1;
                if (parameters.Count != ParameterCount)
                    throw new ArgumentException(InvalidNumberOfParameters);

                _x = new(parameters[0]);
            }

            internal override void ClearCache()
            {
                Function = null;
                _cache?.Clear();
            }

            internal override void PurgeCache()
            {
                if (_cache.Count >= MaxCacheSize)
                    _cache.Clear();
            }

            internal override ReadOnlySpan<Parameter> Parameters => MemoryMarshal.CreateSpan(ref _x, 1);

            internal override string ParameterName(int index)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);

                return _x.Name;
            }

            internal IValue Calculate(in IValue x)
            {
                if (_cache.Count >= MaxCacheSize)
                    _cache.Clear();

                if (x is Value v)
                {
                    ref var y = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, v, out var result);
                    if (!result)
                    {
                        _x.SetValue(x);
                        y = Function();
                    }
                    return y;
                }
                _x.SetValue(x);
                return Function();
            }
        }

        private sealed class CustomFunction2 : CustomFunction
        {
            private readonly struct Tuple : IEquatable<Tuple>
            {
                private readonly Value _x, _y;
                private readonly int _hash;

                internal Tuple(in Value x, in Value y)
                {
                    _x = x;
                    _y = y;
                    _hash = HashCode.Combine(_x, _y);
                }

                public override int GetHashCode() => _hash;

                public override bool Equals(object obj)
                {
                    if (obj is Tuple t)
                        return Equals(t);

                    return false;
                }

                public bool Equals(Tuple other) =>
                    _x.Equals(other._x) &&
                    _y.Equals(other._y);
            }


            private readonly Dictionary<Tuple, IValue> _cache = [];
            private Parameter _x, _y;

            internal override void AddParameters(List<string> parameters)
            {
                ParameterCount = 2;
                if (parameters.Count != ParameterCount)
                    throw new ArgumentException(InvalidNumberOfParameters);

                _x = new(parameters[0]);
                _y = new(parameters[1]);
            }

            internal override ReadOnlySpan<Parameter> Parameters => new Parameter[] { _x, _y };
            internal override string ParameterName(int index) => index switch
            {
                0 => _x.Name,
                1 => _y.Name,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };

            internal override void ClearCache()
            {
                Function = null;
                _cache?.Clear();
            }

            internal override void PurgeCache()
            {
                if (_cache.Count >= MaxCacheSize)
                    _cache.Clear();
            }

            internal IValue Calculate(in IValue x, in IValue y)
            {
                if (_cache.Count >= MaxCacheSize)
                    _cache.Clear();

                if (x is Value vx && y is Value vy)
                {
                    Tuple arguments = new(vx, vy);
                    ref var z = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, arguments, out var result);
                    if (!result)
                    {
                        _x.SetValue(x);
                        _y.SetValue(y);
                        z = Function();
                    }
                    return z;
                }
                _x.SetValue(x);
                _y.SetValue(y);
                return Function();
            }
        }

        private sealed class CustomFunction3 : CustomFunction
        {
            private Parameter _x, _y, _z;

            internal override void AddParameters(List<string> parameters)
            {
                ParameterCount = 3;
                if (parameters.Count != ParameterCount)
                    throw new ArgumentException(InvalidNumberOfParameters);

                _x = new(parameters[0]);
                _y = new(parameters[1]);
                _z = new(parameters[2]);
            }

            internal override ReadOnlySpan<Parameter> Parameters => new Parameter[] { _x, _y, _z };

            internal override string ParameterName(int index) => index switch
            {
                0 => _x.Name,
                1 => _y.Name,
                2 => _z.Name,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };

            internal override void ClearCache() { Function = null; }

            internal override void PurgeCache() { }

            internal IValue Calculate(in IValue x, in IValue y, in IValue z)
            {
                _x.SetValue(x);
                _y.SetValue(y);
                _z.SetValue(z);
                return Function();
            }
        }

        private sealed class CustomFunctionN : CustomFunction
        {
            private Parameter[] _parameters;

            internal override void AddParameters(List<string> parameters)
            {
                ParameterCount = parameters.Count;
                _parameters = new Parameter[ParameterCount];
                for (int i = 0; i < ParameterCount; ++i)
                    _parameters[i] = new(parameters[i]);
                return;
            }

            internal override ReadOnlySpan<Parameter> Parameters => _parameters;

            internal override string ParameterName(int index) => _parameters[index].Name;

            internal override void ClearCache() { Function = null; }

            internal override void PurgeCache() { }

            internal IValue Calculate(IValue[] arguments)
            {
                for (int i = 0, len = arguments.Length; i < len; ++i)
                    _parameters[i].SetValue(arguments[i]);

                return Function();
            }
        }
    }
}