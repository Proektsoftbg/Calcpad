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
           New($"Invalid syntax: \"{s}\".");
        internal static void IncompleteExpressionException() =>
            New("Incomplete expression.");
        internal static void MissingLeftBracketException() =>
            New("Missing left bracket \"(\".");
        internal static void MissingRightBracketException() =>
            New("Missing right bracket \")\".");
        internal static void InvalidSymbolException(char c) =>
            New($"Invalid symbol: \"{c}\".");
        internal static void InvalidUnitsException(in string s) =>
            New($"Invalid units: \"{s}\".");
        internal static void InvalidLiteralException(in string s, in string literal) =>
            New($"Cannot evaluate \"{s}\" as {literal}.");
        internal static void InvalidNumberException(in string s) =>
            InvalidLiteralException(s, "number");
        internal static void InvalidOperatorException(char c) =>
            New($"Invalid operator: \"{c}\".");
        internal static void PowerNotUnitlessException() =>
            New("Power must be unitless.");
        internal static void ResultIsNotUnitsException() =>
            New("The expression on the right does not evaluate to units.");
        internal static void CannotEvaluateFunctionException(in string s) => 
            New($"Cannot evaluate the function %F for %V = {s}.");
        internal static void FunctionNotDefinedException(in string s) =>
            New($"The function %F is not defined for %V = {s}.");
        internal static void InconsistentUnitsException(in string u1, in string u2) =>
            New($"Inconsistent units: \"{u1}\" and \"{u2}\".");
        internal static void IterationLimitsException(in string l1, in string l2) =>
            New($"Limits out of range: [{l1}; {l2}].");
        internal static void InvalidUnitsFunctionException(in string function, in string unit) =>
            New($"Invalid units for function: \"{function}({unit})\".");
        internal static void RootUnitlessException() =>
            New("Root index must be unitless.");
        internal static void RootComplexException() =>
            New("Root index cannot be complex number.");
        internal static void RootIntegerException() =>
            New("Root index must be integer > 1.");
        internal static void FactorialArgumentOutOfRangeException() =>
            New("Argument out of range for n!.");
        internal static void FactorialArgumentUnitlessException() =>
            New("The argument of n! must be unitless.");
        internal static void FactorialArgumentPositiveIntegerException() =>
            New("The argument of n! must be a positive integer.");
        internal static void FactorialArgumentComplexException() =>
            New("The argument of n! cannot be complex.");
        internal static void ReminderUnitsException(in string u1, in string u2) =>
            New($"Cannot evaluate reminder: \"{u1}  %  {u2}\". Denominator must be unitless.");
        internal static void BothValuesIntegerException() =>
            New("Both values must be integer.");
        internal static void VariableNotExistException(in string name) =>
            New($"Variable '{name}' does not exist.");
        internal static void UnitNotExistException(in string name) =>
            New($"Unit '{name}' does not exist.");
        internal static void InteruptedByUserException() =>
            New("Interupted by user.");
        internal static void CalculationsNotActiveException() =>
            New("Calculations not active.");
        internal static void ExpressionEmptyException() =>
            New("Expression is empty.");
        internal static void MissingOperandException() =>
            New("Missing operand.");
        internal static void InvalidFunctionException(in string s) =>
            New($"Invalid function: \"{s}\".");
        internal static void CannotEvaluateAsTypeException(in string literal, in string type) =>
            New($"Cannot evaluate \"{literal}\" as \"{type}\".");
        internal static void StackLeakException() =>
            New("Stack memory leak. Invalid expression.");
        internal static void StackEmptyException() =>
            New("Stack empty. Invalid expression.");
        internal static void UndefinedInputFieldException() =>
            New("Undefined input field.");
        internal static void UndefinedVariableOrUnitsException(in string s) =>
            New($"Undefined variable or units: \"{s}\".");
        internal static void ErrorEvaluatingAsFunctionException(in string s) =>
            New($"Error evaluating \"{s}\" as function.");
        internal static void ErrorEvaluatingAsFunctionOrOperatorException(in string s) =>
            New($"Error evaluating \"{s}\" as function or operator.");
        internal static void CannotRewiriteUnitsException(in string s) =>
            New($"Cannot rewirite existing units: {s}.");
        internal static void InconsistentTargetUnitsException(in string sourceUnits, in string targetUnits) =>
            New($"The calculated units \"{sourceUnits}\" are inconsistent with the target units \"{targetUnits}\".");
        internal static void InvalidCharacterException(char c) =>
            New($"Invalid character: '{c}'. Variables, functions and units must begin with a letter or ∡, except for the following units: ° ′ ″ % ‰.");
        internal static void ImproperAssignmentException() =>
            New("Improper use of the assignment operator '='.");
        internal static void MissingLeftSolverBracketException() =>
            New("Missing left bracket '{' in solver command.");
        internal static void MissingRightSolverBracketException() =>
            New("Missing right bracket '}' in solver command.");
        internal static void InvalidMacroException(in string s) =>
            New($"Invalid macro identifier: \"{s}$\".");
        internal static void InvalidSolverException(in string s) =>
            New($"Invalid solver command definition \"{s}\".");
        internal static void ErrorParsingUnitsException(in string s) =>
            New($"Error parsing \"{s}\" as units.");
        internal static void ErrorParsingNumberException(in string s) =>
            New($"Error parsing \"{s}\" as number.");
        internal static void MissingDelimiterException(char delimiter, in string script) =>
            New($"Missing delimiter \"{delimiter}\" in solver command {{{script}}}.");
        internal static void MultipleAssignmentsException(in string s) =>
            New($"More than one operators '=' in '{s}'.");
        internal static void NotConstantExpressionException(in string s) =>
            New($"The expression on the right side must be constant: \"{s}\".");
        internal static void InconsistentUnits1Exception(in string variable, in string units) =>
            New($"Inconsistent units for \"{variable} = {units}\".");
        internal static void InconsistentUnits2Exception(in string variable, in string u1, in string u2) =>
            New($"Inconsistent units for \"{variable} = {u1} : {u2}\".");
        internal static void NoSolutionException(in string s) =>
            New($"No solution for: {s}.");
        internal static void RecursionNotAllowedException(in string s) =>
            New($"Recursion is not allowed in function definition: \"{s}\".");
        internal static void AssignmentPrecededException() =>
            New("The assignment '=' must be preceded by custom function or variable.");
        internal static void AssignmentNotFirstException() =>
            New("Assignment '=' must be the first operator in the expression.");
        internal static void InvalidSyntaxException(in string s1, in string s2) =>
            New($"Invalid syntax: \"{s1} {s2}\".");
        internal static void UnexpectedDelimiterException() =>
            New("Unexpected delimiter ';'.");
        internal static void InvalidNumberOfArgumentsException() =>
            New("Invalid number of arguments.");
        internal static void ResultNotRealException(in string s) =>
            New($"The result is not a real number: \"{s}\".");
        internal static void MissingFunctionParameterException() =>
            New("Missing parameter in function definition.");
        internal static void MissingFunctionDelimiterException() =>
            New("Missing delimiter in function definition.");
        internal static void InvalidFunctionTokenException(in string name) =>
            New($"Invalid token in function definition: \"{name}\".");
        internal static void CircularReferenceException(in string name) =>
            New($"Circular reference detected for function \"{name}\".");
        internal static void InvalidFunctionDefinitionException() =>
            New("Invalid function definition. It have to match the pattern: \"f(x; y; z...) =\".");
        internal static void ArgumentOutOfRangeException(in string function) =>
            New($"Argument out of range for {function}(x).");
        internal static void ConditionEmptyException() =>
            New("Condition cannot be empty.");
        internal static void ConditionNotInitializedException() =>
            New("Condition block not initialized with \"#if\".");
        internal static void DuplicateElseException() =>
            New("Duplicate \"#else\" in condition block.");
        internal static void ElseIfAfterElseException() =>
            New("\"#else if\" is not allowed after \"#else\" in condition block.");
        internal static void ConditionComplexException() =>
            New("Condition cannot evaluate to a complex number.");
        internal static void ConditionResultInvalidException(in string s) =>
            New($"Condition result is invalid: {s}.");
        internal static void DuplicateMacroParametersException(in string s) =>
            New($"Duplicate macro parameter names: {s} and {s}.");
        internal static void UndefinedMacroException(in string s) =>
            throw new ArgumentException($"Macro not defined: {s}.");
        internal static void MissingMapItemException(in string s) =>
            New($"Missing {s} in surface map command.");
        internal static void PlotLimitsIdenticalException() =>
            New("The limits of plot area are identical.");
        internal static void ErrorWritingPngFileException(in string path) =>
            New($"Error writing a png file to \"{path}\".");
        internal static void ErrorWritingSvgFileException(in string path) =>
            New($"Error writing a svg file to \"{path}\".");
        internal static void ErrorConvertingPngToBase64Exception() =>
            New("Error converting png to Base64.");
        internal static void InconsistentUnitsOperationException(in string ua, char op, in string ub) =>
            New($"Inconsistent units: \"{ua} {op} {ub}\".");
        internal static void UnitsToComplexPowerException() =>
            New("Units cannon be raised to complex power.");
        internal static void CannotEvaluateReminderException(in string ua, in string ub) =>
            New($"Cannot evaluate reminder: \"{ua}  %  {ub}\". The denominator must be unitless.");
        internal static void IvalidFunctionTokenException(in string s) =>
            New($"Invalid token in function definition: \"{s}\".");
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
