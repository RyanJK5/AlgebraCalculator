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

    private static void Main() {
        // Console.Write("Enter expression: ");
        // string? input = Console.ReadLine();
        // while (input != null && !ValidExpression(input)) {
        //     Console.WriteLine("Invalid input");
        //     input = Console.ReadLine();
        // }
        var term = new Term(4, (Variable) "x2", (Variable) "y");
        var term2 = new Term(2, (Variable) "x2", (Variable) "y2");
        Console.WriteLine(term / term2);
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

    // TODO: finish createTerms method
    private static Term[] createTerms(String str) {
        List<Term> result = new List<Term>();
        str.Trim();
        
        int startIndex = 0;
        int endIndex = 0;
        while (true) {
            startIndex = endIndex;
            
            endIndex = Array.FindIndex(str.ToCharArray(), startIndex, c => Char.IsWhiteSpace(c));
            if (endIndex < 0) {
                return result.ToArray();
            }
            string subStr = str.Substring(startIndex, endIndex + 1);
        }
    }

    public static bool ValidChar(char c) =>
        Char.IsLower(c) || Char.IsDigit(c) || Char.IsWhiteSpace(c) || validSymbols.Contains(c);
}