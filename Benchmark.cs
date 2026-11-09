using System.Text;
using AlgorithmsLab3.Core;

namespace AlgorithmsLab3;

/// <summary>
/// Бенчмарк и замер времени выполнения операций со Стеком и Очередями (Части 1 и 2 ТЗ).
/// Генерирует наборы данных разной длины и одинаковой длины разного состава для максимального расхождения по времени.
/// </summary>
public static class Benchmark
{
    public static void Run()
    {
        string benchDir = Path.Combine("data", "benchmarks");
        Directory.CreateDirectory(benchDir);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("  ║                 ЗАМЕРЫ ВРЕМЕНИ И БЕНЧМАРКИ (ЧАСТИ 1 И 2)             ║");
        Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════╝\n");
        Console.ResetColor();

        // 1. Различные по длине:
        //    short (1 000 команд), medium (10 000 команд), long (50 000 команд)
        // 2. Одинаковые по длине (по 10 000 команд), но различные по составу для максимального расхождения:
        //    - compose_alternating: чередование push/pop (размер структуры держится около 1)
        //    - compose_accumulate: массовое накопление (размер растет до тысяч узлов, нагрузка на аллокатор)
        //    - compose_peeking_printing: операции просмотра начала и проверки пустоты под нагрузкой
        void CreateBenchFile(string name, IEnumerable<string> commands)
        {
            string fullPath = Path.Combine(benchDir, $"{name}.txt");
            File.WriteAllText(fullPath, string.Join(' ', commands));
        }

        Console.WriteLine("  [1/3] Генерация тестовых наборов данных...");

        // Наборы разной длины:
        CreateBenchFile("short_1k", Enumerable.Range(0, 1_000).Select(i => i % 3 == 0 ? "2" : "1,val"));
        CreateBenchFile("medium_10k", Enumerable.Range(0, 10_000).Select(i => i % 3 == 0 ? "2" : "1,val"));
        CreateBenchFile("long_50k", Enumerable.Range(0, 50_000).Select(i => i % 3 == 0 ? "2" : "1,val"));

        // Наборы одинаковой длины (10 000 команд), но разного состава:
        // Состав А: Чередование вставки и удаления (размер структуры <= 1)
        CreateBenchFile("equal_alternating_10k", Enumerable.Range(0, 10_000).Select(i => i % 2 == 0 ? "1,item" : "2"));

        // Состав Б: Сначала 8000 вставок (накопление), затем 2000 удалений
        CreateBenchFile("equal_accumulate_10k", Enumerable.Repeat("1,item", 8_000).Concat(Enumerable.Repeat("2", 2_000)));

        // Состав В: Вставки вперемешку с частым просмотром начала (3) и проверкой на пустоту (4)
        CreateBenchFile("equal_lookup_heavy_10k", Enumerable.Range(0, 10_000).Select(i => (i % 4) switch
        {
            0 => "1,item",
            1 => "3",
            2 => "4",
            _ => "1,item"
        }));

        Console.WriteLine("  [2/3] Прогрев JIT компилятора...");
        string warmupFile = Path.Combine(benchDir, "short_1k.txt");
        OperationRunner.Measure(warmupFile, isStack: true, useStandardQueue: false);
        OperationRunner.Measure(warmupFile, isStack: false, useStandardQueue: false);
        OperationRunner.Measure(warmupFile, isStack: false, useStandardQueue: true);

        Console.WriteLine("  [3/3] Выполнение замеров (медиана 5 прогонов)...\n");

        var testSuites = new (string Name, string Description, int Count)[]
        {
            ("short_1k", "Короткий набор (1 тыс.)", 1_000),
            ("medium_10k", "Средний набор (10 тыс.)", 10_000),
            ("long_50k", "Длинный набор (50 тыс.)", 50_000),
            ("equal_alternating_10k", "Один. длина (10k): Чередование 1 и 2", 10_000),
            ("equal_accumulate_10k", "Один. длина (10k): Накопление (8k push, 2k pop)", 10_000),
            ("equal_lookup_heavy_10k", "Один. длина (10k): Частый Top/First и isEmpty", 10_000)
        };

        var report = new StringBuilder();
        report.AppendLine("# Сравнительные замеры производительности структур данных");
        report.AppendLine();
        report.AppendLine("> Замеры времени чтения файла, синтаксического разбора команд и чистого выполнения операций.");
        report.AppendLine("> Каждое значение — медиана 5 последовательных запусков после прогрева JIT.");
        report.AppendLine();
        report.AppendLine("| Набор тестов | Описание | Команд | Структура | Чтение/разбор (мс) | Выполнение (мс) |");
        report.AppendLine("|---|---|---:|---|---:|---:|");

        foreach (var (fileName, desc, count) in testSuites)
        {
            string filePath = Path.Combine(benchDir, $"{fileName}.txt");

            var structures = new (string Label, bool IsStack, bool UseStandard)[]
            {
                ("Стек (LinkedStack)", true, false),
                ("Очередь (LinkedLabQueue)", false, false),
                ("Очередь (Queue<T> стд.)", false, true)
            };

            foreach (var (label, isStack, useStandard) in structures)
            {
                var runs = new List<(double Read, double Exec)>();
                for (int r = 0; r < 5; r++)
                {
                    runs.Add(OperationRunner.Measure(filePath, isStack, useStandard));
                }

                double medianRead = runs.Select(x => x.Read).OrderBy(x => x).ElementAt(2);
                double medianExec = runs.Select(x => x.Exec).OrderBy(x => x).ElementAt(2);

                report.AppendLine($"| `{fileName}` | {desc} | {count:N0} | {label} | {medianRead:F3} | {medianExec:F3} |");
            }
        }

        report.AppendLine();
        report.AppendLine("## Анализ расхождения по времени:");
        report.AppendLine("1. **Связная очередь (`LinkedLabQueue`) vs стандартная (`Queue<T>`)**:");
        report.AppendLine("   - `Queue<T>` использует непрерывный массив (кольцевой буфер). При добавлении элементов память выделяется блоками с удвоением емкости, обеспечивая отличную локализацию данных в кэше процессора (L1/L2 cache locality).");
        report.AppendLine("   - `LinkedLabQueue` на каждую операцию `Enqueue` выделяет отдельный объект узла в куче (`new ListNode<T>`), что вызывает оверхед аллокатора памяти и промахи кэша (pointer chasing).");
        report.AppendLine("2. **Сравнение наборов одинаковой длины (10 000 команд)**:");
        report.AppendLine("   - Набор с накоплением (`equal_accumulate_10k`) держит тысячи живых объектов в памяти, вызывая максимальную нагрузку на сборщик мусора GC.");
        report.AppendLine("   - Набор с чередованием (`equal_alternating_10k`) освобождает узлы сразу после создания, размер списка не превышает 1-2 элементов, поэтому память циклически переиспользуется.");

        string reportText = report.ToString();
        File.WriteAllText("BENCHMARKS.md", reportText);

        Console.WriteLine(reportText);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Результаты успешно сохранены в файл BENCHMARKS.md");
        Console.ResetColor();
    }
}
