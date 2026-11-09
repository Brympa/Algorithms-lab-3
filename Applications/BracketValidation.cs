using AlgorithmsLab3.Core;

namespace AlgorithmsLab3.Applications;

/// <summary>
/// Пример применения структуры данных "Стек" (Часть 3 ТЗ):
/// Проверка правильности расстановки скобок (), [], {} в исходном коде или математическом выражении.
/// Игнорирует строковые литералы и комментарии (// и /* ... */).
/// </summary>
public static class BracketValidation
{
    public sealed record ValidationResult(bool IsValid, string Message, int? ErrorPosition = null);

    /// <summary>
    /// Проверяет согласованность скобок с использованием собственного LinkedStack.
    /// </summary>
    public static ValidationResult Check(string text)
    {
        var stack = new LinkedStack<(char Bracket, int Position)>();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            char next = i + 1 < text.Length ? text[i + 1] : '\0';

            // Пропуск однострочных комментариев //
            if (c == '/' && next == '/')
            {
                i += 2;
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            // Пропуск многострочных комментариев /* ... */
            if (c == '/' && next == '*')
            {
                i += 2;
                while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                {
                    i++;
                }

                if (i + 1 >= text.Length)
                {
                    return new ValidationResult(false, "Незакрытый многострочный комментарий /* ... */", i);
                }

                i++;
                continue;
            }

            // Пропуск строковых и символьных литералов "..." и '...'
            if (c is '"' or '\'')
            {
                char quote = c;
                bool closed = false;
                while (++i < text.Length)
                {
                    if (text[i] == '\\')
                    {
                        i++; // экранирование следующего символа
                        continue;
                    }
                    if (text[i] == quote)
                    {
                        closed = true;
                        break;
                    }
                }

                if (!closed)
                {
                    return new ValidationResult(false, $"Незакрытая кавычка {quote}", i);
                }
                continue;
            }

            // Обработка открывающих скобок
            if (c is '(' or '[' or '{')
            {
                stack.Push((c, i + 1));
            }
            // Обработка закрывающих скобок
            else if (c is ')' or ']' or '}')
            {
                if (stack.isEmpty())
                {
                    return new ValidationResult(false, $"Лишняя закрывающая скобка '{c}' в позиции {i + 1}", i + 1);
                }

                var (openBracket, openPos) = stack.Pop();
                if (!IsMatchingPair(openBracket, c))
                {
                    return new ValidationResult(false, $"Несоответствие скобок: открыта '{openBracket}' (поз. {openPos}), а закрыта '{c}' (поз. {i + 1})", i + 1);
                }
            }
        }

        if (!stack.isEmpty())
        {
            var (unclosedBracket, pos) = stack.Pop();
            return new ValidationResult(false, $"Осталась незакрытая скобка '{unclosedBracket}' в позиции {pos}", pos);
        }

        return new ValidationResult(true, "Все скобки расставлены верно и согласованы.");
    }

    /// <summary>
    /// Упрощенная проверка (возвращает bool).
    /// </summary>
    public static bool IsValid(string text) => Check(text).IsValid;

    private static bool IsMatchingPair(char open, char close) =>
        (open == '(' && close == ')') ||
        (open == '[' && close == ']') ||
        (open == '{' && close == '}');
}
