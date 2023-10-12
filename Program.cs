using AlgebraCalculator;

var poly1 = new Polynomial(new((Variable) "x2"), new((Variable) "y"), new(2, (Variable) "z"));
var poly2 = new Polynomial(new(5, (Variable) "x"), new(-3, (Variable) "y"), new((Variable) "z"));
Console.WriteLine(poly1 + poly2);

// var calc = new AlgebraCalculator.AlgebraCalculator();
// calc.Start();
