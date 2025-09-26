using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    internal class MatrixCalculator
    {
        internal delegate IValue MatrixFunction(in IValue a);
        internal delegate IValue MatrixFunction2(in IValue a, in IValue b);
        internal delegate IValue MatrixFunction3(Matrix A, in IValue b, in IValue c);
        internal delegate IValue MatrixFunction4(Matrix A, in IValue b, in IValue c, in IValue d);
        internal delegate IValue MatrixFunction5(Matrix A, in IValue b, in IValue c, in IValue d, in IValue e);

        private readonly VectorCalculator _vectorCalc;
        private readonly Calculator _calc;
        private readonly MatrixFunction[] MatrixFunctions;
        private readonly MatrixFunction2[] MatrixFunctions2;
        private readonly MatrixFunction3[] MatrixFunctions3;
        private readonly MatrixFunction4[] MatrixFunctions4;
        private readonly MatrixFunction5[] MatrixFunctions5;
        private readonly Func<IValue[], Matrix>[] MatrixMultiFunctions;
        private readonly Func<Matrix, RealValue>[] MultiFunctions;
        private readonly Func<RealValue, RealValue, Matrix, RealValue>[] Interpolations;
        private Vector _indexes;
        internal MathParser Parser;
       
        internal static readonly FrozenDictionary<string, int> FunctionIndex =
        new Dictionary<string, int>()
        {
            { "identity", 0 },
            { "utriang", 1 },
            { "ltriang", 2 },
            { "symmetric", 3 },
            { "vec2diag", 4 },
            { "diag2vec", 5 },
            { "vec2row", 6 },
            { "vec2col", 7 },
            { "n_rows", 8 },
            { "n_cols", 9 },
            { "mnorm_1", 10 },
            { "mnorm_2", 11 },
            { "mnorm", 11 },
            { "mnorm_e", 12 },
            { "mnorm_i", 13 },
            { "cond_1", 14 },
            { "cond_2", 15 },
            { "cond", 15 },
            { "cond_e", 16 },
            { "cond_i", 17 },
            { "det", 18 },
            { "rank", 19 },
            { "transp", 20 },
            { "trace", 21 },
            { "inverse", 22 },
            { "adj", 23 },
            { "cofactor", 24 },
            { "lu", 25 },
            { "qr", 26 },
            { "svd", 27 },
            { "cholesky", 28 },
            { "identity_hp", 29 },
            { "utriang_hp", 30 },
            { "ltriang_hp", 31 },
            { "symmetric_hp", 32 },
            { "hp", 33 },
            { "ishp", 34 },
            { "getunits", 35 },
            { "clrunits", 36 },
            { "fft", 37 },
            { "ift", 38 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function2Index =
        new Dictionary<string, int>()
        {
            { "matrix", 0 },
            { "diagonal", 1 },
            { "column", 2 },
            { "row", 3 },
            { "col", 4 },
            { "extract_rows", 5 },
            { "extract_cols", 6 },
            { "mfill", 7 },
            { "sort_cols", 8 },
            { "rsort_cols", 9 },
            { "sort_rows", 10 },
            { "rsort_rows", 11 },
            { "order_cols", 12 },
            { "revorder_cols", 13 },
            { "order_rows", 14 },
            { "revorder_rows", 15 },
            { "mcount", 16 },
            { "mfind", 17 },
            { "mfind_eq", 18 },
            { "mfind_ne", 19 },
            { "mfind_lt", 20 },
            { "mfind_le", 21 },
            { "mfind_gt", 22 },
            { "mfind_ge", 23 },
            { "lsolve", 24 },
            { "clsolve", 25 },
            { "slsolve", 26 },
            { "msolve", 27 },
            { "cmsolve", 28 },
            { "smsolve", 29 },
            { "hprod", 30 },
            { "fprod", 31 },
            { "kprod", 32 },
            { "eigenvals", 33 },
            { "eigenvecs", 34 },
            { "eigen", 35 },
            { "matrix_hp", 36 },
            { "diagonal_hp", 37 },
            { "column_hp", 38 },
            { "setunits", 39 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<int> FunctionsWithOptionalLastParameter =
            new HashSet<int>() { 33, 34, 35 }.ToFrozenSet();

        internal static bool IsLastParameterOptional(int i) => 
            FunctionsWithOptionalLastParameter.Contains(i);

        internal static readonly FrozenDictionary<string, int> Function3Index =
        new Dictionary<string, int>()
        {
            { "fill_row", 0 },
            { "fill_col", 1 },
            { "mresize", 2 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function4Index =
        new Dictionary<string, int>()
        {
            { "msearch", 0 },
            { "hlookup", 1 },
            { "hlookup_eq", 2 },
            { "hlookup_ne", 3 },
            { "hlookup_lt", 4 },
            { "hlookup_le", 5 },
            { "hlookup_gt", 6 },
            { "hlookup_ge", 7 },
            { "vlookup", 8 },
            { "vlookup_eq", 9 },
            { "vlookup_ne", 10 },
            { "vlookup_lt", 11 },
            { "vlookup_le", 12 },
            { "vlookup_gt", 13 },
            { "vlookup_ge", 14 },
            { "copy", 15 },
            { "add", 16 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function5Index =
        new Dictionary<string, int>()
        {
            { "submatrix", 0 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> MultiFunctionIndex =
        new Dictionary<string, int>()
        {
            { "join_cols", 0 },
            { "join_rows", 1 },
            { "augment", 2 },
            { "stack", 3 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal MatrixCalculator(VectorCalculator vectorCalc)
        {
            _vectorCalc = vectorCalc;
            _calc = _vectorCalc.Calculator;

            MatrixFunctions = [
                Identity,               //0
                UpperTriangular,        //1
                LowerTriangular,        //2
                Symmetric,              //3
                Vec2Diag,               //4
                Diag2Vec,               //5
                Vec2Row,                //6
                Vec2Col,                //7
                NRows,                  //8
                NCols,                  //9
                L1Norm,                 //10
                L2Norm,                 //11
                FrobNorm,               //12
                InfNorm,                //13
                Cond1,                  //14
                Cond2,                  //15
                CondFrob,               //16
                CondInf,                //17
                Det,                    //18
                Rank,                   //19
                Transpose,              //20
                Trace,                  //21
                Invert,                 //22
                Adjoint,                //23
                Cofactor,               //24
                LUDecomposition,        //25
                QRDecomposition,        //26
                SVDDecomposition,       //27
                CholeskyDecomposition,  //28
                IdentityHp,             //29
                UpperTriangularHp,      //30
                LowerTriangularHp,      //31
                SymmetricHp,            //32
                Hp,                     //33
                IsHp,                   //34
                GetUnits,               //35
                ClrUnits,               //36
                Fft,                    //37
                Ift,                   //38
            ];

            MatrixFunctions2 = [
                Create,                 //0
                Diagonal,               //1
                Column,                 //2
                Row,                    //3
                Col,                    //4
                ExtractRows,            //5
                ExtractCols,            //6
                Fill,                   //7
                SortCols,               //8
                RsortCols,              //9
                SortRows,               //10
                RsortRows,              //11
                OrderCols,              //12
                RevOrderCols,           //13
                OrderRows,              //14
                RevOrderRows,           //15
                Count,                  //16
                FindEq,                 //17
                FindEq,                 //18
                FindNe,                 //19
                FindLt,                 //20
                FindLe,                 //21
                FindGt,                 //22
                FindGe,                 //23
                LSolve,                 //24
                ClSolve,                //25
                SlSolve,                //26
                MSolve,                 //27
                CmSolve,                //28
                SmSolve,                //29
                Hadamard,               //30
                Frobenius,              //31
                Kronecker,              //32
                EigenValues,            //33
                EigenVectors,           //34
                Eigen,                  //35
                CreateHp,               //36
                DiagonalHp,             //37
                ColumnHp,               //38       
                SetUnits,               //3
            ];

            MatrixFunctions3 = [
                FillRow,
                FillCol,
                Resize,
            ];

            MatrixFunctions4 = [
                Search,               //0
                HLookupEq,            //1
                HLookupEq,            //2
                HLookupNe,            //3
                HLookupLt,            //4
                HLookupLe,            //5
                HLookupGt,            //6
                HLookupGe,            //7
                VLookupEq,            //8
                VLookupEq,            //9
                VLookupNe,            //10
                VLookupLt,            //11
                VLookupLe,            //12
                VLookupGt,            //13
                VLookupGe,            //14
                Copy,                 //15
                Add,                  //16
            ];

            MatrixFunctions5 = [
                Submatrix
            ];

            MatrixMultiFunctions = [
                JoinCols,
                JoinRows,
                Augment,
                Stack,
            ];

            MultiFunctions = [
                Min,
                Max,
                Sum,
                SumSq,
                Srss,
                Average,
                Product,
                Mean,
                Switch,
                And,
                Or,
                Xor,
                Gcd,
                Lcm,
            ];

            Interpolations = [
                Take,
                Line,
                Spline,
            ];
        }
        internal Calculator Calculator => _calc;
        internal VectorCalculator VectorCalculator => _vectorCalc;
        internal Vector Indexes => _indexes;
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsFunction3(string name) => Function3Index.ContainsKey(name);
        internal static bool IsFunction4(string name) => Function4Index.ContainsKey(name);
        internal static bool IsFunction5(string name) => Function5Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);
        internal IValue EvaluateMatrixFunction(long index, in IValue a) =>
            MatrixFunctions[index](a);

        internal IValue EvaluateMatrixFunction2(long index, in IValue a, in IValue b) =>
            MatrixFunctions2[index](a, b);

        internal IValue EvaluateMatrixFunction3(long index, in IValue a, in IValue b, in IValue c) =>
            MatrixFunctions3[index](IValue.AsMatrix(a), b, c);

        internal IValue EvaluateMatrixFunction4(long index, in IValue a, in IValue b, in IValue c, in IValue d) =>
            MatrixFunctions4[index](IValue.AsMatrix(a), b, c, d);

        internal IValue EvaluateMatrixFunction5(long index, in IValue a, in IValue b, in IValue c, in IValue d, in IValue e) =>
            MatrixFunctions5[index](IValue.AsMatrix(a), b, c, d, e);

        internal IValue EvaluateMatrixMultiFunction(long index, IValue[] a) =>
            MatrixMultiFunctions[index](a);

        internal MatrixFunction GetFunction(long index) =>
            MatrixFunctions[index];

        internal MatrixFunction2 GetFunction2(long index) =>
            MatrixFunctions2[index];

        internal MatrixFunction3 GetFunction3(long index) =>
            MatrixFunctions3[index];

        internal MatrixFunction4 GetFunction4(long index) =>
            MatrixFunctions4[index];

        internal MatrixFunction5 GetFunction5(long index) =>
            MatrixFunctions5[index];

        internal Func<Vector[], Matrix> GetMultiFunction(long index) =>
            MatrixMultiFunctions[index];

        internal Matrix EvaluateOperator(long index, Matrix a, in RealValue b) =>
            a is HpMatrix hp_m ?
            HpMatrix.EvaluateOperator(_calc.GetOperator(index), hp_m, b, index) :
            Matrix.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Matrix EvaluateOperator(long index, in RealValue a, Matrix b) =>
            b is HpMatrix hp_m ?
            HpMatrix.EvaluateOperator(_calc.GetOperator(index), a, hp_m, index) :
            Matrix.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Matrix EvaluateOperator(long index, Matrix a, Matrix b) =>
            a is HpMatrix hp_a && b is HpMatrix hp_b &&
            (index != Calculator.PowerIndex || hp_a.Units == null) ?
            HpMatrix.EvaluateOperator(_calc.GetOperator(index), hp_a, hp_b, index) :
            Matrix.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Matrix EvaluateFunction(long Index, Matrix a) =>
            a is HpMatrix hp_m ?
            HpMatrix.EvaluateFunction(_calc.GetFunction(Index), hp_m) :
            Matrix.EvaluateFunction(_calc.GetFunction(Index), a);

        internal Matrix EvaluateFunction2(long index, Matrix a, Matrix b) =>
            a is HpMatrix hp_a && b is HpMatrix hp_b ?
            HpMatrix.EvaluateOperator(_calc.GetFunction2(index), hp_a, hp_b, -index - 1) :
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal Matrix EvaluateFunction2(long index, Matrix a, in RealValue b) =>
            a is HpMatrix hp_m ?
            HpMatrix.EvaluateOperator(_calc.GetFunction2(index), hp_m, b, -index - 1) :
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal Matrix EvaluateFunction2(long index, in RealValue a, Matrix b) =>
            b is HpMatrix hp_m ?
            HpMatrix.EvaluateOperator(_calc.GetFunction2(index), a, hp_m, -index - 1) :
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal IValue EvaluateMultiFunction(long index, Matrix a) =>
            MultiFunctions[index](a);

        internal IValue EvaluateInterpolation(long index, RealValue a, RealValue b, Matrix c) =>
            Interpolations[index](a, b, c);

        private static DiagonalMatrix Identity(in IValue n) => new(IValue.AsInt(n), RealValue.One);
        private static UpperTriangularMatrix UpperTriangular(in IValue n) => new(IValue.AsInt(n));
        private static LowerTriangularMatrix LowerTriangular(in IValue n) => new(IValue.AsInt(n));
        private static SymmetricMatrix Symmetric(in IValue n) => new(IValue.AsInt(n));


        private static Matrix Vec2Diag(in IValue v)
        {
            var vec = IValue.AsVector(v);
            if (vec is HpVector hp_vec)
                return new HpDiagonalMatrix(hp_vec);

            return new DiagonalMatrix(vec);
        }

        private static Vector Diag2Vec(in IValue M)
        {
             var matrix = IValue.AsMatrix(M);
             if (matrix is HpMatrix hp_m)
                return hp_m.Diagonal();

            return matrix.Diagonal();
        }

        private static Matrix Vec2Row(in IValue v)
        {
            var vec = IValue.AsVector(v);
            if (vec is HpVector hp_vec)
                return new HpMatrix(hp_vec);

            return new Matrix(vec);
        }


        private static Matrix Vec2Col(in IValue v)
        {
            var vec = IValue.AsVector(v);
            if (vec is HpVector hp_vec)
                return new HpColumnMatrix(hp_vec);

            return new ColumnMatrix(vec);
        }

        private static HpDiagonalMatrix IdentityHp(in IValue n) => new(IValue.AsInt(n), RealValue.One);
        private static HpUpperTriangularMatrix UpperTriangularHp(in IValue n) => new(IValue.AsInt(n), null);
        private static HpLowerTriangularMatrix LowerTriangularHp(in IValue n) => new(IValue.AsInt(n), null);
        private static HpSymmetricMatrix SymmetricHp(in IValue n) => new(IValue.AsInt(n), null);

        private static IValue Hp(in IValue v) =>
            v switch
            {
                HpVector or HpMatrix => v,
                RealValue rv => new HpVector([rv.D], rv.Units) as Vector,
                Vector vec => new HpVector(vec, null) as Vector,
                ColumnMatrix cm => new HpColumnMatrix(cm),
                DiagonalMatrix dm => new HpDiagonalMatrix(dm),
                LowerTriangularMatrix ltm => new HpLowerTriangularMatrix(ltm),
                UpperTriangularMatrix utm => new HpUpperTriangularMatrix(utm),
                SymmetricMatrix sm => new HpSymmetricMatrix(sm),
                Matrix matrix => new HpMatrix(matrix), 
                _ => v
            };

        private static IValue IsHp(in IValue v) =>
            v is HpVector || v is HpMatrix ? RealValue.One : RealValue.Zero;

        private static IValue GetUnits(in IValue v)
        {
            var units = v switch
            {
                RealValue rv => rv.Units,
                ComplexValue cv => cv.Units,
                HpVector hp_vec => hp_vec.Units,
                HpMatrix hp_matrix => hp_matrix.Units,
                Vector vec => vec.Length > 0 ? vec[1].Units : null,
                Matrix matrix => matrix.RowCount > 0 && matrix.ColCount > 0 ? matrix[1, 1].Units : null,
                _ => null
            };
            return new RealValue(units);
        }

        private static IValue SetUnits(in IValue v, in IValue unitsValue)
        {
            if (unitsValue is not RealValue rvu || rvu.D != 1d || !rvu.IsUnit )
                throw Exceptions.InvalidUnits(unitsValue.ToString());

            var units = rvu.Units ?? throw Exceptions.InvalidUnits(unitsValue.ToString());

            switch (v)
            {
                case RealValue rv:
                    return new RealValue(rv.D, units);
                case ComplexValue cv:
                    return new ComplexValue(cv.Complex, units);
                case HpVector hp_vec:
                    hp_vec.Units = units;
                    return hp_vec;
                case HpMatrix hp_matrix:
                    hp_matrix.Units = units;
                    return hp_matrix;
                case Vector vec:
                    vec.SetUnits(units);
                    return vec;
                case Matrix matrix:
                    matrix.SetUnits(units);
                    return matrix;
                default:
                    return v;
            }
        }

        private static IValue ClrUnits(in IValue v)
        {
            switch (v)
            {
                case RealValue rv:
                    return new RealValue(rv.D, null);
                case ComplexValue cv:
                    return new ComplexValue(cv.Complex, null);
                case HpVector hp_vec:
                    hp_vec.Units = null;
                    return hp_vec;
                case HpMatrix hp_matrix:
                    hp_matrix.Units = null;
                    return hp_matrix;
                case Vector vec:
                    vec.SetUnits(null);
                    return vec;
                case Matrix matrix:
                    matrix.SetUnits(null);
                    return matrix;
                default:
                    return v;
            }
        }

        private static IValue NRows(in IValue M) => new RealValue(IValue.AsMatrix(M).RowCount);
        private static IValue NCols(in IValue M) => new RealValue(IValue.AsMatrix(M).ColCount);
        private static IValue FrobNorm(in IValue M) => IValue.AsMatrix(M).FrobNorm();
        private static IValue L2Norm(in IValue M) => IValue.AsMatrix(M).L2Norm();
        private static IValue L1Norm(in IValue M) => IValue.AsMatrix(M).L1Norm();
        private static IValue InfNorm(in IValue M) => IValue.AsMatrix(M).InfNorm();
        private static IValue Cond1(in IValue M) => IValue.AsMatrix(M).Cond1();
        private static IValue Cond2(in IValue M) => IValue.AsMatrix(M).Cond2();
        private static IValue CondFrob(in IValue M) => IValue.AsMatrix(M).CondFrob();
        private static IValue CondInf(in IValue M) => IValue.AsMatrix(M).CondInf();
        private static IValue Det(in IValue M) => IValue.AsMatrix(M).Determinant();
        private static IValue Rank(in IValue M) => IValue.AsMatrix(M).Rank();
        private static Matrix Transpose(in IValue M)
        {
            var m = IValue.AsMatrix(M);
            if (m is HpMatrix hp_m)
                return hp_m.Transpose();

            return m.Transpose();
        }
        private static IValue Trace(in IValue M) => IValue.AsMatrix(M).Trace();
        private static Matrix Invert(in IValue M) => IValue.AsMatrix(M).Invert();
        private static Matrix Adjoint(in IValue M) => IValue.AsMatrix(M).Adjoint();
        private static Matrix Cofactor(in IValue M) => IValue.AsMatrix(M).Cofactor();
        private Vector EigenValues(in IValue M, in IValue count)
        {
            var matrix = IValue.AsMatrix(M);
            var n = (int)IValue.AsReal(count).D;
            return matrix switch
            {
                HpSymmetricMatrix hp_sm => hp_sm.EigenValues(n, Parser.Tol),
                HpDiagonalMatrix hp_dm => hp_dm.EigenValues(n),
                SymmetricMatrix sm => sm.EigenValues(n),
                DiagonalMatrix dm => dm.EigenValues(n),
                _ => throw Exceptions.MatrixMustBeSymmetric()
            };
        }

        private Matrix EigenVectors(in IValue M, in IValue count)
        {
            var matrix = IValue.AsMatrix(M);
            var n = (int)IValue.AsReal(count).D;
            return matrix switch
            {
                HpSymmetricMatrix hp_sm => hp_sm.EigenVectors(n, Parser.Tol),
                HpDiagonalMatrix hp_dm => hp_dm.EigenVectors(n),
                SymmetricMatrix sm => sm.EigenVectors(n),
                DiagonalMatrix dm => dm.EigenVectors(n),
                _ => throw Exceptions.MatrixMustBeSymmetric()
            };
        }

        private Matrix Eigen(in IValue M, in IValue count)
        {
            var matrix = IValue.AsMatrix(M);
            var n = (int)IValue.AsReal(count).D;
            return matrix switch
            {
                HpSymmetricMatrix hp_sm => hp_sm.Eigen(n, Parser.Tol),
                HpDiagonalMatrix hp_dm => hp_dm.Eigen(n),
                SymmetricMatrix sm => sm.Eigen(n),
                DiagonalMatrix dm => dm.Eigen(n),
                _ => throw Exceptions.MatrixMustBeSymmetric()
            };
        }

        private Matrix LUDecomposition(in IValue M)
        {
            var LU = IValue.AsMatrix(M).LUDecomposition(out var indexes);
            _indexes = Vector.FromIndexes(indexes);

            return LU;
        }
        private static Matrix QRDecomposition(in IValue M) => IValue.AsMatrix(M).QRDecomposition();
        private static Matrix SVDDecomposition(in IValue M) => IValue.AsMatrix(M).SVDDecomposition();
        private static Matrix CholeskyDecomposition(in IValue M)
        {
            var matrix = IValue.AsMatrix(M);
            return matrix switch
            {
                HpSymmetricMatrix hp_sm => hp_sm.CholeskyDecomposition(),
                HpDiagonalMatrix hp_dm => hp_dm.CholeskyDecomposition(),
                SymmetricMatrix sm => sm.CholeskyDecomposition(),
                DiagonalMatrix dm => dm.CholeskyDecomposition(),
                _ => throw Exceptions.MatrixMustBeSymmetric()
            };
        }
        private static Matrix Create(in IValue m, in IValue n) => new(IValue.AsInt(m), IValue.AsInt(n));
        private static DiagonalMatrix Diagonal(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsReal(value));
        private static ColumnMatrix Column(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsReal(value));
        private static HpMatrix CreateHp(in IValue m, in IValue n) => new(IValue.AsInt(m), IValue.AsInt(n), null);
        private static HpDiagonalMatrix DiagonalHp(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsReal(value));
        private static HpColumnMatrix ColumnHp(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsReal(value));
        private static Vector Row(in IValue M, in IValue row) => IValue.AsMatrix(M).Row(IValue.AsInt(row));
        private static Vector Col(in IValue M, in IValue col) => IValue.AsMatrix(M).Col(IValue.AsInt(col));
        private static Matrix ExtractRows(in IValue M, in IValue rows) => IValue.AsMatrix(M).ExtractRows(IValue.AsVector(rows));
        private static Matrix ExtractCols(in IValue M, in IValue cols) => IValue.AsMatrix(M).ExtractCols(IValue.AsVector(cols));
        private static Matrix Fill(in IValue M, in IValue value)
        {
            var matrix = IValue.AsMatrix(M);
            matrix.Fill(IValue.AsReal(value));
            matrix.Change();
            return matrix;
        }
        private static Matrix SortCols(in IValue M, in IValue row) => IValue.AsMatrix(M).SortCols(IValue.AsInt(row));
        private static Matrix RsortCols(in IValue M, in IValue row) => IValue.AsMatrix(M).SortCols(IValue.AsInt(row), true);
        private static Matrix SortRows(in IValue M, in IValue col) => IValue.AsMatrix(M).SortRows(IValue.AsInt(col));
        private static Matrix RsortRows(in IValue M, in IValue col) => IValue.AsMatrix(M).SortRows(IValue.AsInt(col), true);
        private static Vector OrderCols(in IValue M, in IValue row) => IValue.AsMatrix(M).OrderCols(IValue.AsInt(row));
        private static Vector RevOrderCols(in IValue M, in IValue row) => IValue.AsMatrix(M).OrderCols(IValue.AsInt(row), true);
        private static Vector OrderRows(in IValue M, in IValue col) => IValue.AsMatrix(M).OrderRows(IValue.AsInt(col));
        private static Vector RevOrderRows(in IValue M, in IValue col) => IValue.AsMatrix(M).OrderRows(IValue.AsInt(col), true);
        private static IValue Count(in IValue M, in IValue value) => IValue.AsMatrix(M).Count(IValue.AsReal(value));
        private static Matrix FindEq(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.Equal);
        private static Matrix FindNe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.NotEqual);
        private static Matrix FindLt(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.LessThan);
        private static Matrix FindLe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.LessOrEqual);
        private static Matrix FindGt(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.GreaterThan);
        private static Matrix FindGe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsReal(value), Vector.Relation.GreaterOrEqual);
        private static Vector LSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsVector(B);
            if (a.RowCount != b.Length)
                throw Exceptions.MatrixDimensions();

            if (a is HpMatrix hp_a)
            {
                if (b is HpVector hp_b)
                    return hp_a.LSolve(hp_b);

                throw Exceptions.MustBeHpVector(Exceptions.Items.Argument);
            }
            return a.LSolve(b);
        }

        private static Vector ClSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsVector(B);
            if (a.RowCount != b.Length)
                throw Exceptions.MatrixDimensions();

            if (a is HpSymmetricMatrix hp_sm)
            {
                 if(b is HpVector hp_b)
                    return hp_sm.ClSolve(hp_b);

                throw Exceptions.MustBeHpVector(Exceptions.Items.Argument);
            }
            if (a is SymmetricMatrix sm)
                return sm.ClSolve(b);

            throw Exceptions.MatrixMustBeSymmetric();
        }

        private Vector SlSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrixHp(A);
            var b = IValue.AsVectorHp(B);
            if (a.RowCount != b.Length)
                throw Exceptions.MatrixDimensions();

            if (a is HpSymmetricMatrix sm)
                return sm.SlSolve(b, Parser.Tol);

            throw Exceptions.MatrixMustBeSymmetric();
        }

        private static Matrix MSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a.RowCount != b.RowCount)
                throw Exceptions.MatrixDimensions();

            if (a is HpMatrix hp_a)
            {
                if (b is HpMatrix hp_b)
                    return hp_a.MSolve(hp_b);

                throw Exceptions.MustBeHpMatrix(Exceptions.Items.Argument);
            }
            return a.MSolve(b);
        }

        private static Matrix CmSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a.RowCount != b.RowCount)
                throw Exceptions.MatrixDimensions();

            if (a is HpSymmetricMatrix hp_sm)
            {
                if (b is HpMatrix hp_b)
                    return hp_sm.CmSolve(hp_b);

                throw Exceptions.MustBeHpMatrix(Exceptions.Items.Argument);
            }
            if (a is SymmetricMatrix sm)
                return sm.CmSolve(b);

            throw Exceptions.MatrixMustBeSymmetric();
        }

        private Matrix SmSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrixHp(A);
            var b = IValue.AsMatrixHp(B);
            if (a.RowCount != b.RowCount)
                throw Exceptions.MatrixDimensions();

            if (a is HpSymmetricMatrix sm)
                return sm.SmSolve(b, Parser.Tol);

            throw Exceptions.MatrixMustBeSymmetric();
        }

        private static Matrix Hadamard(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a is HpMatrix hp_a && b is HpMatrix hp_b)
                return HpMatrix.Hadamard(hp_a, hp_b);

            return Matrix.Hadamard(a, b);
        }
        private static IValue Frobenius(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a is HpMatrix hp_a && b is HpMatrix hp_b)
                return HpMatrix.Frobenius(hp_a, hp_b);

            return Matrix.Frobenius(a, b);
        }
        private static Matrix Kronecker(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a is HpMatrix hp_a && b is HpMatrix hp_b)
                return HpMatrix.Kronecker(hp_a, hp_b);

            return Matrix.Kronecker(a, b);
        }

        private static Matrix Fft(in IValue A) => IValue.AsMatrixHp(A).FFT(false);
        private static Matrix Ift(in IValue A) => IValue.AsMatrixHp(A).FFT(true);
        private static Matrix FillRow(Matrix M, in IValue row, in IValue value)
        {
            M.FillRow(IValue.AsInt(row), IValue.AsReal(value));
            M.Change();
            return M;
        }
        private static Matrix FillCol(Matrix M, in IValue col, in IValue value)
        {
            M.FillCol(IValue.AsInt(col), IValue.AsReal(value));
            M.Change();
            return M;
        }
        private static Matrix Resize(Matrix M, in IValue m, in IValue n)
        {
            M.Resize(IValue.AsInt(m), IValue.AsInt(n));
            M.Change();
            return M;
        }
        private static Vector Search(Matrix M, in IValue value, in IValue i, in IValue j) => M.Search(IValue.AsReal(value), IValue.AsInt(i), IValue.AsInt(j));
        private static Vector HLookupEq(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.Equal);
        private static Vector HLookupNe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.NotEqual);
        private static Vector HLookupLt(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.LessThan);
        private static Vector HLookupLe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.LessOrEqual);
        private static Vector HLookupGt(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.GreaterThan);
        private static Vector HLookupGe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsReal(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.GreaterOrEqual);
        private static Vector VLookupEq(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.Equal);
        private static Vector VLookupNe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.NotEqual);
        private static Vector VLookupLt(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.LessThan);
        private static Vector VLookupLe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.LessOrEqual);
        private static Vector VLookupGt(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.GreaterThan);
        private static Vector VLookupGe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsReal(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.GreaterOrEqual);
        private static Matrix Copy(Matrix A, in IValue B, in IValue i, in IValue j)
        {
            var b = IValue.AsMatrix(B);
            var ii = IValue.AsInt(i);
            var jj = IValue.AsInt(j);
            if (A is HpMatrix hp_a && b is HpMatrix hp_b)
                hp_a.CopyTo(hp_b, ii, jj);
            else
                A.CopyTo(b, ii, jj);

            b.Change();
            return b;

        }
        private static Matrix Add(Matrix A, in IValue B, in IValue i, in IValue j)
        {
            var b = IValue.AsMatrix(B);
            var ii = IValue.AsInt(i);
            var jj = IValue.AsInt(j);
            if (A is HpMatrix hp_a && b is HpMatrix hp_b)
                hp_a.AddTo(hp_b, ii, jj);
            else
                A.AddTo(b, ii, jj);

            b.Change();
            return b;
        }

        private static Matrix Submatrix(Matrix M, in IValue i1, in IValue j1, in IValue i2, in IValue j2) => M.Submatrix(IValue.AsInt(i1), IValue.AsInt(j1), IValue.AsInt(i2), IValue.AsInt(j2));
        internal static Matrix JoinCols(IValue[] v)
        {
            var n = v.Length;
            var m = 0;
            var isHp = true;
            for (int i = 0; i < n; ++i)
            {
                if (v[i] is Vector vec)
                {
                    if (vec is not HpVector)
                        isHp = false;

                    if (vec.Length > m)
                        m = vec.Length;
                }
                else
                    throw Exceptions.MustBeVector(Exceptions.Items.Argument);
            }
            if (isHp)
                return HpMatrix.CreateFromCols(v.Cast<HpVector>().ToArray(), m);

            return Matrix.CreateFromCols(v.Cast<Vector>().ToArray(), m);
        }

        internal static Matrix JoinRows(IValue[] v)
        {
            var m = v.Length;
            var n = 0;
            var isHp = true;
            for (int i = 0; i < m; ++i)
            {
                if (v[i] is Vector vec)
                {
                    if (vec is not HpVector)
                        isHp = false;

                    if (vec.Length > n)
                        n = vec.Length;
                }
                else
                    throw Exceptions.MustBeVector(Exceptions.Items.Argument);
            }
            if (isHp)
                return HpMatrix.CreateFromRows(v.Cast<HpVector>().ToArray(), n);

            return Matrix.CreateFromRows(v.Cast<Vector>().ToArray(), n);
        }

        private static bool AreAllHp(IValue[] values)
        {
            for (int i = 0, n = values.Length; i < n; ++i)
            {
                var v = values[i];
                if (!(v is HpVector || v is HpMatrix))
                    return false;
            }
            return true;
        }

        private static Matrix[] CastToMatrices(IValue[] values)
        {
            var n = values.Length;
            var M = new Matrix[n];
            for (int i = 0; i < n; ++i)
                M[i] = IValue.AsMatrix(values[i]);

            return M;
        }

        private static HpMatrix[] CastToHpMatrices(IValue[] values)
        {
            var n = values.Length;
            var M = new HpMatrix[n];
            for (int i = 0; i < n; ++i)
                M[i] = IValue.AsMatrixHp(values[i]);

            return M;
        }

        private static Matrix Augment(IValue[] values) =>
            AreAllHp(values) ?
            HpMatrix.Augment(CastToHpMatrices(values)) :
            Matrix.Augment(CastToMatrices(values));

        private static Matrix Stack(IValue[] values) =>
            AreAllHp(values) ?
            HpMatrix.Stack(CastToHpMatrices(values)) :
            Matrix.Stack(CastToMatrices(values));

        private static RealValue Min(Matrix M) => M.Min();
        private static RealValue Max(Matrix M) => M.Max();
        private static RealValue Sum(Matrix M) => M.Sum();
        private static RealValue SumSq(Matrix M) => M.SumSq();
        private static RealValue Srss(Matrix M) => M.Srss();
        private static RealValue Average(Matrix M) => M.Average();
        private static RealValue Product(Matrix M) => M.Product();
        private static RealValue Mean(Matrix M) => M.Mean();
        private static RealValue Switch(Matrix M) => M[0, 0];
        private static RealValue And(Matrix M) => M.And();
        private static RealValue Or(Matrix M) => M.Or();
        private static RealValue Xor(Matrix M) => M.Xor();
        private static RealValue Gcd(Matrix M) => M.Gcd();
        private static RealValue Lcm(Matrix M) => M.Lcm();
        private static RealValue Take(RealValue x, RealValue y, Matrix m) => m.Take(x, y);
        private static RealValue Line(RealValue x, RealValue y, Matrix m) => m.Line(x, y);
        private static RealValue Spline(RealValue x, RealValue y, Matrix m) => m.Spline(x, y);

        internal static IValue GetElement(IValue matrix, IValue ii, IValue jj)
        {
            var mat = IValue.AsMatrix(matrix, Exceptions.Items.IndexTarget);
            var val = IValue.AsValue(ii, Exceptions.Items.Index);
            var i = (int)val.Re;
            val = IValue.AsValue(jj, Exceptions.Items.Index);
            var j = (int)val.Re;
            if (i < 1 || i > mat.RowCount ||
                j < 1 || j > mat.ColCount)
                throw Exceptions.IndexOutOfRange($"{i}, {j}");

            return mat[i - 1, j - 1];
        }

        internal static IValue SetElement(IValue matrix, IValue ii, IValue jj, IValue value)
        {
            var mat = IValue.AsMatrix(matrix, Exceptions.Items.IndexTarget);
            var val = IValue.AsValue(ii, Exceptions.Items.Index);
            var i = (int)val.Re;
            val = IValue.AsValue(jj, Exceptions.Items.Index);
            var j = (int)val.Re;
            if (i < 1 || i > mat.RowCount ||
                j < 1 || j > mat.ColCount)
                throw Exceptions.IndexOutOfRange($"{i}, {j}");

            var real = IValue.AsReal(value, Exceptions.Items.Value);
            mat[i - 1, j - 1] = real;
            mat.Change();
            return real;
        }
    }
}
