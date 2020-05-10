using main.Helpers;
using Xunit;

namespace tests.Helpers
{
    public class StringHelperTest
    {
        [Fact]
        public void Test_ComputeLevenshteinDistance_WithSimilarStrings_RetunsSame()
        {
            var stringA = "AnyString";
            var stringB = "AnyString";

            var result = StringHelper.ComputeLevenshteinDistance(stringA, stringB);

            Assert.Equal(1, result);
        }
    }
}
