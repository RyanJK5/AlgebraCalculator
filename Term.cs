namespace AlgebraCalculator;

readonly struct Term : IComparable<Term> {

    public readonly int Coefficient;
    private readonly List<Variable> variables;

    public Term(int coefficient, params Variable[] variables) {
        this.Coefficient = coefficient;
        this.variables = new List<Variable>();
        this.variables.AddRange(variables);
        this.variables.Sort();
        simplify();
    }

    public Term() : this(1, new Variable[0]) { }

    private void simplify() {
        var newArr = new List<Variable>();
        for (var i = 0; i < variables.Count; i++) {
            Variable variable = variables[i];
            var found = false;
            for (var j = 0; j < variables.Count; j++) {
                if (i == j) {
                    continue;
                }
                Variable otherVar = variables[j];
                if (variable.Letter == otherVar.Letter && newArr.Any(v => v.Letter == variable.Letter)) {
                    int index = newArr.FindIndex(v => v.Letter == variable.Letter);
                    newArr[index] = new Variable(newArr[index].Letter, newArr[index].Exponent + variable.Exponent);
                    found = true;
                    break;
                }
            }
            if (!found) {
                newArr.Add(variables[i]);
            }
        }
        variables.Clear();
        variables.AddRange(newArr);
        variables.Sort();
    }

    public static Term Parse(string str) {
        if (str.Length == 0) {
            throw new ArgumentException("str must have a length of at least 1");
        }
        if (str.Any(c => c != '-' && (!char.IsLetterOrDigit(c) || char.IsUpper(c)))) {
            throw new ArgumentException("str must only contain lowercase letters and numbers");
        }
        

        var newCoefficient = 1;
        List<Variable> vars = new List<Variable>();

        int letterIndex = Array.FindIndex(str.ToCharArray(), c => char.IsLower(c));
        if (letterIndex >= 0) {
            if (letterIndex > 0) {
                newCoefficient = int.Parse(str.Substring(0, letterIndex));
            }
            int lowIndex = letterIndex;
            int highIndex = letterIndex;
            for (var i = letterIndex; i < str.Length; i++) {
                highIndex = Array.FindIndex(str.ToCharArray(), lowIndex + 1, c => char.IsLower(c));
                if (highIndex < 0) {
                    highIndex = str.Length;
                }
                vars.Add(Variable.Parse(str.Substring(lowIndex, highIndex - lowIndex)));
                lowIndex = highIndex;
                if (highIndex == str.Length) { 
                    break;
                }
            }
        }
        return new(newCoefficient, vars.ToArray());
    }

    public static Term operator *(Term t1, Term t2) {
        int newCoefficient = t1.Coefficient * t2.Coefficient;
        var newVariables = new Variable[t1.variables.Count + t2.variables.Count];
        Term[] terms = {t1, t2};
        for (var i = 0; i < 2; i++) {
            for (var j = 0; j < terms[i].variables.Count; j++) {
                newVariables[j + (i == 0 ? 0 : terms[0].variables.Count)] = terms[i][j]; 
            }
        }
        return new(newCoefficient, newVariables);
    }

    public static Term? operator /(Term t1, Term t2) {
        if (t1.Coefficient % t2.Coefficient != 0 || t1.variables.Count != t2.variables.Count) {
            return null;
        }

        int newCoefficient = t1.Coefficient / t2.Coefficient;
        var newVars = new List<Variable>();
        for (var i = 0; i < t1.variables.Count; i++) {
            if (t1[i].Letter != t2[i].Letter) {
                return null;
            }

            int newExponent = t1[i].Exponent - t2[i].Exponent;
            if (newExponent != 0) {
                newVars.Add(new(t1[i].Letter, newExponent));
            } 
        }
        return new(newCoefficient, newVars.ToArray());
    }

    public static Term? operator +(Term t1, Term t2) {
        if (t1.variables.Count != t2.variables.Count) {
            return null;
        }
        var vars = new List<Variable>();
        for (var i = 0; i < t1.variables.Count; i++) {
            if (t1[i] != t2[i]) {
                return null;
            }
            vars.Add(t1[i]);
        }
        return new(t1.Coefficient + t2.Coefficient, vars.ToArray());
    }

    public static Term? operator -(Term t1, Term t2) =>
        t1 + new Term(-t2.Coefficient, t2.variables.ToArray());

    public static bool operator ==(Term t1, Term t2) => t1.Equals(t2);

    public static bool operator !=(Term t1, Term t2) => !t1.Equals(t2);

    public Variable this[int index] {
        get => variables[index];
    }

    public override string ToString() {
        string result = "";
        if (Coefficient == -1) {
            result += "-";
        }
        else if (Coefficient != 1) {
            result += Coefficient;
        }

        foreach (Variable variable in variables) {
            result += variable.ToString();
        }
        return result;
    }

    public override bool Equals(object? obj) {
        if (obj !is Term || obj == null) {
            return false;
        }
        Term term = (Term) obj;
        for (var i = 0; i < variables.Count; i++) {
            if (variables[i] != term[i]) {
                return false;
            }
        }
        return Coefficient == term.Coefficient;
    }

    public override int GetHashCode() => HashCode.Combine(Coefficient, variables);

    public int CompareTo(Term other) {
        return -this[variables.Count - 1].CompareTo(other[other.variables.Count - 1]);
    }
}