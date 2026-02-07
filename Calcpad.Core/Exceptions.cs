using System;
using System.Xml.Linq;
namespace Calcpad.Core
{
    internal static class Exceptions
    {
        internal enum Items
        {
            Index,
            Count,
            NumRows,
            NumCols,
            Argument,
            Result,
            Variable,
            IndexTarget,
            Limit,
            Value,
        }

        private static string ItemToString(Items item) =>
            item switch
            {
                Items.Index => Messages.Index,
                Items.Count => Messages.Count,
                Items.NumRows => Messages.Number_of_rows,
                Items.NumCols => Messages.Number_of_columns,
                Items.Argument => Messages.Argument,
                Items.Result => Messages.Result,
                Items.Variable => Messages.Variable,
                Items.IndexTarget => Messages.Index_target,
                Items.Limit => Messages.Limit,
                Items.Value => Messages.Value,
                _ => throw new ArgumentException(Messages.Invalid_item),
            };

        internal static MathParserException IncompleteExpression() =>
            new(Messages.Incomplete_expression);

        internal static MathParserException MissingLeftBracket() =>
            new(Messages.Missing_left_bracket);

        internal static MathParserException MissingRightBracket() =>
            new(Messages.Missing_right_bracket);

        internal static MathParserException InvalidSymbol(char c) =>
            new(string.Format(Messages.Invalid_symbol_0, c));

        internal static MathParserException InvalidUnits(string s) =>
            new(string.Format(Messages.Invalid_units_0, s));

        internal static MathParserException InvalidLiteral(string s, string literal) =>
            new(string.Format(Messages.Cannot_evaluate_0_as_1, s, literal));

        internal static MathParserException InvalidNumber(string s) =>
            InvalidLiteral(s, "number");

        internal static MathParserException InvalidOperator(char c) =>
            new(string.Format(Messages.Invalid_operator_0, c));

        internal static MathParserException PowerNotUnitless() =>
            new(Messages.Power_must_be_unitless);

        internal static MathParserException ResultIsNotUnits() =>
            new(Messages.The_expression_on_the_right_does_not_evaluate_to_units);

        internal static MathParserException CannotEvaluateFunction(string s) =>
            new(string.Format(Messages.Cannot_evaluate_the_function_f_for_v_equals_0, s));

        internal static MathParserException FunctionNotDefined(string s) =>
            new(string.Format(Messages.The_function_f_is_not_defined_for_v_equals_0, s));

        internal static MathParserException InconsistentUnits(string u1, string u2) =>
            new(string.Format(Messages.Inconsistent_units_0_and_1, u1, u2));

        internal static MathParserException IterationLimits(string l1, string l2) =>
            new(string.Format(Messages.Limits_out_of_range_0_1, l1, l2));

        internal static MathParserException InvalidUnitsFunction(string function, string unit) =>
            new(string.Format(Messages.Invalid_units_for_function_0_1, function, unit));

        internal static MathParserException RootUnitless() =>
            new(Messages.Root_index_must_be_unitless);

        internal static MathParserException RootComplex() =>
            new(Messages.Root_index_cannot_be_a_complex_number);

        internal static MathParserException RootInteger() =>
            new(Messages.Root_index_must_be_integer_more_than_1);

        internal static MathParserException FactorialArgumentOutOfRange() =>
            new(Messages.Argument_out_of_range_for_n_factorial);

        internal static MathParserException FactorialArgumentUnitless() =>
            new(Messages.The_argument_of_n_factorial_must_be_unitless);

        internal static MathParserException FactorialArgumentPositiveInteger() =>
            new(Messages.The_argument_of_n_factorial_must_be_a_positive_integer);

        internal static MathParserException FactorialArgumentComplex() =>
            new(Messages.The_argument_of_n_factorial_cannot_be_complex);

        internal static MathParserException RemainderUnits(string u1, string u2) =>
            new(string.Format(Messages.Cannot_evaluate_remainder_0_1_Denominator_must_be_unitless, u1, u2));

        internal static MathParserException BothValuesInteger() =>
            new(Messages.Both_values_must_be_integers);

        internal static MathParserException VariableNotExist(string name) =>
            new(string.Format(Messages.Variable_0_does_not_exist, name));

        internal static MathParserException UnitNotExist(string name) =>
            new(string.Format(Messages.Unit_0_does_not_exist, name));

        internal static MathParserException InteruptedByUser() =>
            new(Messages.Interrupted_by_user);

        internal static MathParserException CalculationsNotActive() =>
            new(Messages.Calculations_not_active);

        internal static MathParserException ExpressionEmpty() =>
            new(Messages.Expression_is_empty);

        internal static MathParserException MissingOperand() =>
            new(Messages.Missing_operand);

        internal static MathParserException InvalidFunction(string s) =>
            new(string.Format(Messages.Invalid_function_0, s));

        internal static MathParserException CannotEvaluateAsType(string literal, string type) =>
            new(string.Format(Messages.Cannot_evaluate_0_as_1, literal, type));

        internal static MathParserException StackLeak() =>
            new(Messages.Stack_memory_leak_Invalid_expression);

        internal static MathParserException StackEmpty() =>
            new(Messages.Stack_empty_Invalid_expression);

        internal static MathParserException UndefinedInputField() =>
            new(Messages.Undefined_input_field);

        internal static MathParserException UndefinedVariableOrUnits(string s) =>
            new(string.Format(Messages.Undefined_variable_or_units_0, s));

        internal static MathParserException ErrorEvaluatingAsFunction(string s) =>
            new(string.Format(Messages.Error_evaluating_0_as_function, s));

        internal static MathParserException ErrorEvaluatingAsFunctionOrOperator(string s) =>
            new(string.Format(Messages.Error_evaluating_0_as_function_or_operator, s));

        internal static MathParserException CannotRewriteUnits(string s) =>
            new(string.Format(Messages.Cannot_rewrite_existing_units_0, s));

        internal static MathParserException InconsistentTargetUnits(string sourceUnits, string targetUnits) =>
            new(string.Format(Messages.The_calculated_units_0_are_inconsistent_with_the_target_units_1, sourceUnits, targetUnits));

        internal static MathParserException InvalidCharacter(char c) =>
            new(string.Format(Messages.Invalid_character_exception, c));

        internal static MathParserException ImproperAssignment() =>
            new(Messages.Improper_use_of_the_assignment_operator_equals);

        internal static MathParserException MissingLeftSolverBracket() =>
            new(Messages.Missing_left_bracket_in_solver_command);

        internal static MathParserException MissingRightSolverBracket() =>
            new(Messages.Missing_right_bracket_in_solver_command);

        internal static MathParserException InvalidMacro(string s) =>
            new(string.Format(Messages.Invalid_macro_identifier_0, s));

        internal static MathParserException InvalidSolver(string s) =>
            new(string.Format(Messages.Invalid_solver_command_definition_0, s));

        internal static MathParserException ErrorParsingUnits(string s) =>
            new(string.Format(Messages.Error_parsing_0_as_units, s));

        internal static MathParserException ErrorParsingNumber(string s) =>
            new(string.Format(Messages.Error_parsing_0_as_number, s));

        internal static MathParserException MissingDelimiter(char delimiter, string script) =>
            new(string.Format(Messages.Missing_delimiter_0_in_solver_command_1, delimiter, script));

        internal static MathParserException MultipleAssignments(string s) =>
            new(string.Format(Messages.More_than_one_operator_equals_in_0, s));

        internal static MathParserException NotConstantExpression(string s) =>
            new(string.Format(Messages.The_expression_on_the_right_side_must_be_constant_0, s));

        internal static MathParserException InconsistentUnits1(string variable, string units) =>
            new(string.Format(Messages.Inconsistent_units_for_0_equals_1, variable, units));

        internal static MathParserException InconsistentUnits2(string variable, string u1, string u2) =>
            new(string.Format(Messages.Inconsistent_units_for_0_equals_1_2, variable, u1, u2));

        internal static MathParserException NoSolution(string s) =>
            new(string.Format(Messages.No_solution_for_0, s));

        internal static MathParserException RecursionNotAllowed(string s) =>
            new(string.Format(Messages.Recursion_is_not_allowed_in_function_definition_0, s));

        internal static MathParserException AssignmentPreceded() =>
            new(Messages.The_assignment_equals_must_be_preceded_by_custom_function_or_variable);

        internal static MathParserException AssignmentNotFirst() =>
            new(Messages.Assignment_equals_must_be_the_first_operator_in_the_expression);

        internal static MathParserException InvalidSyntax(string s) =>
           new(string.Format(Messages.Invalid_syntax_0, s));

        internal static MathParserException InvalidSyntax(string s1, string s2) =>
            new(string.Format(Messages.Invalid_syntax_0_1, s1, s2));

        internal static MathParserException UnexpectedDelimiter() =>
            new(Messages.Unexpected_delimiter);

        internal static MathParserException InvalidNumberOfArguments() =>
            new(Messages.Invalid_number_of_arguments);

        internal static MathParserException ResultNotReal(string s) =>
            new(string.Format(Messages.The_result_is_not_a_real_number_0, s));

        internal static MathParserException MissingFunctionParameter() =>
            new(Messages.Missing_parameter_in_function_definition);

        internal static MathParserException MissingFunctionDelimiter() =>
            new(Messages.Missing_delimiter_in_function_definition);

        internal static MathParserException InvalidFunctionToken(string name) =>
            new(string.Format(Messages.Invalid_token_in_function_definition_0, name));

        internal static MathParserException CircularReference(string name) =>
            new(string.Format(Messages.Circular_reference_detected_for_function_0, name));

        internal static MathParserException InvalidFunctionDefinition() =>
            new(Messages.Invalid_function_definition_exception);

        internal static MathParserException ArgumentOutOfRange(string function) =>
            new(string.Format(Messages.Argument_out_of_range_for_0_x, function));

        internal static MathParserException ConditionEmpty() =>
            new(Messages.Condition_cannot_be_empty);

        internal static MathParserException ConditionNotInitialized() =>
            new(Messages.Condition_block_not_initialized_with_if);

        internal static MathParserException DuplicateElse() =>
            new(Messages.Duplicate_else_in_condition_block);

        internal static MathParserException ElseIfAfterElse() =>
            new(Messages.else_if_is_not_allowed_after_else_in_condition_block);

        internal static MathParserException ConditionComplex() =>
            new(Messages.Condition_cannot_evaluate_to_a_complex_number);

        internal static MathParserException ConditionResultInvalid(string s) =>
            new(string.Format(Messages.Condition_result_is_invalid_0, s));

        internal static MathParserException DuplicateMacroParameters(string s) =>
            new(string.Format(Messages.Duplicate_macro_parameter_names_0_and_1, s, s));

        internal static MathParserException UndefinedMacro(string s) =>
            new(string.Format(Messages.Macro_not_defined_0, s));

        internal static MathParserException MissingMapItem(string s) =>
            new(string.Format(Messages.Missing_0_in_surface_map_command, s));

        internal static MathParserException PlotLimitsIdentical() =>
            new(Messages.The_limits_of_plot_area_are_identical);

        internal static MathParserException ErrorWritingPngFile(string path) =>
            new(string.Format(Messages.Error_writing_a_png_file_to_0, path));

        internal static MathParserException ErrorWritingSvgFile(string path) =>
            new(string.Format(Messages.Error_writing_a_svg_file_to_0, path));

        internal static MathParserException ErrorConvertingPngToBase64() =>
            new(Messages.Error_converting_png_to_Base64);

        internal static MathParserException InconsistentUnitsOperation(string ua, char op, string ub) =>
            new(string.Format(Messages.Inconsistent_units_0_1_2, ua, op, ub));

        internal static MathParserException UnitsToComplexPower() =>
            new(Messages.Units_cannon_be_raised_to_complex_power);

        internal static MathParserException CannotEvaluateRemainder(string ua, string ub) =>
            new(string.Format(Messages.Cannot_evaluate_remainder_0_1_The_denominator_must_be_unitless, ua, ub));

        internal static MathParserException MissingVectorOpeningBracket() =>
            new(Messages.Missing_vector_opening_bracket);

        internal static MathParserException BracketMismatch() =>
            new(Messages.Bracket_mismatch);

        internal static MathParserException IndexOutOfRange(string index) =>
            new(string.Format(Messages.Index_out_of_range_0, index));

        internal static MathParserException CannotAssignVectorToScalar() =>
            new(Messages.Cannot_assign_vector_to_scalar);

        internal static MathParserException MissingVectorClosingBracket() =>
            new(Messages.Missing_vector_closing_bracket);

        internal static MathParserException MustBeScalar(Items item) =>
            new(ItemToString(item) + Messages._must_be_scalar);

        internal static MathParserException MustBeReal(Items item) =>
            new(ItemToString(item) + Messages._must_be_real);

        internal static MathParserException MustBePositiveInteger(Items item) =>
            new(ItemToString(item) + Messages._must_be_positive_integer);

        internal static MathParserException MustBeMatrix(Items item) =>
            new(ItemToString(item) + Messages._must_be_matrix);

        internal static MathParserException MustBeVector(Items item) =>
            new(ItemToString(item) + Messages._must_be_vector);

        internal static MathParserException StepCannotBeZero() =>
            new(Messages.Step_cannot_be_zero);

        internal static MathParserException VectorSizeLimit() =>
            new(string.Format(Messages.Vector_size_cannot_exceed_0, Vector.MaxLength));

        internal static MathParserException CrossProductVectorDimensions() =>
            new(Messages.Cross_product_is_definedonly_for_vectors_with_2_and_3_elements);

        internal static MathParserException FunctionOnlyRealMode(string name) =>
            new(string.Format(Messages.Function_0_is_not_defined_in_complex_mode, name));

        internal static MathParserException MatrixDimensions() =>
            new(Messages.Matrix_dimensions_do_not_match);

        internal static MathParserException MatrixNotSquare() =>
            new(Messages.Matrix_must_be_square);

        internal static MathParserException InvalidLpNormArgument() =>
            new(Messages.The_Lp_norm_argument_must_be_p_ge_1);

        internal static MathParserException MatrixMustBeSymmetric() =>
            new(Messages.Matrix_must_be_symmetric);

        internal static MathParserException MatrixNotPositiveDefinite() =>
            new(Messages.Matrix_is_not_positive_definite);

        internal static MathParserException MatrixSingular() =>
            new(Messages.Matrix_is_singular);

        internal static MathParserException MatrixCloseToSingular() =>
            new(Messages.Matrix_is_singular_or_close_to_singular);

        internal static MathParserException ComplexVectorsAndMatricesNotSupported() =>
            new(Messages.Vectors_and_matrices_are_not_supported_in_complex_mode);

        internal static MathParserException CannotInterpolateWithNonScalarValue() =>
            new(Messages.Cannot_interpolate_with_non_scalar_value);

        internal static MathParserException JacobiFailed() =>
            new(Messages.Jacobi_iteration_failed);

        internal static MathParserException InvalidOperand(string value) =>
            new(string.Format(Messages.Invalid_operand_0, value));

        internal static MathParserException InvalidArgument(string value) =>
            new(string.Format(Messages.Invalid_argument_0, value));

        internal static MathParserException ArgumentNotEigenvalue(string value) =>
            new(string.Format(Messages.The_argument_is_not_an_eigenvalue_0, value));

        internal static MathParserException QLAlgorithmFailed() =>
            new(Messages.The_QL_algorithm_failed_to_converge);

        internal static MathParserException MatirixSizeLimit() =>
            new(string.Format(Messages.Matrix_size_cannot_exceed_0, Matrix.MaxSize));

        internal static MathParserException MatrixNotHigh() =>
            new(Messages.The_number_of_matrix_rows_must_be_greater_than_or_equal_to_the_number_of_columns);

        internal static MathParserException FunctionDefinitionInSolver() =>
            new(Messages.Function_definition_not_allowed_in_solver_block);

        internal static MathParserException InvalidFormatString(string s) =>
            new(string.Format(Messages.Invalid_format_string_0, s));

        internal static MathParserException VariableNotMatrix(string name) =>
            new(string.Format(Messages.The_variable_is_not_a_matrix_0, name));

        internal static MathParserException FileNotFound(string path) =>
            new(string.Format(Messages.File_not_found_0, path));

        internal static MathParserException MissingFileName() =>
            new(Messages.Missing_file_name);

        internal static MathParserException InvalidType(char type) =>
            new (string.Format(Messages.Invalid_type_0, type));

        internal static MathParserException PathNotFound(string path) =>
            new(string.Format(Messages.Path_not_found_0, path));

        internal static MathParserException FileFormatNotSupported(string ext) =>
            new(string.Format(Messages.ThisFileFormatIsNotSupported0, ext));

        internal static MathParserException MustBeHpMatrix(Items item) =>
            new(ItemToString(item) + Messages.MustBeHpMatrix);

        internal static MathParserException MustBeHpVector(Items item) =>
             new(ItemToString(item) + Messages.MustBeHpVector);

        internal static MathParserException MatrixOneOrTwoRows() =>
            new (Messages.TheInputMatrixForFFTMustHaveOneRowForRealAndTwoRowsForComplexData);
        internal static MathParserException MissingMatrixIndex(string matrix) =>
            new(string.Format(Messages.MissingMatrixIndexIn0, matrix));

        internal static MathParserException CounterMustBeASingleVariableName() =>
            new(Messages.CounterMustBeASingleVariableName);

        internal static MathParserException InvalidNumberOfIndexes() =>
            new(Messages.InvalidNumberOfIndexes);

        internal static MathParserException InfiniteLoop(int maxCount) =>
            new(string.Format(Messages.PossiblyInfiniteLoopDetected, maxCount));

        internal static MathParserException CannotModifyConstant(string name) =>
            new(string.Format(Messages.CannotModifyConstant, name));
    }
}