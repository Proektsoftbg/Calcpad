using System;
using System.Reflection;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed partial class Compiler
        {
            private static readonly MethodInfo EvaluateEqual =
                typeof(IValue).GetMethod(
                    nameof(IValue.Equal),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateNotEqual =
                typeof(IValue).GetMethod(
                    nameof(IValue.NotEqual),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateFunctionMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateFunction),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateVectorFunctionMethod =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.EvaluateVectorFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(int),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateMatrixFunctionMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(int),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateOperatorMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateOperator),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateFunction2Method =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateFunction2),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateVectorFunction2Method =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.EvaluateVectorFunction2),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(int),
                        typeof(IValue).MakeByRefType(),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateMatrixFunction2Method =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixFunction2),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(int),
                        typeof(IValue).MakeByRefType(),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateVectorFunction3Method =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.EvaluateVectorFunction3),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMatrixFunction3Method =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixFunction3),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMatrixFunction4Method =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixFunction4),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMatrixFunction5Method =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixFunction5),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMultiFunctionMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateMultiFunction),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateVectorMultiFunctionMethod =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.EvaluateVectorMultiFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMatrixMultiFunctionMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.EvaluateMatrixMultiFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateInterpolationMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateInterpolation),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateCustomFunction1Method =
                typeof(Evaluator).GetMethod(
                    nameof(Evaluator.EvaluateFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(CustomFunction1),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateCustomFunction2Method =
                typeof(Evaluator).GetMethod(
                    nameof(Evaluator.EvaluateFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(CustomFunction2),
                        typeof(IValue).MakeByRefType(),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateCustomFunction3Method =
                typeof(Evaluator).GetMethod(
                    nameof(Evaluator.EvaluateFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(CustomFunction3),
                        typeof(IValue).MakeByRefType(),
                        typeof(IValue).MakeByRefType(),
                        typeof(IValue).MakeByRefType()
                    ],
                    null
                );

            private static readonly MethodInfo EvaluateCustomFunctionNMethod =
                typeof(Evaluator).GetMethod(
                    nameof(Evaluator.EvaluateFunction),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    [
                        typeof(CustomFunctionN),
                        typeof(IValue[])
                    ],
                    null
                );

            private static readonly MethodInfo CalculateMethod =
                typeof(SolveBlock).GetMethod(
                    nameof(SolveBlock.Calculate),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            private static readonly MethodInfo ExpandRealValuesMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.ExpandRealValues),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo AsValueMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.AsValue),
                    BindingFlags.Static | BindingFlags.NonPublic
                );


            private static readonly MethodInfo AsRealMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.AsReal),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo AsVectorMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.AsVector),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo AsMatrixMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.AsMatrix),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo ThrowIndexOutOfRangeException =
                typeof(Throw).GetMethod(
                    nameof(Throw.IndexOutOfRangeException),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly ConstructorInfo VectorConstructor =
                typeof(Vector).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    [typeof(RealValue[])]
                );

            private static readonly MethodInfo JoinRowsMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.JoinRows),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo AssignVariableMethod =
                typeof(Variable).GetMethod(
                    nameof(Variable.Assign),
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
        }
    }
}