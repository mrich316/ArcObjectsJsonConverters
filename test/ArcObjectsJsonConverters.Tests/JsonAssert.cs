using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace ArcObjectJsonConverters.Tests
{
    public class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            var expectedToken = JToken.Parse(expected);
            var actualToken = JToken.Parse(actual);

            var diff = new JsonDiffPatch().Diff(expectedToken, actualToken);
            if (diff != null)
            {
                throw new AssertActualExpectedException(expected, actual, "Json is not equal.");
            }
        }

        public static void Contains(string expectedSubstring, string actual)
        {
            var expectedToken = JToken.Parse($"{{\"expected\":{expectedSubstring}}}");
            var actualToken = JToken.Parse(actual);

            Assert.Contains(expectedToken["expected"].ToString(), actualToken.ToString());
        }
    }
}