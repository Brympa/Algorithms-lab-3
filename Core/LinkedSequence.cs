namespace AlgorithmsLab3.Core;

public sealed class LinkedSequence<T> where T : notnull
{
    private sealed class Node(T value) { public T Value = value; public Node? Next; }
    private Node? head, tail;
    public int Count { get; private set; }
    public bool IsEmpty => head is null;
    public void AddFirst(T value) { var n = new Node(value) { Next = head }; head = n; tail ??= n; Count++; }
    public void AddLast(T value) { var n = new Node(value); if (tail is null) head = n; else tail.Next = n; tail = n; Count++; }
    public T First => head is null ? throw new InvalidOperationException("Структура пуста.") : head.Value;
    public T RemoveFirst() { var v = First; head = head!.Next; if (head is null) tail = null; Count--; return v; }
    public IEnumerable<T> Values() { for (var n = head; n != null; n = n.Next) yield return n.Value; }
    public override string ToString() => "[ " + string.Join(" → ", Values()) + " ]";

    public void Reverse() { 
        Node? prev = null, cur = head; 
        tail = head; 
        while (cur != null) { 
            var next = cur.Next; 
            cur.Next = prev; 
            prev = cur; 
            cur = next; 
        }
        head = prev; 
    }
    public void MoveLastToFront() { 
        if (Count < 2) return; 
        var p = head!; 
        while (p.Next != tail) p = p.Next!; 
        tail!.Next = head; 
        head = tail; 
        tail = p; 
        tail.Next = null; 
    }
    public void MoveFirstToEnd() { 
        if (Count < 2) return; 
        var n = head!; 
        head = n.Next; 
        n.Next = null; 
        tail!.Next = n; 
        tail = n; 
    }

    public int DistinctCount() => new HashSet<T>(Values()).Count;


    // Удаляются все вхождения значений, встречающихся более одного раза.
    public void RemoveNonUnique() { 
        var counts = new Dictionary<T, int>(EqualityComparer<T>.Default); 
        foreach (var v in Values()) { 
            if (v is null) throw new InvalidOperationException("Null не поддерживается."); 
            counts.TryGetValue(v, out int c); 
            counts[v] = c + 1;
        } RemoveWhere(v => counts[v] > 1); }


    private void RemoveWhere(Func<T, bool> predicate) { 
        Node? prev = null, cur = head;
        while (cur != null) { 
            var next = cur.Next; 
            if (predicate(cur.Value)) { 
                if (prev is null) head = next; 
                else prev.Next = next; 
                if (cur == tail) tail = prev; 
                Count--; 
            } 
            else prev = cur; 
            cur = next; 
        }
    }

    public int RemoveAll(T value) { int old = Count; RemoveWhere(v => EqualityComparer<T>.Default.Equals(v, value)); return old - Count; }

    public bool InsertBefore(T target, T value) { 
        Node? prev = null, cur = head; 
        while (cur != null && !EqualityComparer<T>.Default.Equals(cur.Value, target)) { 
            prev = cur; 
            cur = cur.Next; 
        } 
        if (cur is null) return false; 
        var n = new Node(value) { Next = cur }; 
        if (prev is null) head = n;
        else prev.Next = n; 
        Count++; 
        return true; }

    public bool InsertCopyAfter(T target) { 
        var cur = head; 
        while (cur != null && !EqualityComparer<T>.Default.Equals(cur.Value, target)) cur = cur.Next; 
        if (cur is null) return false; 
        var copy = Values().ToArray(); 
        var after = cur.Next; 
        foreach (var v in copy) { 
            cur.Next = new Node(v); 
            cur = cur.Next; Count++;
        } 
        cur.Next = after; 
        if (after is null) tail = cur; 
        return true; }

    public void InsertSorted(T value, IComparer<T>? comparer = null) { 
        comparer ??= Comparer<T>.Default; 
        Node? prev = null, cur = head; 
        while (cur != null && comparer.Compare(cur.Value, value) <= 0) { 
            prev = cur; 
            cur = cur.Next; 
        } 
        var n = new Node(value) { Next = cur };
        if (prev is null) head = n;
        else prev.Next = n; 
        if (cur is null) tail = n; 
        Count++; }
    
    public void AppendCopy(LinkedSequence<T> other) { 
        foreach (var v in other.Values().ToArray()) 
            AddLast(v); 
    }

    // Первое вхождение остаётся в первом списке, элементы после него переходят во второй.

    public LinkedSequence<T> SplitAfter(T value) { 
        var result = new LinkedSequence<T>(); 
        var n = head; 
        while (n != null && !EqualityComparer<T>.Default.Equals(n.Value, value)) n = n.Next; 
        if (n is null) return result; 
        result.head = n.Next; 
        n.Next = null; 
        result.tail = result.head is null ? null : tail; tail = n; 
        result.Count = result.Values().Count(); 
        Count -= result.Count; 
        return result;
    }

    public bool SwapAt(int a, int b) { 
        if (a < 0 || b < 0 || a >= Count || b >= Count) return false; 
        var x = head!; 
        var y = head!; 
        for (int i = 0; i < a; i++) x = x.Next!; 
        for (int i = 0; i < b; i++) y = y.Next!;
        (x.Value, y.Value) = (y.Value, x.Value); 
        return true;
    }

    public static LinkedSequence<T> From(IEnumerable<T> items) { var list = new LinkedSequence<T>(); foreach (var item in items) list.AddLast(item); return list; }
}

public sealed class LinkedStack<T> where T : notnull { 
    private readonly LinkedSequence<T> list = new(); 
    public int Count => list.Count; 
    public bool IsEmpty => list.IsEmpty; 
    
    public void Push(T value) => list.AddFirst(value); 
    public T Pop() => list.RemoveFirst(); 
    public T Top() => list.First; 
    public IEnumerable<T> Values() => list.Values(); 
}

public interface ILabQueue<T> { 
    bool IsEmpty { get; } 
    void Enqueue(T value); 
    T Dequeue(); 
    T Peek(); 
    IEnumerable<T> Values(); 
}
public sealed class LinkedLabQueue<T> : ILabQueue<T> where T : notnull { 
    private readonly LinkedSequence<T> list = new(); 
    public bool IsEmpty => list.IsEmpty; 
    public void Enqueue(T value) => list.AddLast(value); 
    public T Dequeue() => list.RemoveFirst(); 
    public T Peek() => list.First; 
    public IEnumerable<T> Values() => list.Values(); 
}
public sealed class StandardLabQueue<T> : ILabQueue<T> { 
    private readonly Queue<T> queue = new(); 
    public bool IsEmpty => queue.Count == 0; 
    public void Enqueue(T value) => queue.Enqueue(value); 
    public T Dequeue() => queue.Dequeue(); 
    public T Peek() => queue.Peek(); 
    public IEnumerable<T> Values() => queue; 
}
