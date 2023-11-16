namespace AlgebraCalculator;

public class AlgebraCalculator {

    private static readonly char[] validSymbols = {
        '+',
        '-',
        '/',
        '*',
        '(',
        ')',
        '^'
    };

    private const string OpenDelimeter = "(";
    private const string CloseDelimeter = ")";

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

    private static List<string> CreateTokens(string str) {
        var result = new List<string>();
        var tokenStartIndex = 0;
        for (var i = 0; i < str.Length; i++) {
            if (Term.IsTerm(str[tokenStartIndex..(i + 1)])) {
                if (i == str.Length - 1) {
                    result.Add(str[tokenStartIndex..]);
                }
                continue;
            }
            else if (Term.IsTerm(str[tokenStartIndex..i])) {
                if (str[i] == '^') {
                    continue;
                }
                result.Add(str[tokenStartIndex..i]);
            }
            result.Add(str[i].ToString());
            tokenStartIndex = i + 1;
        }
        AddMultiplicationSymbols(result);
        return result;
    }

    private static void AddMultiplicationSymbols(List<string> tokens) {
        for (var i = 0; i < tokens.Count; i++) {
            if (tokens[i] == OpenDelimeter) {
                if (IndexOfCloseDelimeter(tokens, i, tokens.Count - 1) < 0) {
                    tokens.Add(CloseDelimeter);
                }
                if (i > 0 && (Term.IsTerm(tokens[i - 1]) || tokens[i - 1] == CloseDelimeter)) {
                    tokens.Insert(i, "*");
                }
            }
            if (tokens[i] == CloseDelimeter && i < tokens.Count - 1 && Term.IsTerm(tokens[i + 1])) {
                tokens.Insert(i + 1, "*");
            }
            if (i < tokens.Count - 1 && Term.IsTerm(tokens[i]) && Term.IsTerm(tokens[i + 1])) {
                tokens.Insert(i + 1, "*");
            }
        }
    }

    private static int IndexOfCloseDelimeter(List<string> tokens, int delimIndex, int endIndex) {
        int delimTotal = 1;
        for (var j = delimIndex + 1; j <= endIndex; j++) {
            if (tokens[j] == OpenDelimeter) {
                delimTotal++;
            }
            else if (tokens[j] == CloseDelimeter) {
                delimTotal--;
            }
            if (delimTotal == 0) {
                return j;
            }
        }
        return -1;
    }

    public static List<string> Simplify(List<string> tokens) {
        while (tokens.Contains(OpenDelimeter)) {
            int deepestIndex = FindDeepestPolynomialIndex(tokens);

            int rangeStart = deepestIndex + 1;
            int rangeEnd = tokens.IndexOf(CloseDelimeter, rangeStart);

            ExecuteOperations(tokens, rangeStart, ref rangeEnd, CreateDictionary(new string[] {"*"}, 
                              new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a * b}), Polynomial.Parse);
            ExecuteOperations(tokens, rangeStart, ref rangeEnd, CreateDictionary(new string[] {"+", "-"}, 
                              new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a + b, (a, b) => a - b}), Polynomial.Parse);

            tokens[deepestIndex] = string.Concat(tokens.GetRange(rangeStart, rangeEnd - rangeStart));
            tokens.RemoveRange(rangeStart, rangeEnd - rangeStart + 1);
        }

        int endIndex = tokens.Count - 1;
        ExecuteOperations(tokens, 0, ref endIndex, CreateDictionary(new string[] {"*"}, 
                          new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a * b}), Polynomial.Parse);
        ExecuteOperations(tokens, 0, ref endIndex, CreateDictionary(new string[] {"+", "-"}, 
                          new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a + b, (a, b) => a - b}), Polynomial.Parse);

        return tokens;
    }

    public static List<string> Factor(List<string> tokens) {
        GreatestCommonFactor(ref tokens);
        return tokens;
    }

    private static void GreatestCommonFactor(ref List<string> tokens) {
        int lowestCoefficient = int.MaxValue;
        for (var i = 0; i < tokens.Count; i++) {
            string str = tokens[i];
            if (Term.IsTerm(str))  { 
                int newCoefficient = Term.Parse(str).Coefficient;
                if (newCoefficient < lowestCoefficient) {
                    lowestCoefficient = newCoefficient;
                }
            }
        }

        int gcf = 1;
        for (var i = 1; i <= lowestCoefficient; i++) {
            if (tokens.FindAll(str => Term.IsTerm(str)).All(str => Term.Parse(str).Coefficient % i == 0)) {
                gcf = i;
            }
        }
        if (gcf == 1) {
            return;
        }

        for (var i = 0; i < tokens.Count; i++) {
            string str = tokens[i];
            if (Term.IsTerm(str)) {
                Term oldTerm = Term.Parse(str);
                tokens[i] = new Term(oldTerm.Coefficient / gcf, oldTerm.Vars).ToString();
            }
        }
        tokens.Insert(0, OpenDelimeter);
        tokens.Insert(0, "*");
        tokens.Insert(0, gcf.ToString());
        tokens.Add(CloseDelimeter);
    }

    private static void FactorDOTS(ref List<string> tokens) {
        if (tokens.Count != 3 || !Term.IsTerm(tokens[0]) || !Term.IsTerm(tokens[2]) || tokens[1] != "-") {
            return;
        }
        
        Term term1 = Term.Parse(tokens[0]);
        Term term2 = Term.Parse(tokens[2]);
        
        double sqrtCoeff1 = Math.Sqrt(term1.Coefficient);
        double sqrtCoeff2 = Math.Sqrt(term2.Coefficient);
        if (sqrtCoeff1.ToString()[^1] != '0' || sqrtCoeff2.ToString()[^1] != '0' || term1.Vars.Any(v => v.Exponent % 2 == 1) || term2.Vars.Any(v => v.Exponent % 2 == 1)) {
            return;
        }

        var newTerm1List = new List<Variable>();
        foreach (var v in term1.Vars) {
            newTerm1List.Add(v.Symbol, v.Exponent / 2);
        }

        tokens.Insert(0, OpenDelimeter);
        tokens.Add(CloseDelimeter);
    }

    private static int FindDeepestPolynomialIndex(List<string> tokens) {
        int layersDown = 0;
        int maxLayersDown = 0;
        int maxLayerIndex = -1;
        for (var i = 0; i < tokens.Count; i++) {
            if (tokens[i] == OpenDelimeter) {
                layersDown++;
            }
            else if (tokens[i] == CloseDelimeter && layersDown > maxLayersDown) {
                maxLayersDown = layersDown;
                maxLayerIndex = tokens.LastIndexOf(OpenDelimeter, i);
                layersDown = 0;
            }
        }
        return maxLayerIndex;
    }

    private static void ExecuteOperations<T>(List<string> tokens, int startIndex, ref int endIndex, 
                                             Dictionary<string, Func<T, T, T?>> symbolsToOperations, Func<string, T> parseMethod) {
        for (var i = startIndex; i < endIndex; i++) {
            foreach (string symbol in symbolsToOperations.Keys) {
                if (tokens[i] == symbol) {
                    T? result = symbolsToOperations[symbol].Invoke(parseMethod.Invoke(tokens[i - 1]), parseMethod.Invoke(tokens[i + 1]));
                    string? resultStr = result?.ToString();
                    if (result is null || resultStr is null) {
                        continue;    
                    }
                    tokens[i - 1] = resultStr;
                    tokens.RemoveAt(i + 1);
                    tokens.RemoveAt(i);
                    endIndex -= 2;
                    i--;
                    break;
                }
            }
        }
    }

    private static Dictionary<K, V> CreateDictionary<K, V>(K[] keys, V[] values) where K : notnull {
        if (values.Length != keys.Length) {
            throw new ArgumentException("values and keys must have same length");
        }
        var result = new Dictionary<K, V>();
        for (var i = 0; i < keys.Length; i++) {
            result.Add(keys[i], values[i]);
        }
        return result;
    }

    private static List<string> Parse(string str) {
        List<string> tokens = CreateTokens(str);
        if (!ValidExpresion(tokens)) {
            Console.WriteLine("SYNTAX ERROR");
            return new List<string>();
        }
        Simplify(tokens);
        return tokens;
    }

    private static bool ValidExpresion(List<string> tokens) {
        if (tokens == null) {
            throw new NullReferenceException();
        }
        if (tokens.Find(str => Term.IsTerm(str)) == null) {
            return false;
        }
        int openCount = 0;
        int closeCount = 0;
        for (var i = 0; i < tokens.Count; i++) {
            string token = tokens[i];
            if (token == CloseDelimeter) {
                closeCount++;
                if (closeCount > openCount || (!Term.IsTerm(tokens[i - 1]) && tokens[i - 1] != CloseDelimeter)) {
                    return false;
                }
                continue;
            }
            if (token == OpenDelimeter) {
                if (i == tokens.Count) {
                    return false;
                }
                openCount++;
                if (!Term.IsTerm(tokens[i + 1]) && tokens[i + 1] != OpenDelimeter) {
                    return false;
                }
            }
            else if (i == tokens.Count || 
                (!Term.IsTerm(token) && !Term.IsTerm(tokens[i + 1]) && tokens[i + 1] != CloseDelimeter && tokens[i + 1] != OpenDelimeter)) {
                return false;
            }
        }
        return openCount == closeCount;
    }

    public static void Start() {
        Console.Write("Enter expression: ");
        string? input = Console.ReadLine();
        while (true) {
            if (input != null && ValidExpression(input)) {
                input = RemoveWhiteSpaces(input);
                List<string> tokens = Parse(input);
                tokens = Factor(CreateTokens(tokens[0]));
                foreach (string token in tokens) {
                   Console.Write(token);
                }
                Console.WriteLine();
                tokens = Simplify(tokens);
                foreach (string token in tokens) {
                   Console.Write(token);
                }
                break;
            }
            Console.WriteLine("Invalid input");
            input = Console.ReadLine();
        }
    }
}