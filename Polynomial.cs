namespace AlgebraCalculator;

readonly struct Polynomial {

    private readonly Term[] terms;

    public Polynomial(params Term[] terms) =>
        this.terms = terms
    ;

    public Polynomial() : this(new Term[0]) { }

    public Term this[int index] {
        get => terms[index];
    }
} 