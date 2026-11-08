using AlgorithmsLab3.Core;
namespace AlgorithmsLab3.Applications;
public sealed class PacketBuffer(int capacity)
{
    private readonly LinkedLabQueue<string> queue = new(); private int count;
    public bool Receive(string packet) { if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity)); if (count == capacity) return false; queue.Enqueue(packet); count++; return true; }
    public string Process() { var item = queue.Dequeue(); count--; return item; }
    public IEnumerable<string> Pending() => queue.Values();
}
