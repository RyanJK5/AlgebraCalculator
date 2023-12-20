namespace AlgebraCalculator.Tests;

using System.Runtime.InteropServices;
using Xunit;

public static class AlgebraCalculatorTest {

    [Theory]
    [InlineData("2(x+4(x^2-3)(2x+1))-4", "16x^3+8x^2-46x-28")]
    [InlineData("0","0")]
    [InlineData("4(9+3-2*4-3)*4-9", "7")]
    [InlineData("3ab^2+3a^2b-2ab^2+4a^2b", "7a^2b+ab^2")]
    [InlineData("(x+3)(x+2)", "x^2+5x+6")]
    [InlineData("(x-4)^2", "x^2-8x+16")]
    [InlineData("(x-4)^2*4(x^2+3)^3", "4x^8-32x^7+100x^6-288x^5+684x^4-864x^3+1836x^2-864x+1728")]
    [InlineData("(a+b+c)^3","a^3+3a^2b+3a^2c+3ab^2+6abc+3ac^2+b^3+3b^2c+3bc^2+c^3")]
    [InlineData("8*10^2+ax+5b*23ax*b", "115ab^2x+ax+800")]
    public static void SimplifyTest(string input, string expected) {
        Assert.True(AlgebraCalculator.ValidString(input) && AlgebraCalculator.Simplify(input)[0] == expected);
    }

    [Theory]
    
    // GCF
    [InlineData("3x^2-6x+9","3(x^2-2x+3)")]
    [InlineData("4x^3-8x^2+12x","4x(x^2-2x+3)")]
    [InlineData("5x^4-10x^2+70x^2", "5x^2(x^2+12)")]
    [InlineData("4x^4-3x^3+2x^2-2x", "x(4x^3-3x^2+2x-2)")]
    [InlineData("x^2y+4xy", "xy(x+4)")]
    [InlineData("a^3b^3c^3-a^2b^2c^2+a^2b^2c", "a^2b^2c(abc^2-c+1)")]
    [InlineData("x^2y", "x^2y")]
    
    // DOTS
    [InlineData("x^2-4","(x-2)(x+2)")]
    [InlineData("4x^2-9","(2x-3)(2x+3)")]
    [InlineData("16x^2y^2-9a^2b^4","(4xy-3ab^2)(3ab^2+4xy)")]
    [InlineData("x^2+4","x^2+4")]

    // Quadratics
    [InlineData("x^2+2x-3", "(x-1)(x+3)")]
    [InlineData("x^2-8xy+16y^2", "(x-4y)(x-4y)")]
    [InlineData("x^4+9x^2-10", "(x-1)(x+1)(x^2+10)")]
    [InlineData("2x^2-3x+1","(x-1)(2x-1)")]
    [InlineData("4x^4y^2-11x^2yz-3z^2","(x^2y-3z)(4x^2y+z)")]

    // Mixed
    [InlineData("5x^2y^2-45y^4","5y^2(x-3y)(x+3y)")]
    [InlineData("6x^4-96","6(x-2)(x+2)(x^2+4)")]

    public static void FactorTest(string input, string expected) {
        //  x^2y = (11z +/- sqrt(121z^2-4(4)(-3z^2))) / 2(4)
        // x^2y = 24z / 8 = 3z
        // x^2y = -2z / 8 = -z/4
        // (x^2y-3z)(4x^2y+z) 
        Assert.True(AlgebraCalculator.ValidString(input) && AlgebraCalculator.SimplifyAndFactor(input).Equals(expected));
    }
}