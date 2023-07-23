using System;

namespace Calcpad.Core
{
    internal static class Throw
    {
#if BG
        internal static void InvalidSyntax(in string s) => 
            ThrowNew($"Невалиден синтаксис: \"{s}\".");
        internal static void IncompleteExpression() =>
            ThrowNew("Непълен израз.");
        internal static void MissingLeftBracket() =>
            ThrowNew("Липсва лява скоба \"(\".");
        internal static void MissingRightBracket() =>
            ThrowNew("Липсва дясна скоба \")\".");
        internal static void InvalidSymbol(char c) =>  
            ThrowNew($"Невалиден символ: \"{c}\".");
        internal static void InvalidUnits(in string s) =>    
            ThrowNew($"Невалидни мерни единици: \"{s}\".");
        internal static void InvalidLiteral(in string s, in string literal) =>
            ThrowNew($"Не мога да изчисля \"{s}\" като {literal}.");
        internal static void InvalidNumber(in string s) =>
            InvalidLiteral(s, "число");
        internal static void MissingOperand() =>
            ThrowNew("Липсва операнд.");
        internal static void InvalidOperator(char c) =>
            ThrowNew($"Невалиден оператор: \"{c}\".");
        internal static void PowerNotUnitless() =>
            ThrowNew("Степенният показател трябва да е бездименсионен.");
        internal static void ResultIsNotUnits() =>
            ThrowNew("Изразът отдясно не се изчислява до мерни единици.");
        internal static void CannotEvaluateFunction(in string s) =>
            ThrowNew($"Не мога да изчисля функцията %F за %V = {s}.");
        internal static void FunctionNotDefined(in string s) =>
            ThrowNew($"Функцията %F не е дефинирана за %V = {s}.");
        internal static void InconsistentUnits(in string u1, in string u2) =>
            ThrowNew($"Несъвместими мерни единици: \"{u1}\" и \"{u2}\".");
        internal static void IterationLimits(in string l1, in string l2) =>
            ThrowNew($"Границите са извън допустимите: [{l1}; {l2}].");
        internal static void InvalidUnitsFunction(in string func, in string unit) =>
            ThrowNew($"Невалидни мерни единици за функция: \"{func}({unit})\".");
        internal static void RootUnitless() =>
	        ThrowNew("Коренният показател трябва да е бездименсионен.");
        internal static void RootComplex() =>
	        ThrowNew("Коренният показател не може да е комплексно число.");
        internal static void RootInteger() =>
	        ThrowNew("Коренният показател трябва да е цяло число > 1.");
        internal static void FactorialArgumentOutOfRange() =>
	        ThrowNew("Аргументът e извън допустимите стойности n!.");
        internal static void FactorialArgumentUnitless() =>
	        ThrowNew("Аргументът на n! трябва да е бездименсионен.");
        internal static void FactorialArgumentPositiveInteger() =>
            ThrowNew("Аргументът на n! трябва да е цяло положително число.");
        internal static void FactorialArgumentComplex() =>
            ThrowNew("Аргументът на n! не може да е комплексно число.");
        internal static void ReminderUnits(in string u1, in string u2) =>
            ThrowNew($"Не мога да изчисля остатъка: \"{u1}  %  {u2}\". Делителя трябва да е бездименсионен.");
        internal static void BothValuesInteger() =>
            ThrowNew("Двете стойности трябва да са цели числа.");
        internal static void VariableNotExist(in string name) =>
            ThrowNew($"Недифинирана променлива '{name}'.");
        internal static void UnitNotExist(in string name) =>
            ThrowNew($"Недифинирани мерни единици '{name}'.");
        internal static void InteruptedByUser() =>
            ThrowNew("Прекъсване от потребителя.");
        internal static void CalculationsNotActive() =>
            ThrowNew("Изчислителното ядро не е активно.");
        internal static void ExpressionEmpty() =>
            ThrowNew("Изразът е празен.");
        internal static void InvalidFunction(in string s) =>
            ThrowNew($"Невалидна функция: \"{s}\".");
        internal static void CannotEvaluateAsType(in string literal, in string type) =>
            ThrowNew($"Не мога да изчисля \"{literal}\" като \"{type}\".");
        internal static void StackLeak() =>
            ThrowNew("Неосвободена памет в стека. Невалиден израз.");
        internal static void StackEmpty() => 
            ThrowNew("Стекът е празен. Невалиден израз.");
        internal static void UndefinedInputField() =>
            ThrowNew("Недефинирано поле за въвеждане.");
        internal static void UndefinedVariableOrUnits(in string s) =>
            ThrowNew($"Недефинирана променлива или мерни единици: \"{s}\".");
        internal static void ErrorEvaluatingAsFunction(in string s) =>
            ThrowNew($"Грешка при изчисляване на \"{s}\" като функция.");
        internal static void ErrorEvaluatingAsFunctionOrOperator(in string s) =>
            ThrowNew($"Грешка при изчисляване на \"{s}\" като функция или оператор.");
        internal static void CannotRewiriteUnits(in string s) =>
            ThrowNew($"Не мога да презапиша съществуващи единици: {s}.");
        internal static void InconsistentTargetUnits(in string sourceUnits, in string targetUnits) =>
            ThrowNew($"Получените мерни единици \"{sourceUnits}\" не съответстват на отправните \"{targetUnits}\".");
        internal static void InvalidCharacter(char c) =>
            ThrowNew($"Невалиден символ: '{c}'. Имената на променливи, функции и мерни единици трябва да започват с буква или ∡, освен единиците: ° ′ ″ % ‰.");
        internal static void ImproperAssignment() =>
            ThrowNew($"Неправилно използване на оператора за присвояване '='.");
        internal static void MissingLeftSolverBracket() =>
            ThrowNew("Липсва лява фигурна скоба '{' в команда за числени методи.");
        internal static void MissingRightSolverBracket() =>
            ThrowNew("Липсва дясна фигурна скоба '}' в команда за числени методи.");
        internal static void InvalidMacro(in string s) =>
            ThrowNew($"Невалидeн идентификатор на макро \"{s}$\".");
        internal static void InvalidSolver(in string s) =>
            ThrowNew($"Невалидна дефиниция на команда за числени методи \"{s}\".");
        internal static void ErrorParsingUnits(in string s) =>
            ThrowNew($"Грешка при опит за разпознаване на \"{s}\" като мерни единици.");
        internal static void ErrorParsingNumber(in string s) =>
            ThrowNew($"Грешка при опит за разпознаване на \"{s}\" като число.");
        internal static void MissingDelimiter(char delimiter, in string script) =>
            ThrowNew($"Липсва разделител \"{delimiter}\" в команда за числени методи {{{script}}}.");
        internal static void MultipleAssignments(in string s) =>
            ThrowNew($"Повече от един оператор '=' в '{s}'.");
        internal static void InconsistentUnits2(in string variable, in string u1, in string u2) =>
            ThrowNew($"Несъвместими мерни единици за \"{variable} = {u1} : {u2}\".");
        internal static void ConstantExpression(in string s) =>
            ThrowNew($"Изразът от дясната страна трябва да е константа: \"{s}\".");
        internal static void InconsistentUnits1(in string variable, in string units) =>
            ThrowNew($"Несъвместими мерни единици за \"{variable} = {units}\".");
        internal static void NoSolution(in string s) =>
            ThrowNew($"Няма решение за: {s}.");
        internal static void RecursionNotAllowed(in string s) =>
            ThrowNew($"Не e разрешена рекурсия в дефиницията на функция: \"{s}\".");
        internal static void AssignmentPreceded() =>
            ThrowNew("Преди оператора за присвояване '=' трябва да има функция или променлива.");
        internal static void AssignmentNotFirst() =>
            ThrowNew("Преди оператора за присвояване '=' не може да има други оператори.");
        internal static void InvalidSyntax(in string s1, in string s2) =>
            ThrowNew($"Невалиден синтаксис: \"{s1} {s2}\".");
        internal static void UnexpectedDelimiter() =>
            ThrowNew("Неочакван символ за разделител ';'.");
        internal static void InvalidNumberOfArguments() =>
            ThrowNew("Невалиден брой аргументи на функция.");
        internal static void ResultNotReal(in string s) =>
            ThrowNew($"Резултатът не е реално число: \"{s}\".");
        internal static void MissingFunctionParameter() =>
            ThrowNew("Липсва параметър в дефиниция на функция.");
        internal static void MissingFunctionDelimiter() =>
            ThrowNew("Липсва разделител в дефиниция на функция.");
        internal static void InvalidFunctionToken(in string name) =>
            ThrowNew($"Невалиден обект в дефиниция на функция: \"{name}\".");
        internal static void CircularReference(in string name) =>
            ThrowNew($"Открита е циклична референция за функция \"{name}\".");
        internal static void InvalidFunctionDefinition() =>
            ThrowNew("Невалидна дефиниция на функция. Трябва да съответства на шаблона: \"f(x; y; z...) =\".");
        internal static void ArgumentOutOfRange(in string func) =>
            ThrowNew($"Аргументът е извън допустимия интервал за {func}(x).");
        internal static void ConditionEmpty() =>
            ThrowNew("Условието не може да бъде празно.");
        internal static void ConditionNotInitialized() =>
	        ThrowNew("Условният блок не е инициализиран с \"#if\".");                   
        internal static void DuplicateElse() =>
	        ThrowNew("Може да има само едно \"#else\" в условен блок.");                         
        internal static void ElseIfAfterElse() =>
	        ThrowNew("Не може да има \"#else if\" след \"#else\" в условен блок.");
        internal static void ConditionCoplex() =>               
	        ThrowNew("Условието не може да бъде комплексно число.");                  
        internal static void ConditionResultInvalid(in string s) =>
	        ThrowNew($"Невалиден резултат от проверка на условие: {s}.");
        internal static void ConditionComplex() =>
            ThrowNew("Условието не може да бъде комплексно число.");
        internal static void DuplicateMacroParameters(in string s) =>
            ThrowNew($"Дублиране на имената на параметрите на макрос: {s} и {s}.");
        internal static void UndefinedMacro(in string s) =>
            throw new ArgumentException($"Недефиниран макрос: {s}.");
        internal static void MissingMapItem(in string s) =>
            ThrowNew($"Липсва {s} в команда за 2D графика.");
        internal static void PlotLimitsIdentical() =>
            ThrowNew($"Границите на чертожната площ съвпадат.");
        internal static void ErrorWritingPngFile(in string path) =>
            ThrowNew($"Грешка при запис на png файл като \"{path}\".");
        internal static void ErrorWritingSvgFile(in string path) =>
            ThrowNew($"Грешка при запис на svg файл като \"{path}\".");
        internal static void ErrorConvertingPngToBase64() =>
            ThrowNew("Грешка при конвертиране на png към Base64.");
        internal static void InconsistentUnitsOp(in string ua, char op, in string ub) =>
            ThrowNew($"Несъвместими мерни единици: \"{ua} {op} {ub}\".");
        internal static void UnitsToComplexPower() =>
            ThrowNew("Не мога да повдигна мерни единици на комплексна степен.");
        internal static void CannotEvaluateReminder(in string ua, in string ub) =>
            ThrowNew($"Не мога да изчисля остатъка: \"{ua}  %  {ub}\". Делителят трябва да е бездименсионен.");
        internal static void IvalidFunctionToken(in string s) =>
            ThrowNew($"Невалиден обект в дефиниция на функция: \"{s}\".");
#else
        internal static void InvalidSyntax(in string s) =>
           ThrowNew($"Invalid syntax: \"{s}\".");
        internal static void IncompleteExpression() =>
            ThrowNew("Incomplete expression.");
        internal static void MissingLeftBracket() =>
            ThrowNew("Missing left bracket \"(\".");
        internal static void MissingRightBracket() =>
            ThrowNew("Missing right bracket \")\".");
        internal static void InvalidSymbol(char c) =>
            ThrowNew($"Invalid symbol: \"{c}\".");
        internal static void InvalidUnits(in string s) =>
            ThrowNew($"Invalid units: \"{s}\".");
        internal static void InvalidLiteral(in string s, in string literal) =>
            ThrowNew($"Cannot evaluate \"{s}\" as {literal}.");
        internal static void InvalidNumber(in string s) =>
            InvalidLiteral(s, "number");
        internal static void InvalidOperator(char c) =>
            ThrowNew($"Invalid operator: \"{c}\".");
        internal static void PowerNotUnitless() =>
            ThrowNew("Power must be unitless.");
        internal static void ResultIsNotUnits() =>
            ThrowNew("The expression on the right does not evaluate to units.");
        internal static void CannotEvaluateFunction(in string s) => 
            ThrowNew($"Cannot evaluate the function %F for %V = {s}.");
        internal static void FunctionNotDefined(in string s) =>
            ThrowNew($"The function %F is not defined for %V = {s}.");
        internal static void InconsistentUnits(in string u1, in string u2) =>
            ThrowNew($"Inconsistent units: \"{u1}\" and \"{u2}\".");
        internal static void IterationLimits(in string l1, in string l2) =>
            ThrowNew($"Limits out of range: [{l1}; {l2}].");
        internal static void InvalidUnitsFunction(in string func, in string unit) =>
            ThrowNew($"Invalid units for function: \"{func}({unit})\".");
        internal static void RootUnitless() =>
            ThrowNew("Root index must be unitless.");
        internal static void RootComplex() =>
            ThrowNew("Root index cannot be complex number.");
        internal static void RootInteger() =>
            ThrowNew("Root index must be integer > 1.");
        internal static void FactorialArgumentOutOfRange() =>
            ThrowNew("Argument out of range for n!.");
        internal static void FactorialArgumentUnitless() =>
            ThrowNew("The argument of n! must be unitless.");
        internal static void FactorialArgumentPositiveInteger() =>
            ThrowNew("The argument of n! must be a positive integer.");
        internal static void FactorialArgumentComplex() =>
            ThrowNew("The argument of n! cannot be complex.");
        internal static void ReminderUnits(in string u1, in string u2) =>
            ThrowNew($"Cannot evaluate reminder: \"{u1}  %  {u2}\". Denominator must be unitless.");
        internal static void BothValuesInteger() =>
            ThrowNew("Both values must be integer.");
        internal static void VariableNotExist(in string name) =>
            ThrowNew($"Variable '{name}' does not exist.");
        internal static void UnitNotExist(in string name) =>
            ThrowNew($"Unit '{name}' does not exist.");
        internal static void InteruptedByUser() =>
            ThrowNew("Interupted by user.");
        internal static void CalculationsNotActive() =>
            ThrowNew("Calculations not active.");
        internal static void ExpressionEmpty() =>
            ThrowNew("Expression is empty.");
        internal static void MissingOperand() =>
            ThrowNew("Missing operand.");
        internal static void InvalidFunction(in string s) =>
            ThrowNew($"Invalid function: \"{s}\".");
        internal static void CannotEvaluateAsType(in string literal, in string type) =>
            ThrowNew($"Cannot evaluate \"{literal}\" as \"{type}\".");
        internal static void StackLeak() =>
            ThrowNew("Stack memory leak. Invalid expression.");
        internal static void StackEmpty() =>
            ThrowNew("Stack empty. Invalid expression.");
        internal static void UndefinedInputField() =>
            ThrowNew("Undefined input field.");
        internal static void UndefinedVariableOrUnits(in string s) =>
            ThrowNew($"Undefined variable or units: \"{s}\".");
        internal static void ErrorEvaluatingAsFunction(in string s) =>
            ThrowNew($"Error evaluating \"{s}\" as function.");
        internal static void ErrorEvaluatingAsFunctionOrOperator(in string s) =>
            ThrowNew($"Error evaluating \"{s}\" as function or operator.");
        internal static void CannotRewiriteUnits(in string s) =>
            ThrowNew($"Cannot rewirite existing units: {s}.");
        internal static void InconsistentTargetUnits(in string sourceUnits, in string targetUnits) =>
            ThrowNew($"The calculated units \"{sourceUnits}\" are inconsistent with the target units \"{targetUnits}\".");
        internal static void InvalidCharacter(char c) =>
            ThrowNew($"Invalid character: '{c}'. Variables, functions and units must begin with a letter or ∡, except for the following units: ° ′ ″ % ‰.");
        internal static void ImproperAssignment() =>
            ThrowNew($"Improper use of the assignment operator '='.");
        internal static void MissingLeftSolverBracket() =>
            ThrowNew("Missing left bracket '{' in solver command.");
        internal static void MissingRightSolverBracket() =>
            ThrowNew("Missing right bracket '}' in solver command.");
        internal static void InvalidMacro(in string s) =>
            ThrowNew($"Invalid macro identifier: \"{s}$\".");
        internal static void InvalidSolver(in string s) =>
            ThrowNew($"Invalid solver command definition \"{s}\".");
        internal static void ErrorParsingUnits(in string s) =>
            ThrowNew($"Error parsing \"{s}\" as units.");
        internal static void ErrorParsingNumber(in string s) =>
            ThrowNew($"Error parsing \"{s}\" as number.");
        internal static void MissingDelimiter(char delimiter, in string script) =>
            ThrowNew($"Missing delimiter \"{delimiter}\" in solver command {{{script}}}.");
        internal static void MultipleAssignments(in string s) =>
            ThrowNew($"More than one operators '=' in '{s}'.");
        internal static void InconsistentUnits2(in string variable, in string u1, in string u2) =>
            ThrowNew($"Inconsistent units for \"{variable} = {u1} : {u2}\".");
        internal static void ConstantExpression(in string s) =>
            ThrowNew($"The expression on the right side must be constant: \"{s}\".");
        internal static void InconsistentUnits1(in string variable, in string units) =>
            ThrowNew($"Inconsistent units for \"{variable} = {units}\".");
        internal static void NoSolution(in string s) =>
            ThrowNew($"No solution for: {s}.");
        internal static void RecursionNotAllowed(in string s) =>
            ThrowNew($"Recursion is not allowed in function definition: \"{s}\".");
        internal static void AssignmentPreceded() =>
            ThrowNew("The assignment '=' must be preceded by custom function or variable.");
        internal static void AssignmentNotFirst() =>
            ThrowNew("Assignment '=' must be the first operator in the expression.");
        internal static void InvalidSyntax(in string s1, in string s2) =>
            ThrowNew($"Invalid syntax: \"{s1} {s2}\".");
        internal static void UnexpectedDelimiter() =>
            ThrowNew("Unexpected delimiter ';'.");
        internal static void InvalidNumberOfArguments() =>
            ThrowNew("Invalid number of arguments.");
        internal static void ResultNotReal(in string s) =>
            ThrowNew($"The result is not a real number: \"{s}\".");
        internal static void MissingFunctionParameter() =>
            ThrowNew("Missing parameter in function definition.");
        internal static void MissingFunctionDelimiter() =>
            ThrowNew("Missing delimiter in function definition.");
        internal static void InvalidFunctionToken(in string name) =>
            ThrowNew($"Invalid token in function definition: \"{name}\".");
        internal static void CircularReference(in string name) =>
            ThrowNew($"Circular reference detected for function \"{name}\".");
        internal static void InvalidFunctionDefinition() =>
            ThrowNew("Invalid function definition. It have to match the pattern: \"f(x; y; z...) =\".");
        internal static void ArgumentOutOfRange(in string func) =>
            ThrowNew($"Argument out of range for {func}(x).");
        internal static void ConditionEmpty() =>
            ThrowNew("Condition cannot be empty.");
        internal static void ConditionNotInitialized() =>
            ThrowNew("Condition block not initialized with \"#if\".");
        internal static void DuplicateElse() =>
            ThrowNew("Duplicate \"#else\" in condition block.");
        internal static void ElseIfAfterElse() =>
            ThrowNew("\"#else if\" is not allowed after \"#else\" in condition block.");
        internal static void ConditionCoplex() =>
            ThrowNew("Condition cannot evaluate to a complex number.");
        internal static void ConditionResultInvalid(in string s) =>
            ThrowNew($"Condition result is invalid: {s}.");
        internal static void ConditionComplex() =>
            ThrowNew("Condition cannot evaluate to a complex number.");
        internal static void DuplicateMacroParameters(in string s) =>
            ThrowNew($"Duplicate macro parameter names: {s} and {s}.");
        internal static void UndefinedMacro(in string s) =>
            throw new ArgumentException($"Macro not defined: {s}.");
        internal static void MissingMapItem(in string s) =>
            ThrowNew($"Missing {s} in surface map command.");
        internal static void PlotLimitsIdentical() =>
            ThrowNew($"The limits of plot area are identical.");
        internal static void ErrorWritingPngFile(in string path) =>
            ThrowNew($"Error writing a png file to \"{path}\".");
        internal static void ErrorWritingSvgFile(in string path) =>
            ThrowNew($"Error writing a svg file to \"{path}\".");
        internal static void ErrorConvertingPngToBase64() =>
            ThrowNew("Error converting png to Base64.");
        internal static void InconsistentUnitsOp(in string ua, char op, in string ub) =>
            ThrowNew($"Inconsistent units: \"{ua} {op} {ub}\".");
        internal static void UnitsToComplexPower() =>
            ThrowNew("Units cannon be raised to complex power.");
        internal static void CannotEvaluateReminder(in string ua, in string ub) =>
            ThrowNew($"Cannot evaluate reminder: \"{ua}  %  {ub}\". The denominator must be unitless.");
        internal static void IvalidFunctionToken(in string s) =>
            ThrowNew($"Invalid token in function definition: \"{s}\".");
#endif

        private static void ThrowNew(in string s) =>
            throw new MathParser.MathParserException(s);

        internal static T InvalidOperator<T>(char c)
        {
            InvalidOperator(c);
            return default;
        }

        internal static T PowerNotUnitless<T>()
        {
            PowerNotUnitless();
            return default;
        }

        internal static T InvalidUnits<T>(in string s)
        {
            InvalidUnits(s);
            return default;
        }
    }
}
