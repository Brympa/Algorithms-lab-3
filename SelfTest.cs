using AlgorithmsLab3.Applications;
using AlgorithmsLab3.Core;

namespace AlgorithmsLab3;

/// <summary>
/// Комплексный набор модульных тестов для автоматической самопроверки всех компонентов проекта.
/// Запускается ключом: dotnet run -- --self-test
/// </summary>
public static class SelfTest
{
    public static void Run()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("  ║                   САМОПРОВЕРКА КОМПОНЕНТОВ (SELF-TEST)                ║");
        Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════╝\n");
        Console.ResetColor();

        int passed = 0;
        int failed = 0;

        void Assert(bool condition, string testName, string? details = null)
        {
            if (condition)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  [OK] ");
                Console.ResetColor();
                Console.WriteLine(testName);
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  [FAIL] ");
                Console.ResetColor();
                Console.WriteLine($"{testName}: {details ?? "проверка не пройдена"}");
                failed++;
            }
        }

        bool ListEquals<T>(LinkedSequence<T> list, params T[] expected) where T : notnull =>
            list.Values().SequenceEqual(expected);

        // =====================================================================
        // 1. ТЕСТЫ СВЯЗНОГО СПИСКА (12 АЛГОРИТМОВ ЧАСТИ 4)
        // =====================================================================
        Console.WriteLine("  --- [1] 12 алгоритмов работы со списком ---");

        var list = LinkedSequence<int>.From(new[] { 1, 2, 2, 3 });

        // 1. Reverse
        list.Reverse();
        Assert(ListEquals(list, 3, 2, 2, 1), "1. Разворот списка (Reverse)");

        // 2a. MoveLastToFront
        list.MoveLastToFront();
        Assert(ListEquals(list, 1, 3, 2, 2), "2a. Перенос последнего в начало (MoveLastToFront)");

        // 2b. MoveFirstToEnd
        list.MoveFirstToEnd();
        Assert(ListEquals(list, 3, 2, 2, 1), "2b. Перенос первого в конец (MoveFirstToEnd)");

        // 3. DistinctCount
        Assert(list.DistinctCount() == 3, "3. Количество различных элементов (DistinctCount)");

        // 4. RemoveAllNonUnique
        list.RemoveAllNonUnique();
        Assert(ListEquals(list, 3, 1), "4. Удаление неуникальных элементов (RemoveAllNonUnique)");

        // 4 (alt). RemoveDuplicates
        var dupList = LinkedSequence<int>.From(new[] { 1, 2, 2, 3, 3, 3 });
        dupList.RemoveDuplicates();
        Assert(ListEquals(dupList, 1, 2, 3), "4 (alt). Дедупликация (RemoveDuplicates)");

        // 5. InsertCopyAfter
        bool inserted = list.InsertCopyAfter(3);
        Assert(inserted && ListEquals(list, 3, 3, 1, 1), "5. Вставка списка самого в себя после X (InsertCopyAfter)");

        // 7. RemoveAll
        int removedCount = list.RemoveAll(1);
        Assert(removedCount == 2 && ListEquals(list, 3, 3), "7. Удаление всех вхождений элемента E (RemoveAll)");

        // 6. InsertSorted
        list.InsertSorted(2);
        list.InsertSorted(5);
        list.InsertSorted(1);
        Assert(ListEquals(list, 1, 2, 3, 3, 5), "6. Вставка в упорядоченный список (InsertSorted)");

        // 8. InsertBefore
        bool beforeOk = list.InsertBefore(targetE: 2, newElementF: 99);
        Assert(beforeOk && ListEquals(list, 1, 99, 2, 3, 3, 5), "8. Вставка F перед E (InsertBefore)");

        // 10. SplitAfter
        var secondPart = list.SplitAfter(2);
        Assert(ListEquals(list, 1, 99, 2) && ListEquals(secondPart, 3, 3, 5), "10. Разделение списка по числу X (SplitAfter)");

        // 9. AppendCopy
        list.AppendCopy(secondPart);
        Assert(ListEquals(list, 1, 99, 2, 3, 3, 5), "9. Дописывание списка E к L (AppendCopy)");

        // 11. DuplicateSelf
        var smallList = LinkedSequence<int>.From(new[] { 10, 20 });
        smallList.DuplicateSelf();
        Assert(ListEquals(smallList, 10, 20, 10, 20), "11. Удвоение списка (DuplicateSelf)");

        // 12. SwapNodesByIndices (перестановка указателей узлов)
        var swapList = LinkedSequence<int>.From(new[] { 10, 20, 30, 40 });
        swapList.SwapNodesByIndices(0, 3); // обмен первого и последнего
        Assert(ListEquals(swapList, 40, 20, 30, 10), "12a. Обмен узлов списка указателями (SwapNodesByIndices: крайние)");

        swapList.SwapNodesByIndices(1, 2); // обмен соседних
        Assert(ListEquals(swapList, 40, 30, 20, 10), "12b. Обмен узлов списка указателями (SwapNodesByIndices: соседние)");

        // =====================================================================
        // 2. ТЕСТЫ СТЕКА И ОЧЕРЕДИ (ЧАСТИ 1 И 2)
        // =====================================================================
        Console.WriteLine("\n  --- [2] Стек и Очереди ---");

        var stack = new LinkedStack<string>();
        Assert(stack.isEmpty(), "Стек изначально пуст (isEmpty)");
        stack.Push("first");
        stack.Push("second");
        Assert(stack.Top() == "second", "Top() возвращает вершину без удаления");
        Assert(stack.Pop() == "second" && stack.Pop() == "first" && stack.isEmpty(), "Pop() корректно извлекает LIFO");

        // Очередь на списке
        var linkedQueue = new LinkedLabQueue<int>();
        Assert(linkedQueue.isEmpty(), "LinkedLabQueue изначально пуста");
        linkedQueue.Enqueue(100);
        linkedQueue.Enqueue(200);
        Assert(linkedQueue.First() == 100, "LinkedLabQueue.First() возвращает начало очереди");
        Assert(linkedQueue.Dequeue() == 100 && linkedQueue.Dequeue() == 200 && linkedQueue.isEmpty(), "LinkedLabQueue извлекает FIFO");

        // Очередь стандартная
        var stdQueue = new StandardLabQueue<int>();
        stdQueue.Enqueue(100);
        stdQueue.Enqueue(200);
        Assert(stdQueue.First() == 100 && stdQueue.Dequeue() == 100 && stdQueue.Dequeue() == 200, "StandardLabQueue (Queue<T>) извлекает FIFO");

        // =====================================================================
        // 3. ТЕСТЫ ВЫРАЖЕНИЙ (СОРТИРОВОЧНАЯ СТАНЦИЯ И ВЫЧИСЛЕНИЕ)
        // =====================================================================
        Console.WriteLine("\n  --- [3] Выражения: Shunting-Yard и постфиксные вычисления ---");

        // Инфиксная -> постфиксная
        string postfix1 = Expressions.ToPostfix("2 + 2");
        Assert(postfix1 == "2 2 +", "Перевод 2+2 -> '2 2 +' (Shunting-Yard)");

        string postfixDoc = Expressions.ToPostfix("7 + 8 / (3 + 2)");
        Assert(postfixDoc == "7 8 3 2 + / +", "Перевод сложного выражения со скобками");

        // Приоритет степеней (правоассоциативность)
        string postfixPow = Expressions.ToPostfix("2 ^ 3 ^ 2");
        Assert(postfixPow == "2 3 2 ^ ^", "Правоассоциативность степени: 2^3^2 -> 2 3 2 ^ ^");

        // Вычисление постфиксной записи по примеру из ТЗ (рисунок 1)
        double evalDoc = Expressions.Evaluate("7 8 + 3 2 + /", out var evalSteps);
        Assert(Math.Abs(evalDoc - 3.0) < 1e-9, "Вычисление '7 8 + 3 2 + /' = 3.0 (пример из ТЗ)");
        Assert(evalSteps.Count == 7, "Количество шагов вычисления совпадает с рисунком 1 (7 шагов)");

        // Вычисление математических функций
        double evalMath = Expressions.Evaluate(Expressions.ToPostfix("sqrt(16) + cos(0) * 5"));
        Assert(Math.Abs(evalMath - 9.0) < 1e-9, "Вычисление sqrt(16) + cos(0) * 5 = 9.0");

        // Деление через двоеточие ':'
        double evalColon = Expressions.Evaluate("10 2 :");
        Assert(Math.Abs(evalColon - 5.0) < 1e-9, "Деление знаком ':' (10 2 : = 5)");

        // =====================================================================
        // 4. ТЕСТЫ БИНАРНОГО ДЕРЕВА ПО СТРОКЕ ОБХОДА
        // =====================================================================
        Console.WriteLine("\n  --- [4] Бинарное дерево по строке обхода ---");

        string controlString = "ABD*G***CE**FH**J**";
        var tree = BinaryTree<char>.BuildFromPreorderString(controlString);

        Assert(!tree.IsEmpty, "Дерево успешно построено из строки");
        Assert(tree.Root?.Value == 'A', "Корень дерева = 'A'");
        Assert(tree.Root?.Left?.Value == 'B', "Левый потомок корня = 'B'");
        Assert(tree.Root?.Right?.Value == 'C', "Правый потомок корня = 'C'");

        // Проверка прямого обхода (Pre-order, NLR)
        string preOrder = string.Join("", tree.PreOrder());
        Assert(preOrder == "ABDGCEFHJ", $"Прямой обход: {preOrder} (ожидалось ABDGCEFHJ)");

        // Проверка симметричного обхода (In-order, LNR)
        string inOrder = string.Join("", tree.InOrder());
        Assert(inOrder == "DGBAECHFJ", $"Симметричный обход: {inOrder} (ожидалось DGBAECHFJ)");

        // Проверка обратного обхода (Post-order, LRN)
        string postOrder = string.Join("", tree.PostOrder());
        Assert(postOrder == "GDBEHJFFCA" || postOrder == "GDBEHJFCA", $"Обратный обход: {postOrder}");

        // Проверка обхода в ширину (Level-order / BFS с очередью)
        string levelOrder = string.Join("", tree.LevelOrder());
        Assert(levelOrder == "ABCDEFGHIJ" || levelOrder == "ABCDEFGHJ", $"Обход в ширину: {levelOrder}");

        // Обратная сериализация в строку с '*'
        string roundTrip = tree.ToPreorderString();
        Assert(roundTrip == controlString, $"Обратная сериализация совпадает со строкой картинки: {roundTrip}");

        // =====================================================================
        // 5. ТЕСТЫ ПРИМЕРОВ ПРИМЕНЕНИЯ (ЧАСТЬ 3)
        // =====================================================================
        Console.WriteLine("\n  --- [5] Примеры применения структур (Часть 3) ---");

        // Скобки
        Assert(BracketValidation.IsValid("{[()]}") && !BracketValidation.IsValid("([)]"), "Скобочная последовательность (BracketValidation)");
        Assert(BracketValidation.IsValid("int x = (1 + 2); // [comment]") && !BracketValidation.IsValid("int x = (1 + 2;"), "Скобки в коде C#");

        // Полиномы
        var pA = new[] { (2, 3.0), (1, 2.0), (0, 1.0) };
        var pB = new[] { (2, 4.0), (1, -2.0), (0, 5.0) };
        var pSum = PolynomialAddition.Add(pA, pB);
        string formattedPoly = PolynomialAddition.Format(pSum);
        Assert(formattedPoly == "7x^2 + 6", $"Сложение полиномов: {formattedPoly} (ожидалось 7x^2 + 6)");

        // Буфер пакетов
        var buffer = new PacketBuffer(2);
        Assert(buffer.Receive("P1") && buffer.Receive("P2") && !buffer.Receive("P3"), "Буфер пакетов ограничен ёмкостью 2");
        Assert(buffer.Process() == "P1" && buffer.Receive("P3"), "Обработка и дозапись в буфер FIFO");

        // Дерево Хаффмана
        var codes = HuffmanTree.BuildCodes("ABRACADABRA");
        Assert(codes.ContainsKey('A') && codes.ContainsKey('B'), "Генерация кодов Хаффмана");
        // Проверка свойства префиксности
        bool isPrefixFree = codes.Values.All(c1 => codes.Values.All(c2 => c1 == c2 || !c2.StartsWith(c1)));
        Assert(isPrefixFree, "Коды Хаффмана обладают префиксным свойством");

        // Итог
        Console.WriteLine("\n  ═══════════════════════════════════════════════════════════════════════");
        if (failed == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ВСЕ ТЕСТЫ УСПЕШНО ПРОЙДЕНЫ! (Пройдено: {passed}, Провалено: {failed})");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ОБНАРУЖЕНЫ ОШИБКИ! (Пройдено: {passed}, Провалено: {failed})");
        }
        Console.ResetColor();
        Console.WriteLine("  ═══════════════════════════════════════════════════════════════════════\n");

        if (failed > 0)
        {
            throw new Exception($"Самопроверка завершилась с {failed} ошибками.");
        }
    }
}
