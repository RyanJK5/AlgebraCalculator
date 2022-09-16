namespace AlgebraCalculator;

readonly struct Polynomial {

    private readonly List<Term> terms;

    public Polynomial(params Term[] terms) {
        this.terms = terms.ToList();
        simplify();
    }

    public Polynomial() : this(new Term[0]) { }

    public Term this[int index] {
        get => terms[index];
    }

    private void simplify() {
        terms.Sort();
    }

    public override string ToString() {
        string result = "";
        for (var i = 0; i < terms.Count; i++) {
            if (terms[i].Coefficient < 0) {
                result += terms[i].ToString().Substring(1);
            } else {
                result += terms[i];
            }

            if (i != terms.Count - 1) {
                result += $" {(terms[i + 1].Coefficient >= 0 ? "+" : "-")} ";
            }
        }
        return result;
    }

    public static Polynomial Parse(string str) {
        if (str.Length == 0) {
            throw new ArgumentException("str must have a length of at least 1");
        }
        if (str.Any(c => !Util.ValidChar(c))) {
            throw new ArgumentException("str contains invalid characters");
        }

        List<Term> terms = new List<Term>();
        int lowIndex = 0;
        int highIndex = 0;
        for (var i = 0; i < str.Length; i++) {
            highIndex = Array.FindIndex(str.ToCharArray(), lowIndex, c => char.IsWhiteSpace(c));
            if (highIndex == -1) {
                highIndex = str.Length;
            }
            terms.Add(Term.Parse(str.Substring(lowIndex, highIndex - lowIndex)));
            lowIndex = highIndex + 1;

            if (highIndex >= str.Length) {
                break;
            }
        }

        return new(terms.ToArray());
    }
} 