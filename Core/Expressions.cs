using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AlgorithmsLab3.Core;

/// <summary>
/// Шаг алгоритма сортировочной станции (для пошаговой демонстрации).
/// </summary>
public sealed record ShuntingYardStep(
    int StepNumber,
    string Token,
    string Action,
    string StackState,
    string OutputState
);

/// <summary>
/// Шаг вычисления постфиксного выражения (в точности воспроизводит рисунок 1 из ТЗ).
/// </summary>
public sealed record EvaluationStep(
    int StepNumber,
    string Token,
    string Action,
    string StackState
);

/// <summary>
/// Модуль для работы с арифметическими выражениями:
/// 1. Перевод инфиксной записи в постфиксную алгоритмом Дейкстры (Shunting-Yard) НА СОБСТВЕННОМ СТЕКЕ.
/// 2. Вычисление постфиксной записи на собственном стеке с пошаговой иллюстрацией (по ТЗ).
/// Поддерживаемые операции: +, -, *, /, :, ^, ln, cos, sin, sqrt, скобки (, ).
/// </summary>
public static class Expressions
{
    private static readonly Regex TokenRegex = new(
        @"\G\s*(?:(?<num>\d+(?:[.,]\d+)?)|(?<func>[a-zA-Z]+)|(?<op>[+\-*/:^()]))",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Проверка, является ли токен именем математической функции.
    /// </summary>
    public static bool IsFunction(string token) =>
        token is "ln" or "cos" or "sin" or "sqrt";

    /// <summary>
    /// Приоритет операций.
    /// </summary>
    private static int GetPrecedence(string op) => op switch
    {
        "ln" or "cos" or "sin" or "sqrt" => 5,
        "neg" => 4, // унарный минус
        "^" => 3,
        "*" or "/" or ":" => 2,
        "+" or "-" => 1,
        _ => 0
    };

    /// <summary>
    /// Правоассоциативность операции (степень ^ и унарный минус neg правоассоциативны).
    /// </summary>
    private static bool IsRightAssociative(string op) =>
        op is "^" or "neg";

    /// <summary>
    /// Лексический анализ: разбиение инфиксной строки на токены с распознаванием унарного минуса.
    /// </summary>
    public static List<string> TokenizeInfix(string text)
    {
        var tokens = new List<string>();
        int pos = 0;

        while (pos < text.Length)
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
            {
                pos++;
            }

            if (pos >= text.Length) break;

            var match = TokenRegex.Match(text, pos);
            if (!match.Success)
            {
                throw new FormatException($"Некорректный символ '{text[pos]}' в позиции {pos + 1}.");
            }

            string token;
            if (match.Groups["num"].Success)
            {
                token = match.Groups["num"].Value.Replace(',', '.');
            }
            else if (match.Groups["func"].Success)
            {
                token = match.Groups["func"].Value.ToLowerInvariant();
            }
            else
            {
                token = match.Groups["op"].Value;
            }

            tokens.Add(token);
            pos = match.Index + match.Length;
        }

        // Вторичный проход: определение унарного минуса
        var normalized = new List<string>();
        for (int i = 0; i < tokens.Count; i++)
        {
            string t = tokens[i];
            if (t == "-")
            {
                // Минус унарный, если он в начале выражения или идет после открывающей скобки или другого оператора
                bool isUnary = i == 0 || tokens[i - 1] is "(" or "+" or "-" or "*" or "/" or ":" or "^";
                normalized.Add(isUnary ? "neg" : "-");
            }
            else if (t == "+")
            {
                bool isUnary = i == 0 || tokens[i - 1] is "(" or "+" or "-" or "*" or "/" or ":" or "^";
                if (!isUnary)
                {
                    normalized.Add("+");
                }
                // Унарный плюс просто опускаем
            }
            else
            {
                normalized.Add(t);
            }
        }

        return normalized;
    }

    /// <summary>
    /// Перевод инфиксной записи в постфиксную алгоритмом сортировочной станции (Shunting-Yard)
    /// С ИСПОЛЬЗОВАНИЕМ СОБСТВЕННОГО СТЕКА LinkedStack.
    /// </summary>
    public static string ToPostfix(string infix, out List<ShuntingYardStep> steps)
    {
        var tokens = TokenizeInfix(infix);
        var operatorStack = new LinkedStack<string>();
        var output = new List<string>();
        var stepList = new List<ShuntingYardStep>();
        int stepCount = 1;

        void RecordStep(string token, string action)
        {
            string stackStr = operatorStack.isEmpty() ? "∅" : string.Join(" ", operatorStack.Values().Reverse());
            string outStr = output.Count == 0 ? "∅" : string.Join(" ", output);
            stepList.Add(new ShuntingYardStep(stepCount++, token, action, stackStr, outStr));
        }

        foreach (var token in tokens)
        {
            // 1. Число -> сразу в выходной поток
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                output.Add(token);
                RecordStep(token, "Число -> в выходную строку");
            }
            // 2. Функция -> в стек операторов
            else if (IsFunction(token))
            {
                operatorStack.Push(token);
                RecordStep(token, $"Функция '{token}' -> в стек");
            }
            // 3. Открывающая скобка -> в стек
            else if (token == "(")
            {
                operatorStack.Push(token);
                RecordStep(token, "Открывающая скобка '(' -> в стек");
            }
            // 4. Закрывающая скобка -> выталкиваем из стека до '('
            else if (token == ")")
            {
                bool foundLeftParen = false;
                while (!operatorStack.isEmpty())
                {
                    string top = operatorStack.Pop();
                    if (top == "(")
                    {
                        foundLeftParen = true;
                        break;
                    }
                    output.Add(top);
                }

                if (!foundLeftParen)
                {
                    throw new FormatException("Несогласованные скобки: отсутствует открывающая '('");
                }

                // Если перед скобкой была функция, перемещаем её в выход
                if (!operatorStack.isEmpty() && IsFunction(operatorStack.Top()))
                {
                    output.Add(operatorStack.Pop());
                }

                RecordStep(token, "Закрывающая скобка ')' -> вытолкнули операторы до '('");
            }
            // 5. Операторы (+, -, *, /, :, ^, neg)
            else if (GetPrecedence(token) > 0)
            {
                int currPrec = GetPrecedence(token);
                bool rightAssoc = IsRightAssociative(token);

                while (!operatorStack.isEmpty() && operatorStack.Top() != "(")
                {
                    string top = operatorStack.Top();
                    int topPrec = GetPrecedence(top);

                    if ((!rightAssoc && topPrec >= currPrec) || (rightAssoc && topPrec > currPrec))
                    {
                        output.Add(operatorStack.Pop());
                    }
                    else
                    {
                        break;
                    }
                }

                operatorStack.Push(token);
                RecordStep(token, $"Оператор '{token}' -> помещен в стек");
            }
            else
            {
                throw new FormatException($"Неизвестный токен: '{token}'");
            }
        }

        // Выталкиваем оставшиеся операторы из стека
        while (!operatorStack.isEmpty())
        {
            string top = operatorStack.Pop();
            if (top is "(" or ")")
            {
                throw new FormatException("Несогласованные скобки в выражении.");
            }
            output.Add(top);
            RecordStep(top, "Остаток стека -> перенос в выходную строку");
        }

        steps = stepList;
        return string.Join(" ", output);
    }

    /// <summary>
    /// Перевод в постфиксную запись (упрощенная перегрузка без сбора шагов).
    /// </summary>
    public static string ToPostfix(string infix) => ToPostfix(infix, out _);

    /// <summary>
    /// Вычисление постфиксного выражения на собственном стеке LinkedStack.
    /// Сбор шагов вычисления в точности соответствует рисунку 1 (image1.png) из ТЗ!
    /// </summary>
    public static double Evaluate(string postfixText, out List<EvaluationStep> steps)
    {
        var stack = new LinkedStack<double>();
        var stepList = new List<EvaluationStep>();
        int stepNumber = 1;

        var tokens = postfixText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new FormatException("Пустое постфиксное выражение.");
        }

        void RecordEvalStep(string token, string action)
        {
            string stackStr = stack.isEmpty()
                ? "[ пусто ]"
                : "[ " + string.Join(", ", stack.Values().Reverse()) + " ]"; // снизу вверх
            stepList.Add(new EvaluationStep(stepNumber++, token, action, stackStr));
        }

        foreach (var raw in tokens)
        {
            string token = raw.ToLowerInvariant();

            // 1. Операнд (число)
            if (double.TryParse(token.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
            {
                if (!double.IsFinite(num))
                {
                    throw new FormatException($"Число '{token}' не является конечным.");
                }
                stack.Push(num);
                RecordEvalStep(token, $"Push({num.ToString(CultureInfo.InvariantCulture)})");
                continue;
            }

            // 2. Унарная операция / функция
            if (token is "ln" or "cos" or "sin" or "sqrt" or "neg")
            {
                if (stack.isEmpty())
                {
                    throw new InvalidOperationException($"Недостаточно аргументов в стеке для функции '{token}'.");
                }

                double arg = stack.Pop();
                double result = token switch
                {
                    "ln" when arg > 0 => Math.Log(arg),
                    "ln" => throw new ArithmeticException($"Аргумент ln({arg}) должен быть строго больше нуля."),
                    "sqrt" when arg >= 0 => Math.Sqrt(arg),
                    "sqrt" => throw new ArithmeticException($"Аргумент sqrt({arg}) не может быть отрицательным."),
                    "cos" => Math.Cos(arg),
                    "sin" => Math.Sin(arg),
                    "neg" => -arg,
                    _ => throw new InvalidOperationException($"Неизвестная функция '{token}'.")
                };

                if (!double.IsFinite(result))
                {
                    throw new ArithmeticException($"Результат функции {token}({arg}) не является конечным числом.");
                }

                stack.Push(result);
                RecordEvalStep(token, $"{token}({arg:G6}) => Push({result:G6})");
                continue;
            }

            // 3. Бинарная операция (+, -, *, /, :, ^)
            if (token is "+" or "-" or "*" or "/" or ":" or "^")
            {
                if (stack.Count < 2)
                {
                    throw new InvalidOperationException($"Недостаточно операндов в стеке для бинарной операции '{token}'.");
                }

                double b = stack.Pop();
                double a = stack.Pop();

                double res = token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" or ":" when Math.Abs(b) < 1e-15 => throw new DivideByZeroException("Деление на ноль."),
                    "/" or ":" => a / b,
                    "^" => Math.Pow(a, b),
                    _ => throw new InvalidOperationException($"Неизвестный оператор '{token}'.")
                };

                if (!double.IsFinite(res))
                {
                    throw new ArithmeticException($"Результат операции '{a} {token} {b}' переполнен или не является числом.");
                }

                stack.Push(res);
                RecordEvalStep(token, $"{a:G6} {token} {b:G6} = {res:G6} => Push({res:G6})");
                continue;
            }

            throw new FormatException($"Неизвестный токен в постфиксном выражении: '{raw}'");
        }

        if (stack.Count != 1)
        {
            throw new FormatException($"Выражение некорректно: в стеке осталось {stack.Count} значений вместо одного.");
        }

        steps = stepList;
        return stack.Pop();
    }

    /// <summary>
    /// Вычисление постфиксной записи без сбора шагов.
    /// </summary>
    public static double Evaluate(string postfixText) => Evaluate(postfixText, out _);

    /// <summary>
    /// Расчет и обоснование оценки алгоритмической сложности (по требованию ТЗ).
    /// </summary>
    public static string GetComplexityAnalysis()
    {
        return """
        ================================================================================
        РАСЧЕТ И ОЦЕНКА СЛОЖНОСТИ АЛГОРИТМА ВЫЧИСЛЕНИЯ ПОСТФИКСНОЙ ЗАПИСИ
        ================================================================================
        
        1. ВРЕМЕННАЯ СЛОЖНОСТЬ (Time Complexity):
           Пусть n — количество токенов (чисел и знаков операций) в постфиксном выражении.
           - Лексический разбор строки: O(n) шагов.
           - Для каждого токена выполняется:
             * Если число: операция Push(elem) в связный стек -> O(1).
             * Если унарная операция: 1 Pop() + 1 вычисление + 1 Push() -> O(1).
             * Если бинарная операция: 2 Pop() + 1 операция + 1 Push() -> O(1).
           Каждый операнд попадает в стек ровно 1 раз и извлекается ровно 1 раз.
           Итоговая временная сложность: T(n) = O(n) — строго линейная.
        
        2. ПРОСТРАНСТВЕННАЯ СЛОЖНОСТЬ (Space Complexity):
           - Связный стек хранит одновременно максимум число операндов.
           - В худшем случае (все операнды записаны подряд в начале, например "1 2 3 ... k + +"):
             максимальная глубина стека не превышает числа операндов ≤ n.
           - Каждый узел стека занимает фиксированный объем памяти: O(1).
           Итоговая емкостная сложность: S(n) = O(n) памяти в худшем случае.
        
        3. АЛГОРИТМ СОРТИРОВОЧНОЙ СТАНЦИИ (Shunting-Yard):
           - Каждый токен входной строки инфиксного выражения считывается один раз: O(n).
           - Каждый оператор помещается в стек операторов ровно 1 раз и выталкивается ровно 1 раз: O(1) амортизированно на токен.
           - Итоговая сложность перевода: O(n) по времени и O(n) по дополнительной памяти.
        ================================================================================
        """;
    }
}
