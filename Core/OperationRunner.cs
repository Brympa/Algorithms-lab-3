using System.Diagnostics;
namespace AlgorithmsLab3.Core;
public static class OperationRunner
{
    public static (int Op, string? Value)[] Parse(string text) => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(token => { var parts = token.Split(',', 2); if (!int.TryParse(parts[0], out int op) || op < 1 || op > 5 || (op == 1 && (parts.Length != 2 || parts[1].Length == 0)) || (op != 1 && parts.Length != 1)) throw new FormatException($"Неверная команда: {token}"); return (op, parts.Length == 2 ? parts[1] : null); }).ToArray();
    public static void Run((int Op, string? Value)[] operations, bool stack, bool standard, Action<string>? output)
    {
        var s = new LinkedStack<string>(); 
        ILabQueue<string> q = standard ? new StandardLabQueue<string>() : new LinkedLabQueue<string>();
        foreach (var (op, value) in operations) { 
            string? result = null; switch (op) {
                case 1: 
                    if (stack) s.Push(value!); 
                    else q.Enqueue(value!); 
                    if (output != null) result = $"Добавлено: {value}"; 
                    break;
                case 2: 
                    if (stack ? s.IsEmpty : q.IsEmpty) { if (output != null) result = "Удаление: структура пуста"; } else { var v = stack ? s.Pop() : q.Dequeue(); if (output != null) result = $"Удалено: {v}"; } break;
                case 3: 
                    var first = (stack ? s.IsEmpty : q.IsEmpty) ? null : (stack ? s.Top() : q.Peek()); 
                    if (output != null) result = first is null ? "Просмотр: структура пуста" : $"Первый элемент: {first}"; 
                    break;
                case 4: bool empty = stack ? s.IsEmpty : q.IsEmpty; if (output != null) result = $"Пусто: {empty}"; break;
                case 5: var contents = string.Join(" → ", stack ? s.Values() : q.Values()); if (output != null) result = "Содержимое: [ " + contents + " ]"; break;
        } if (output != null) output($"{op}{(op == 1 ? "," + value : "")}  |  {result}"); }
    }
    public static (double Read, double Execute) Measure(string path, bool stack, bool standard) { 
        var watch = Stopwatch.StartNew(); 
        string text = File.ReadAllText(path); 
        var operations = Parse(text); 
        watch.Stop(); 
        double read = watch.Elapsed.TotalMilliseconds; 
        watch.Restart(); 
        Run(operations, stack, standard, null); 
        watch.Stop(); 
        return (read, watch.Elapsed.TotalMilliseconds); 
    }
}

