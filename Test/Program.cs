//foreach (var num in Enumerable.Range(1, 100))
//{
//    var value = string.Empty;

//    if (num % 3 == 0)
//    {
//        value = "Ben";
//    }
//    if (num % 5 == 0)
//    {
//        value += " Sheppard";
//    }

//    if (string.IsNullOrWhiteSpace(value))
//        value = num.ToString();

//    Console.WriteLine(value.Trim());
//}

// Provide a library to call this function to receive output. Also allow for custom upper bound.
using BenGenerator;

var generator = new NumberGenerator();

foreach (var line in generator.GenerateNumbers(int.MaxValue))
    Console.WriteLine(line);