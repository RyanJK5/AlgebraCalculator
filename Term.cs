using System.Diagnostics.CodeAnalysis;

namespace AlgebraCalculator;

class Term : IComparable<Term> {

    public readonly int Coefficient;
    private List<Variable> _vars;

    public Variable[] Vars { 
        get => _vars.ToArray();
    }

    public Term(int coefficient, params Variable[] variables) {
        Coefficient = coefficient;
        _vars = new List<Variable>();
        if (Coefficient == 0) {
            return;
        }
        
        _vars.AddRange(variables);
        Simplify();
        _vars.Sort();
    }

    public Term() : this(1, new Variable[0]) { }

    private void Simplify() {
        var newList = new List<Variable>();
        foreach (var variable in _vars) {
            Variable matchingVar = newList.Find(v => v.Symbol == variable.Symbol);
            if (matchingVar.ValidVar()) {
                newList[newList.IndexOf(matchingVar)] = new Variable(matchingVar.Symbol, matchingVar.Exponent + variable.Exponent);
                continue;
            }
            newList.Add(variable);
        }
        _vars = newList;
    }

    public static Term Parse(string str) {
        if (str.Length == 0) {
            throw new ArgumentException("str must have a length of at least 1");
        }
        if (str.Any(c => !AdditiveSymbol(c) && (!char.IsLetterOrDigit(c) || char.IsUpper(c)))) {
            throw new ArgumentException("str must only contain lowercase letters and numbers");
        }
        
        var coefficient = 1;
        var vars = new List<Variable>();

        int letterIndex = Array.FindIndex(str.ToCharArray(), c => char.IsLower(c));
        if (letterIndex >= 0) {
            if (letterIndex > 0) {
                coefficient = int.Parse(str[..letterIndex]);
            }
            int lowIndex = letterIndex;
            int highIndex = letterIndex;
            for (var i = letterIndex; i < str.Length; i++) {
                highIndex = Array.FindIndex(str.ToCharArray(), lowIndex + 1, c => char.IsLower(c));
                if (highIndex < 0) {
                    highIndex = str.Length;
                }
                vars.Add(Variable.Parse(str[lowIndex..highIndex]));
                lowIndex = highIndex;
                if (highIndex == str.Length) { 
                    break;
                }
            }
        }
        else {
            coefficient = int.Parse(str);
        }
        return new(coefficient, vars.ToArray());
    }

    public static bool IsTerm(string str) =>
        (char.IsNumber(str[0]) || char.IsLower(str[0]) || AdditiveSymbol(str[0])) && (str.Length == 1 || str[1..].All(c => char.IsNumber(c) || char.IsLower(c)));

    public static bool AdditiveSymbol(char c) => c == '+' || c == '-';

    public static Term operator *(Term t1, Term t2) {
        int newCoefficient = t1.Coefficient * t2.Coefficient;
        var newVariables = new Variable[t1._vars.Count + t2._vars.Count];
        Term[] terms = {t1, t2};
        for (var i = 0; i < 2; i++) {
            for (var j = 0; j < terms[i]._vars.Count; j++) {
                newVariables[j + (i == 0 ? 0 : terms[0]._vars.Count)] = terms[i][j]; 
            }
        }
        return new(newCoefficient, newVariables);
    }

    public static Term? operator /(Term t1, Term t2) {
        if (t1.Coefficient % t2.Coefficient != 0 || t1._vars.Count != t2._vars.Count) {
            return null;
        }

        int newCoefficient = t1.Coefficient / t2.Coefficient;
        var newVars = new List<Variable>();
        for (var i = 0; i < t1._vars.Count; i++) {
            if (t1[i].Symbol != t2[i].Symbol) {
                return null;
            }

            int newExponent = t1[i].Exponent - t2[i].Exponent;
            if (newExponent != 0) {
                newVars.Add(new(t1[i].Symbol, newExponent));
            } 
        }
        return new(newCoefficient, newVars.ToArray());
    }

    public static Term? operator +(Term t1, Term t2) {
        if (t1._vars.Count != t2._vars.Count) {
            return null;
        }
        var vars = new List<Variable>();
        for (var i = 0; i < t1._vars.Count; i++) {
            if (t1[i] != t2[i]) {
                return null;
            }
            vars.Add(t1[i]);
        }
        return new(t1.Coefficient + t2.Coefficient, vars.ToArray());
    }

    public static Term? operator -(Term t1, Term t2) =>
        t1 + new Term(-t2.Coefficient, t2._vars.ToArray());

    public static bool operator ==(Term t1, Term t2) => t1.Equals(t2);

    public static bool operator !=(Term t1, Term t2) => !t1.Equals(t2);

    public Variable this[int index] {
        get => _vars[index];
    }

    public override string ToString() {
        string result = "";
        if (Coefficient == -1) {
            result += "-";
        }
        else if (Coefficient != 1) {
            result += Coefficient;
        }

        foreach (Variable variable in _vars) {
            result += variable.ToString();
        }
        return result;
    }

    public override bool Equals(object? obj) {
        if (obj is not Term || obj == null) {
            return false;
        }
        Term term = (Term) obj;
        for (var i = 0; i < _vars.Count; i++) {
            if (_vars[i] != term[i]) {
                return false;
            }
        }
        return Coefficient == term.Coefficient;
    }

    public override int GetHashCode() => HashCode.Combine(Coefficient, _vars);
  
    public int CompareTo(Term? other) {
        if (other is null) {
            throw new NullReferenceException("other must not be null");
        }
        return other[other._vars.Count - 1].CompareTo(this[_vars.Count - 1]);
    }
}