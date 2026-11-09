using System.Globalization;
using System.Text;
using AlgorithmsLab3.Applications;
using AlgorithmsLab3.Core;

namespace AlgorithmsLab3;

public static class Program
{
    private static LinkedSequence<int> _currentList = LinkedSequence<int>.From(new[] { 1, 2, 2, 3 });

    public static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Contains("--self-test"))
        {
            SelfTest.Run();
            return;
        }

        if (args.Contains("--benchmark"))
        {
            Benchmark.Run();
            return;
        }

        RunMainMenu();
    }

    private static void RunMainMenu()
    {
        while (true)
        {
            PrintHeader("ГЛАВНОЕ МЕНЮ");
            Console.WriteLine("  1  Часть 1: Стек (операции из файла input.txt)");
            Console.WriteLine("  2  Часть 1: Арифметические выражения");
            Console.WriteLine("  3  Часть 2: Очередь (операции из файла: LinkedLabQueue и Queue<T>)");
            Console.WriteLine("  4  Часть 3: Примеры применения (Список, Стек, Очередь, Хаффман)");
            Console.WriteLine("  5  Часть 4: Связный список и бинарное дерево");
            Console.WriteLine("  6  Сравнительные замеры времени (Бенчмарк)");
            Console.WriteLine("  7  Автоматическая самопроверка (Self-Test)");
            Console.WriteLine("  0  Выход из программы");
            Console.WriteLine();

            try
            {
                string choice = Prompt("Выберите раздел");
                if (choice == "0")
                {
                    Console.WriteLine("\n  Работа программы завершена. До свидания!");
                    break;
                }

                switch (choice)
                {
                    case "1":
                        RunStackOperationsMenu();
                        break;
                    case "2":
                        RunExpressionMenu();
                        break;
                    case "3":
                        RunQueueOperationsMenu();
                        break;
                    case "4":
                        RunApplicationsMenu();
                        break;
                    case "5":
                        RunListAlgorithmsMenu();
                        break;
                    case "6":
                        Benchmark.Run();
                        break;
                    case "7":
                        SelfTest.Run();
                        break;
                    default:
                        Console.WriteLine("  Неизвестный пункт меню.");
                        break;
                }

                Pause();
            }
            catch (EndOfStreamException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  [ОШИБКА]: {ex.Message}");
                Console.ResetColor();
                Pause();
            }
        }
    }

    // =========================================================================
    // 1. СТЕК (ЧАСТЬ 1)
    // =========================================================================

    private static void RunStackOperationsMenu()
    {
        PrintHeader("ЧАСТЬ 1: СТЕК НА СВЯЗНОМ СПИСКЕ (ОПЕРАЦИИ ИЗ ФАЙЛА)");
        Console.WriteLine("  Команды в файле input.txt:");
        Console.WriteLine("  1,значение - Push(elem), 2 - Pop(), 3 - Top(), 4 - isEmpty(), 5 - Print()\n");

        string defaultPath = Path.Combine("data", "input.txt");
        string path = Prompt($"Путь к файлу [{defaultPath}]");
        if (string.IsNullOrWhiteSpace(path))
        {
            path = defaultPath;
        }

        if (!File.Exists(path))
        {
            Console.WriteLine($"  Файл '{path}' не найден!");
            return;
        }

        string content = File.ReadAllText(path);
        Console.WriteLine($"  Содержимое файла: {content.Trim()}\n");

        var commands = OperationRunner.Parse(content);
        OperationRunner.RunStack(commands, Console.WriteLine);

        var (readMs, execMs) = OperationRunner.Measure(path, isStack: true, useStandardQueue: false);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"\n  Время чтения и разбора файла: {readMs:F4} мс");
        Console.WriteLine($"  Чистое время выполнения стеком (без вывода): {execMs:F4} мс");
        Console.ResetColor();
    }

    // =========================================================================
    // ВЫРАЖЕНИЯ (ЧАСТЬ 1)
    // =========================================================================

    private static void RunExpressionMenu()
    {
        PrintHeader("ЧАСТЬ 1: АРИФМЕТИЧЕСКИЕ ВЫРАЖЕНИЯ");
        Console.WriteLine("  1  Вычислить постфиксную запись из файла (с пошаговой трассировкой стека как на рис. 1)");
        Console.WriteLine("  2  Перевести инфиксную запись (2+2) в постфиксную со стеком (Shunting-Yard) и вычислить");
        Console.WriteLine("  3  Показать расчет оценки сложности алгоритма");
        Console.WriteLine("  0  Назад");
        Console.WriteLine();

        string choice = Prompt("Выбор");
        switch (choice)
        {
            case "1":
                {
                    string defaultPath = Path.Combine("data", "postfix.txt");
                    string p = Prompt($"Путь к файлу [{defaultPath}]");
                    if (string.IsNullOrWhiteSpace(p)) p = defaultPath;

                    if (!File.Exists(p))
                    {
                        Console.WriteLine($"  Файл '{p}' не найден!");
                        return;
                    }

                    string expr = File.ReadAllText(p).Trim();
                    Console.WriteLine($"\n  Постфиксное выражение из файла: {expr}\n");

                    double result = Expressions.Evaluate(expr, out var steps);

                    Console.WriteLine("  Пошаговая работа стека (воспроизводит рисунок 1 из ТЗ):");
                    Console.WriteLine("  ┌──────┬────────────┬──────────────────────────────────────┬──────────────────────────────┐");
                    Console.WriteLine("  │ Шаг  │ Токен      │ Действие                             │ Содержимое стека             │");
                    Console.WriteLine("  ├──────┼────────────┼──────────────────────────────────────┼──────────────────────────────┤");
                    foreach (var s in steps)
                    {
                        Console.WriteLine($"  │ {s.StepNumber,4} │ {s.Token,-10} │ {s.Action,-36} │ {s.StackState,-28} │");
                    }
                    Console.WriteLine("  └──────┴────────────┴──────────────────────────────────────┴──────────────────────────────┘");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  Итоговый результат: {result:G17}");
                    Console.ResetColor();
                    break;
                }

            case "2":
                {
                    Console.WriteLine("  Поддерживаются: +, -, *, /, :, ^, функции ln, cos, sin, sqrt, скобки (, )");
                    string infix = Prompt("Введите инфиксное выражение (например: 7 + 8 / (3 + 2) или sqrt(9) + 2^3^2)");
                    if (string.IsNullOrWhiteSpace(infix)) return;

                    string postfix = Expressions.ToPostfix(infix, out var steps);

                    Console.WriteLine("\n  Пошаговая работа сортировочной станции (Shunting-Yard на LinkedStack):");
                    Console.WriteLine("  ┌──────┬────────────┬──────────────────────────────────────────┬────────────────────┬──────────────────────────────┐");
                    Console.WriteLine("  │ Шаг  │ Токен      │ Действие                                 │ Стек операторов    │ Выходной поток               │");
                    Console.WriteLine("  ├──────┼────────────┼──────────────────────────────────────────┼────────────────────┼──────────────────────────────┤");
                    foreach (var s in steps)
                    {
                        Console.WriteLine($"  │ {s.StepNumber,4} │ {s.Token,-10} │ {s.Action,-40} │ {s.StackState,-18} │ {s.OutputState,-28} │");
                    }
                    Console.WriteLine("  └──────┴────────────┴──────────────────────────────────────────┴────────────────────┴──────────────────────────────┘");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n  Постфиксная запись: {postfix}");
                    Console.ResetColor();

                    double res = Expressions.Evaluate(postfix, out var evalSteps);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  Результат вычисления: {res:G17}");
                    Console.ResetColor();
                    break;
                }

            case "3":
                Console.WriteLine(Expressions.GetComplexityAnalysis());
                break;
        }
    }

    // =========================================================================
    // 2. ОЧЕРЕДЬ (ЧАСТЬ 2)
    // =========================================================================

    private static void RunQueueOperationsMenu()
    {
        PrintHeader("ЧАСТЬ 2: ОЧЕРЕДЬ (СВЯЗНАЯ И СТАНДАРТНАЯ)");
        Console.WriteLine("  Команды: 1,значение - вставка, 2 - удаление, 3 - начало, 4 - пустота, 5 - печать\n");

        string defaultPath = Path.Combine("data", "input.txt");
        string path = Prompt($"Путь к файлу [{defaultPath}]");
        if (string.IsNullOrWhiteSpace(path)) path = defaultPath;

        if (!File.Exists(path))
        {
            Console.WriteLine($"  Файл '{path}' не найден!");
            return;
        }

        string choice = Prompt("Реализация: 1 — своя LinkedLabQueue (на списке), 2 — стандартная Queue<T> [1]");
        bool standard = choice == "2";

        string content = File.ReadAllText(path);
        Console.WriteLine($"  Содержимое файла: {content.Trim()}\n");

        var commands = OperationRunner.Parse(content);
        OperationRunner.RunQueue(commands, standard, Console.WriteLine);

        var (readMs, execMs) = OperationRunner.Measure(path, isStack: false, useStandardQueue: standard);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"\n  Время чтения и разбора: {readMs:F4} мс");
        Console.WriteLine($"  Чистое время выполнения очередью (без вывода): {execMs:F4} мс");
        Console.ResetColor();
    }

    // =========================================================================
    // 3. ПРИМЕРЫ ПРИМЕНЕНИЯ (ЧАСТЬ 3)
    // =========================================================================

    private static void RunApplicationsMenu()
    {
        PrintHeader("ЧАСТЬ 3: ПРИМЕРЫ ПРИМЕНЕНИЯ ДИНАМИЧЕСКИХ СТРУКТУР");
        Console.WriteLine("  1  Связный список: Сложение многочленов (произвольные степени)");
        Console.WriteLine("  2  Стек: Проверка согласованности скобок (), [], {} в коде");
        Console.WriteLine("  3  Очередь: Сетевой буфер пакетов (Drop-Tail при переполнении)");
        Console.WriteLine("  4  Деревья: Оптимальное кодирование Хаффмана");
        Console.WriteLine("  0  Назад");
        Console.WriteLine();

        string choice = Prompt("Выбор");
        switch (choice)
        {
            case "1":
                {
                    Console.WriteLine("\n  Демонстрация сложения двух многочленов:");
                    var pA = new (int, double)[] { (2, 3), (1, 2), (0, 1) }; // 3x^2 + 2x + 1
                    var pB = new (int, double)[] { (2, 4), (1, -2), (0, 5) }; // 4x^2 - 2x + 5
                    var listA = LinkedSequence<PolynomialAddition.Term>.From(pA.Select(t => new PolynomialAddition.Term(t.Item1, t.Item2)));
                    var listB = LinkedSequence<PolynomialAddition.Term>.From(pB.Select(t => new PolynomialAddition.Term(t.Item1, t.Item2)));

                    Console.WriteLine($"  A(x) = {PolynomialAddition.Format(listA)}");
                    Console.WriteLine($"  B(x) = {PolynomialAddition.Format(listB)}");

                    var sum = PolynomialAddition.Add(pA, pB);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  A(x) + B(x) = {PolynomialAddition.Format(sum)}");
                    Console.ResetColor();

                    Console.WriteLine("\n  Структура списка суммы в памяти:");
                    Console.WriteLine($"  {sum}");
                    break;
                }

            case "2":
                {
                    string input = Prompt("Введите текст или фрагмент кода для проверки скобок (Enter для примера)");
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        input = "for (int i = 0; i < n; i++) { items[i] = (a + b) * c; } // [ok]";
                        Console.WriteLine($"  Пример: {input}");
                    }

                    var res = BracketValidation.Check(input);
                    if (res.IsValid)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ {res.Message}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  ✗ {res.Message}");
                    }
                    Console.ResetColor();
                    break;
                }

            case "3":
                {
                    int cap = 3;
                    var buf = new PacketBuffer(cap);
                    Console.WriteLine($"\n  Создан сетевой буфер пакетов FIFO ёмкостью {cap}.\n");

                    string[] testPackets = { "Пакет-1 (SYN)", "Пакет-2 (DATA-1)", "Пакет-3 (DATA-2)", "Пакет-4 (DATA-3)", "Пакет-5 (FIN)" };

                    foreach (var pkt in testPackets)
                    {
                        bool received = buf.Receive(pkt);
                        if (received)
                        {
                            Console.WriteLine($"  [+] Принят: {pkt,-18} | В буфере: {buf.CurrentCount}/{buf.Capacity}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"  [!] СБРОШЕН: {pkt,-17} | Буфер переполнен! (Drop-Tail)");
                            Console.ResetColor();
                        }
                    }

                    Console.WriteLine("\n  Обработка пакетов процессором маршрутизатора:");
                    while (!buf.IsEmpty)
                    {
                        Console.WriteLine($"  [-] Отправлен в сеть: {buf.Process()}");
                    }
                    break;
                }

            case "4":
                {
                    string text = Prompt("Введите текст для построения дерева Хаффмана [ABRACADABRA]");
                    if (string.IsNullOrWhiteSpace(text)) text = "ABRACADABRA";

                    var codes = HuffmanTree.BuildCodes(text);
                    Console.WriteLine($"\n  Коды Хаффмана для текста \"{text}\":");
                    Console.WriteLine("  ┌────────┬────────────────┐");
                    Console.WriteLine("  │ Символ │ Код Хаффмана   │");
                    Console.WriteLine("  ├────────┼────────────────┤");
                    foreach (var (ch, code) in codes.OrderBy(kv => kv.Key))
                    {
                        Console.WriteLine($"  │   '{ch}'  │ {code,-14} │");
                    }
                    Console.WriteLine("  └────────┴────────────────┘");

                    var encoded = string.Concat(text.Select(c => codes[c]));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  Закодированная битовая строка: {encoded}");
                    Console.WriteLine($"  Сжатие: {text.Length * 8} бит (ASCII) -> {encoded.Length} бит (Хаффман)");
                    Console.ResetColor();
                    break;
                }
        }
    }

    // =========================================================================
    // 4. 12 АЛГОРИТМОВ СВЯЗНОГО СПИСКА (ЧАСТЬ 4)
    // =========================================================================

    private static void RunListAlgorithmsMenu()
    {
        while (true)
        {
            PrintHeader("ЧАСТЬ 4: СВЯЗНЫЕ СТРУКТУРЫ (СПИСКИ И ДЕРЕВЬЯ)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Текущий список L: {_currentList}\n");
            Console.ResetColor();

            Console.WriteLine("  1  Ввести новый список целых чисел");
            Console.WriteLine("  2  Перевернуть список (изменить ссылки узлов на обратные)");
            Console.WriteLine("  3  Перенести последний элемент в начало");
            Console.WriteLine("  4  Перенести первый элемент в конец");
            Console.WriteLine("  5  Определить количество различных элементов");
            Console.WriteLine("  6  Удалить неуникальные элементы (выбор: все повторы или дедупликация)");
            Console.WriteLine("  7  Вставить список самого в себя вслед за первым вхождением числа X");
            Console.WriteLine("  8  Вставить новый элемент E в упорядоченный список (сохраняя порядок)");
            Console.WriteLine("  9  Удалить из списка все вхождения элемента E");
            Console.WriteLine("  10 Вставить элемент F перед первым вхождением элемента E");
            Console.WriteLine("  11 Дописать к списку L список E (считывание из файла data/lists.txt)");
            Console.WriteLine("  12 Разбить список на два по первому вхождению числа X");
            Console.WriteLine("  13 Удвоить список (приписать в конец себя самого)");
            Console.WriteLine("  14 Поменять местами два элемента (перестановкой ссылок узлов)");
            Console.WriteLine("  15 Построить бинарное дерево по строке обхода (ABD*G***CE**FH**J**)");
            Console.WriteLine("  0  Назад в главное меню");
            Console.WriteLine();

            try
            {
                string choice = Prompt("Выберите операцию");
                if (choice == "0") return;

                switch (choice)
                {
                    case "1":
                        string raw = Prompt("Введите целые числа через пробел (например: 1 5 2 3 5)");
                        _currentList = LinkedSequence<int>.From(ParseInts(raw));
                        break;

                    case "2":
                        _currentList.Reverse();
                        Console.WriteLine("  ✓ Список перевернут.");
                        break;

                    case "3":
                        _currentList.MoveLastToFront();
                        Console.WriteLine("  ✓ Последний элемент перенесен в начало.");
                        break;

                    case "4":
                        _currentList.MoveFirstToEnd();
                        Console.WriteLine("  ✓ Первый элемент перенесен в конец.");
                        break;

                    case "5":
                        int distinct = _currentList.DistinctCount();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  Количество различных элементов: {distinct}");
                        Console.ResetColor();
                        break;

                    case "6":
                        string mode = Prompt("Вариант удаления: 1 — удалить все повторяющиеся [1,2,2,3 -> 1,3], 2 — оставить по одной копии [1,2,2,3 -> 1,2,3] [1]");
                        if (mode == "2")
                        {
                            int removed = _currentList.RemoveDuplicates();
                            Console.WriteLine($"  Удалено дубликатов: {removed}");
                        }
                        else
                        {
                            int removed = _currentList.RemoveAllNonUnique();
                            Console.WriteLine($"  Удалено неуникальных элементов (всех их копий): {removed}");
                        }
                        break;

                    case "7":
                        int xVal = PromptInt("Введите число X, после которого вставить копию списка");
                        bool copyOk = _currentList.InsertCopyAfter(xVal);
                        Console.WriteLine(copyOk ? "  ✓ Копия списка успешно вставлена." : $"  Элемент {xVal} не найден в списке.");
                        break;

                    case "8":
                        // Проверка упорядоченности
                        var arr = _currentList.Values().ToArray();
                        bool isSorted = arr.SequenceEqual(arr.OrderBy(x => x));
                        if (!isSorted)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("  Внимание: текущий список не упорядочен по неубыванию. Отсортировать его перед вставкой? (y/n) [y]");
                            if (Prompt("") != "n")
                            {
                                _currentList = LinkedSequence<int>.From(arr.OrderBy(x => x));
                                Console.WriteLine($"  Список отсортирован: {_currentList}");
                            }
                        }
                        int eVal = PromptInt("Введите элемент E для упорядоченной вставки");
                        _currentList.InsertSorted(eVal);
                        Console.WriteLine($"  ✓ Элемент {eVal} вставлен с сохранением порядка.");
                        break;

                    case "9":
                        int valToRemove = PromptInt("Введите элемент E для удаления");
                        int rem = _currentList.RemoveAll(valToRemove);
                        Console.WriteLine($"  ✓ Удалено вхождений: {rem}");
                        break;

                    case "10":
                        int targetE = PromptInt("Введите элемент E (перед которым вставлять)");
                        int newF = PromptInt("Введите новый элемент F");
                        bool insOk = _currentList.InsertBefore(targetE, newF);
                        Console.WriteLine(insOk ? $"  ✓ Элемент {newF} вставлен перед {targetE}." : $"  Элемент {targetE} не найден в списке.");
                        break;

                    case "11":
                        string defaultListsPath = Path.Combine("data", "lists.txt");
                        string fpath = Prompt($"Путь к файлу с двумя списками [{defaultListsPath}]");
                        if (string.IsNullOrWhiteSpace(fpath)) fpath = defaultListsPath;

                        if (!File.Exists(fpath))
                        {
                            Console.WriteLine($"  Файл '{fpath}' не найден!");
                            break;
                        }

                        var lines = File.ReadAllLines(fpath);
                        if (lines.Length < 2)
                        {
                            throw new FormatException("Файл должен содержать как минимум 2 строки (список L и список E).");
                        }

                        _currentList = LinkedSequence<int>.From(ParseInts(lines[0]));
                        var listE = LinkedSequence<int>.From(ParseInts(lines[1]));
                        Console.WriteLine($"  Считан список L: {_currentList}");
                        Console.WriteLine($"  Считан список E: {listE}");

                        _currentList.AppendCopy(listE);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ Список E успешно приписан к L: {_currentList}");
                        Console.ResetColor();
                        break;

                    case "12":
                        int splitTarget = PromptInt("Введите число X для разбиения списка");
                        var secondList = _currentList.SplitAfter(splitTarget);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  Первый список (L1): {_currentList}");
                        Console.WriteLine($"  Второй список (L2): {secondList}");
                        Console.ResetColor();
                        break;

                    case "13":
                        _currentList.DuplicateSelf();
                        Console.WriteLine("  ✓ Список удвоен (приписан сам к себе).");
                        break;

                    case "14":
                        Console.WriteLine("  Обмен элементов ПЕРЕСТАНОВКОЙ ССЫЛОК узлов:");
                        Console.WriteLine("  1 — Порядковые номера (индексы с 1)");
                        Console.WriteLine("  2 — По значениям элементов");
                        string swapMode = Prompt("Выбор [1]");
                        if (swapMode == "2")
                        {
                            int valA = PromptInt("Значение первого элемента");
                            int valB = PromptInt("Значение второго элемента");
                            bool swapped = _currentList.SwapNodesByValues(valA, valB);
                            Console.WriteLine(swapped ? "  ✓ Узлы успешно перелинкованы." : "  Один или оба элемента не найдены.");
                        }
                        else
                        {
                            int idxA = PromptInt("Номер первого элемента (с 1)") - 1;
                            int idxB = PromptInt("Номер второго элемента (с 1)") - 1;
                            bool swapped = _currentList.SwapNodesByIndices(idxA, idxB);
                            Console.WriteLine(swapped ? "  ✓ Узлы успешно перелинкованы." : "  Индексы выходят за границы списка.");
                        }
                        break;

                    case "15":
                        RunBinaryTreeMenu();
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  Результат: L = {_currentList}");
                Console.ResetColor();
                Pause();
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [ОШИБКА]: {ex.Message}");
                Console.ResetColor();
                Pause();
            }
        }
    }

    // =========================================================================
    // БИНАРНОЕ ДЕРЕВО ПО СТРОКЕ ОБХОДА
    // =========================================================================

    private static void RunBinaryTreeMenu()
    {
        PrintHeader("ПОСТРОЕНИЕ БИНАРНОГО ДЕРЕВА ПО СТРОКЕ ОБХОДА");

        string defaultInput = "ABD*G***CE**FH**J**";
        Console.WriteLine($"  Строка префиксного обхода: {defaultInput}");
        Console.WriteLine("  Каждый символ — узел дерева; символ '*' обозначает NULL (пустой указатель).\n");

        string input = Prompt($"Введите строку префиксного обхода [{defaultInput}]");
        if (string.IsNullOrWhiteSpace(input)) input = defaultInput;

        var tree = BinaryTree<char>.BuildFromPreorderString(input);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  ✓ Дерево успешно построено в памяти!");
        Console.ResetColor();

        Console.WriteLine("\n  1. Псевдографическая структура дерева (ветви с метками L - левый, R - правый):");
        Console.WriteLine(tree.RenderVisualTree(showNulls: true));

        Console.WriteLine("\n  2. Таблица узлов и указателей структуры дерева:");
        Console.WriteLine(tree.RenderPointersTable());

        Console.WriteLine("\n  3. Обходы дерева:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  • Прямой обход (Pre-order, NLR):       {string.Join(" ", tree.PreOrder())}");
        Console.WriteLine($"  • Симметричный обход (In-order, LNR):  {string.Join(" ", tree.InOrder())}");
        Console.WriteLine($"  • Обратный обход (Post-order, LRN):    {string.Join(" ", tree.PostOrder())}");
        Console.WriteLine($"  • В ширину / по уровням (Level-order): {string.Join(" ", tree.LevelOrder())}");
        Console.ResetColor();

        string roundTrip = tree.ToPreorderString();
        Console.WriteLine($"\n  4. Обратная сериализация в строку со звездочками '*':");
        Console.WriteLine($"     Получено: {roundTrip}");
        Console.WriteLine($"     Исходное: {input}");
        if (roundTrip == input)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("     ✓ 100% совпадение структуры и обхода!");
            Console.ResetColor();
        }
    }

    // =========================================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ UI
    // =========================================================================

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"  ║  {title,-68} ║");
        Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static string Prompt(string message)
    {
        Console.Write($"  {message}: ");
        string? line = Console.ReadLine();
        if (line is null) throw new EndOfStreamException();
        return line.Trim();
    }

    private static int PromptInt(string message)
    {
        while (true)
        {
            string s = Prompt(message);
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                return val;
            }
            Console.WriteLine("  Пожалуйста, введите корректное целое число.");
        }
    }

    private static void Pause()
    {
        if (Console.IsInputRedirected) return;
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Нажмите Enter для продолжения... ");
        Console.ResetColor();
        Console.ReadLine();
    }

    private static int[] ParseInts(string text)
    {
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => int.Parse(x, CultureInfo.InvariantCulture))
                   .ToArray();
    }
}
