using System;
using System.Collections.Generic;

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
                        return _x.Equals(t._x) && _y.Equals(t._y);

                    return false;
                }
                public bool Equals(Tuple other) => _x.Equals(other._x) && _y.Equals(other._y);
            }

            private const int MaxCacheSize = 100;
            internal delegate void ChangeEvent();
            internal event ChangeEvent OnChange;
            internal Token[] Rpn;
            private Dictionary<Value, Value> _cache;
            private Dictionary<Tuple, Value> _cache2;
            private Parameter[] _parameters;
            internal Unit Units;
            internal int ParameterCount { get; private set; }
            internal bool IsRecursion { get; private set; }

            internal Func<Value> Function;

            internal void AddParameters(List<string> parameters)
            {
                ParameterCount = parameters.Count;
                _parameters = new Parameter[ParameterCount];
                for (int i = 0; i < ParameterCount; ++i)
                    _parameters[i] = new Parameter(parameters[i]);

                switch (ParameterCount)
                {
                    case 1:
                        _cache = new();
                        break;
                    case 2:
                        _cache2 = new();
                        break;
                }
            }

            internal Parameter[] Parameters => _parameters;

            internal string ParameterName(int index) => _parameters[index].Name;

            private void ClearCache()
            {
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

            internal void BeforeChange()
            {
                ClearCache();
                OnChange?.Invoke();
            }

            internal void SubscribeCache(Container<CustomFunction> functions)
            {
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t is VariableToken vt)
                        vt.Variable.OnChange += ClearCache;
                    else if (t.Type == TokenTypes.CustomFunction)
                    {
                        var index = functions.IndexOf(t.Content);
                        if (index >= 0)
                            functions[index].OnChange += ClearCache;
                    }
                }
            }

            internal bool CheckReqursion(CustomFunction f, Container<CustomFunction> functions)
            {
                if (ReferenceEquals(f, this))
                {
                    IsRecursion = true;
                    return true;
                }
                IsRecursion = false;
                f ??= this;
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t.Type == TokenTypes.CustomFunction)
                    {
                        var cf = functions[functions.IndexOf(t.Content)];
                        if (cf.CheckReqursion(f, functions))
                        {
                            IsRecursion = true;
                            break;
                        }
                    }
                }
                return IsRecursion;
            }

            internal Value Calculate(Value[] parameters)
            {
                var len = parameters.Length;
                if (len == 1)
                {
                    ref var v = ref parameters[0];
                    if (!_cache.TryGetValue(v, out var z))
                    {
                        _parameters[0].SetValue(parameters[0]);
                        z = Function();
                        _cache.Add(v, z);
                    }
                    return z;
                }
                if (len == 2)
                {
                    Tuple args = new(parameters[0], parameters[1]);
                    if (!_cache2.TryGetValue(args, out var z))
                    {
                        _parameters[0].SetValue(parameters[0]);
                        _parameters[1].SetValue(parameters[1]);
                        z = Function();
                        _cache2.Add(args, z);
                    }
                    return z;
                }
                for (int i = 0; i < len; ++i)
                    _parameters[i].SetValue(parameters[i]);

                return Function();
            }
        }
    }
}