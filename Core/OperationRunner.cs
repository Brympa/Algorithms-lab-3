using System.Diagnostics;
using AlgorithmsLab3.UI;

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
    /// Если log равен null, выполняется только «чистое» исполнение без
    /// форматирования строк — именно этот режим используется в бенчмарке.
    /// </summary>
    public static void RunStack(OperationCommand[] commands, Action<string>? log = null)
    {
        if (log is null)
        {
            ExecuteStack(commands);
            return;
        }

        var stack = new LinkedStack<string>();
        var table = new ConsoleTable(
            new[] { "Шаг", "Команда", "Операция", "Результат и состояние стека" },
            ColumnAlignment.Right,
            ColumnAlignment.Left,
            ColumnAlignment.Left,
            ColumnAlignment.Left);

        int step = 1;
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

            table.AddRow(step.ToString(), cmd.RawToken, opName, details);
            step++;
        }

        log(table.Render());
    }

    /// <summary>
    /// Выполнение команд для Очереди (своей или стандартной) с наглядным выводом протокола.
    /// Если log равен null, выполняется только «чистое» исполнение без форматирования строк.
    /// </summary>
    public static void RunQueue(OperationCommand[] commands, bool useStandardQueue, Action<string>? log = null)
    {
        if (log is null)
        {
            ExecuteQueue(commands, useStandardQueue);
            return;
        }

        ILabQueue<string> queue = useStandardQueue
            ? new StandardLabQueue<string>()
            : new LinkedLabQueue<string>();

        string queueType = useStandardQueue ? "Queue<T> (стандартная)" : "LinkedLabQueue (на списке)";
        var table = new ConsoleTable(
            new[] { "Шаг", "Команда", "Операция", "Результат и состояние очереди" },
            ColumnAlignment.Right,
            ColumnAlignment.Left,
            ColumnAlignment.Left,
            ColumnAlignment.Left);

        log($"  Реализация: {queueType}");

        int step = 1;
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

            table.AddRow(step.ToString(), cmd.RawToken, opName, details);
            step++;
        }

        log(table.Render());
    }

    /// <summary>
    /// «Чистое» выполнение команд стеком — без построения строк протокола.
    /// </summary>
    private static void ExecuteStack(OperationCommand[] commands)
    {
        var stack = new LinkedStack<string>();

        foreach (var cmd in commands)
        {
            switch (cmd.Code)
            {
                case 1:
                    stack.Push(cmd.Value!);
                    break;
                case 2:
                    if (!stack.isEmpty()) stack.Pop();
                    break;
                case 3:
                    if (!stack.isEmpty()) _ = stack.Top();
                    break;
                case 4:
                    _ = stack.isEmpty();
                    break;
                case 5:
                    _ = stack.Print();
                    break;
            }
        }
    }

    /// <summary>
    /// «Чистое» выполнение команд очередью — без построения строк протокола.
    /// </summary>
    private static void ExecuteQueue(OperationCommand[] commands, bool useStandardQueue)
    {
        ILabQueue<string> queue = useStandardQueue
            ? new StandardLabQueue<string>()
            : new LinkedLabQueue<string>();

        foreach (var cmd in commands)
        {
            switch (cmd.Code)
            {
                case 1:
                    queue.Enqueue(cmd.Value!);
                    break;
                case 2:
                    if (!queue.isEmpty()) queue.Dequeue();
                    break;
                case 3:
                    if (!queue.isEmpty()) _ = queue.First();
                    break;
                case 4:
                    _ = queue.isEmpty();
                    break;
                case 5:
                    _ = queue.Print();
                    break;
            }
        }
    }

    /// <summary>
    /// Замер времени чтения/разбора и чистого выполнения без вывода в консоль (по ТЗ).
    /// В режиме замера НЕ вызывается Print() для каждой команды и не строятся
    /// строки таблицы, поэтому время отражает именно работу структуры данных.
    ///
    /// <paramref name="repetitions"/> позволяет выполнить один и тот же набор
    /// несколько раз подряд (каждый раз в новой структуре). Это нужно для
    /// устойчивых замеров очень быстрых операций: возвращается СРЕДНЕЕ время
    /// одного выполнения набора. Для одиночного замера используется 1.
    /// </summary>
    public static (double ReadMs, double ExecuteMs) Measure(
        string path,
        bool isStack,
        bool useStandardQueue,
        int repetitions = 1)
    {
        if (repetitions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(repetitions), "Количество повторов должно быть не меньше 1.");
        }

        var sw = Stopwatch.StartNew();
        string text = File.ReadAllText(path);
        var commands = Parse(text);
        sw.Stop();
        double readMs = sw.Elapsed.TotalMilliseconds;

        double totalMs = 0;
        for (int i = 0; i < repetitions; i++)
        {
            sw.Restart();
            if (isStack)
            {
                ExecuteStack(commands);
            }
            else
            {
                ExecuteQueue(commands, useStandardQueue);
            }
            sw.Stop();
            totalMs += sw.Elapsed.TotalMilliseconds;

            // Перед следующим повтором очищаем кучу, чтобы предыдущая
            // структура не влияла на замер следующей. Это время не входит
            // в измеряемое выполнение.
            if (i + 1 < repetitions)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        return (readMs, totalMs / repetitions);
    }
}
