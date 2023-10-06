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

    private static bool validExpression(String str) {
        if (str == null || str.Length == 0) {
            return false;
        }

        foreach (char currentChar in str) {
            if (!ValidChar(currentChar)) {
                return false;
            }
        }
        return true;
    }

    public static bool ValidChar(char c) =>
        char.IsLower(c) || char.IsDigit(c) || char.IsWhiteSpace(c) || validSymbols.Contains(c);

    private static void Main() {
        Console.Write("Enter expression: ");
        string? input = Console.ReadLine();
        while (true) {
            if (input != null && validExpression(input)) {
                Console.WriteLine(Polynomial.Parse(input));
                break;
            }
            Console.WriteLine("Invalid input");
            input = Console.ReadLine();
        }
    }
}