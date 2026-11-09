using System.Globalization;
using System.Text.RegularExpressions;
namespace AlgorithmsLab3.Core;

public static class Expressions
{
    private static readonly Regex Tokens = new(@"\G\s*(?:(\d+(?:[.,]\d+)?)|([a-zA-Z]+)|(.))", RegexOptions.Compiled);
    public static string ToPostfix(string text)
    {
        var tokens = new List<string>(); int pos = 0;
        while (pos < text.Length) { while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++; if (pos == text.Length) break; var m = Tokens.Match(text, pos); if (!m.Success) throw new FormatException("Некорректный символ."); tokens.Add(m.Groups[1].Success ? m.Groups[1].Value.Replace(',', '.') : m.Groups[2].Success ? m.Groups[2].Value.ToLowerInvariant() : m.Groups[3].Value); pos = m.Index + m.Length; }
        int i = 0; var output = new List<string>();
        void Expression() { Term(); while (i < tokens.Count && (tokens[i] == "+" || tokens[i] == "-")) { var op = tokens[i++]; Term(); output.Add(op); } }
        void Term() { Unary(); while (i < tokens.Count && (tokens[i] == "*" || tokens[i] == "/" || tokens[i] == ":")) { var op = tokens[i++]; Unary(); output.Add(op); } }
        void Unary() { if (i < tokens.Count && (tokens[i] == "-" || tokens[i] == "+")) { var op = tokens[i++]; Unary(); if (op == "-") output.Add("neg"); } else Power(); }
        void Power() { Primary(); if (i < tokens.Count && tokens[i] == "^") { i++; Unary(); output.Add("^"); } }
        void Primary() { if (i >= tokens.Count) throw new FormatException("Ожидалось число или выражение."); var t = tokens[i++]; if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out _)) { output.Add(t); return; } if (t is "ln" or "cos" or "sin" or "sqrt") { if (i >= tokens.Count || tokens[i++] != "(") throw new FormatException("После функции нужна открывающая скобка."); Expression(); if (i >= tokens.Count || tokens[i++] != ")") throw new FormatException("Ожидалась закрывающая скобка."); output.Add(t); return; } if (t == "(") { Expression(); if (i >= tokens.Count || tokens[i++] != ")") throw new FormatException("Ожидалась закрывающая скобка."); return; } throw new FormatException($"Неожиданный токен: {t}"); }
        Expression(); if (i != tokens.Count) throw new FormatException($"Лишний токен: {tokens[i]}"); return string.Join(' ', output);
    }
    public static double Evaluate(string text)
    {
        var stack = new LinkedStack<double>();
        foreach (var raw in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)) { 
            var t = raw.ToLowerInvariant(); 
            if (double.TryParse(t.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double number)) { 
                if (!double.IsFinite(number)) throw new FormatException("Число должно быть конечным."); 
                stack.Push(number); 
                continue; 
            }
            if (t is "ln" or "cos" or "sin" or "sqrt" or "neg") { 
                double x = stack.Pop(); 
                double v = t switch { 
                    "ln" when x > 0 => Math.Log(x), 
                    "sqrt" when x >= 0 => Math.Sqrt(x), 
                    "cos" => Math.Cos(x), "sin" => Math.Sin(x), 
                    "neg" => -x, _ => throw new ArithmeticException("Аргумент вне области определения функции.") 
                }; 
                stack.Push(v); 
                continue; 
            }
            if (t is not ("+" or "-" or "*" or "/" or ":" or "^")) throw new FormatException($"Неизвестная операция: {raw}");
            double b = stack.Pop(), a = stack.Pop(); 
            double result = t switch { 
                "+" => a + b, 
                "-" => a - b, 
                "*" => a * b, 
                "/" or ":" when b != 0 => a / b, 
                "/" or ":" => throw new DivideByZeroException(), _ => Math.Pow(a, b) 
            }; 
            if (!double.IsFinite(result)) throw new ArithmeticException("Результат не является конечным числом."); 
            stack.Push(result);
        }
        if (stack.Count != 1) throw new FormatException("Выражение должно оставлять ровно один результат."); 
        return stack.Pop();
    }
}
