using AlgorithmsLab3.Core;

namespace AlgorithmsLab3.Applications;

/// <summary>
/// Пример применения структуры данных "Очередь" (Часть 3 ТЗ):
/// Буфер сетевого коммутатора / маршрутизатора с политикой FIFO (First-In, First-Out)
/// и ограниченной ёмкостью (Drop-Tail при переполнении).
/// Реализован на базе собственной очереди LinkedLabQueue.
/// </summary>
public sealed class PacketBuffer
{
    private readonly LinkedLabQueue<string> _queue = new();
    public int Capacity { get; }

    public int CurrentCount => _queue.Count;
    public bool IsEmpty => _queue.isEmpty();
    public bool IsFull => _queue.Count >= Capacity;

    public PacketBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Вместимость буфера должна быть больше 0.");
        }
        Capacity = capacity;
    }

    /// <summary>
    /// Прием входящего сетевого пакета. Если буфер полон, пакет сбрасывается (Drop-Tail).
    /// </summary>
    public bool Receive(string packet)
    {
        if (IsFull)
        {
            return false; // Пакет отброшен из-за переполнения буфера
        }

        _queue.Enqueue(packet);
        return true;
    }

    /// <summary>
    /// Обработка и отправка следующего пакета из очереди.
    /// </summary>
    public string? Process()
    {
        if (IsEmpty)
        {
            return null;
        }

        return _queue.Dequeue();
    }

    /// <summary>
    /// Список пакетов, ожидающих в буфере.
    /// </summary>
    public IEnumerable<string> PendingPackets() => _queue.Values();
}
