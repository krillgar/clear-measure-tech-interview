namespace BenGenerator.Test
{
    public class GeneratorTests
    {
        private readonly NumberGenerator _generator;
        private readonly string _test1;
        private readonly string _test2;

        public GeneratorTests()
        {
            // Assign these here as we don't need to clutter tests by setting up something in every test.
            _generator = new NumberGenerator();
            _test1 = Guid.NewGuid().ToString();
            _test2 = Guid.NewGuid().ToString();
        }

        [Theory]
        [InlineData(5)]
        [InlineData(750)]
        // This one costs a LOT of time when running the tests. However, it did catch that good case with needing to yield return.
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        public void WhenGivenACountGreaterThanOrEqualToZero_WillReturnThatNumberOfRecords(int maxCount)
        {
            var values = _generator.GenerateNumbers(maxCount);

            Assert.Equal(maxCount, values.Count());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        // No need to worry about this taking time as it defaults to 0.
        [InlineData(int.MinValue)]
        public void WhenGivenACountLessThanZero_WillReturnEmptyCollection(int maxCount)
        {
            var values = _generator.GenerateNumbers(maxCount);

            Assert.Empty(values);
        }

        [Fact]
        // Straight tests with no changing anything. Default behavior
        public void WithNoReplacements_WillNotReplaceAny()
        {
            var values = _generator.GenerateNumbers();

            Assert.All(values, num => int.TryParse(num, out _));
        }

        [Fact]
        public void WithOneReplacementForAllLines_WillReplaceValueForAllRecords()
        {
            var replacements = new Dictionary<int, string>
            {
                {1, _test1}
            };

            var values = _generator.GenerateNumbers(replacementNames: replacements);

            Assert.All(values, line => string.Equals(line, _test1, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        // If this fails, the next test will fail also, but this is good because if this passes and that
        // one fails, we know it's not because of an Exception that was thrown.
        public void WithNullReplacementCollection_WillNotThrowError()
        {
            var ex = Record.Exception(() => _ = _generator.GenerateNumbers(replacementNames: null));

            Assert.Null(ex);
        }

        [Fact]
        public void WithNullReplacement_WillNotChangeAnyValues()
        {
            var results = _generator.GenerateNumbers(replacementNames: null);

            foreach (var item in results.Select((string line, int index) => (line, value: index + 1)))
                Assert.Equal(item.line, item.value.ToString());
        }

        [Fact]
        public void WithEmptyReplacements_WillNotChangeAnyValues()
        {
            var results = _generator.GenerateNumbers(replacementNames: new Dictionary<int, string>());

            foreach (var item in results.Select((string line, int index) => (line, value: index + 1)))
                Assert.Equal(item.line, item.value.ToString());
        }

        [Fact]
        public void WithReplacingOnlySomeValues_WillReplaceAllAtThoseIndexes()
        {
            const int lineNum = 2;
            var replacements = new Dictionary<int, string>
            {
                {lineNum, _test1}
            };

            // We can perform a Where here because we are testing all of the same values for anything that was changed.
            var values = _generator.GenerateNumbers(replacementNames: replacements).Where((string line, int index) => (index + 1) % lineNum == 0);

            Assert.All(values, line => string.Equals(line, _test1, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public void WithReplacingOnlySomeValues_WillNotReplaceAnyNotAtThoseIndexes()
        {
            const int lineNum = 2;
            var replacements = new Dictionary<int, string>
            {
                {lineNum, _test1}
            };

            // Able to do Where again because we're testing the ones that were not changed.
            var values = _generator.GenerateNumbers(replacementNames: replacements).Where((string line, int index) => (index + 1) % lineNum != 0);

            Assert.True(values.All(line => !string.Equals(line, _test1, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Fact]
        public void WithReplacingMultipleValues_WillReplaceBothValues()
        {
            var replacements = new Dictionary<int, string>
            {
                {4, _test1},
                {7, _test2}
            };

            var values = _generator.GenerateNumbers(replacementNames: replacements);

            foreach (var item in values.Select((string line, int index) => (line, value: index + 1)))
                foreach (var kvp in replacements)
                    if (item.value % kvp.Key == 0)
                        // Normally don't like having an Assert happen more than once, but there
                        // is lots of complexity around only some things getting replaced.
                        // We also can't grab all of each Key and do Equals because of the multiples of both keys.
                        // I guess maybe do Assert.All and a Contains in the lambda, but that lambda gets messy.
                        Assert.Contains(kvp.Value, item.line);
        }

        [Fact]
        public void WithReplacingMultipleValues_WillUseBothValuesOnMultiplesOfBothIndexes()
        {
            const int key1 = 4;
            const int key2 = 7;
            const int combinedKey = key1 * key2;
            var replacements = new Dictionary<int, string>
            {
                {key1, _test1},
                {key2, _test2}
            };

            var values = _generator.GenerateNumbers(replacementNames: replacements);

            foreach (var item in values.Select((string line, int index) => (line, value: index + 1)))
                if (item.value % combinedKey == 0)
                {
                    // Again not happy with multiple Asserts, but I wanted to separate just BEING
                    // in the line vs the order due to my OrderBy being added.
                    Assert.Contains(_test1, item.line);
                    Assert.Contains(_test2, item.line);
                }
        }

        [Fact]
        public void WithMultipleValues_WillPutReplacementsInOrderOfKeyValues()
        {
            const int key1 = 4;
            const int key2 = 7;
            const int combinedKey = key1 * key2;
            var replacements = new Dictionary<int, string>
            {
                {key2, _test2},
                {key1, _test1}
            };

            var values = _generator.GenerateNumbers(replacementNames: replacements);

            foreach (var item in values.Select((string line, int index) => (line, value: index + 1)))
                if (item.value % combinedKey == 0)
                    // So testing here now that the OrderBy does what it is supposed to do.
                    Assert.Equal(_test1 + " " + _test2, item.line);
        }

        // With all of the tests being generic and not dependent on specific positions, I would consider
        // this fairly well tested. Perhaps some with three or four replacements, but I don't think adding more
        // tests along those lines would provide the ability to find more potential failures. However, that
        // could be something that we need if we find some Bugs during Production, but I don't have any
        // experiences in my past that are ringing alarm bells for further testing.
    }
}