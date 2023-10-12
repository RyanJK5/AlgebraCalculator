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

    public List<string> Simplify(List<string> tokens) => 
        Simplify(tokens, 0, tokens.Count - 1);

    private List<string> Simplify(List<string> tokens, int startIndex, int endIndex) {
        for (var i = startIndex; i <= endIndex; i++) {
            if (tokens[i] == OpenDelimeter) {
                int oldCount = tokens.Count;
                Simplify(tokens, i + 1, IndexOfCloseDelimeter(tokens, i, endIndex) - 1);
                endIndex -= oldCount - tokens.Count;
                tokens.RemoveAt(IndexOfCloseDelimeter(tokens, i, endIndex));
                tokens.RemoveAt(i);
                endIndex -= 2;
            }
        }
        ExecuteOperations(tokens, startIndex, ref endIndex, CreateDictionary(new string[] {"*", "/"}, 
            new Func<Term, Term, Term?>[] {(a, b) => a * b, (a, b) => a / b}));
        ExecuteOperations(tokens, startIndex, ref endIndex, CreateDictionary(new string[] {"+", "-"}, 
            new Func<Term, Term, Term?>[] {(a, b) => a + b, (a, b) => a - b}));
        return tokens;
    }

    private void ExecuteOperations(List<string> tokens, int startIndex, ref int endIndex, 
        Dictionary<string, Func<Term, Term, Term?>> symbolsToOperations) {
        for (var i = startIndex; i < endIndex; i++) {
            foreach (string symbol in symbolsToOperations.Keys) {
                if (tokens[i] == symbol) {
                    Term? result = symbolsToOperations[symbol].Invoke(Term.Parse(tokens[i - 1]), Term.Parse(tokens[i + 1]));
                    if (result is null) {
                        continue;
                    }
                    tokens[i - 1] = result.ToString();
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
        foreach (var token in tokens) {
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