using System.Collections;
using System.Text;

namespace AlgorithmsLab3.Core;

/// <summary>
/// Узел односвязного списка.
/// </summary>
public sealed class ListNode<T>
{
    public T Value { get; set; }
    public ListNode<T>? Next { get; set; }

    public ListNode(T value, ListNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }
}

/// <summary>
/// Собственный односвязный список с поддержкой всех 12 обязательных операций лабораторной работы.
/// </summary>
public sealed class LinkedSequence<T> : IEnumerable<T> where T : notnull
{
    private ListNode<T>? _head;
    private ListNode<T>? _tail;

    public int Count { get; private set; }

    public bool IsEmpty => _head is null;

    public ListNode<T>? Head => _head;
    public ListNode<T>? Tail => _tail;

    public T First => _head is null
        ? throw new InvalidOperationException("Список пуст.")
        : _head.Value;

    public T Last => _tail is null
        ? throw new InvalidOperationException("Список пуст.")
        : _tail.Value;

    /// <summary>
    /// Добавление элемента в начало списка (O(1)).
    /// </summary>
    public void AddFirst(T value)
    {
        var node = new ListNode<T>(value, _head);
        _head = node;
        _tail ??= node;
        Count++;
    }

    /// <summary>
    /// Добавление элемента в конец списка (O(1)).
    /// </summary>
    public void AddLast(T value)
    {
        var node = new ListNode<T>(value);
        if (_tail is null)
        {
            _head = node;
            _tail = node;
        }
        else
        {
            _tail.Next = node;
            _tail = node;
        }
        Count++;
    }

    /// <summary>
    /// Удаление первого элемента списка (O(1)).
    /// </summary>
    public T RemoveFirst()
    {
        if (_head is null)
        {
            throw new InvalidOperationException("Список пуст.");
        }

        T val = _head.Value;
        _head = _head.Next;
        if (_head is null)
        {
            _tail = null;
        }

        Count--;
        return val;
    }

    /// <summary>
    /// Очистка списка.
    /// </summary>
    public void Clear()
    {
        _head = null;
        _tail = null;
        Count = 0;
    }

    /// <summary>
    /// Возвращает перечисление значений списка.
    /// </summary>
    public IEnumerable<T> Values()
    {
        for (var current = _head; current != null; current = current.Next)
        {
            yield return current.Value;
        }
    }

    public IEnumerator<T> GetEnumerator() => Values().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        if (IsEmpty)
        {
            return "[ пусто ]";
        }
        return "[ " + string.Join(" → ", Values()) + " ]";
    }

    // =========================================================================
    // ЧАСТЬ 4. 12 АЛГОРИТМОВ РАБОТЫ СО СВЯЗНЫМИ СПИСКАМИ
    // =========================================================================

    /// <summary>
    /// 1. Переворачивает список L, изменяя ссылки в узлах на обратные (O(n) времени, O(1) памяти).
    /// </summary>
    public void Reverse()
    {
        if (Count <= 1) return;

        ListNode<T>? prev = null;
        ListNode<T>? current = _head;
        _tail = _head;

        while (current != null)
        {
            ListNode<T>? next = current.Next;
            current.Next = prev;
            prev = current;
            current = next;
        }

        _head = prev;
    }

    /// <summary>
    /// 2а. Переносит в начало непустого списка L его последний элемент (O(n)).
    /// </summary>
    public void MoveLastToFront()
    {
        if (Count <= 1) return;

        ListNode<T>? prev = null;
        ListNode<T> current = _head!;
        while (current.Next != null)
        {
            prev = current;
            current = current.Next;
        }

        // current - это последний узел (_tail), prev - предпоследний
        prev!.Next = null;
        _tail = prev;

        current.Next = _head;
        _head = current;
    }

    /// <summary>
    /// 2б. Переносит в конец непустого списка L его первый элемент (O(1)).
    /// </summary>
    public void MoveFirstToEnd()
    {
        if (Count <= 1) return;

        ListNode<T> first = _head!;
        _head = first.Next;
        first.Next = null;

        _tail!.Next = first;
        _tail = first;
    }

    /// <summary>
    /// 3. Определяет количество различных элементов списка (O(n)).
    /// </summary>
    public int DistinctCount()
    {
        var set = new HashSet<T>(EqualityComparer<T>.Default);
        foreach (var item in Values())
        {
            set.Add(item);
        }
        return set.Count;
    }

    /// <summary>
    /// 4. Удаляет из списка неуникальные элементы.
    /// Вариант А: Удаляет ВСЕ вхождения элементов, которые встречаются больше одного раза.
    /// Например: [1, 2, 2, 3] -> [1, 3].
    /// </summary>
    public int RemoveAllNonUnique()
    {
        var frequencies = new Dictionary<T, int>(EqualityComparer<T>.Default);
        foreach (var item in Values())
        {
            frequencies.TryGetValue(item, out int count);
            frequencies[item] = count + 1;
        }

        int removed = RemoveWhere(v => frequencies[v] > 1);
        return removed;
    }

    /// <summary>
    /// 4 (альтернатива). Удаляет повторные копии, оставляя по одному экземпляру каждого значения (дедупликация).
    /// Например: [1, 2, 2, 3] -> [1, 2, 3].
    /// </summary>
    public int RemoveDuplicates()
    {
        var seen = new HashSet<T>(EqualityComparer<T>.Default);
        int old = Count;
        RemoveWhere(v => !seen.Add(v));
        return old - Count;
    }

    /// <summary>
    /// 5. Вставляет копию списка самого в себя вслед за первым вхождением числа x (O(n)).
    /// </summary>
    public bool InsertCopyAfter(T target)
    {
        ListNode<T>? targetNode = _head;
        while (targetNode != null && !EqualityComparer<T>.Default.Equals(targetNode.Value, target))
        {
            targetNode = targetNode.Next;
        }

        if (targetNode is null)
        {
            return false;
        }

        // Создаем снимок исходных элементов
        T[] snapshot = Values().ToArray();
        ListNode<T>? rest = targetNode.Next;
        ListNode<T> current = targetNode;

        foreach (var val in snapshot)
        {
            var newNode = new ListNode<T>(val);
            current.Next = newNode;
            current = newNode;
            Count++;
        }

        current.Next = rest;
        if (rest is null)
        {
            _tail = current;
        }

        return true;
    }

    /// <summary>
    /// 6. Вставляет в непустой список L, упорядоченный по неубыванию, новый элемент E с сохранением порядка (O(n)).
    /// </summary>
    public void InsertSorted(T value, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;

        // Вставка перед головой, если значение меньше первого
        if (_head is null || comparer.Compare(value, _head.Value) <= 0)
        {
            AddFirst(value);
            return;
        }

        ListNode<T> prev = _head;
        ListNode<T>? current = _head.Next;

        while (current != null && comparer.Compare(current.Value, value) <= 0)
        {
            prev = current;
            current = current.Next;
        }

        var newNode = new ListNode<T>(value, current);
        prev.Next = newNode;
        if (current is null)
        {
            _tail = newNode;
        }

        Count++;
    }

    /// <summary>
    /// 7. Удаляет из списка L все элементы E, если таковые имеются (O(n)).
    /// </summary>
    public int RemoveAll(T value)
    {
        return RemoveWhere(v => EqualityComparer<T>.Default.Equals(v, value));
    }

    /// <summary>
    /// 8. Вставляет в список новый элемент F перед первым вхождением элемента E (O(n)).
    /// </summary>
    public bool InsertBefore(T targetE, T newElementF)
    {
        if (_head is null) return false;

        if (EqualityComparer<T>.Default.Equals(_head.Value, targetE))
        {
            AddFirst(newElementF);
            return true;
        }

        ListNode<T> prev = _head;
        ListNode<T>? current = _head.Next;

        while (current != null && !EqualityComparer<T>.Default.Equals(current.Value, targetE))
        {
            prev = current;
            current = current.Next;
        }

        if (current is null)
        {
            return false;
        }

        var newNode = new ListNode<T>(newElementF, current);
        prev.Next = newNode;
        Count++;
        return true;
    }

    /// <summary>
    /// 9. Дописывает к списку L копию списка E (O(k), где k — длина E).
    /// </summary>
    public void AppendCopy(LinkedSequence<T> other)
    {
        foreach (var val in other.Values().ToArray())
        {
            AddLast(val);
        }
    }

    /// <summary>
    /// 10. Разбивает список на два по первому вхождению заданного числа.
    /// Заданное число остаётся в первом списке; элементы после него переходят во второй список.
    /// Если число не найдено, второй список пуст, а первый не меняется.
    /// </summary>
    public LinkedSequence<T> SplitAfter(T target)
    {
        var second = new LinkedSequence<T>();
        ListNode<T>? current = _head;

        while (current != null && !EqualityComparer<T>.Default.Equals(current.Value, target))
        {
            current = current.Next;
        }

        if (current is null || current.Next is null)
        {
            return second;
        }

        // Переносим суффикс списка во второй список
        second._head = current.Next;
        second._tail = _tail;
        current.Next = null;
        _tail = current;

        // Пересчет количества элементов
        int secondCount = 0;
        for (var n = second._head; n != null; n = n.Next)
        {
            secondCount++;
        }

        second.Count = secondCount;
        Count -= secondCount;

        return second;
    }

    /// <summary>
    /// 11. Удваивает список, т.е. приписывает в конец списка себя самого.
    /// </summary>
    public void DuplicateSelf()
    {
        if (IsEmpty) return;
        AppendCopy(this);
    }

    /// <summary>
    /// 12. Меняет местами два элемента списка по их порядковым номерам (индексам с 0)
    /// ПУТЕМ ПЕРЕСТАНОВКИ УКАЗАТЕЛЕЙ (ссылок Next узлов), а не просто значений!
    /// </summary>
    public bool SwapNodesByIndices(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= Count || indexB < 0 || indexB >= Count)
        {
            return false;
        }

        if (indexA == indexB)
        {
            return true;
        }

        if (indexA > indexB)
        {
            (indexA, indexB) = (indexB, indexA);
        }

        // Поиск узлов и их предшественников
        ListNode<T>? prevA = null, nodeA = _head;
        for (int i = 0; i < indexA; i++)
        {
            prevA = nodeA;
            nodeA = nodeA!.Next;
        }

        ListNode<T>? prevB = null, nodeB = _head;
        for (int i = 0; i < indexB; i++)
        {
            prevB = nodeB;
            nodeB = nodeB!.Next;
        }

        if (nodeA is null || nodeB is null)
        {
            return false;
        }

        // Перелинковка узлов
        if (nodeA.Next == nodeB)
        {
            // Узлы идут подряд: prevA -> nodeA -> nodeB -> nextB
            ListNode<T>? nextB = nodeB.Next;

            if (prevA is null)
            {
                _head = nodeB;
            }
            else
            {
                prevA.Next = nodeB;
            }

            nodeB.Next = nodeA;
            nodeA.Next = nextB;

            if (nodeB == _tail) _tail = nodeA;
            else if (nodeA == _tail) _tail = nodeB;
        }
        else
        {
            // Узлы не соседние
            ListNode<T>? nextA = nodeA.Next;
            ListNode<T>? nextB = nodeB.Next;

            if (prevA is null)
            {
                _head = nodeB;
            }
            else
            {
                prevA.Next = nodeB;
            }

            prevB!.Next = nodeA;

            nodeA.Next = nextB;
            nodeB.Next = nextA;

            if (nodeB == _tail) _tail = nodeA;
        }

        // Обновление хвоста
        if (_head is not null)
        {
            var p = _head;
            while (p.Next != null) p = p.Next;
            _tail = p;
        }

        return true;
    }

    /// <summary>
    /// 12 (альтернатива). Меняет местами первые вхождения двух элементов по их значениям (перестановкой указателей).
    /// </summary>
    public bool SwapNodesByValues(T valA, T valB)
    {
        if (EqualityComparer<T>.Default.Equals(valA, valB))
        {
            return true;
        }

        int idxA = -1, idxB = -1;
        int i = 0;
        for (var n = _head; n != null; n = n.Next, i++)
        {
            if (idxA == -1 && EqualityComparer<T>.Default.Equals(n.Value, valA))
            {
                idxA = i;
            }
            else if (idxB == -1 && EqualityComparer<T>.Default.Equals(n.Value, valB))
            {
                idxB = i;
            }
        }

        if (idxA == -1 || idxB == -1)
        {
            return false;
        }

        return SwapNodesByIndices(idxA, idxB);
    }

    /// <summary>
    /// Вспомогательное удаление элементов по условию предикату.
    /// </summary>
    private int RemoveWhere(Func<T, bool> predicate)
    {
        int initialCount = Count;
        ListNode<T>? prev = null;
        ListNode<T>? current = _head;

        while (current != null)
        {
            ListNode<T>? next = current.Next;
            if (predicate(current.Value))
            {
                if (prev is null)
                {
                    _head = next;
                }
                else
                {
                    prev.Next = next;
                }

                if (current == _tail)
                {
                    _tail = prev;
                }

                Count--;
            }
            else
            {
                prev = current;
            }

            current = next;
        }

        return initialCount - Count;
    }

    /// <summary>
    /// Фабричный метод создания списка из перечисления.
    /// </summary>
    public static LinkedSequence<T> From(IEnumerable<T> items)
    {
        var list = new LinkedSequence<T>();
        foreach (var item in items)
        {
            list.AddLast(item);
        }
        return list;
    }
}
