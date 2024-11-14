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
        private readonly Func<Matrix, Value>[] MultiFunctions;
        private readonly Func<Value, Value, Matrix, Value>[] Interpolations;
        private Vector _indexes;

        internal static readonly FrozenDictionary<string, int> FunctionIndex =
        new Dictionary<string, int>()
        {
            { "identity", 0 },
            { "utriang", 1 },
            { "ltriang", 2 },
            { "symmetric", 3 },
            { "vec2diag", 4 },
            { "diag2vec", 5 },
            { "vec2col", 6 },
            { "n_rows", 7 },
            { "n_cols", 8 },
            { "mnorm_1", 9 },
            { "mnorm_2", 10 },
            { "mnorm", 10 },
            { "mnorm_e", 11 },
            { "mnorm_i", 12 },
            { "cond_1", 13 },
            { "cond_2", 14 },
            { "cond", 14 },
            { "cond_e", 15 },
            { "cond_i", 16 },
            { "det", 17 },
            { "rank", 18 },
            { "transp", 19 },
            { "trace", 20 },
            { "inverse", 21 },
            { "adj", 22 },
            { "cofactor", 23 },
            { "eigenvals", 24 },
            { "eigenvecs", 25 },
            { "eigen", 26 },
            { "lu", 27 },
            { "qr", 28 },
            { "svd", 29 },
            { "cholesky", 30 },

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
            { "msolve", 26 },
            { "cmsolve", 27 },
            { "hprod", 28 },
            { "fprod", 29 },
            { "kprod", 30 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

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
                Vec2Col,                //6
                NRows,                  //7
                NCols,                  //8
                L1Norm,                 //9
                L2Norm,                 //10
                FrobNorm,               //11
                InfNorm,                //12
                Cond1,                  //13
                Cond2,                  //14
                CondFrob,               //15
                CondInf,                //16
                Det,                    //17
                Rank,                   //18
                Transpose,              //19
                Trace,                  //20
                Invert,                 //21
                Adjoint,                //22
                Cofactor,               //23
                EigenValues,            //24
                EigenVectors,           //25
                Eigen,                  //26
                LUDecomposition,        //27
                QRDecomposition,        //28
                SVDDecomposition,       //29
                CholeskyDecomposition,  //30
            ];
            MatrixFunctions2 = [
                Create,
                Diagonal,
                Column,
                Row,
                Col,
                ExtractRows,
                ExtractCols,
                Fill,
                SortCols,
                RsortCols,
                SortRows,
                RsortRows,
                OrderCols,
                RevOrderCols,
                OrderRows,
                RevOrderRows,
                Count,
                FindEq,
                FindEq,
                FindNe,
                FindLt,
                FindLe,
                FindGt,
                FindGe,
                LSolve,
                ClSolve,
                MSolve,
                CmSolve,
                Hadamard,
                Frobenius,
                Kronecker
         ];
            MatrixFunctions3 = [
                FillRow,
                FillCol,
                Resize,
            ];
            MatrixFunctions4 = [
                Search,
                HLookupEq,
                HLookupEq,
                HLookupNe,
                HLookupLt,
                HLookupLe,
                HLookupGt,
                HLookupGe,
                VLookupEq,
                VLookupEq,
                VLookupNe,
                VLookupLt,
                VLookupLe,
                VLookupGt,
                VLookupGe,
                Copy,
                Add,
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

        internal Matrix EvaluateOperator(long index, Matrix a, in Value b) =>
            Matrix.EvaluateOperator(_calc.GetOperator(index), a, b,
                Calculator.IsZeroPreservingOperator[index], Calculator.OperatorRequireConsistentUnits(index));

        internal Matrix EvaluateOperator(long index, in Value a, Matrix b) =>
            Matrix.EvaluateOperator(_calc.GetOperator(index), a, b,
                Calculator.IsZeroPreservingOperator[index], Calculator.OperatorRequireConsistentUnits(index));

        internal Matrix EvaluateOperator(long index, Matrix a, Matrix b)
        {
            if (index == Calculator.MultiplyIndex)
                return a * b;

            return Matrix.EvaluateOperator(_calc.GetOperator(index), a, b,
                Calculator.IsZeroPreservingOperator[index], Calculator.OperatorRequireConsistentUnits(index));
        }

        internal Matrix EvaluateFunction(long Index, Matrix a) =>
            Matrix.EvaluateFunction(_calc.GetFunction(Index), a);

        internal Matrix EvaluateFunction2(long index, Matrix a, Matrix b) =>
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, index == 0, false);

        internal Matrix EvaluateFunction2(long index, Matrix a, in Value b) =>
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, index == 0, false);

        internal Matrix EvaluateFunction2(long index, in Value a, Matrix b) =>
            Matrix.EvaluateOperator(_calc.GetFunction2(index), a, b, index == 0, false);

        internal IValue EvaluateMultiFunction(long index, Matrix a) =>
            MultiFunctions[index](a);

        internal IValue EvaluateInterpolation(long index, Value a, Value b, Matrix c) =>
            Interpolations[index](a, b, c);

        private static DiagonalMatrix Identity(in IValue n) => new(IValue.AsInt(n), Value.One);
        private static UpperTriangularMatrix UpperTriangular(in IValue n) => new(IValue.AsInt(n));
        private static LowerTriangularMatrix LowerTriangular(in IValue n) => new(IValue.AsInt(n));
        private static SymmetricMatrix Symmetric(in IValue n) => new(IValue.AsInt(n));
        private static DiagonalMatrix Vec2Diag(in IValue v) => new(IValue.AsVector(v));
        private static Vector Diag2Vec(in IValue M) => IValue.AsMatrix(M).Diagonal();
        private static ColumnMatrix Vec2Col(in IValue v) => new(IValue.AsVector(v));
        private static IValue NRows(in IValue M) => new Value(IValue.AsMatrix(M).RowCount);
        private static IValue NCols(in IValue M) => new Value(IValue.AsMatrix(M).ColCount);
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
        private static Matrix Transpose(in IValue M) => IValue.AsMatrix(M).Transpose();
        private static IValue Trace(in IValue M) => IValue.AsMatrix(M).Trace();
        private static Matrix Invert(in IValue M) => IValue.AsMatrix(M).Invert();
        private static Matrix Adjoint(in IValue M) => IValue.AsMatrix(M).Adjoint();
        private static Matrix Cofactor(in IValue M) => IValue.AsMatrix(M).Cofactor();
        private static Vector EigenValues(in IValue M)
        {
            var matrix = IValue.AsMatrix(M);
            if (matrix is SymmetricMatrix sm)
                return sm.EigenValues();
            if (matrix is DiagonalMatrix dm)
                return dm.EigenValues();

            Throw.MatrixMustBeSymmetricException();
            return null;
        }

        private static Matrix EigenVectors(in IValue M)
        {
            var matrix = IValue.AsMatrix(M);
            if (matrix is SymmetricMatrix sm)
                return sm.EigenVectors();
            if (matrix is DiagonalMatrix dm)
                return dm.EigenVectors();

            Throw.MatrixMustBeSymmetricException();
            return null;
        }

        private static Matrix Eigen(in IValue M)
        {
            var matrix = IValue.AsMatrix(M);
            if (matrix is SymmetricMatrix sm)
                return sm.Eigen();
            if (matrix is DiagonalMatrix dm)
                return dm.Eigen();

            Throw.MatrixMustBeSymmetricException();
            return null;
        }

        private Matrix LUDecomposition(in IValue M)
        {
            var LU = IValue.AsMatrix(M).LUDecomposition(out var indexes);
            _indexes = Vector.FromIndexes(indexes);

            return LU;
        }
        private static Matrix QRDecomposition(in IValue M) => IValue.AsMatrix(M).QRDecomposition();
        private static Matrix SVDDecomposition(in IValue M) => IValue.AsMatrix(M).SVDDecomposition();
        private static UpperTriangularMatrix CholeskyDecomposition(in IValue a)
        {
            var M = IValue.AsMatrix(a);
            if (M is SymmetricMatrix sm)
                return sm.CholeskyDecomposition();
            if (M is DiagonalMatrix dm)
                return dm.CholeskyDecomposition();

            Throw.MatrixMustBeSymmetricException();
            return null;
        }
        private static Matrix Create(in IValue m, in IValue n) => new(IValue.AsInt(m), IValue.AsInt(n));
        private static DiagonalMatrix Diagonal(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsValue(value));
        private static ColumnMatrix Column(in IValue n, in IValue value) => new(IValue.AsInt(n), IValue.AsValue(value));
        private static Vector Row(in IValue M, in IValue row) => IValue.AsMatrix(M).Row(IValue.AsInt(row));
        private static Vector Col(in IValue M, in IValue col) => IValue.AsMatrix(M).Col(IValue.AsInt(col));
        private static Matrix ExtractRows(in IValue M, in IValue rows) => IValue.AsMatrix(M).ExtractRows(IValue.AsVector(rows));
        private static Matrix ExtractCols(in IValue M, in IValue cols) => IValue.AsMatrix(M).ExtractCols(IValue.AsVector(cols));
        private static Matrix Fill(in IValue M, in IValue value) => IValue.AsMatrix(M).Fill(IValue.AsValue(value));
        private static Matrix SortCols(in IValue M, in IValue row) => IValue.AsMatrix(M).SortCols(IValue.AsInt(row));
        private static Matrix RsortCols(in IValue M, in IValue row) => IValue.AsMatrix(M).SortCols(IValue.AsInt(row), true);
        private static Matrix SortRows(in IValue M, in IValue col) => IValue.AsMatrix(M).SortRows(IValue.AsInt(col));
        private static Matrix RsortRows(in IValue M, in IValue col) => IValue.AsMatrix(M).SortRows(IValue.AsInt(col), true);
        private static Vector OrderCols(in IValue M, in IValue row) => IValue.AsMatrix(M).OrderCols(IValue.AsInt(row));
        private static Vector RevOrderCols(in IValue M, in IValue row) => IValue.AsMatrix(M).OrderCols(IValue.AsInt(row), true);
        private static Vector OrderRows(in IValue M, in IValue col) => IValue.AsMatrix(M).OrderRows(IValue.AsInt(col));
        private static Vector RevOrderRows(in IValue M, in IValue col) => IValue.AsMatrix(M).OrderRows(IValue.AsInt(col), true);
        private static IValue Count(in IValue M, in IValue value) => IValue.AsMatrix(M).Count(IValue.AsValue(value));
        private static Matrix FindEq(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.Equal);
        private static Matrix FindNe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.NotEqual);
        private static Matrix FindLt(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.LessThan);
        private static Matrix FindLe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.LessOrEqual);
        private static Matrix FindGt(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.GreaterThan);
        private static Matrix FindGe(in IValue M, in IValue value) => IValue.AsMatrix(M).FindAll(IValue.AsValue(value), Vector.Relation.GreaterOrEqual);
        private static Vector LSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsVector(B);
            if (a.RowCount != b.Length)
                Throw.MatrixDimensionsException();

            return a.LSolve(b);
        }

        private static Vector ClSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsVector(B);
            if (a.RowCount != b.Length)
                Throw.MatrixDimensionsException();

            if (a is SymmetricMatrix sm)
                return sm.ClSolve(b);

            Throw.MatrixMustBeSymmetricException();
            return null;
        }

        private static Matrix MSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a.RowCount != b.RowCount)
                Throw.MatrixDimensionsException();

            return a.MSolve(b);
        }

        private static Matrix CmSolve(in IValue A, in IValue B)
        {
            var a = IValue.AsMatrix(A);
            var b = IValue.AsMatrix(B);
            if (a.RowCount != b.RowCount)
                Throw.MatrixDimensionsException();

            if (a is SymmetricMatrix sm)
                return sm.CmSolve(b);

            Throw.MatrixMustBeSymmetricException();
            return null;
        }


        private static Matrix Hadamard(in IValue A, in IValue B) => Matrix.Hadamard(IValue.AsMatrix(A), IValue.AsMatrix(B));

        private static IValue Frobenius(in IValue A, in IValue B) => Matrix.Frobenius(IValue.AsMatrix(A), IValue.AsMatrix(B));

        private static Matrix Kronecker(in IValue A, in IValue B) => Matrix.Kronecker(IValue.AsMatrix(A), IValue.AsMatrix(B));
        private static Matrix FillRow(Matrix M, in IValue row, in IValue value) => M.FillRow(IValue.AsInt(row), IValue.AsValue(value));
        private static Matrix FillCol(Matrix M, in IValue col, in IValue value) => M.FillCol(IValue.AsInt(col), IValue.AsValue(value));
        private static Matrix Resize(Matrix M, in IValue m, in IValue n) => M.Resize(IValue.AsInt(m), IValue.AsInt(n));
        private static Vector Search(Matrix M, in IValue value, in IValue i, in IValue j) => M.Search(IValue.AsValue(value), IValue.AsInt(i), IValue.AsInt(j));
        private static Vector HLookupEq(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.Equal);
        private static Vector HLookupNe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.NotEqual);
        private static Vector HLookupLt(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.LessThan);
        private static Vector HLookupLe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.LessOrEqual);
        private static Vector HLookupGt(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.GreaterThan);
        private static Vector HLookupGe(Matrix M, in IValue value, in IValue searchRow, in IValue returnRow) => M.HLookup(IValue.AsValue(value), IValue.AsInt(searchRow), IValue.AsInt(returnRow), Vector.Relation.GreaterOrEqual);
        private static Vector VLookupEq(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.Equal);
        private static Vector VLookupNe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.NotEqual);
        private static Vector VLookupLt(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.LessThan);
        private static Vector VLookupLe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.LessOrEqual);
        private static Vector VLookupGt(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.GreaterThan);
        private static Vector VLookupGe(Matrix M, in IValue value, in IValue searchCol, in IValue returnCol) => M.VLookup(IValue.AsValue(value), IValue.AsInt(searchCol), IValue.AsInt(returnCol), Vector.Relation.GreaterOrEqual);
        private static Matrix Copy(Matrix A, in IValue B, in IValue i, in IValue j) => A.CopyTo(IValue.AsMatrix(B), IValue.AsInt(i), IValue.AsInt(j));
        private static Matrix Add(Matrix A, in IValue B, in IValue i, in IValue j) => A.AddTo(IValue.AsMatrix(B), IValue.AsInt(i), IValue.AsInt(j));
        private static Matrix Submatrix(Matrix M, in IValue i1, in IValue j1, in IValue i2, in IValue j2) => M.Submatrix(IValue.AsInt(i1), IValue.AsInt(j1), IValue.AsInt(i2), IValue.AsInt(j2));
        internal static Matrix JoinCols(IValue[] v)
        {
            var n = v.Length;
            var m = 0;
            for (int i = 0; i < n; ++i)
            {
                if (v[i] is Vector vec)
                {
                    if (vec.Length > m)
                        m = vec.Length;
                }
                else
                    Throw.MustBeVectorException(Throw.Items.Argument);
            }
            return Matrix.CreateFromCols(v.Cast<Vector>().ToArray(), m);
        }

        internal static Matrix JoinRows(IValue[] v)
        {
            var m = v.Length;
            var n = 0;
            for (int i = 0; i < m; ++i)
            {
                if (v[i] is Vector vec)
                {
                    if (vec.Length > n)
                        n = vec.Length;
                }
                else
                    Throw.MustBeVectorException(Throw.Items.Argument);
            }
            return Matrix.CreateFromRows(v.Cast<Vector>().ToArray(), n);
        }

        private static Matrix[] CastValues(IValue[] values)
        {
            var n = values.Length;
            var M = new Matrix[n];
            for (int i = 0; i < n; ++i)
                M[i] = IValue.AsMatrix(values[i]);

            return M;
        }
        private static Matrix Augment(IValue[] values) =>
            Matrix.Augment(CastValues(values));
        private static Matrix Stack(IValue[] values) =>
            Matrix.Stack(CastValues(values));
        private static Value Min(Matrix M) => M.Min();
        private static Value Max(Matrix M) => M.Max();
        private static Value Sum(Matrix M) => M.Sum();
        private static Value SumSq(Matrix M) => M.SumSq();
        private static Value Srss(Matrix M) => M.Srss();
        private static Value Average(Matrix M) => M.Average();
        private static Value Product(Matrix M) => M.Product();
        private static Value Mean(Matrix M) => M.Mean();
        private static Value Switch(Matrix M) => M[0, 0];
        private static Value And(Matrix M) => M.And();
        private static Value Or(Matrix M) => M.Or();
        private static Value Xor(Matrix M) => M.Xor();
        private static Value Gcd(Matrix M) => M.Gcd();
        private static Value Lcm(Matrix M) => M.Lcm();
        private static Value Take(Value x, Value y, Matrix m) => m.Take(x, y);
        private static Value Line(Value x, Value y, Matrix m) => m.Line(x, y);
        private static Value Spline(Value x, Value y, Matrix m) => m.Spline(x, y);
    }
}
