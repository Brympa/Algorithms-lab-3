namespace AlgorithmsLab3.UI;

/// <summary>
/// Общие настройки консольного интерфейса: ширина окна и заголовки разделов.
/// </summary>
public static class UiLayout
{
    /// <summary>
    /// Ширина консоли с безопасным запасным значением. При перенаправленном
    /// выводе Console.WindowWidth может быть недоступна или равна нулю.
    /// Переменная окружения LAB_CONSOLE_WIDTH позволяет проверить вёрстку
    /// на заданной ширине (например, 80 или 100 колонок).
    /// </summary>
    public static int ConsoleWidth
    {
        get
        {
            string? configured = Environment.GetEnvironmentVariable("LAB_CONSOLE_WIDTH");
            if (int.TryParse(configured, out int parsed) && parsed >= 40)
            {
                return parsed;
            }

            try
            {
                int width = Console.WindowWidth;
                if (width >= 40) return width;
            }
            catch (IOException)
            {
                // Вывод перенаправлен в файл/конвейер — используем значение по умолчанию.
            }
            catch (PlatformNotSupportedException)
            {
                // Платформа не предоставляет размер окна.
            }

            return 120;
        }
    }

    /// <summary>
    /// Печатает заголовок раздела в рамке. Ширина рамки подстраивается под
    /// заголовок, поэтому длинные названия не выходят за границы.
    /// </summary>
    public static void PrintHeader(string title)
    {
        title ??= string.Empty;

        int titleWidth = ConsoleTable.DisplayWidth(title);
        int maxInnerWidth = Math.Max(20, ConsoleWidth - 4);
        int innerWidth = Math.Clamp(Math.Max(71, titleWidth + 4), 20, maxInnerWidth);

        // Если заголовок не помещается в рамку, переносим его по словам.
        var titleLines = WrapTitle(title, innerWidth - 4);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ╔" + new string('═', innerWidth) + "╗");
        foreach (string line in titleLines)
        {
            string centered = Center(line, innerWidth - 4);
            Console.WriteLine("  ║  " + centered + "  ║");
        }
        Console.WriteLine("  ╚" + new string('═', innerWidth) + "╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static string Center(string text, int width)
    {
        int padding = width - ConsoleTable.DisplayWidth(text);
        if (padding <= 0) return text;

        int left = padding / 2;
        return new string(' ', left) + text + new string(' ', padding - left);
    }

    private static List<string> WrapTitle(string title, int width)
    {
        var lines = new List<string>();
        if (width <= 0)
        {
            lines.Add(title);
            return lines;
        }

        var current = new System.Text.StringBuilder();
        int currentWidth = 0;

        foreach (string word in title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            int wordWidth = ConsoleTable.DisplayWidth(word);
            if (currentWidth > 0 && currentWidth + 1 + wordWidth > width)
            {
                lines.Add(current.ToString());
                current.Clear();
                currentWidth = 0;
            }

            if (currentWidth > 0)
            {
                current.Append(' ');
                currentWidth++;
            }

            current.Append(word);
            currentWidth += wordWidth;
        }

        if (currentWidth > 0 || lines.Count == 0)
        {
            lines.Add(current.ToString());
        }

        return lines;
    }
}
