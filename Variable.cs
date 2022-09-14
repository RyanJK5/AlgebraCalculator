namespace AlgebraCalculator;

readonly struct Variable : IComparable<Variable> {

    public readonly char Letter;
    public readonly int Exponent;

    public Variable(char letter, int exponent) {
        if (!Char.IsLower(letter)) {
            throw new ArgumentException("symbol must be a lowercase letter");
        }
        Letter = letter;
        Exponent = exponent;
    }

    public Variable(char letter) : this(letter, 1) { }

    public Variable() : this('x', 1) { }

    public int CompareTo(Variable variable) {
        int status = Letter.CompareTo(variable.Letter);
        if (status == 0) {
            return Exponent.CompareTo(variable.Exponent);
        }
        return status;
    }

    public static Variable Parse(string str) {
        if (!Char.IsLower(str[0])) {
            throw new ArgumentException("First character of str must be a lowercase letter");
        }
        for (var i = 1; i < str.Length; i++) {
            if (!Char.IsNumber(str[i])) {
                throw new ArgumentException("Only one non-number character can be present in the first character of str");
            }
        }
        return new(str[0], str.Length > 1 ? Int32.Parse(str.Substring(1)) : 1);
    }

    public static Variable Parse(char c) {
        if (!Char.IsLower(c)) {
            throw new ArgumentException("c must be a lowercase letter");
        }
        return new(c);
    }

    public static explicit operator Variable(string str) => Parse(str);

    public static explicit operator Variable(char c) => Parse(c);

    public override string ToString() =>
        Letter.ToString() + (Exponent != 1 ? Exponent.ToString() : "");

    public override bool Equals(object? obj) =>
        obj != null && obj is Variable variable && Letter == variable.Letter && Exponent == variable.Exponent
    ;

    public override int GetHashCode() =>
        HashCode.Combine(Letter, Exponent)
    ;

    public static bool operator ==(Variable var1, Variable var2) =>
        var1.Equals(var2)
    ;

    public static bool operator !=(Variable var1, Variable var2) =>
        !var1.Equals(var2)
    ;
}