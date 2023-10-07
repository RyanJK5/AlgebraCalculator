namespace AlgebraCalculator;
class Polynomial {

    private List<Term> Terms;

    public Polynomial(params Term[] terms) {
        Terms = new();
        Terms.AddRange(terms);
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