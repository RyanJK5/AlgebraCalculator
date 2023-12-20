using System.Collections;
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
        _vars.Sort(CompareVars);
    }

    public Term(params Variable[] variables) : this(1, variables) { }

    public Term() : this(Array.Empty<Variable>()) { }

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

    public static bool SameVariableSet(Term a, Term b) {
        if (a._vars.Count != b._vars.Count) {
            return false;
        }
        for (var i = 0; i < a._vars.Count; i++) {
            if (a[i] != b[i]) {
                return false;
            }
        }
        return true;
    }

    public static Term Parse(string str) {
        if (str.Length == 0) {
            throw new ArgumentException("str must have a length of at least 1");
        }
        if (str.Any(c => c != '^' && !AdditiveSymbol(c) && (!char.IsLetterOrDigit(c) || char.IsUpper(c)))) {
            throw new ArgumentException("str must only contain lowercase letters and numbers");
        }
    
        if (str[0] == '+') {
            str = str[1..];
        }

        var coefficient = 1;
        var vars = new List<Variable>();

        int letterIndex = Array.FindIndex(str.ToCharArray(), c => char.IsLower(c));
        if (letterIndex >= 0) {
            if (letterIndex > 0) {
                if (str[..letterIndex] == "-") {
                    coefficient = -1;
                }
                else if (str[..letterIndex] != "+") {
                    coefficient = int.Parse(str[..letterIndex]);
                }
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

    public static bool IsTerm(string str) {
        if (str.Length == 0) {
            return false;
        }
        for (var i = 0; i < str.Length; i++) {
            char c = str[i];
            if  (char.IsLetter(c) || 
                (char.IsDigit(c) && i == 0) || 
                (i - 1 >= 0 && char.IsDigit(c) && (char.IsDigit(str[i - 1]) || str[i - 1] == '^')) ||
                (i - 1 >= 0 && c == '^' && char.IsLetter(str[i - 1]) && i + 1 < str.Length && char.IsDigit(str[i + 1])) ||
                (i == 0 && AdditiveSymbol(c) && str.Length > 1)) {
                continue;
            }
            return false;
        }
        return true;
    }

    public static bool AdditiveSymbol(char c) => c == '+' || c == '-';

    public static char OppositeAdditiveSymbol(char c) => c == '+' ? '-' : '+';

    public static bool PerfectPower(Term t, int exponent) => Math.Pow(Math.Abs(t.Coefficient), Math.ReciprocalEstimate(exponent)) % 1 == 0 && t.Vars.All(v => v.Exponent % exponent == 0);

    public static Term? Root(Term t, int rootPower) {
        if (t.Equals(new Term(0))) {
            return t;
        }
        if (!PerfectPower(t, rootPower)) {
            return null;
        }
        var newVarList = new List<Variable>();
        foreach (Variable v in t.Vars) {
            newVarList.Add(new Variable(v.Symbol, v.Exponent / rootPower));
        }
        return new Term((int) Math.Pow(Math.Abs(t.Coefficient), Math.ReciprocalEstimate(rootPower)) * (t.Coefficient / Math.Abs(t.Coefficient)), newVarList.ToArray());
    }

    public static Term Pow(Term t, int power) {
        var newVarList = new List<Variable>();
        foreach (Variable v in t.Vars) {
            newVarList.Add(new Variable(v.Symbol, v.Exponent * power));
        }
        return new Term((int) Math.Pow(t.Coefficient, power), newVarList.ToArray());
    }

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
        if (t1.Coefficient % t2.Coefficient != 0) {
            return null;
        }

        int newCoefficient = t1.Coefficient / t2.Coefficient;
        if (t2.Vars.Length == 0) {
            return new(newCoefficient, t1.Vars);
        }

        // -8xy / x = -8y
        var newVars = new List<Variable>();
        for (var i = 0; i < t2.Vars.Length; i++) {
            Variable denominator = t2.Vars[i];
            bool atLeastOne = false;
            for (var j = 0; j < t1.Vars.Length; j++) {
                var numerator = t1.Vars[j];
                if (newVars.Any(v => v.Symbol == numerator.Symbol)) {
                    continue;
                }
                if (numerator.Symbol == denominator.Symbol) {
                    int newExponent = numerator.Exponent - denominator.Exponent;
                    if (newExponent < 0) {
                        return null;
                    }
                    atLeastOne = true;
                    newVars.Add(new(t1[j].Symbol, newExponent));
                    continue;
                }
            }
            if (!atLeastOne) {
                return null;
            }
        }
        
        for (var i = 0; i < t1.Vars.Length; i++) {
            if (newVars.All(v => v.Symbol != t1.Vars[i].Symbol)) {
                newVars.Add(t1.Vars[i]);
            }
        }
        newVars.RemoveAll(v => v.Exponent == 0);
        return new(newCoefficient, newVars.ToArray());
    }

    public static bool Divisible(Term t1, Term t2) {
        return t1 / t2 is not null;
    }

    public static Term? operator +(Term t1, Term t2) {
        if (t1.Coefficient == 0) {
            return t2;
        }
        if (t2.Coefficient == 0) {
            return t1;
        }
        if (!SameVariableSet(t1, t2)) {
            return null;
        }
        return new(t1.Coefficient + t2.Coefficient, t1._vars.ToArray());
    }

    public static Term? operator -(Term t1, Term t2) =>
        t1 + new Term(-t2.Coefficient, t2._vars.ToArray());

    public static Term operator -(Term t) =>
        new(-t.Coefficient, t.Vars.ToArray());

    public static bool operator ==(Term t1, Term t2) => t1.Equals(t2);

    public static bool operator !=(Term t1, Term t2) => !t1.Equals(t2);

    public Variable this[int index] {
        get => _vars[index];
    }

    public bool Contains(Variable variable) {
        foreach (var otherVar in Vars) {
            if (otherVar.Equals(variable)) {
                return true;
            }
        }
        return false;
    }

    public override string ToString() {
        string result = "";
        if (Coefficient == -1) {
            result += "-";
            if (_vars.Count == 0) {
                result += "1";
            }
        }
        else if (_vars.Count == 0 || Coefficient != 1) {
            result += Coefficient;
        }

        foreach (Variable variable in _vars) {
            result += variable.ToString();
        }
        return result;
    }

    public override bool Equals(object? obj) {
        if (obj is not Term || obj is null) {
            return false;
        }
        Term term = (Term) obj;
        return SameVariableSet(this, term) && Coefficient == term.Coefficient;
    }

    public override int GetHashCode() => HashCode.Combine(Coefficient, _vars);
  
    public int CompareTo(Term? other) {
        if (other is null) {
            throw new NullReferenceException("other must not be null");
        }
        if (_vars.Count == 0 || other._vars.Count == 0) {
            return -_vars.Count.CompareTo(other._vars.Count);
        }
        
        for (var i = 0; i < Math.Min(Vars.Length, other.Vars.Length); i++) {
            int comparison = CompareVars(Vars[i], other.Vars[i]);
            if (comparison != 0) {
                return comparison;
            }
        }
        return 0;
    }

    private static int CompareVars(Variable a, Variable b) {
        if (a.Symbol == b.Symbol) {
            return -a.Exponent.CompareTo(b.Exponent);
        }
        return a.Symbol.CompareTo(b.Symbol);
    }
}