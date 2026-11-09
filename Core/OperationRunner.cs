using System.Diagnostics;
using System.Text;

namespace AlgorithmsLab3.Core;

/// <summary>
/// Исполнитель команд из файла input.txt по ТЗ (Части 1 и 2).
/// Команды:
/// 1 - Push(elem) / Enqueue(elem) (значение передается через запятую, например 1,56 или 1,cat)
/// 2 - Pop() / Dequeue()
/// 3 - Top() / First() (просмотр вершины / начала)
/// 4 - isEmpty() (проверка на пустоту)
/// 5 - Print() (печать структуры)
/// </summary>
public static class OperationRunner
{
    public sealed record OperationCommand(int Code, string? Value, string RawToken);

    /// <summary>
    /// Разбор последовательности команд из текстовой строки.
    /// </summary>
    public static OperationCommand[] Parse(string text)
    {
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<OperationCommand>(tokens.Length);

        foreach (var token in tokens)
        {
            var parts = token.Split(',', 2);
            if (!int.TryParse(parts[0], out int opCode) || opCode < 1 || opCode > 5)
            {
                throw new FormatException($"Некорректная команда '{token}'. Допустимы коды от 1 до 5.");
            }

            if (opCode == 1)
            {
                if (parts.Length != 2 || string.IsNullOrEmpty(parts[1]))
                {
                    throw new FormatException($"Для команды добавления 1 требуется указать значение через запятую (например, 1,56 или 1,cat), получено: '{token}'.");
                }
                result.Add(new OperationCommand(1, parts[1], token));
            }
            else
            {
                if (parts.Length > 1)
                {
                    throw new FormatException($"Команда {opCode} не принимает аргументов, получено: '{token}'.");
                }
                result.Add(new OperationCommand(opCode, null, token));
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Выполнение команд для Стека с наглядным выводом протокола.
    /// </summary>
    public static void RunStack(OperationCommand[] commands, Action<string>? log = null)
    {
        var stack = new LinkedStack<string>();
        int step = 1;

        if (log != null)
        {
            log("  ┌──────┬──────────────┬──────────────────┬───────────────────────────────────────────┐");
            log("  │ Шаг  │ Команда      │ Операция         │ Результат и состояние стека               │");
            log("  ├──────┼──────────────┼──────────────────┼───────────────────────────────────────────┤");
        }

        foreach (var cmd in commands)
        {
            string opName;
            string details;

            switch (cmd.Code)
            {
                case 1:
                    opName = $"Push(\"{cmd.Value}\")";
                    stack.Push(cmd.Value!);
                    details = $"Добавлено. Стек: {stack.Print()}";
                    break;

                case 2:
                    opName = "Pop()";
                    if (stack.isEmpty())
                    {
                        details = "Ошибка: стек пуст!";
                    }
                    else
                    {
                        string popped = stack.Pop();
                        details = $"Извлечено: \"{popped}\". Стек: {stack.Print()}";
                    }
                    break;

                case 3:
                    opName = "Top()";
                    if (stack.isEmpty())
                    {
                        details = "Стек пуст (вершина отсутствует)";
                    }
                    else
                    {
                        details = $"Вершина = \"{stack.Top()}\"";
                    }
                    break;

                case 4:
                    opName = "isEmpty()";
                    details = $"Результат: {stack.isEmpty()}";
                    break;

                case 5:
                    opName = "Print()";
                    details = $"Печать: {stack.Print()}";
                    break;

                default:
                    opName = $"Код {cmd.Code}";
                    details = "Неизвестная операция";
                    break;
            }

            if (log != null)
            {
                log($"  │ {step,4} │ {cmd.RawToken,-12} │ {opName,-16} │ {details,-41} │");
            }
            step++;
        }

        if (log != null)
        {
            log("  └──────┴──────────────┴──────────────────┴───────────────────────────────────────────┘");
        }
    }

    /// <summary>
    /// Выполнение команд для Очереди (своей или стандартной) с наглядным выводом протокола.
    /// </summary>
    public static void RunQueue(OperationCommand[] commands, bool useStandardQueue, Action<string>? log = null)
    {
        ILabQueue<string> queue = useStandardQueue
            ? new StandardLabQueue<string>()
            : new LinkedLabQueue<string>();

        string queueType = useStandardQueue ? "Queue<T> (стандартная)" : "LinkedLabQueue (на списке)";
        int step = 1;

        if (log != null)
        {
            log($"  Реализация: {queueType}");
            log("  ┌──────┬──────────────┬──────────────────┬───────────────────────────────────────────┐");
            log("  │ Шаг  │ Команда      │ Операция         │ Результат и состояние очереди             │");
            log("  ├──────┼──────────────┼──────────────────┼───────────────────────────────────────────┤");
        }

        foreach (var cmd in commands)
        {
            string opName;
            string details;

            switch (cmd.Code)
            {
                case 1:
                    opName = $"Enqueue(\"{cmd.Value}\")";
                    queue.Enqueue(cmd.Value!);
                    details = $"Добавлено. Очередь: {queue.Print()}";
                    break;

                case 2:
                    opName = "Dequeue()";
                    if (queue.isEmpty())
                    {
                        details = "Ошибка: очередь пуста!";
                    }
                    else
                    {
                        string dequeued = queue.Dequeue();
                        details = $"Извлечено: \"{dequeued}\". Очередь: {queue.Print()}";
                    }
                    break;

                case 3:
                    opName = "First() / Peek()";
                    if (queue.isEmpty())
                    {
                        details = "Очередь пуста (нет первого элемента)";
                    }
                    else
                    {
                        details = $"Первый элемент = \"{queue.First()}\"";
                    }
                    break;

                case 4:
                    opName = "isEmpty()";
                    details = $"Результат: {queue.isEmpty()}";
                    break;

                case 5:
                    opName = "Print()";
                    details = $"Печать: {queue.Print()}";
                    break;

                default:
                    opName = $"Код {cmd.Code}";
                    details = "Неизвестная операция";
                    break;
            }

            if (log != null)
            {
                log($"  │ {step,4} │ {cmd.RawToken,-12} │ {opName,-16} │ {details,-41} │");
            }
            step++;
        }

        if (log != null)
        {
            log("  └──────┴──────────────┴──────────────────┴───────────────────────────────────────────┘");
        }
    }

    /// <summary>
    /// Замер времени чтения/разбора и чистого выполнения без вывода в консоль (по ТЗ).
    /// </summary>
    public static (double ReadMs, double ExecuteMs) Measure(string path, bool isStack, bool useStandardQueue)
    {
        var sw = Stopwatch.StartNew();
        string text = File.ReadAllText(path);
        var commands = Parse(text);
        sw.Stop();
        double readMs = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        if (isStack)
        {
            RunStack(commands, log: null);
        }
        else
        {
            RunQueue(commands, useStandardQueue, log: null);
        }
        sw.Stop();
        double executeMs = sw.Elapsed.TotalMilliseconds;

        return (readMs, executeMs);
    }
}
