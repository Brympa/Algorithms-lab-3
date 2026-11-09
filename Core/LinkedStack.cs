namespace AlgorithmsLab3.Core;

/// <summary>
/// Реализация структуры данных "Стек" на базе собственного связного списка.
/// Поддерживает все обязательные операции по ТЗ: Push(elem), Pop(), Top(), isEmpty(), Print().
/// </summary>
public sealed class LinkedStack<T> where T : notnull
{
    private readonly LinkedSequence<T> _list = new();

    public int Count => _list.Count;

    public bool IsEmpty => _list.IsEmpty;

    /// <summary>
    /// Проверка на пустоту по требованию ТЗ: isEmpty()
    /// </summary>
    public bool isEmpty() => _list.IsEmpty;

    /// <summary>
    /// Помещение элемента на вершину стека (O(1)).
    /// </summary>
    public void Push(T value)
    {
        _list.AddFirst(value);
    }

    /// <summary>
    /// Извлечение элемента с вершины стека (O(1)).
    /// </summary>
    public T Pop()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Попытка выполнить Pop() из пустого стека.");
        }
        return _list.RemoveFirst();
    }

    /// <summary>
    /// Просмотр элемента на вершине стека без удаления (O(1)).
    /// </summary>
    public T Top()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Попытка выполнить Top() в пустом стеке.");
        }
        return _list.First;
    }

    /// <summary>
    /// Печать содержимого стека от вершины к основанию по требованию ТЗ: Print()
    /// </summary>
    public string Print()
    {
        if (isEmpty())
        {
            return "[ пусто ]";
        }
        return "[ " + string.Join(" → ", _list.Values()) + " ] (вершина слева)";
    }

    /// <summary>
    /// Очистить стек.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Перечисление элементов стека от вершины к основанию.
    /// </summary>
    public IEnumerable<T> Values() => _list.Values();

    public override string ToString() => Print();
}
