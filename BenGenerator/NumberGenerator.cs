namespace BenGenerator;

public class NumberGenerator
{
    public IEnumerable<string> GenerateNumbers(int upperBound = 100, IDictionary<int, string>? replacementNames = default)
    {
        replacementNames = (replacementNames ?? new Dictionary<int, string>()).OrderBy(kvp => kvp.Key).ToDictionary();

        if (upperBound < 0)
            upperBound = 0;

        foreach (var num in Enumerable.Range(1, upperBound))
        {
            var value = string.Empty;

            foreach (var kvp in replacementNames.Where(key => num % key.Key == 0))
                value += " " + kvp.Value;

            if (string.IsNullOrWhiteSpace(value))
                value = num.ToString();

            yield return value.Trim();
        }
    }
}
