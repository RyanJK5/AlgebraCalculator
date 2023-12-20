﻿namespace AlgebraCalculator;

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

    public static bool ValidString(string str) =>
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
                if (str[i] == '^' && Term.Parse(str[tokenStartIndex..i]).Vars.Length > 0) {
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

            ExecutePEMDAS(tokens, rangeStart, ref rangeEnd);

            tokens[deepestIndex] = string.Concat(tokens.GetRange(rangeStart, rangeEnd - rangeStart));
            tokens.RemoveRange(rangeStart, rangeEnd - rangeStart + 1);
        }

        int endIndex = tokens.Count - 1;
        ExecutePEMDAS(tokens, 0, ref endIndex);
        return tokens;
    }

    private static void FactorDOTS(ref List<string> tokens) {
        if (tokens.Count != 3 || !Term.IsTerm(tokens[0]) || !Term.IsTerm(tokens[2]) || tokens[1] != "-") {
            return;
        }
        
        Term? term1 = Term.Root(Term.Parse(tokens[0]), 2);
        Term? term2 = Term.Root(Term.Parse(tokens[2]), 2);
        
        if (term1 is null || term2 is null) {
            return;
        }

        tokens[0] = term1.ToString();
        tokens[2] = term2.ToString();
        tokens.Insert(0, OpenDelimeter);
        tokens.Add(CloseDelimeter);
        tokens.AddRange(tokens);
        tokens[^3] = "+";
    }

    private static void FactorDOTC(ref List<string> tokens) {
        if (tokens.Count != 3 || !Term.IsTerm(tokens[0]) || !Term.IsTerm(tokens[2]) || !Term.AdditiveSymbol(tokens[1][0])) {
            return;
        }

        Term? term1 = Term.Root(Term.Parse(tokens[0]), 3);
        Term? term2 = Term.Root(Term.Parse(tokens[2]), 3);
        
        if (term1 is null || term2 is null) {
            return;
        }
        tokens[0] = term1.ToString();
        tokens[2] = term2.ToString();
        tokens.Insert(0, OpenDelimeter);
        tokens.Add(CloseDelimeter);
        tokens.Add(OpenDelimeter);
        tokens.Add(Term.Pow(term1, 2).ToString());
        tokens.Add(Term.OppositeAdditiveSymbol(tokens[2][0]).ToString());
        tokens.Add((term1 * term2).ToString());
        tokens.Add("+");
        tokens.Add(Term.Pow(term2, 2).ToString());
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

    private static void ExecutePEMDAS(List<string> tokens, int startIndex, ref int endIndex) {
        ExecuteOperations(tokens, startIndex, ref endIndex, CreateDictionary(new string[] {"^"}, new Func<Polynomial, Polynomial, Polynomial?>[] {
            (a, b) => {
                if (int.TryParse(b.ToString(), out int bInt)) {
                    return Polynomial.Pow(a, bInt);
                }
                return null;
            }
        }));
        ExecuteOperations(tokens, startIndex, ref endIndex, CreateDictionary(new string[] {"*"}, 
                          new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a * b}));
        ExecuteOperations(tokens, startIndex, ref endIndex, CreateDictionary(new string[] {"+", "-"}, 
                          new Func<Polynomial, Polynomial, Polynomial?>[] {(a, b) => a + b, (a, b) => a - b}));
    }

    private static void ExecuteOperations(List<string> tokens, int startIndex, ref int endIndex, 
                                             Dictionary<string, Func<Polynomial, Polynomial, Polynomial?>> symbolsToOperations) {
        for (var i = startIndex; i < endIndex; i++) {
            foreach (string symbol in symbolsToOperations.Keys) {
                if (tokens[i] == symbol) {
                    Polynomial? result = symbolsToOperations[symbol].Invoke(Polynomial.Parse(tokens[i - 1]), Polynomial.Parse(tokens[i + 1]));
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

    public static bool ValidExpresion(List<string> tokens) {
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

    public static void Main() {
        Console.Write("Enter expression: ");
        string? input = Console.ReadLine();
        while (true) {
            if (input != null && ValidString(input)) {
                string result = SimplifyAndFactor(input);
                Console.WriteLine(result);
                return;
            }
            Console.WriteLine("Invalid input");
            input = Console.ReadLine();
        }
    }

    public static string SimplifyAndFactor(string input) {
        List<string> tokens = Simplify(input);
        FactoredPolynomial factored = FactoredPolynomial.Factor((FactoredPolynomial)new(Polynomial.Parse(tokens[0])));
        return factored.ToString();
    }

    public static List<string> Simplify(string input) {
        input = RemoveWhiteSpaces(input);
        List<string> tokens = Parse(input);
        return tokens;
    }
}