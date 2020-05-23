using main.Helpers;
using Xunit;

namespace test.Helpers
{
    public class StringHelperTest
    {
        [Theory]
        [InlineData("string", "string", 0)]
        [InlineData("", "", 0)]
        [InlineData("", "string", 6)]
        [InlineData("string", "", 6)]
        [InlineData("stringA", "string", 1)]
        [InlineData("string", "stringA", 1)]
        [InlineData("this_string", "string", 5)]
        public void Test_ComputeLevenshteinDistance_WithSimilarStrings_RetunsCorrectValue(string stringA, string stringB, int expected)
        {
            var result = StringHelper.ComputeLevenshteinDistance(stringA, stringB);

            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void Test_GenerateRandom_WithGivenLength_RetunsRandomStringOfCorrectLength()
        {
            var result = StringHelper.GenerateRandom(10);

            Assert.Equal(10, result.Length);
        }
    }
}
