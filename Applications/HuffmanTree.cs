namespace AlgorithmsLab3.Applications;

/// <summary>
/// Пример применения структуры данных "Деревья" (Часть 3 ТЗ):
/// Построение оптимального префиксного кода сжатия данных методом Хаффмана.
/// </summary>
public static class HuffmanTree
{
    public sealed class HuffmanNode
    {
        public char? Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode? Left { get; set; }
        public HuffmanNode? Right { get; set; }

        public HuffmanNode(char? symbol, int frequency, HuffmanNode? left = null, HuffmanNode? right = null)
        {
            Symbol = symbol;
            Frequency = frequency;
            Left = left;
            Right = right;
        }

        public bool IsLeaf => Left is null && Right is null;
    }

    /// <summary>
    /// Построение таблицы кодов Хаффмана для заданного текста.
    /// </summary>
    public static Dictionary<char, string> BuildCodes(string text)
    {
        var result = new Dictionary<char, string>();
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        // 1. Подсчет частот символов
        var frequencies = new Dictionary<char, int>();
        foreach (char c in text)
        {
            frequencies.TryGetValue(c, out int count);
            frequencies[c] = count + 1;
        }

        // Если в строке только один уникальный символ
        if (frequencies.Count == 1)
        {
            result[frequencies.Keys.First()] = "0";
            return result;
        }

        // 2. Инициализация очереди с приоритетом узлов
        var priorityQueue = new PriorityQueue<HuffmanNode, (int Freq, int TieBreaker)>();
        int order = 0;

        foreach (var kvp in frequencies.OrderBy(k => k.Key))
        {
            var node = new HuffmanNode(kvp.Key, kvp.Value);
            priorityQueue.Enqueue(node, (node.Frequency, order++));
        }

        // 3. Построение дерева слиянием двух узлов с наименьшими частотами
        while (priorityQueue.Count > 1)
        {
            var left = priorityQueue.Dequeue();
            var right = priorityQueue.Dequeue();

            var parent = new HuffmanNode(
                symbol: null,
                frequency: left.Frequency + right.Frequency,
                left: left,
                right: right
            );

            priorityQueue.Enqueue(parent, (parent.Frequency, order++));
        }

        var root = priorityQueue.Dequeue();

        // 4. Рекурсивный обход дерева для генерации префиксных кодов
        void GenerateCodes(HuffmanNode node, string currentCode)
        {
            if (node.IsLeaf && node.Symbol.HasValue)
            {
                result[node.Symbol.Value] = currentCode.Length == 0 ? "0" : currentCode;
                return;
            }

            if (node.Left != null)
            {
                GenerateCodes(node.Left, currentCode + "0");
            }
            if (node.Right != null)
            {
                GenerateCodes(node.Right, currentCode + "1");
            }
        }

        GenerateCodes(root, "");
        return result;
    }
}
