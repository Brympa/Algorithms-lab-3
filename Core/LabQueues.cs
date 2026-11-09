namespace AlgorithmsLab3.Core;

/// <summary>
/// Общий интерфейс очереди для лабораторной работы.
/// Поддерживает операции по ТЗ: вставка, удаление, проверка на пустоту, печать, вывод первого элемента.
/// </summary>
public interface ILabQueue<T>
{
    int Count { get; }
    bool IsEmpty { get; }

    /// <summary>
    /// Проверка на пустоту.
    /// </summary>
    bool isEmpty();

    /// <summary>
    /// Вставка элемента в конец очереди (O(1)).
    /// </summary>
    void Enqueue(T value);

    /// <summary>
    /// Удаление элемента из начала очереди (O(1)).
    /// </summary>
    T Dequeue();

    /// <summary>
    /// Просмотр/вывод первого элемента очереди без удаления (O(1)).
    /// </summary>
    T First();

    /// <summary>
    /// Псевдоним для First() в стиле .NET.
    /// </summary>
    T Peek();

    /// <summary>
    /// Печать содержимого очереди от начала к концу.
    /// </summary>
    string Print();

    /// <summary>
    /// Перечисление элементов очереди от начала к концу.
    /// </summary>
    IEnumerable<T> Values();
}

/// <summary>
/// Очередь на базе собственного связного списка LinkedSequence.
/// </summary>
public sealed class LinkedLabQueue<T> : ILabQueue<T> where T : notnull
{
    private readonly LinkedSequence<T> _list = new();

    public int Count => _list.Count;
    public bool IsEmpty => _list.IsEmpty;
    public bool isEmpty() => _list.IsEmpty;

    public void Enqueue(T value)
    {
        _list.AddLast(value);
    }

    public T Dequeue()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Попытка удаления из пустой очереди.");
        }
        return _list.RemoveFirst();
    }

    public T First()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Очередь пуста.");
        }
        return _list.First;
    }

    public T Peek() => First();

    public string Print()
    {
        if (isEmpty())
        {
            return "[ пусто ]";
        }
        return "[ " + string.Join(" → ", _list.Values()) + " ] (начало слева)";
    }

    public IEnumerable<T> Values() => _list.Values();

    public override string ToString() => Print();
}

/// <summary>
/// Очередь на базе стандартного класса System.Collections.Generic.Queue.
/// </summary>
public sealed class StandardLabQueue<T> : ILabQueue<T>
{
    private readonly Queue<T> _queue = new();

    public int Count => _queue.Count;
    public bool IsEmpty => _queue.Count == 0;
    public bool isEmpty() => _queue.Count == 0;

    public void Enqueue(T value)
    {
        _queue.Enqueue(value);
    }

    public T Dequeue()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Попытка удаления из пустой очереди.");
        }
        return _queue.Dequeue();
    }

    public T First()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Очередь пуста.");
        }
        return _queue.Peek();
    }

    public T Peek() => First();

    public string Print()
    {
        if (isEmpty())
        {
            return "[ пусто ]";
        }
        return "[ " + string.Join(" → ", _queue) + " ] (начало слева)";
    }

    public IEnumerable<T> Values() => _queue;

    public override string ToString() => Print();
}
