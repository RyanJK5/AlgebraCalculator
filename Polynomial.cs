namespace AlgebraCalculator;
class Polynomial {

    private List<Term> Terms;

    public Polynomial(params Term[] terms) {
        Terms = new();
        Terms.AddRange(terms);
    }

    public static Polynomial Parse(string str) {
        var result = new List<Term>();
        var termStartIndex = 0;
        for (var i = 0; i < str.Length; i++) {
            if (Term.IsTerm(str[termStartIndex..(i + 1)])) {
                if (i == str.Length - 1) {
                    result.Add(Term.Parse(str[termStartIndex..]));
                }
            }
            else if (Term.IsTerm(str[termStartIndex..i])) {
                result.Add(Term.Parse(str[termStartIndex..i]));
                termStartIndex = i;
            }
        }
        return new Polynomial(result.ToArray());
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
}