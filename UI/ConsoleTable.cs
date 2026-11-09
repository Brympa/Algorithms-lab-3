using System.Text;

namespace AlgorithmsLab3.UI;

/// <summary>
/// Выравнивание содержимого колонки таблицы.
/// </summary>
public enum ColumnAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Универсальная консольная таблица с автоматическим расчётом ширины колонок.
///
/// Главное свойство: таблица никогда не «разъезжается». Если суммарная ширина
/// колонок больше ширины консоли, самые широкие колонки сжимаются, а длинный
/// текст переносится на несколько строк внутри ячейки.
/// </summary>
public sealed class ConsoleTable
{
    private readonly string[] _headers;
    private readonly List<string[]> _rows = new();
    private readonly ColumnAlignment[] _alignments;

    /// <summary>Отступ слева перед таблицей.</summary>
    public string Indent { get; init; } = "  ";

    /// <summary>
    /// Максимальная ширина таблицы. Если не задана, берётся ширина консоли
    /// (с безопасным запасным значением для перенаправленного вывода).
    /// </summary>
    public int? MaxWidth { get; init; }

    /// <summary>Минимальная ширина одной колонки при сжатии.</summary>
    public int MinColumnWidth { get; init; } = 3;

    public ConsoleTable(string[] headers, params ColumnAlignment[] alignments)
    {
        ArgumentNullException.ThrowIfNull(headers);
        if (headers.Length == 0)
        {
            throw new ArgumentException("Таблица должна содержать хотя бы одну колонку.", nameof(headers));
        }

        _headers = headers;
        _alignments = new ColumnAlignment[headers.Length];
        for (int i = 0; i < _alignments.Length; i++)
        {
            _alignments[i] = i < alignments.Length ? alignments[i] : ColumnAlignment.Left;
        }
    }

    public void AddRow(params string[] cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        if (cells.Length != _headers.Length)
        {
            throw new ArgumentException(
                $"Ожидалось {_headers.Length} ячеек, передано {cells.Length}.",
                nameof(cells));
        }

        _rows.Add(cells.Select(c => c ?? string.Empty).ToArray());
    }

    /// <summary>
    /// Формирует готовую к выводу таблицу. Возвращаемая строка не заканчивается
    /// переводом строки, поэтому её удобно передавать в Console.WriteLine.
    /// </summary>
    public string Render()
    {
        int columnCount = _headers.Length;
        int[] widths = CalculateColumnWidths();

        var sb = new StringBuilder();
        AppendBorder(sb, '┌', '┬', '┐', widths);
        AppendRow(sb, _headers, widths, header: true);
        AppendBorder(sb, '├', '┼', '┤', widths);

        foreach (string[] row in _rows)
        {
            AppendRow(sb, row, widths, header: false);
        }

        AppendBorder(sb, '└', '┴', '┘', widths);
        return sb.ToString().TrimEnd('\r', '\n');
    }

    private int[] CalculateColumnWidths()
    {
        int columnCount = _headers.Length;
        int[] widths = new int[columnCount];

        for (int i = 0; i < columnCount; i++)
        {
            widths[i] = DisplayWidth(_headers[i]);
            foreach (string[] row in _rows)
            {
                widths[i] = Math.Max(widths[i], DisplayWidth(row[i]));
            }
        }

        int indentWidth = DisplayWidth(Indent);
        int maxWidth = MaxWidth ?? UiLayout.ConsoleWidth;
        int available = Math.Max(columnCount * (MinColumnWidth + 2) + columnCount + 1, maxWidth - indentWidth);

        int TotalWidth()
        {
            int total = columnCount + 1; // вертикальные линии
            for (int i = 0; i < columnCount; i++)
            {
                total += widths[i] + 2; // содержимое + отступы по краям
            }
            return total;
        }

        int excess = TotalWidth() - available;
        while (excess > 0)
        {
            int widest = -1;
            for (int i = 0; i < columnCount; i++)
            {
                if (widths[i] <= MinColumnWidth) continue;
                if (widest < 0 || widths[i] > widths[widest]) widest = i;
            }

            if (widest < 0) break;

            widths[widest]--;
            excess--;
        }

        return widths;
    }

    private void AppendBorder(StringBuilder sb, char left, char middle, char right, int[] widths)
    {
        sb.Append(Indent).Append(left);
        for (int i = 0; i < widths.Length; i++)
        {
            sb.Append(new string('─', widths[i] + 2));
            sb.Append(i == widths.Length - 1 ? right : middle);
        }
        sb.AppendLine();
    }

    private void AppendRow(StringBuilder sb, string[] cells, int[] widths, bool header)
    {
        // Сначала переносим каждую ячейку на нужное число строк.
        List<string>[] wrapped = new List<string>[cells.Length];
        int rowHeight = 1;

        for (int i = 0; i < cells.Length; i++)
        {
            wrapped[i] = WrapText(cells[i], widths[i]);
            rowHeight = Math.Max(rowHeight, wrapped[i].Count);
        }

        for (int line = 0; line < rowHeight; line++)
        {
            sb.Append(Indent).Append('│');
            for (int col = 0; col < cells.Length; col++)
            {
                string text = line < wrapped[col].Count ? wrapped[col][line] : string.Empty;
                ColumnAlignment alignment = header ? ColumnAlignment.Center : _alignments[col];
                sb.Append(' ').Append(Pad(text, widths[col], alignment)).Append(' ').Append('│');
            }
            sb.AppendLine();
        }
    }

    private static string Pad(string text, int width, ColumnAlignment alignment)
    {
        int padding = width - DisplayWidth(text);
        if (padding <= 0) return text;

        return alignment switch
        {
            ColumnAlignment.Right => new string(' ', padding) + text,
            ColumnAlignment.Center => new string(' ', padding / 2) + text + new string(' ', padding - padding / 2),
            _ => text + new string(' ', padding)
        };
    }

    /// <summary>
    /// Перенос текста по словам. Слишком длинные слова (например, длинный
    /// идентификатор или код Хаффмана) аккуратно делятся по ширине колонки.
    /// </summary>
    private static List<string> WrapText(string text, int width)
    {
        var result = new List<string>();
        if (width <= 0)
        {
            result.Add(string.Empty);
            return result;
        }

        string normalized = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
        foreach (string paragraph in normalized.Split('\n'))
        {
            if (paragraph.Length == 0)
            {
                result.Add(string.Empty);
                continue;
            }

            var current = new StringBuilder();
            int currentWidth = 0;

            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int wordWidth = DisplayWidth(word);

                if (currentWidth > 0 && currentWidth + 1 + wordWidth > width)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    currentWidth = 0;
                }

                if (wordWidth <= width)
                {
                    if (currentWidth > 0)
                    {
                        current.Append(' ');
                        currentWidth++;
                    }

                    current.Append(word);
                    currentWidth += wordWidth;
                }
                else
                {
                    if (currentWidth > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        currentWidth = 0;
                    }

                    foreach (string chunk in SplitByWidth(word, width))
                    {
                        if (DisplayWidth(chunk) >= width)
                        {
                            result.Add(chunk);
                        }
                        else
                        {
                            current.Append(chunk);
                            currentWidth = DisplayWidth(chunk);
                        }
                    }
                }
            }

            if (currentWidth > 0 || result.Count == 0)
            {
                result.Add(current.ToString());
            }
        }

        if (result.Count == 0)
        {
            result.Add(string.Empty);
        }

        return result;
    }

    /// <summary>
    /// Делит строку на куски, каждый из которых помещается в заданную ширину.
    /// </summary>
    private static IEnumerable<string> SplitByWidth(string text, int width)
    {
        var chunk = new StringBuilder();
        int chunkWidth = 0;

        foreach (Rune rune in text.EnumerateRunes())
        {
            int runeWidth = RuneWidth(rune);
            if (chunkWidth > 0 && chunkWidth + runeWidth > width)
            {
                yield return chunk.ToString();
                chunk.Clear();
                chunkWidth = 0;
            }

            chunk.Append(rune);
            chunkWidth += runeWidth;
        }

        if (chunk.Length > 0)
        {
            yield return chunk.ToString();
        }
    }

    /// <summary>
    /// Ширина строки в ячейках консоли (с учётом широких CJK-символов).
    /// </summary>
    public static int DisplayWidth(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int width = 0;
        foreach (Rune rune in text.EnumerateRunes())
        {
            width += RuneWidth(rune);
        }
        return width;
    }

    private static int RuneWidth(Rune rune)
    {
        int value = rune.Value;

        // Управляющие символы не занимают места.
        if (value is < 0x20 or 0x7F) return 0;

        // Комбинирующие символы и модификаторы начертания.
        if (value is >= 0x0300 and <= 0x036F) return 0;
        if (value is >= 0xFE00 and <= 0xFE0F) return 0;
        if (value is >= 0x1AB0 and <= 0x1AFF) return 0;
        if (value is >= 0x1DC0 and <= 0x1DFF) return 0;
        if (value is >= 0x20D0 and <= 0x20FF) return 0;
        if (value == 0x200B || value == 0x200C || value == 0x200D) return 0;

        // Широкие символы: CJK, хангыль, эмодзи и т.п.
        if (value is >= 0x1100 and <= 0x115F) return 2;
        if (value is >= 0x2E80 and <= 0xA4CF) return 2;
        if (value is >= 0xAC00 and <= 0xD7A3) return 2;
        if (value is >= 0xF900 and <= 0xFAFF) return 2;
        if (value is >= 0xFE30 and <= 0xFE6F) return 2;
        if (value is >= 0xFF00 and <= 0xFF60) return 2;
        if (value is >= 0xFFE0 and <= 0xFFE6) return 2;
        if (value is >= 0x1F300 and <= 0x1FAFF) return 2;
        if (value is >= 0x20000 and <= 0x3FFFD) return 2;

        return 1;
    }
}
