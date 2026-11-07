using AlgorithmsLab3.Core;
namespace AlgorithmsLab3.Applications;

public static class BracketValidation
{
    // Обычные и verbatim-строки, символьные литералы и комментарии игнорируются.
    // Интерполированные и raw-строки требуют полноценного лексического анализатора.
    public static bool IsValid(string text)
    {
        var stack = new LinkedStack<char>();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i], next = i + 1 < text.Length ? text[i + 1] : '\0';
            if (c == '/' && next == '/') { i += 2; while (i < text.Length && text[i] != '\n') i++; continue; }
            if (c == '/' && next == '*') { i += 2; while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++; if (i + 1 >= text.Length) return false; i++; continue; }
            if (c == '@' && next == '"') { i += 2; bool closed = false; for (; i < text.Length; i++) { if (text[i] != '"') continue; if (i + 1 < text.Length && text[i + 1] == '"') { i++; continue; } closed = true; break; } if (!closed) return false; continue; }
            if (c is '"' or '\'') { char quote = c; bool closed = false; while (++i < text.Length) { if (text[i] == '\\') { i++; continue; } if (text[i] == quote) { closed = true; break; } } if (!closed) return false; continue; }
            if (c is '(' or '[' or '{') stack.Push(c);
            else if (c is ')' or ']' or '}') { if (stack.IsEmpty) return false; char open = stack.Pop(); if (!((open == '(' && c == ')') || (open == '[' && c == ']') || (open == '{' && c == '}'))) return false; }
        }
        return stack.IsEmpty;
    }
}
