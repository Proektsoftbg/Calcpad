using System;

namespace Calcpad.Core
{
    internal static class Throw
    {
#if BG
        internal static void InvalidSyntaxException(in string s) => 
            New($"Невалиден синтаксис: \"{s}\".");
        internal static void IncompleteExpressionException() =>
            New("Непълен израз.");
        internal static void MissingLeftBracketException() =>
            New("Липсва лява скоба \"(\".");
        internal static void MissingRightBracketException() =>
            New("Липсва дясна скоба \")\".");
        internal static void InvalidSymbolException(char c) =>  
            New($"Невалиден символ: \"{c}\".");
        internal static void InvalidUnitsException(in string s) =>    
            New($"Невалидни мерни единици: \"{s}\".");
        internal static void InvalidLiteralException(in string s, in string literal) =>
            New($"Не мога да изчисля \"{s}\" като {literal}.");
        internal static void InvalidNumberException(in string s) =>
            InvalidLiteralException(s, "число");
        internal static void MissingOperandException() =>
            New("Липсва операнд.");
        internal static void InvalidOperatorException(char c) =>
            New($"Невалиден оператор: \"{c}\".");
        internal static void PowerNotUnitlessException() =>
            New("Степенният показател трябва да е бездименсионен.");
        internal static void ResultIsNotUnitsException() =>
            New("Изразът отдясно не се изчислява до мерни единици.");
        internal static void CannotEvaluateFunctionException(in string s) =>
            New($"Не мога да изчисля функцията %F за %V = {s}.");
        internal static void FunctionNotDefinedException(in string s) =>
            New($"Функцията %F не е дефинирана за %V = {s}.");
        internal static void InconsistentUnitsException(in string u1, in string u2) =>
            New($"Несъвместими мерни единици: \"{u1}\" и \"{u2}\".");
        internal static void IterationLimitsException(in string l1, in string l2) =>
            New($"Границите са извън допустимите: [{l1}; {l2}].");
        internal static void InvalidUnitsFunctionException(in string function, in string unit) =>
            New($"Невалидни мерни единици за функция: \"{function}({unit})\".");
        internal static void RootUnitlessException() =>
	        New("Коренният показател трябва да е бездименсионен.");
        internal static void RootComplexException() =>
	        New("Коренният показател не може да е комплексно число.");
        internal static void RootIntegerException() =>
	        New("Коренният показател трябва да е цяло число > 1.");
        internal static void FactorialArgumentOutOfRangeException() =>
	        New("Аргументът e извън допустимите стойности n!.");
        internal static void FactorialArgumentUnitlessException() =>
	        New("Аргументът на n! трябва да е бездименсионен.");
        internal static void FactorialArgumentPositiveIntegerException() =>
            New("Аргументът на n! трябва да е цяло положително число.");
        internal static void FactorialArgumentComplexException() =>
            New("Аргументът на n! не може да е комплексно число.");
        internal static void ReminderUnitsException(in string u1, in string u2) =>
            New($"Не мога да изчисля остатъка: \"{u1}  %  {u2}\". Делителя трябва да е бездименсионен.");
        internal static void BothValuesIntegerException() =>
            New("Двете стойности трябва да са цели числа.");
        internal static void VariableNotExistException(in string name) =>
            New($"Недифинирана променлива '{name}'.");
        internal static void UnitNotExistException(in string name) =>
            New($"Недифинирани мерни единици '{name}'.");
        internal static void InteruptedByUserException() =>
            New("Прекъсване от потребителя.");
        internal static void CalculationsNotActiveException() =>
            New("Изчислителното ядро не е активно.");
        internal static void ExpressionEmptyException() =>
            New("Изразът е празен.");
        internal static void InvalidFunctionException(in string s) =>
            New($"Невалидна функция: \"{s}\".");
        internal static void CannotEvaluateAsTypeException(in string literal, in string type) =>
            New($"Не мога да изчисля \"{literal}\" като \"{type}\".");
        internal static void StackLeakException() =>
            New("Неосвободена памет в стека. Невалиден израз.");
        internal static void StackEmptyException() => 
            New("Стекът е празен. Невалиден израз.");
        internal static void UndefinedInputFieldException() =>
            New("Недефинирано поле за въвеждане.");
        internal static void UndefinedVariableOrUnitsException(in string s) =>
            New($"Недефинирана променлива или мерни единици: \"{s}\".");
        internal static void ErrorEvaluatingAsFunctionException(in string s) =>
            New($"Грешка при изчисляване на \"{s}\" като функция.");
        internal static void ErrorEvaluatingAsFunctionOrOperatorException(in string s) =>
            New($"Грешка при изчисляване на \"{s}\" като функция или оператор.");
        internal static void CannotRewiriteUnitsException(in string s) =>
            New($"Не мога да презапиша съществуващи единици: {s}.");
        internal static void InconsistentTargetUnitsException(in string sourceUnits, in string targetUnits) =>
            New($"Получените мерни единици \"{sourceUnits}\" не съответстват на отправните \"{targetUnits}\".");
        internal static void InvalidCharacterException(char c) =>
            New($"Невалиден символ: '{c}'. Имената на променливи, функции и мерни единици трябва да започват с буква или ∡, освен единиците: ° ′ ″ % ‰.");
        internal static void ImproperAssignmentException() =>
            New("Неправилно използване на оператора за присвояване '='.");
        internal static void MissingLeftSolverBracketException() =>
            New("Липсва лява фигурна скоба '{' в команда за числени методи.");
        internal static void MissingRightSolverBracketException() =>
            New("Липсва дясна фигурна скоба '}' в команда за числени методи.");
        internal static void InvalidMacroException(in string s) =>
            New($"Невалидeн идентификатор на макро \"{s}$\".");
        internal static void InvalidSolverException(in string s) =>
            New($"Невалидна дефиниция на команда за числени методи \"{s}\".");
        internal static void ErrorParsingUnitsException(in string s) =>
            New($"Грешка при опит за разпознаване на \"{s}\" като мерни единици.");
        internal static void ErrorParsingNumberException(in string s) =>
            New($"Грешка при опит за разпознаване на \"{s}\" като число.");
        internal static void MissingDelimiterException(char delimiter, in string script) =>
            New($"Липсва разделител \"{delimiter}\" в команда за числени методи {{{script}}}.");
        internal static void MultipleAssignmentsException(in string s) =>
            New($"Повече от един оператор '=' в '{s}'.");
        internal static void NotConstantExpressionException(in string s) =>
            New($"Изразът от дясната страна трябва да е константа: \"{s}\".");
        internal static void InconsistentUnits1Exception(in string variable, in string units) =>
            New($"Несъвместими мерни единици за \"{variable} = {units}\".");
        internal static void InconsistentUnits2Exception(in string variable, in string u1, in string u2) =>
            New($"Несъвместими мерни единици за \"{variable} = {u1} : {u2}\".");
        internal static void NoSolutionException(in string s) =>
            New($"Няма решение за: {s}.");
        internal static void RecursionNotAllowedException(in string s) =>
            New($"Не e разрешена рекурсия в дефиницията на функция: \"{s}\".");
        internal static void AssignmentPrecededException() =>
            New("Преди оператора за присвояване '=' трябва да има функция или променлива.");
        internal static void AssignmentNotFirstException() =>
            New("Преди оператора за присвояване '=' не може да има други оператори.");
        internal static void InvalidSyntaxException(in string s1, in string s2) =>
            New($"Невалиден синтаксис: \"{s1} {s2}\".");
        internal static void UnexpectedDelimiterException() =>
            New("Неочакван символ за разделител ';'.");
        internal static void InvalidNumberOfArgumentsException() =>
            New("Невалиден брой аргументи на функция.");
        internal static void ResultNotRealException(in string s) =>
            New($"Резултатът не е реално число: \"{s}\".");
        internal static void MissingFunctionParameterException() =>
            New("Липсва параметър в дефиниция на функция.");
        internal static void MissingFunctionDelimiterException() =>
            New("Липсва разделител в дефиниция на функция.");
        internal static void InvalidFunctionTokenException(in string name) =>
            New($"Невалиден обект в дефиниция на функция: \"{name}\".");
        internal static void CircularReferenceException(in string name) =>
            New($"Открита е циклична референция за функция \"{name}\".");
        internal static void InvalidFunctionDefinitionException() =>
            New("Невалидна дефиниция на функция. Трябва да съответства на шаблона: \"f(x; y; z...) =\".");
        internal static void ArgumentOutOfRangeException(in string function) =>
            New($"Аргументът е извън допустимия интервал за {function}(x).");
        internal static void ConditionEmptyException() =>
            New("Условието не може да бъде празно.");
        internal static void ConditionNotInitializedException() =>
	        New("Условният блок не е инициализиран с \"#if\".");                   
        internal static void DuplicateElseException() =>
	        New("Може да има само едно \"#else\" в условен блок.");                         
        internal static void ElseIfAfterElseException() =>
	        New("Не може да има \"#else if\" след \"#else\" в условен блок.");
        internal static void ConditionComplexException() =>               
	        New("Условието не може да бъде комплексно число.");                  
        internal static void ConditionResultInvalidException(in string s) =>
	        New($"Невалиден резултат от проверка на условие: {s}.");
        internal static void DuplicateMacroParametersException(in string s) =>
            New($"Дублиране на имената на параметрите на макрос: {s} и {s}.");
        internal static void UndefinedMacroException(in string s) =>
            throw new ArgumentException($"Недефиниран макрос: {s}.");
        internal static void MissingMapItemException(in string s) =>
            New($"Липсва {s} в команда за 2D графика.");
        internal static void PlotLimitsIdenticalException() =>
            New("Границите на чертожната площ съвпадат.");
        internal static void ErrorWritingPngFileException(in string path) =>
            New($"Грешка при запис на png файл като \"{path}\".");
        internal static void ErrorWritingSvgFileException(in string path) =>
            New($"Грешка при запис на svg файл като \"{path}\".");
        internal static void ErrorConvertingPngToBase64Exception() =>
            New("Грешка при конвертиране на png към Base64.");
        internal static void InconsistentUnitsOperationException(in string ua, char op, in string ub) =>
            New($"Несъвместими мерни единици: \"{ua} {op} {ub}\".");
        internal static void UnitsToComplexPowerException() =>
            New("Не мога да повдигна мерни единици на комплексна степен.");
        internal static void CannotEvaluateReminderException(in string ua, in string ub) =>
            New($"Не мога да изчисля остатъка: \"{ua}  %  {ub}\". Делителят трябва да е бездименсионен.");
        internal static void IvalidFunctionTokenException(in string s) =>
            New($"Невалиден обект в дефиниция на функция: \"{s}\".");
#else
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
        internal static void ReminderUnitsException(in string u1, in string u2) =>
            New(string.Format(Messages.Cannot_evaluate_reminder__0__1__Denominator_must_be_unitless, u1, u2));
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
        internal static void CannotEvaluateReminderException(in string ua, in string ub) =>
            New(string.Format(Messages.Cannot_evaluate_reminder__0__1__The_denominator_must_be_unitless_, ua, ub));
        internal static void IvalidFunctionTokenException(in string s) =>
            New(string.Format(Messages.Invalid_token_in_function_definition__0__, s));
#endif

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
