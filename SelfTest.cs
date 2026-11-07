using AlgorithmsLab3.Core;
using AlgorithmsLab3.Applications;
static class SelfTest
{
    public static void Run() {
        void Check(bool ok, string name) { if (!ok) throw new Exception("Ошибка проверки: " + name); Console.WriteLine("OK " + name); }
        bool Eq(LinkedSequence<int> l, params int[] v) => l.Values().SequenceEqual(v);
        var l = LinkedSequence<int>.From(new[] { 1, 2, 2, 3 }); l.Reverse(); Check(Eq(l, 3, 2, 2, 1), "reverse"); l.MoveLastToFront(); Check(Eq(l, 1, 3, 2, 2), "last to front"); l.MoveFirstToEnd(); Check(Eq(l, 3, 2, 2, 1), "first to end"); Check(l.DistinctCount() == 3, "distinct"); l.RemoveNonUnique(); Check(Eq(l, 3, 1), "remove nonunique"); Check(l.InsertCopyAfter(3) && Eq(l, 3, 3, 1, 1), "insert copy"); l.RemoveAll(1); Check(Eq(l, 3, 3), "remove all"); l.InsertSorted(2); Check(Eq(l, 2, 3, 3), "insert sorted"); Check(l.InsertBefore(2, 1) && Eq(l, 1, 2, 3, 3), "insert before"); var second = l.SplitAfter(2); Check(Eq(l, 1, 2) && Eq(second, 3, 3), "split"); l.AppendCopy(second); l.AppendCopy(l); Check(Eq(l, 1, 2, 3, 3, 1, 2, 3, 3), "append/double"); l.SwapAt(0, 1); Check(Eq(l, 2, 1, 3, 3, 1, 2, 3, 3), "swap");
        var missing = l.SplitAfter(99); Check(missing.IsEmpty && l.Count == 8 && !l.InsertBefore(99, 1) && !l.InsertCopyAfter(99), "missing targets");
        Check(Math.Abs(Expressions.Evaluate("7 8 + 3 2 + /") - 3) < 1e-10, "postfix"); Check(Math.Abs(Expressions.Evaluate(Expressions.ToPostfix("-2^2 + sqrt(9) + 2^3^2")) - 511) < 1e-10, "infix precedence");
        foreach (bool standard in new[] { false, true }) { ILabQueue<int> q = standard ? new StandardLabQueue<int>() : new LinkedLabQueue<int>(); q.Enqueue(1); q.Enqueue(2); Check(q.Dequeue() == 1 && q.Peek() == 2 && q.Dequeue() == 2 && q.IsEmpty, "queue " + standard); }
        var s = new LinkedStack<int>(); s.Push(1); s.Push(2); Check(s.Pop() == 2 && s.Top() == 1, "stack");
        Check(BracketValidation.IsValid("{[()]}") && !BracketValidation.IsValid("([)]"), "brackets"); Check(BracketValidation.IsValid("Console.WriteLine(\")\"); // ]") && !BracketValidation.IsValid("/* unfinished"), "brackets in code"); var p = new PacketBuffer(1); Check(p.Receive("a") && !p.Receive("b") && p.Process() == "a", "packet"); Check(HuffmanTree.BuildCodes("aaa")['a'] == "0", "huffman single"); var codes = HuffmanTree.BuildCodes("abbccc").Values.ToArray(); Check(codes.All(a => codes.All(b => a == b || !b.StartsWith(a))), "huffman prefix"); Check(PolynomialAddition.Add(new[] { (2, 1d), (0, 1d) }, new[] { (2, -1d), (1, 2d) }).Values().SequenceEqual(new[] { (1, 2d), (0, 1d) }), "polynomial");
        Check(OperationRunner.Parse("3 4 1,cat 2 5").Length == 5, "commands"); Console.WriteLine("Все проверки пройдены.");
    }
}
