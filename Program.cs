using AlgorithmsLab3.Applications;
using AlgorithmsLab3.Core;
using System.Globalization;
using System.Text;

Console.InputEncoding = Encoding.UTF8; 
Console.OutputEncoding = Encoding.UTF8;

if (args.Contains("--self-test")) { SelfTest.Run(); return; }
if (args.Contains("--benchmark")) { Benchmark.Run(); return; }

var list = LinkedSequence<int>.From(new[] { 1, 2, 2, 3 });

string Read(string prompt) { 
    Console.Write($"  {prompt}: "); 
    return Console.ReadLine() ?? throw new EndOfStreamException(); 
}

int Number(string prompt) => int.Parse(Read(prompt), CultureInfo.InvariantCulture);

void Header(string title) { 
    if (!Console.IsOutputRedirected) Console.Clear(); 
    Console.ForegroundColor = ConsoleColor.Cyan; 
    Console.WriteLine("\n  ┌──────────────────────────────────────────────┐"); 
    Console.WriteLine("  │  ЛАБОРАТОРКА 3  -  ДИНАМИЧЕСКИЕ СТРУКТУРЫ  │"); 
    Console.WriteLine("  └──────────────────────────────────────────────┘"); 
    Console.ResetColor(); 
    Console.WriteLine($"\n  {title}\n"); 
}

void Pause() { 
    Console.Write("\n  Enter — продолжить "); 
    Console.ReadLine(); 
}

void Operations(bool stack) { 
    Header(stack ? "Стек · операции из файла" : "Очередь · операции из файла"); 
    string path = Read("Путь к файлу [data/input.txt]"); 
    if (path.Length == 0) path = "data/input.txt"; 
    bool standard = !stack && Read("Реализация: 1 — своя, 2 — Queue [1]") == "2"; 
    var ops = OperationRunner.Parse(File.ReadAllText(path)); 
    OperationRunner.Run(ops, stack, standard, Console.WriteLine); 
    var t = OperationRunner.Measure(path, stack, standard); 
    Console.WriteLine($"\n  Без вывода: чтение и разбор {t.Read:F3} мс, выполнение {t.Execute:F3} мс"); 
}

void ExpressionMenu() { 
    Header("Выражения"); 
    Console.WriteLine("  1  Вычислить постфиксную запись из файла\n  2  Перевести инфиксную запись и вычислить"); 
    var choice = Read("Выбор"); 
    if (choice == "1") { 
        var p = Read("Путь [data/postfix.txt]");
        var text = File.ReadAllText(p.Length == 0 ? "data/postfix.txt" : p); 
        Console.WriteLine($"  Результат: {Expressions.Evaluate(text):G17}");
    }
    else if (choice == "2") { 
        var postfix = Expressions.ToPostfix(Read("Выражение")); 
        Console.WriteLine($"  Постфиксная запись: {postfix}\n  Результат: {Expressions.Evaluate(postfix):G17}");
    }
}

void ListMenu() { 
    while (true) { 
        Header("Связный список целых чисел"); 
        Console.WriteLine($"  L = {list}\n\n  1 Ввод L             2 Разворот             3 Последний в начало\n  4 Первый в конец     5 Число различных      6 Удалить неуникальные\n  7 Вставить копию     8 Вставить по порядку  9 Удалить E\n  10 Вставить F перед E  11 Дописать E из файла 12 Разделить после X\n  13 Удвоить           14 Поменять по индексам  0 Назад"); 
        var c = Read("Выбор"); 
        if (c == "0") return; 
        try { 
            switch (c) {
                case "1": 
                    list = LinkedSequence<int>.From(ParseInts(Read("Целые числа через пробел"))); 
                    break;
                case "2": 
                    list.Reverse(); 
                    break; 
                case "3": 
                    list.MoveLastToFront(); 
                    break; 
                case "4": 
                    list.MoveFirstToEnd(); 
                    break;
                case "5": 
                    Console.WriteLine($"  Различных: {list.DistinctCount()}"); 
                    break; 
                case "6": 
                    list.RemoveNonUnique(); 
                    break;
                case "7":
                    Console.WriteLine(list.InsertCopyAfter(Number("X")) ? "  Копия вставлена" : "  X не найден"); 
                    break;
                case "8": 
                    if (!list.Values().SequenceEqual(list.Values().OrderBy(x => x))) { 
                        Console.WriteLine("  Список должен быть упорядочен по неубыванию."); 
                        break; 
                    } 
                    list.InsertSorted(Number("E")); 
                    break;
                case "9": 
                    Console.WriteLine($"  Удалено: {list.RemoveAll(Number("E"))}"); 
                    break;
                case "10": 
                    int e = Number("E"), f = Number("F");
                    Console.WriteLine(list.InsertBefore(e, f) ? "  Вставлено" : "  E не найден");
                    break;
                case "11":
                    var path = Read("Файл с двумя строками L и E [data/lists.txt]"); 
                    var lines = File.ReadAllLines(path.Length == 0 ? "data/lists.txt" : path); 
                    if (lines.Length < 2) throw new FormatException("Нужны две строки списков."); 
                    list = LinkedSequence<int>.From(ParseInts(lines[0]));
                    list.AppendCopy(LinkedSequence<int>.From(ParseInts(lines[1])));
                    break;
                case "12": 
                    var second = list.SplitAfter(Number("X")); 
                    Console.WriteLine($"  Второй список: {second}"); 
                    break;
                case "13": 
                    list.AppendCopy(list); 
                    break;
                case "14":
                    Console.WriteLine(list.SwapAt(Number("Первый индекс (с 1)") - 1, Number("Второй индекс (с 1)") - 1) ? "  Элементы обменяны" : "  Индекс вне списка"); 
                    break;
                } 
            Console.WriteLine($"  L = {list}");
        } 
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException or OverflowException) { 
            Console.WriteLine($"  Ошибка: {ex.Message}");
        }
        Pause(); 
    }
}

static int[] ParseInts(string s) => s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();

void Demos() { 
    Header("Применение структур"); 
    Console.WriteLine("  1 Сложение полиномов · список\n  2 Проверка скобок · стек\n  3 Буфер пакетов · очередь\n  4 Дерево Хаффмана");
    switch (Read("Выбор")) 
    {
        case "1": 
            Console.WriteLine("  (3x² + 2x + 1) + (4x² − 2x + 5)"); 
            Console.WriteLine("  " + string.Join(" + ", PolynomialAddition.Add(new[] { (2, 3d), (1, 2d), (0, 1d) }, new[] { (2, 4d), (1, -2d), (0, 5d) }).Values().Select(t => $"{t.Coefficient}x^{t.Degree}"))); 
            break;
        case "2": 
            Console.WriteLine(BracketValidation.IsValid(Read("Текст")) ? "  Скобки согласованы" : "  Ошибка в скобках"); 
            break;
        case "3": 
            var buffer = new PacketBuffer(2);
            foreach (var p in new[] { "пакет A", "пакет B", "пакет C" }) 
                Console.WriteLine($"  {p}: {(buffer.Receive(p) ? "принят" : "буфер заполнен")}"); 
            Console.WriteLine($"  Обработан: {buffer.Process()}\n  Осталось: {string.Join(", ", buffer.Pending())}");
            break;
        case "4": 
            var codes = HuffmanTree.BuildCodes(Read("Текст")); 
            foreach (var item in codes) Console.WriteLine($"  '{item.Key}' : {item.Value}"); 
            break;
        } 
}
while (true) { 
    try { 
        Header("Главное меню"); 
        Console.WriteLine("  1  Стек: операции из файла\n  2  Очередь: операции из файла\n  3  Выражения\n  4  Алгоритмы связного списка\n  5  Примеры применения\n  6  Сравнение времени\n  0  Выход"); 
        string c = Read("Выбор"); 
        if (c == "0") break; 
        switch (c) { 
            case "1": 
                Operations(true); 
                break; 
            case "2": 
                Operations(false); 
                break; 
            case "3": 
                ExpressionMenu();
                break;
            case "4":
                ListMenu(); 
                continue;
            case "5": 
                Demos();
                break; 
            case "6": 
                Benchmark.Run(); 
                break; 
            default: 
                continue;
        } 
        Pause(); 
    } catch (EndOfStreamException) { break; } 
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or InvalidOperationException or ArithmeticException) { 
        Console.WriteLine($"\n  Ошибка: {ex.Message}"); 
        Pause();
    } 
}
