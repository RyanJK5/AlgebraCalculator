using System.Diagnostics.CodeAnalysis;

namespace AlgebraCalculator;
class Polynomial {

    private List<Term> Terms;

    public Polynomial(params Term[] terms) {
        Terms = new();
        Terms.AddRange(terms);
        Simplify();
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
                if (sumTerm is not null) {
                    newList[newList.IndexOf(matchingTerm)] = sumTerm;
                   continue;
                }
            }
            newList.Add(term);
        }
        Terms = newList;
    }

    public static Polynomial operator *(Polynomial a, Polynomial b) {
        var newPolyList = new List<Term>();
        foreach (Term term in a.Terms) {
            foreach (Term term2 in b.Terms) {
                newPolyList.Add(term * term2);                
            }
        }
        return new Polynomial(newPolyList.ToArray());
    }

    public static Polynomial operator +(Polynomial a, Polynomial b) {
        var newPolyList = new List<Term>();
        newPolyList.AddRange(a.Terms);
        newPolyList.AddRange(b.Terms);
        return new Polynomial(newPolyList.ToArray());
    }

    public static Polynomial operator -(Polynomial a) {
        var newPolyList = new List<Term>();
        foreach (Term term in a.Terms) {
            newPolyList.Add(new Term(-term.Coefficient, term.Vars));
        }
        return new Polynomial(newPolyList.ToArray());
    }

    public static Polynomial operator -(Polynomial a, Polynomial b) =>
        a + -b;
}