using System.Data.Common;

namespace AlgebraCalculator;

readonly struct Variable : IComparable<Variable> {

    public readonly char Symbol;
    public readonly int Exponent;

    public Variable(char symbol, int exponent) {
        if (!char.IsLower(symbol)) {
            throw new ArgumentException("symbol must be a lowercase letter");
        }
        Symbol = symbol;
        Exponent = exponent;
    }

    public Variable(char symbol) : this(symbol, 1) { }

    public Variable() : this('?', 1) { }

    public bool ValidVar() => char.IsLower(Symbol);

    public int CompareTo(Variable variable) {
        if (!ValidVar() || !variable.ValidVar()) {
            throw new ArgumentException("Invalid variable type");
        }
        return Symbol.CompareTo(variable.Symbol);
    }

    public static Variable Parse(string str) {
        if (str.Length == 0) {
            throw new ArgumentException("str must have a length greater than 0");
        }
        if (!char.IsLower(str[0])) {
            throw new ArgumentException("First character of str must be a lowercase letter");
        }
        for (var i = 1; i < str.Length; i++) {
            if (str[i] != '-' && !char.IsNumber(str[i])) {
                throw new ArgumentException("Only one non-number character can be present in the first character of str");
            }
        }
        return new(str[0], str.Length > 1 ? int.Parse(str.Substring(1)) : 1);
    }

    public static Variable Parse(char c) {
        if (!char.IsLower(c)) {
            throw new ArgumentException("c must be a lowercase letter");
        }
        return new(c);
    }

    public static explicit operator Variable(string str) => Parse(str);

    public static explicit operator Variable(char c) => Parse(c);

    public override string ToString() =>
        Symbol + (Exponent != 1 ? Exponent.ToString() : "");

    public override bool Equals(object? obj) => 
        obj != null && obj is Variable variable && Symbol == variable.Symbol && Exponent == variable.Exponent
    ;

    public override int GetHashCode() =>
        HashCode.Combine(Symbol, Exponent)
    ;

    public static bool operator ==(Variable var1, Variable var2) =>
        var1.Equals(var2)
    ;

    public static bool operator !=(Variable var1, Variable var2) =>
        !var1.Equals(var2)
    ;
}