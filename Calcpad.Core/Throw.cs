using System;

namespace Calcpad.Core
{
    internal static class Throw
    {
        internal static void InvalidSyntaxException(in string s) =>
           New(string.Format(Messages.Invalid_syntax__0__, s));
        internal static void IncompleteExpressionException() =>
            New(Messages.Incomplete_expression);
        internal static void MissingLeftBracketException() =>
            New(Messages.Missing_left_bracket);
        internal static void MissingRightBracketException() =>
            New(Messages.Missing_right_bracket);
        internal static void InvalidSymbolException(char c) =>
            New(string.Format(Messages.Invalid_symbol__0__, c));
        internal static void InvalidUnitsException(in string s) =>
            New(string.Format(Messages.Invalid_units__0__, s));
        internal static void InvalidLiteralException(in string s, in string literal) =>
            New(string.Format(Messages.Cannot_evaluate__0__as__1__, s, literal));
        internal static void InvalidNumberException(in string s) =>
            InvalidLiteralException(s, "number");
        internal static void InvalidOperatorException(char c) =>
            New(string.Format(Messages.Invalid_operator__0__, c));
        internal static void PowerNotUnitlessException() =>
            New(Messages.Power_must_be_unitless);
        internal static void ResultIsNotUnitsException() =>
            New(Messages.The_expression_on_the_right_does_not_evaluate_to_units);
        internal static void CannotEvaluateFunctionException(in string s) =>
            New(string.Format(Messages.Cannot_evaluate_the_function_f_for_v_equals__0__, s));
        internal static void FunctionNotDefinedException(in string s) =>
            New(string.Format(Messages.The_function_f_is_not_defined_for_v_equals__0__, s));
        internal static void InconsistentUnitsException(in string u1, in string u2) =>
            New(string.Format(Messages.Inconsistent_units__0__and__1__, u1, u2));
        internal static void IterationLimitsException(in string l1, in string l2) =>
            New(string.Format(Messages.Limits_out_of_range__0____1__, l1, l2));
        internal static void InvalidUnitsFunctionException(in string function, in string unit) =>
            New(string.Format(Messages.Invalid_units_for_function__0__1__, function, unit));
        internal static void RootUnitlessException() =>
            New(Messages.Root_index_must_be_unitless);
        internal static void RootComplexException() =>
            New(Messages.Root_index_cannot_be_a_complex_number);
        internal static void RootIntegerException() =>
            New(Messages.Root_index_must_be_integer_more_than_1);
        internal static void FactorialArgumentOutOfRangeException() =>
            New(Messages.Argument_out_of_range_for_n_factorial);
        internal static void FactorialArgumentUnitlessException() =>
            New(Messages.The_argument_of_n_factorial_must_be_unitless);
        internal static void FactorialArgumentPositiveIntegerException() =>
            New(Messages.The_argument_of_n_factorial_must_be_a_positive_integer);
        internal static void FactorialArgumentComplexException() =>
            New(Messages.The_argument_of_n_factorial_cannot_be_complex);
        internal static void RemainderUnitsException(in string u1, in string u2) =>
            New(string.Format(Messages.Cannot_evaluate_remainder__0__1__Denominator_must_be_unitless, u1, u2));
        internal static void BothValuesIntegerException() =>
            New(Messages.Both_values_must_be_integers);
        internal static void VariableNotExistException(in string name) =>
            New(string.Format(Messages.Variable__0__does_not_exist, name));
        internal static void UnitNotExistException(in string name) =>
            New(string.Format(Messages.Unit__0__does_not_exist, name));
        internal static void InteruptedByUserException() =>
            New(Messages.Interrupted_by_user);
        internal static void CalculationsNotActiveException() =>
            New(Messages.Calculations_not_active);
        internal static void ExpressionEmptyException() =>
            New(Messages.Expression_is_empty);
        internal static void MissingOperandException() =>
            New(Messages.Missing_operand);
        internal static void InvalidFunctionException(in string s) =>
            New(string.Format(Messages.Invalid_function__0__, s));
        internal static void CannotEvaluateAsTypeException(in string literal, in string type) =>
            New(string.Format(Messages.Cannot_evaluate__0__as__1__, literal, type));
        internal static void StackLeakException() =>
            New(Messages.Stack_memory_leak__Invalid_expression);
        internal static void StackEmptyException() =>
            New(Messages.Stack_empty_Invalid_expression);
        internal static void UndefinedInputFieldException() =>
            New(Messages.Undefined_input_field);
        internal static void UndefinedVariableOrUnitsException(in string s) =>
            New(string.Format(Messages.Undefined_variable_or_units__0__, s));
        internal static void ErrorEvaluatingAsFunctionException(in string s) =>
            New(string.Format(Messages.Error_evaluating__0__as_function, s));
        internal static void ErrorEvaluatingAsFunctionOrOperatorException(in string s) =>
            New(string.Format(Messages.Error_evaluating__0__as_function_or_operator, s));
        internal static void CannotRewiriteUnitsException(in string s) =>
            New(string.Format(Messages.Cannot_rewrite_existing_units__0__, s));
        internal static void InconsistentTargetUnitsException(in string sourceUnits, in string targetUnits) =>
            New(string.Format(Messages.The_calculated_units__0__are_inconsistent_with_the_target_units__1__, sourceUnits, targetUnits));
        internal static void InvalidCharacterException(char c) =>
            New(string.Format(Messages.InvalidCharacterException, c));
        internal static void ImproperAssignmentException() =>
            New(Messages.Improper_use_of_the_assignment_operator_equals);
        internal static void MissingLeftSolverBracketException() =>
            New(Messages.Missing_left_bracket___in_solver_command);
        internal static void MissingRightSolverBracketException() =>
            New(Messages.Missing_right_bracket___in_solver_command);
        internal static void InvalidMacroException(in string s) =>
            New(string.Format(Messages.Invalid_macro_identifier__0__, s));
        internal static void InvalidSolverException(in string s) =>
            New(string.Format(Messages.Invalid_solver_command_definition__0__, s));
        internal static void ErrorParsingUnitsException(in string s) =>
            New(string.Format(Messages.Error_parsing__0__as_units, s));
        internal static void ErrorParsingNumberException(in string s) =>
            New(string.Format(Messages.Error_parsing__0__as_number, s));
        internal static void MissingDelimiterException(char delimiter, in string script) =>
            New(string.Format(Messages.Missing_delimiter__0__in_solver_command__1__, delimiter, script));
        internal static void MultipleAssignmentsException(in string s) =>
            New(string.Format(Messages.More_than_one_operator_equals_in__0__, s));
        internal static void NotConstantExpressionException(in string s) =>
            New(string.Format(Messages.The_expression_on_the_right_side_must_be_constant__0__, s));
        internal static void InconsistentUnits1Exception(in string variable, in string units) =>
            New(string.Format(Messages.Inconsistent_units_for__0__equals__1__, variable, units));
        internal static void InconsistentUnits2Exception(in string variable, in string u1, in string u2) =>
            New(string.Format(Messages.Inconsistent_units_for__0__equals__1_____2__, variable, u1, u2));
        internal static void NoSolutionException(in string s) =>
            New(string.Format(Messages.No_solution_for__0__, s));
        internal static void RecursionNotAllowedException(in string s) =>
            New(string.Format(Messages.Recursion_is_not_allowed_in_function_definition__0__, s));
        internal static void AssignmentPrecededException() =>
            New(Messages.The_assignment_equals_must_be_preceded_by_custom_function_or_variable);
        internal static void AssignmentNotFirstException() =>
            New(Messages.Assignment_equals_must_be_the_first_operator_in_the_expression);
        internal static void InvalidSyntaxException(in string s1, in string s2) =>
            New(string.Format(Messages.Invalid_syntax__0__1__, s1, s2));
        internal static void UnexpectedDelimiterException() =>
            New(Messages.Unexpected_delimiter__);
        internal static void InvalidNumberOfArgumentsException() =>
            New(Messages.Invalid_number_of_arguments);
        internal static void ResultNotRealException(in string s) =>
            New(string.Format(Messages.The_result_is_not_a_real_number__0__, s));
        internal static void MissingFunctionParameterException() =>
            New(Messages.Missing_parameter_in_function_definition);
        internal static void MissingFunctionDelimiterException() =>
            New(Messages.Missing_delimiter_in_function_definition);
        internal static void InvalidFunctionTokenException(in string name) =>
            New(string.Format(Messages.Invalid_token_in_function_definition__0__, name));
        internal static void CircularReferenceException(in string name) =>
            New(string.Format(Messages.Circular_reference_detected_for_function__0__, name));
        internal static void InvalidFunctionDefinitionException() =>
            New(Messages.InvalidFunctionDefinitionException);
        internal static void ArgumentOutOfRangeException(in string function) =>
            New(string.Format(Messages.Argument_out_of_range_for__0__x_, function));
        internal static void ConditionEmptyException() =>
            New(Messages.Condition_cannot_be_empty);
        internal static void ConditionNotInitializedException() =>
            New(Messages.Condition_block_not_initialized_with_if_);
        internal static void DuplicateElseException() =>
            New(Messages.Duplicate_else_in_condition_block);
        internal static void ElseIfAfterElseException() =>
            New(Messages.else_if_is_not_allowed_after_else_in_condition_block);
        internal static void ConditionComplexException() =>
            New(Messages.Condition_cannot_evaluate_to_a_complex_number);
        internal static void ConditionResultInvalidException(in string s) =>
            New(string.Format(Messages.Condition_result_is_invalid_0_, s));
        internal static void DuplicateMacroParametersException(in string s) =>
            New(string.Format(Messages.Duplicate_macro_parameter_names__0__and__1__, s, s));
        internal static void UndefinedMacroException(in string s) =>
            throw new ArgumentException(string.Format(Messages.Macro_not_defined__0__, s));
        internal static void MissingMapItemException(in string s) =>
            New(string.Format(Messages.Missing__0__in_surface_map_command, s));
        internal static void PlotLimitsIdenticalException() =>
            New(Messages.The_limits_of_plot_area_are_identical);
        internal static void ErrorWritingPngFileException(in string path) =>
            New(string.Format(Messages.Error_writing_a_png_file_to__0__, path));
        internal static void ErrorWritingSvgFileException(in string path) =>
            New(string.Format(Messages.Error_writing_a_svg_file_to__0__, path));
        internal static void ErrorConvertingPngToBase64Exception() =>
            New(Messages.Error_converting_png_to_Base64);
        internal static void InconsistentUnitsOperationException(in string ua, char op, in string ub) =>
            New(string.Format(Messages.Inconsistent_units__0__1__2__, ua, op, ub));
        internal static void UnitsToComplexPowerException() =>
            New(Messages.Units_cannon_be_raised_to_complex_power);
        internal static void CannotEvaluateRemainderException(in string ua, in string ub) =>
            New(string.Format(Messages.Cannot_evaluate_remainder__0__1__The_denominator_must_be_unitless_, ua, ub));
        internal static void IvalidFunctionTokenException(in string s) =>
            New(string.Format(Messages.Invalid_token_in_function_definition__0__, s));

        private static void New(in string s) =>
            throw new MathParser.MathParserException(s);

        internal static T InvalidOperator<T>(char c)
        {
            InvalidOperatorException(c);
            return default;
        }

        internal static T PowerNotUnitless<T>()
        {
            PowerNotUnitlessException();
            return default;
        }

        internal static T InvalidUnits<T>(in string s)
        {
            InvalidUnitsException(s);
            return default;
        }
    }
}
