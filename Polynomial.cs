using System.Diagnostics.CodeAnalysis;

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

    private void Simplify() {
        var newList = new List<Term>();
        foreach (Term term in Terms) {
            Term? matchingTerm = newList.Find(t => Term.SameVariableSet(t, term));
            if (matchingTerm is not null) {
                newList[newList.IndexOf(matchingTerm)] = new Term(matchingTerm);
                continue;
            }
            newList.Add(variable);
        }
        _vars = newList;
    }

    public static Polynomial operator *(Polynomial a, Polynomial b) {
        var newPoly = new Polynomial();
        foreach (Term term in a.Terms) {
            foreach (Term term2 in b.Terms) {
                newPoly.Terms.Add(term * term2);                
            }
        }
        return newPoly;
    }

    public static Polynomial operator +(Polynomial a, Polynomial b) {
        var newPolyList = new List<Term>();
        foreach (Term term in a.Terms) {
            foreach (Term term2 in b.Terms) {
                Term? sum = term + term2;
                if (sum is null) {
                    continue;
                }
                int index = newPolyList.FindIndex(t => Term.SameVariableSet(t, term));
                if (index >= 0) {
                    Term? result = newPolyList[index] + sum;
                    if (result is not null) {
                        newPolyList[index] = result;
                    }
                    continue;
                }
                newPolyList.Add(sum);
            }
        }
        return new Polynomial(newPolyList.ToArray());
    }
}