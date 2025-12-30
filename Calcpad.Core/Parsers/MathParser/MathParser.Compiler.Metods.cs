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

            private static readonly MethodInfo EvaluateOperatorMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateOperator),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluatePhasorMethod =
                typeof(ComplexCalculator).GetMethod(
                    nameof(ComplexCalculator.EvaluateOperator),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateFunction2Method =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateFunction2),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo EvaluateMultiFunctionMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.EvaluateMultiFunction),
                    BindingFlags.Static | BindingFlags.NonPublic
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
                typeof(SolverBlock).GetMethod(
                    nameof(SolverBlock.Calculate),
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

            private static readonly MethodInfo AsMatrixMethod =
                typeof(IValue).GetMethod(
                    nameof(IValue.AsMatrix),
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

            private static readonly MethodInfo GetVectorElementMethod =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.GetElement),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo GetMatrixElementMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.GetElement),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo SetVectorElementMethod =
                typeof(VectorCalculator).GetMethod(
                    nameof(VectorCalculator.SetElement),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo SetMatrixElementMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.SetElement),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo ThrowInfiniteLoopMethod =
                typeof(Exceptions).GetMethod(
                    nameof(Exceptions.InfiniteLoop),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            private static readonly MethodInfo LuDecompositionMethod =
                typeof(MatrixCalculator).GetMethod(
                    nameof(MatrixCalculator.LUDecomposition),
                    BindingFlags.Static | BindingFlags.NonPublic
                );
        }
    }
}