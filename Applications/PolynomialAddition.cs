using AlgorithmsLab3.Core;
namespace AlgorithmsLab3.Applications;

public static class PolynomialAddition
{
    // Одночлены представлены собственным списком; совпадающие степени объединяются.
    public static LinkedSequence<(int Degree, double Coefficient)> Add(IEnumerable<(int Degree, double Coefficient)> a, 
        IEnumerable<(int Degree, double Coefficient)> b)
    {
        var left = LinkedSequence<(int Degree, double Coefficient)>.From(a.OrderByDescending(t => t.Degree));
        var right = LinkedSequence<(int Degree, double Coefficient)>.From(b.OrderByDescending(t => t.Degree));
        var result = new LinkedSequence<(int Degree, double Coefficient)>();
        using var x = left.Values().GetEnumerator(); using var y = right.Values().GetEnumerator();
        bool hasX = x.MoveNext(), hasY = y.MoveNext();
        while (hasX || hasY) { 
            int degree = !hasY || (hasX && x.Current.Degree >= y.Current.Degree) 
                ? x.Current.Degree : y.Current.Degree; 
            double sum = 0; 
            while (hasX && x.Current.Degree == degree) { 
                sum += x.Current.Coefficient; 
                hasX = x.MoveNext(); 
            }
            while (hasY && y.Current.Degree == degree) { 
                sum += y.Current.Coefficient; 
                hasY = y.MoveNext(); 
            }
            if (sum != 0) result.AddLast((degree, sum)); 
        }
        return result;
    }
}
