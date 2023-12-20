namespace AlgebraCalculator;

class Polynomial {

    public List<Term> Terms { get; private set; }

    public int TermCount { get => Terms.Count; }

    public Polynomial(params Term[] terms) {
        Terms = new();
        Terms.AddRange(terms);
        Simplify();
    }

    public Polynomial(List<Term> terms) {
        Terms = new();
        Terms.AddRange(terms);
        Simplify();
    }

    public Term GetTerm(int index) {
        return Terms[index];
    }

    public override string ToString() {
        string result = "";
        for (var i = 0; i < Terms.Count; i++) {
            if (i > 0 && Terms[i].Coefficient > 0) {
                result += "+";
            }
            result += Terms[i];
        }
        return result;
    }

    private void Simplify() {
        var newList = new List<Term>();
        foreach (Term term in Terms) {
            Term? matchingTerm = newList.Find(t => Term.SameVariableSet(t, term));
            if (matchingTerm is not null) {
                Term? sumTerm = matchingTerm + term;
                if (sumTerm?.Coefficient == 0) {
                    newList.Remove(matchingTerm);
                    continue;
                }
                if (sumTerm is not null) {
                    newList[newList.IndexOf(matchingTerm)] = sumTerm;
                   continue;
                }
            }
            newList.Add(term);
        }
        Terms = newList;
        Sort();
    }

    private void Sort() {
        Terms.Sort();
        if (Terms.Count == 2 && ((Terms[0].Coefficient < 0) != (Terms[1].Coefficient < 0)) && Terms[1].Coefficient > Terms[0].Coefficient) {
            (Terms[1], Terms[0]) = (Terms[0], Terms[1]);
        }
    }

    public int HighestExponent() {
        int result = 0;
        for (var i = 0; i < Terms.Count; i++) {
            var termVars = Terms[i].Vars;
            for (var j = 0; j < termVars.Length; j++) {
                if (termVars[j].Exponent > result) {
                    result = termVars[j].Exponent;
                }
            }
        }
        return result;
    }

    public static Polynomial Parse(string str) {
        var termList = new List<Term>();
        for (var i = 1; i < str.Length; i++) {
            if (str[i] == '+' || str[i] == '-') {
                termList.Add(Term.Parse(str[0..i]));
                str = str[i..];
                i = 1;
            }
        }
        termList.Add(Term.Parse(str));
        return new Polynomial(termList);
    }

    public static Polynomial Pow(Polynomial p, int exponent) {
        Polynomial result = p;
        for (var i = 1; i < exponent; i++) {
            result *= p;
        }
        return result;
    }

    public static Polynomial operator *(Polynomial a, Polynomial b) {
        var newPolyList = new List<Term>();
        foreach (Term term in a.Terms) {
            foreach (Term term2 in b.Terms) {
                newPolyList.Add(term * term2);                
            }
        }
        return new Polynomial(newPolyList);
    }

    public static Polynomial operator +(Polynomial a, Polynomial b) {
        var newPolyList = new List<Term>();
        newPolyList.AddRange(a.Terms);
        newPolyList.AddRange(b.Terms);
        return new Polynomial(newPolyList);
    }

    public static Polynomial operator -(Polynomial a) {
        var newPolyList = new List<Term>();
        foreach (Term term in a.Terms) {
            newPolyList.Add(new Term(-term.Coefficient, term.Vars));
        }
        return new Polynomial(newPolyList);
    }

    public static Polynomial operator -(Polynomial a, Polynomial b) =>
        a + -b;
}