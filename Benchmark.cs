using AlgorithmsLab3.Core;
using System.Text;
static class Benchmark
{
    public static void Run()
    {
        Directory.CreateDirectory("data/benchmarks");
        void Write(string name, IEnumerable<string> commands) => File.WriteAllText($"data/benchmarks/{name}.txt", string.Join(' ', commands));
        Write("short", Enumerable.Range(0, 1000).Select(i => i % 2 == 0 ? "1,x" : "2"));
        Write("alternating", Enumerable.Range(0, 4000).Select(i => i % 2 == 0 ? "1,x" : "2"));
        Write("printing", Enumerable.Repeat("1,x", 2000).Concat(Enumerable.Repeat("5", 2000)));
        Write("long", Enumerable.Range(0, 20000).Select(i => i % 2 == 0 ? "1,x" : "2"));
        OperationRunner.Measure("data/benchmarks/short.txt", false, false); OperationRunner.Measure("data/benchmarks/short.txt", false, true); OperationRunner.Measure("data/benchmarks/short.txt", true, false);
        var report = new StringBuilder("# Результаты измерений\n\nЧтение включает разбор команд; выполнение включает формирование строки печати, но не вывод в терминал. Медиана трёх запусков после прогрева, миллисекунды. Результаты зависят от компьютера и нагрузки. Наборы alternating и printing содержат по 4000 команд: накопление элементов и повторная печать намеренно увеличивают расхождение времени.\n\n| Набор | Команд | Структура | Чтение и разбор, мс | Выполнение, мс |\n|---|---:|---|---:|---:|\n");
        foreach (var (name, count) in new[] { ("short", 1000), ("alternating", 4000), ("printing", 4000), ("long", 20000) }) foreach (var (label, stack, standard) in new[] { ("Стек", true, false), ("Очередь на списке", false, false), ("Queue<T>", false, true) }) { var results = Enumerable.Range(0, 3).Select(_ => OperationRunner.Measure($"data/benchmarks/{name}.txt", stack, standard)).ToArray(); double read = results.Select(x => x.Read).Order().ElementAt(1), exec = results.Select(x => x.Execute).Order().ElementAt(1); report.AppendLine($"| {name} | {count} | {label} | {read:F3} | {exec:F3} |"); }
        File.WriteAllText("BENCHMARKS.md", report.ToString()); Console.WriteLine(report.ToString()); Console.WriteLine("  Результаты сохранены в BENCHMARKS.md");
    }
}
