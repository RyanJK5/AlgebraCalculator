namespace AlgebraCalculator;

public class AlgebraCalculator {

    private static readonly char[] validSymbols = {
        '+',
        '-',
        '/',
        '*',
        '(',
        ')',
    };

    private const string OpenDelimeter = "(";
    private const string CloseDelimeter = ")";

    private char[] GetValidSymbols() {
        char[] result = new char[validSymbols.Length];
        validSymbols.CopyTo(result, 0);
        return result;
    }

    private bool ValidExpression(string str) =>
        str != null && str.Length != 0 && str.ToCharArray().All(ValidChar)
    ;

    public bool ValidChar(char c) =>
        char.IsLower(c) || char.IsDigit(c) || char.IsWhiteSpace(c) || validSymbols.Contains(c);

    private string RemoveWhiteSpaces(string str) {
        for (var i = 0; i < str.Length; i++) {
            if (char.IsWhiteSpace(str[i])) {
                str = str.Remove(i, 1);
                i--;
            }
        }
        return str;
    }

    private List<string> CreateTokens(string str) {
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
                result.Add(str[tokenStartIndex..i]);
            }
            result.Add(str[i].ToString());
            tokenStartIndex = i + 1;
        }
        AddMultiplicationSymbols(result);
        return result;
    }

    private void AddMultiplicationSymbols(List<string> tokens) {
        for (var i = 0; i < tokens.Count; i++) {
            if (tokens[i] == OpenDelimeter) {
                if (IndexOfCloseDelimeter(tokens, i, tokens.Count - 1) < 0) {
                    tokens.Add(CloseDelimeter);
                }
                if (i > 0 && (Term.IsTerm(tokens[i - 1]) || tokens[i - 1] == CloseDelimeter)) {
                    tokens.Insert(i, "*");
                }
            }
            if (tokens[i] == CloseDelimeter) {
                if (i < tokens.Count - 1 && Term.IsTerm(tokens[i + 1])) {
                    tokens.Insert(i + 1, "*");
                }
            }
        }
    }

    private int IndexOfCloseDelimeter(List<string> tokens, int delimIndex, int endIndex) {
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

    public List<string> Simplify(List<string> tokens) {
        while (tokens.Contains(OpenDelimeter)) {
            int deepestIndex = FindDeepestPolynomialIndex(tokens);

            int rangeStart = deepestIndex + 1;
            int rangeEnd = tokens.IndexOf(CloseDelimeter, rangeStart);
            
            ExecuteOperations(tokens, rangeStart, ref rangeEnd, CreateDictionary(new string[] {"*", "/"}, 
                new Func<Term, Term, Term?>[] {(a, b) => a * b, (a, b) => a / b}), Term.Parse);
                
            tokens[deepestIndex] = string.Concat(tokens.GetRange(rangeStart, rangeEnd - rangeStart));
            tokens.RemoveRange(rangeStart, rangeEnd - rangeStart + 1);
        }

        int endIndex = tokens.Count - 1;
        ExecuteOperations(tokens, 0, ref endIndex, CreateDictionary(new string[] {"*"}, 
            new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a * b}), Polynomial.Parse);

        return tokens;
    }

    private int FindDeepestPolynomialIndex(List<string> tokens) {
        int layersDown = 0;
        int maxLayersDown = 0;
        int maxLayerIndex = -1;
        for (var i = 0; i < tokens.Count; i++) {
            if (tokens[i] == OpenDelimeter) {
                layersDown++;
            }
            else if (tokens[i] == CloseDelimeter) {
                maxLayersDown = layersDown;
                maxLayerIndex = tokens.LastIndexOf(OpenDelimeter, i);
                layersDown = 0;
            }
        }
        return maxLayerIndex;
    }

    private void ExecuteOperations<T>(List<string> tokens, int startIndex, ref int endIndex, 
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

    private Dictionary<K, V> CreateDictionary<K, V>(K[] keys, V[] values) where K : notnull {
        if (values.Length != keys.Length) {
            throw new ArgumentException("values and keys must have same length");
        }
        var result = new Dictionary<K, V>();
        for (var i = 0; i < keys.Length; i++) {
            result.Add(keys[i], values[i]);
        }
        return result;
    }

    private void Parse(string str) {
        List<string> tokens = CreateTokens(str);
        if (!ValidExpresion(tokens)) {
            Console.WriteLine("SYNTAX ERROR");
            return;
        }
        Simplify(tokens);
        foreach (string token in tokens) {
            Console.Write(token);
        }
    }

    private bool ValidExpresion(List<string> tokens) {
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

    public void Start() {
        Console.Write("Enter expression: ");
        string? input = Console.ReadLine();
        while (true) {
            if (input != null && ValidExpression(input)) {
                RemoveWhiteSpaces(input);
                Parse(input);
                break;
            }
            Console.WriteLine("Invalid input");
            input = Console.ReadLine();
        }
    }
}