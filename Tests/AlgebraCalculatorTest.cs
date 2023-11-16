namespace AlgebraCalculator.Tests;

using Xunit;

public static class AlgebraCalculatorTest {

    [Theory]
    [InlineData("2(x+4(x^2-3)(2x+1))-4", "")]
    public static void SimplifyTest(string input, string expected) {
        Assert.True(AlgebraCalculator.ValidString(input) && AlgebraCalculator.SimplifyAndFactor(input) == expected);
        
    }
}