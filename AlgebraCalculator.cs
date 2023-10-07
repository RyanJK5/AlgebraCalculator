namespace AlgebraCalculator;

static class Util {

    private static readonly char[] validSymbols = {
        '+',
        '-',
        '/',
        '*',
        '^',
        '(',
        ')',
    };

    public static char[] GetValidSymbols() {
        char[] result = new char[validSymbols.Length];
        validSymbols.CopyTo(result, 0);
        return result;
    }

    private static bool ValidExpression(string str) =>
        str != null && str.Length != 0 && str.ToCharArray().All(ValidChar)
    ;

    public static bool ValidChar(char c) =>
        char.IsLower(c) || char.IsDigit(c) || char.IsWhiteSpace(c) || validSymbols.Contains(c);

    private static string RemoveWhiteSpaces(string str) {
        for (var i = 0; i < str.Length; i++) {
            if (char.IsWhiteSpace(str[i])) {
                str = str.Remove(i, 1);
                i--;
            }
        }
        return str;
    }

    private static void Main() {
        Console.Write("Enter expression: ");
        string? input = Console.ReadLine();
        while (true) {
            if (input != null && ValidExpression(input)) {
                RemoveWhiteSpaces(input);
                Console.WriteLine(Polynomial.Parse(input));
                break;
            }
            Console.WriteLine("Invalid input");
            input = Console.ReadLine();
        }
    }
}