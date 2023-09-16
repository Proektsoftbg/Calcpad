using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class CustomFunction
        {
            private readonly struct Tuple : IEquatable<Tuple>
            {
                private readonly Value _x, _y;

                internal Tuple(in Value x, in Value y)
                {
                    _x = x;
                    _y = y;
                }

                public override int GetHashCode() => HashCode.Combine(_x, _y);

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

            private const int MaxCacheSize = 1000;
            internal event Action OnChange;
            internal Token[] Rpn;
            private Dictionary<Value, Value> _cache;
            private Dictionary<Tuple, Value> _cache2;
            private Parameter[] _parameters;
            internal Unit Units;
            internal int ParameterCount { get; private set; }
            internal bool IsRecursion;
            internal Func<Value> Function;

            internal void AddParameters(List<string> parameters)
            {
                ParameterCount = parameters.Count;
                _parameters = new Parameter[ParameterCount];
                _parameters[0] = new(parameters[0]);
                switch (ParameterCount)
                {
                    case 1:
                        _cache = new();
                        return;
                    case 2:
                        _parameters[1] = new(parameters[1]);
                        _cache2 = new();
                        return;
                    default:
                        for (int i = 1; i < ParameterCount; ++i)
                            _parameters[i] = new(parameters[i]);
                        return;
                }
            }

            internal Parameter[] Parameters => _parameters;

            internal string ParameterName(int index) => _parameters[index].Name;

            internal void ClearCache()
            {
                Function = null;
                _cache?.Clear();
                _cache2?.Clear();
            }

            internal void PurgeCache()
            {
                if (_cache?.Count >= MaxCacheSize)
                    _cache.Clear();
                else if (_cache2?.Count >= MaxCacheSize)
                    _cache2.Clear();
            }

            internal void Change()
            {
                ClearCache();
                OnChange?.Invoke();
            }

            internal void SubscribeCache(MathParser parser)
            {
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t is VariableToken vt)
                        vt.Variable.OnChange += Change;
                    else if (t.Type == TokenTypes.Solver)
                        parser._solveBlocks[t.Index].OnChange += Change;
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
                        if (cf.CheckRecursion(f, functions))
                            return true;
                    }
                }
                return false;
            }

            internal Value Calculate(in Value x)
            {
                if (_cache?.Count >= MaxCacheSize)
                    _cache.Clear();

                ref var y = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, x, out bool result);
                if (result)
                    return y;

                _parameters[0].SetValue(x);
                y = Function();
                return y;
            }

            internal Value Calculate(in Value x, in Value y)
            {
                if (_cache2?.Count >= MaxCacheSize)
                    _cache2.Clear();

                Tuple arguments = new(x, y);
                ref var z = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache2, arguments, out bool result);
                if (result)
                    return z;

                _parameters[0].SetValue(x);
                _parameters[1].SetValue(y);
                z = Function();
                return z;
            }

            internal Value Calculate(in Value x, in Value y, in Value z)
            {
                _parameters[0].SetValue(x);
                _parameters[1].SetValue(y);
                _parameters[2].SetValue(z);
                return Function();
            }

            internal Value Calculate(Value[] arguments)
            {
                for (int i = 0, len = arguments.Length; i < len; ++i)
                    _parameters[i].SetValue(arguments[i]);

                return Function();
            }
        }
    }
}