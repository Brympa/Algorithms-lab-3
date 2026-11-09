using System.Globalization;
using System.Text;
using AlgorithmsLab3.Core;

namespace AlgorithmsLab3.Applications;

/// <summary>
/// Пример применения структуры данных "Связный список" (Часть 3 ТЗ):
/// Сложение многочленов произвольной степени.
/// Каждый одночлен хранится в виде (степень, коэффициент) в узле списка LinkedSequence.
/// </summary>
public static class PolynomialAddition
{
    public sealed record Term(int Degree, double Coefficient);

    /// <summary>
    /// Сложение двух многочленов, заданных в виде последовательностей членов.
    /// Алгоритм выполняет слияние за O(N + M) аналогично слиянию двух отсортированных списков.
    /// </summary>
    public static LinkedSequence<Term> Add(
        IEnumerable<(int Degree, double Coefficient)> polyA,
        IEnumerable<(int Degree, double Coefficient)> polyB)
    {
        // Нормализуем входные данные: сортируем по убыванию степеней
        var listA = LinkedSequence<Term>.From(
            polyA.GroupBy(t => t.Degree)
                 .Select(g => new Term(g.Key, g.Sum(x => x.Coefficient)))
                 .Where(t => Math.Abs(t.Coefficient) > 1e-12)
                 .OrderByDescending(t => t.Degree)
        );

        var listB = LinkedSequence<Term>.From(
            polyB.GroupBy(t => t.Degree)
                 .Select(g => new Term(g.Key, g.Sum(x => x.Coefficient)))
                 .Where(t => Math.Abs(t.Coefficient) > 1e-12)
                 .OrderByDescending(t => t.Degree)
        );

        var result = new LinkedSequence<Term>();

        using var iterA = listA.Values().GetEnumerator();
        using var iterB = listB.Values().GetEnumerator();

        bool hasA = iterA.MoveNext();
        bool hasB = iterB.MoveNext();

        while (hasA || hasB)
        {
            if (hasA && hasB)
            {
                if (iterA.Current.Degree == iterB.Current.Degree)
                {
                    double sumCoeff = iterA.Current.Coefficient + iterB.Current.Coefficient;
                    if (Math.Abs(sumCoeff) > 1e-12)
                    {
                        result.AddLast(new Term(iterA.Current.Degree, sumCoeff));
                    }
                    hasA = iterA.MoveNext();
                    hasB = iterB.MoveNext();
                }
                else if (iterA.Current.Degree > iterB.Current.Degree)
                {
                    result.AddLast(iterA.Current);
                    hasA = iterA.MoveNext();
                }
                else
                {
                    result.AddLast(iterB.Current);
                    hasB = iterB.MoveNext();
                }
            }
            else if (hasA)
            {
                result.AddLast(iterA.Current);
                hasA = iterA.MoveNext();
            }
            else // hasB
            {
                result.AddLast(iterB.Current);
                hasB = iterB.MoveNext();
            }
        }

        return result;
    }

    /// <summary>
    /// Преобразует список одночленов в красивую математическую строку (например "3x² − 2x + 5").
    /// </summary>
    public static string Format(LinkedSequence<Term> poly)
    {
        if (poly.IsEmpty)
        {
            return "0";
        }

        var sb = new StringBuilder();
        bool first = true;

        foreach (var term in poly.Values())
        {
            double coeff = term.Coefficient;
            int deg = term.Degree;

            if (Math.Abs(coeff) < 1e-12) continue;

            if (first)
            {
                if (coeff < 0) sb.Append('-');
                first = false;
            }
            else
            {
                sb.Append(coeff < 0 ? " − " : " + ");
            }

            double absCoeff = Math.Abs(coeff);
            string coeffStr = absCoeff.ToString("G6", CultureInfo.InvariantCulture);

            if (deg == 0)
            {
                sb.Append(coeffStr);
            }
            else
            {
                if (Math.Abs(absCoeff - 1.0) > 1e-12)
                {
                    sb.Append(coeffStr);
                }

                sb.Append('x');
                if (deg > 1)
                {
                    sb.Append('^').Append(deg);
                }
            }
        }

        return sb.Length == 0 ? "0" : sb.ToString();
    }
}
