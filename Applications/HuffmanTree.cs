namespace AlgorithmsLab3.Applications;
public static class HuffmanTree
{
    private sealed record Node(char? Symbol, int Frequency, Node? Left = null, Node? Right = null);
    public static IReadOnlyDictionary<char, string> BuildCodes(string text)
    {
        var queue = new PriorityQueue<Node, (int Frequency, int Order)>(); 
        int order = 0;
        foreach (var g in text.GroupBy(c => c).OrderBy(g => g.Key)) { 
            var n = new Node(g.Key, g.Count()); 
            queue.Enqueue(n, (n.Frequency, order++)); 
        }
        while (queue.Count > 1) { 
            var l = queue.Dequeue(); 
            var r = queue.Dequeue(); 
            var n = new Node(null, l.Frequency + r.Frequency, l, r); 
            queue.Enqueue(n, (n.Frequency, order++)); 
        }
        var result = new Dictionary<char, string>(); 
        if (queue.Count == 0) return result;
        void Walk(Node n, string code) { 
            if (n.Symbol is char c) { result[c] = code.Length == 0 ? "0" : code; return; } Walk(n.Left!, code + "0"); Walk(n.Right!, code + "1"); }
        Walk(queue.Dequeue(), ""); return result;
    }
}
