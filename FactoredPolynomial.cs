using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;

namespace AlgebraCalculator;

class FactoredPolynomial {

    private Term LeadingTerm;
    private readonly List<Polynomial> Factors;

    public FactoredPolynomial(Term leadingTerm, params Polynomial[] factors) {
        Factors = new();
        Factors.AddRange(factors);
        LeadingTerm = leadingTerm;
        CreateLeadingTerm();
    }

    public FactoredPolynomial(params Polynomial[] factors) : this(new Term(1), factors) { }

    private void AddFactor(Polynomial factor) {
        Factors.Add(factor);
        CreateLeadingTerm();
    }

    private void CreateLeadingTerm() {
        for (var i = 0; i < Factors.Count; i++) {
            if (Factors[i].TermCount == 1) {
                LeadingTerm *= Factors[i].GetTerm(0);
                Factors.RemoveAt(i);
                i--;
            }
        }
    }

    public override string ToString() {
        string result = "";
        if (!LeadingTerm.Equals(new Term(1))) {
            result += LeadingTerm;
        }
        else if (Factors.Count == 1) {
            return Factors[0].ToString();
        }
        
        foreach (var factor in Factors) {
            result += "(" + factor.ToString() + ")";
        }
        return result;
    }

    public static FactoredPolynomial Factor(FactoredPolynomial poly) {
        var result = new FactoredPolynomial(poly.LeadingTerm);
        for (var i = 0; i < poly.Factors.Count; i++) {
            FactoredPolynomial newPoly = FactorOnce(poly.Factors[i]);
            if (newPoly.Factors[0].Equals(poly.Factors[i])) {
                result *= newPoly;
                continue;
            }
            newPoly = Factor(newPoly);
            result *= newPoly;
        }
        if (result.Factors.Count == 0) {
            return poly;
        }
        return result;
    }

    public static FactoredPolynomial Factor(Polynomial poly) {
        return Factor(new FactoredPolynomial(poly));
    }

    public static FactoredPolynomial FactorOnce(Polynomial poly) {
        FactoredPolynomial result = DifferenceOfTwoSquares(poly);
        if (result.Factors[0].Equals(poly)) {
            result = FactorQuadratic(poly);
            if (result.Factors[0].Equals(poly)) {
                return GreatestCommonFactor(poly);
            }
        }
        return result;
    }

    public static FactoredPolynomial FactorQuadratic(Polynomial poly) {
        var nullResult = new FactoredPolynomial(poly);
        
        if (poly.Terms.Count != 3 || poly.HighestExponent() % 2 != 0) {
            return nullResult;
        }
        
        Term subjectVar = GreatestCommonFactor(new Polynomial(poly.Terms[0], poly.Terms[1])).LeadingTerm;

        Term? a = poly.Terms[0] / Term.Pow(subjectVar, 2);
        Term? b = poly.Terms[1] / subjectVar;
        Term c = poly.Terms[2];
        if (a is null || b is null) {
            return nullResult;
        }

        Term? radicand = b*b - new Term(4)*a*c;
        if (radicand is null) {
            return nullResult;
        }
        Term? discriminant = Term.Root(radicand, 2);
        if (discriminant is null) {
            return nullResult;
        }

        Term?[] solutions = { -b + discriminant, -b - discriminant};
        Term denominator = new Term(2)*a;
        var result = new FactoredPolynomial();
        foreach (Term? sol in solutions) {
            if (sol is null) {
                return nullResult;
            }
            
            Polynomial factor = GreatestCommonFactor(new Polynomial(subjectVar * denominator, -sol)).Factors[0];;
            result.AddFactor(factor);
        }
        return result;
    }

    public static FactoredPolynomial DifferenceOfTwoSquares(Polynomial poly) {
        if (poly.TermCount == 0) {
            throw new ArgumentException("Polynomial must have at least one term");
        }
        if (poly.TermCount != 2 || ((poly.Terms[0].Coefficient < 0) == (poly.Terms[1].Coefficient < 0))) {
            return new FactoredPolynomial(poly);
        }
        Term? rootTerm1 = Term.Root(poly.Terms[0], 2);
        Term? rootTerm2 = Term.Root(poly.Terms[1], 2);
        if (rootTerm1 is null || rootTerm2 is null) {
            return new FactoredPolynomial(poly);
        }
        Term negativeTerm = rootTerm1.Coefficient < 0 ? rootTerm1 : rootTerm2;
        Term positiveTerm = negativeTerm == rootTerm1 ? rootTerm2 : rootTerm1;

        return new FactoredPolynomial(new Polynomial(positiveTerm, negativeTerm), new Polynomial(positiveTerm, -negativeTerm));
    }

    public static FactoredPolynomial GreatestCommonFactor(Polynomial poly) {
        if (poly.TermCount == 0) {
            throw new ArgumentException("Polynomial must have at least one term");
        }

        Term gcf = FindGCF(poly);

        if (gcf.Equals(new Term(1))) {
            return new FactoredPolynomial(poly);
        }

        var newTerms = new List<Term>();
        for (var i = 0; i < poly.TermCount; i++) {
            Term oldTerm = poly.Terms[i];
            Term? newTerm = oldTerm / gcf;
            if (newTerm is not null) {
                newTerms.Add(newTerm);
            }
        }

        return new FactoredPolynomial(gcf, new Polynomial(newTerms));
    }

    private static Term FindGCF(Polynomial poly) {
        int lowestCoefficient = int.MaxValue;
        for (var i = 0; i < poly.TermCount; i++) {
            Term term = poly.GetTerm(i);
            int newCoefficient = Math.Abs(term.Coefficient);
            if (newCoefficient < lowestCoefficient) {
                lowestCoefficient = newCoefficient;
            }
        }
        
        int gcfCoeff = 1;
        for (var i = lowestCoefficient; i > 1; i--) {
            if (poly.Terms.All(term => term.Coefficient % i == 0)) {
                gcfCoeff = i;
                break;
            }
        }

        var oldVars = new List<Variable>();
        foreach (Term term in poly.Terms) {
            foreach (Variable variable in term.Vars) {
                if (!oldVars.Contains(variable)) {
                    oldVars.Add(variable);
                }
            }
        }
        
        var gcfVars = new List<Variable>();
        Term term1 = poly.GetTerm(0);
        foreach (Variable variable in oldVars) {
            if (poly.Terms.All(t => t / new Term(variable) is not null)) {
                gcfVars.Add(variable);
            }
        }

        return new Term(gcfCoeff, gcfVars.ToArray());
    }

    public static FactoredPolynomial operator *(FactoredPolynomial p1, FactoredPolynomial p2) {
        var newList = new List<Polynomial>(p1.Factors);
        newList.AddRange(p2.Factors);
        return new FactoredPolynomial(p1.LeadingTerm * p2.LeadingTerm, newList.ToArray());
    }
}