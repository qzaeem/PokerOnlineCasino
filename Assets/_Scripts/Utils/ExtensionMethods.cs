using System;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
    public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { new T[0] } :
          elements.SelectMany((e, i) =>
            elements.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] { e }).Concat(c)));
    }

    public static string Truncate(this string value, int maxLength, string suffix = "")
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + suffix;
    }
}
