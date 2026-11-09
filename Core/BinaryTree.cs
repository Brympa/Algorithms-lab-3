using System.Text;
using AlgorithmsLab3.UI;

namespace AlgorithmsLab3.Core;

/// <summary>
/// Узел бинарного дерева.
/// Содержит значение и ссылки на левое и правое поддеревья.
/// </summary>
public sealed class BinaryTreeNode<T>
{
    public T Value { get; set; }
    public BinaryTreeNode<T>? Left { get; set; }
    public BinaryTreeNode<T>? Right { get; set; }

    public BinaryTreeNode(T value, BinaryTreeNode<T>? left = null, BinaryTreeNode<T>? right = null)
    {
        Value = value;
        Left = left;
        Right = right;
    }

    public bool IsLeaf => Left is null && Right is null;

    public override string ToString() => Value?.ToString() ?? "null";
}

/// <summary>
/// Бинарное дерево с поддержкой построения по строке прямого обхода с маркерами '*'.
/// Пример строки: "ABD*G***CE**FH**J**".
/// </summary>
public sealed class BinaryTree<T>
{
    public BinaryTreeNode<T>? Root { get; set; }

    public BinaryTree(BinaryTreeNode<T>? root = null)
    {
        Root = root;
    }

    public bool IsEmpty => Root is null;

    /// <summary>
    /// Построение бинарного дерева по строке прямого обхода с маркером null-узлов.
    /// </summary>
    /// <param name="preorderString">Строка обхода, например "ABD*G***CE**FH**J**"</param>
    /// <param name="nullMarker">Символ пустого поддерева (по умолчанию '*')</param>
    public static BinaryTree<char> BuildFromPreorderString(string preorderString, char nullMarker = '*')
    {
        if (string.IsNullOrWhiteSpace(preorderString))
        {
            return new BinaryTree<char>(null);
        }

        int index = 0;
        BinaryTreeNode<char>? root = ParsePreorder(preorderString, ref index, nullMarker);

        if (index != preorderString.Length)
        {
            throw new FormatException(
                $"Лишние символы после конца дерева в позиции {index + 1}: '{preorderString[index..]}'.");
        }

        return new BinaryTree<char>(root);
    }

    private static BinaryTreeNode<char>? ParsePreorder(string s, ref int index, char nullMarker)
    {
        if (index >= s.Length)
        {
            return null;
        }

        char ch = s[index++];
        if (ch == nullMarker)
        {
            return null;
        }

        var node = new BinaryTreeNode<char>(ch);
        node.Left = ParsePreorder(s, ref index, nullMarker);
        node.Right = ParsePreorder(s, ref index, nullMarker);

        return node;
    }

    /// <summary>
    /// Преобразует дерево обратно в строку прямого обхода со звездочками '*'.
    /// Для контрольного дерева результат совпадает с исходной строкой "ABD*G***CE**FH**J**".
    /// </summary>
    public string ToPreorderString(char nullMarker = '*')
    {
        var sb = new StringBuilder();
        Serialize(Root, sb, nullMarker);
        return sb.ToString();

        static void Serialize(BinaryTreeNode<T>? node, StringBuilder sb, char nullMarker)
        {
            if (node is null)
            {
                sb.Append(nullMarker);
                return;
            }

            sb.Append(node.Value);
            Serialize(node.Left, sb, nullMarker);
            Serialize(node.Right, sb, nullMarker);
        }
    }

    // =========================================================================
    // ОБХОДЫ ДЕРЕВА
    // =========================================================================

    /// <summary>
    /// Прямой обход (Pre-order: Корень -> Левый -> Правый).
    /// </summary>
    public List<T> PreOrder()
    {
        var result = new List<T>();
        Traverse(Root);
        return result;

        void Traverse(BinaryTreeNode<T>? node)
        {
            if (node is null) return;
            result.Add(node.Value);
            Traverse(node.Left);
            Traverse(node.Right);
        }
    }

    /// <summary>
    /// Центрированный / симметричный обход (In-order: Левый -> Корень -> Правый).
    /// </summary>
    public List<T> InOrder()
    {
        var result = new List<T>();
        Traverse(Root);
        return result;

        void Traverse(BinaryTreeNode<T>? node)
        {
            if (node is null) return;
            Traverse(node.Left);
            result.Add(node.Value);
            Traverse(node.Right);
        }
    }

    /// <summary>
    /// Обратный обход (Post-order: Левый -> Правый -> Корень).
    /// </summary>
    public List<T> PostOrder()
    {
        var result = new List<T>();
        Traverse(Root);
        return result;

        void Traverse(BinaryTreeNode<T>? node)
        {
            if (node is null) return;
            Traverse(node.Left);
            Traverse(node.Right);
            result.Add(node.Value);
        }
    }

    /// <summary>
    /// Обход в ширину / по уровням (Level-order / BFS).
    /// Демонстрирует практическое применение собственной структуры данных LinkedLabQueue!
    /// </summary>
    public List<T> LevelOrder()
    {
        var result = new List<T>();
        if (Root is null) return result;

        var queue = new LinkedLabQueue<BinaryTreeNode<T>>();
        queue.Enqueue(Root);

        while (!queue.isEmpty())
        {
            var node = queue.Dequeue();
            result.Add(node.Value);

            if (node.Left != null)
            {
                queue.Enqueue(node.Left);
            }
            if (node.Right != null)
            {
                queue.Enqueue(node.Right);
            }
        }

        return result;
    }

    // =========================================================================
    // ВИЗУАЛИЗАЦИЯ ДЕРЕВА
    // =========================================================================

    /// <summary>
    /// Формирует наглядный псевдографический вывод структуры дерева в консоль с ветками.
    /// </summary>
    public string RenderVisualTree(bool showNulls = true)
    {
        if (Root is null)
        {
            return "  [ Пустое дерево ]";
        }

        var sb = new StringBuilder();
        BuildBranch(Root, "  ", isLast: true, sb, label: "Root", showNulls: showNulls);
        return sb.ToString();

        static void BuildBranch(BinaryTreeNode<T>? node, string indent, bool isLast, StringBuilder sb, string label, bool showNulls)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");

            if (node is null)
            {
                if (showNulls)
                {
                    sb.AppendLine($"({label}: NULL)");
                }
                return;
            }

            sb.AppendLine($"[{label}] {node.Value}");

            string childIndent = indent + (isLast ? "    " : "│   ");

            bool hasLeft = node.Left != null;
            bool hasRight = node.Right != null;

            if (hasLeft || showNulls)
            {
                BuildBranch(node.Left, childIndent, isLast: !hasRight && !showNulls, sb, "L", showNulls);
            }

            if (hasRight || showNulls)
            {
                BuildBranch(node.Right, childIndent, isLast: true, sb, "R", showNulls);
            }
        }
    }

    /// <summary>
    /// Выводит таблицу узлов с указателями на левого и правого потомка.
    /// </summary>
    public string RenderPointersTable()
    {
        if (Root is null)
        {
            return "  Дерево пусто.";
        }

        var table = new ConsoleTable(
            new[] { "Узел", "Левый (Left)", "Правый (Right)" },
            ColumnAlignment.Center,
            ColumnAlignment.Center,
            ColumnAlignment.Center);

        var queue = new LinkedLabQueue<BinaryTreeNode<T>>();
        queue.Enqueue(Root);

        while (!queue.isEmpty())
        {
            var node = queue.Dequeue();
            string val = node.Value?.ToString() ?? "";
            string left = node.Left != null ? node.Left.Value?.ToString() ?? "" : "NULL";
            string right = node.Right != null ? node.Right.Value?.ToString() ?? "" : "NULL";

            table.AddRow(val, left, right);

            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }

        return table.Render();
    }
}
