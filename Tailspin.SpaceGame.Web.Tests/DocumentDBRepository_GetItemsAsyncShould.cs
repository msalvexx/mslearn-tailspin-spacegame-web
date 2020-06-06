using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TailSpin.SpaceGame.Web;
using TailSpin.SpaceGame.Web.Models;

namespace Tests
{
    public class DocumentDBRepository_GetItemsAsyncShould
    {
        private IDocumentDBRepository<Score> _scoreRepository;

        [SetUp]
        public void Setup()
        {
            using (Stream scoresData = typeof(IDocumentDBRepository<Score>)
                .Assembly
                .GetManifestResourceStream("Tailspin.SpaceGame.Web.SampleData.scores.json"))
            {
                _scoreRepository = new LocalDocumentDBRepository<Score>(scoresData);
            }
        }

        [TestCase("Milky Way")]
        [TestCase("Andromeda")]
        [TestCase("Pinwheel")]
        [TestCase("NGC 1300")]
        [TestCase("Messier 82")]
        public void FetchOnlyRequestedGameRegion(string gameRegion)
        {
            const int PAGE = 0; // take the first page of results
            const int MAX_RESULTS = 10; // sample up to 10 results

            // Form the query predicate.
            // This expression selects all scores for the provided game region.
            Expression<Func<Score, bool>> queryPredicate = score => (score.GameRegion == gameRegion);

            // Fetch the scores.
            Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetItemsAsync(
                queryPredicate, // the predicate defined above
                score => 1, // we don't care about the order
                PAGE,
                MAX_RESULTS
            );
            IEnumerable<Score> scores = scoresTask.Result;

            // Verify that each score's game region matches the provided game region.
            Assert.That(scores, Is.All.Matches<Score>(score => score.GameRegion == gameRegion));
        }

        [TestCase("Milky Way", 6)]
        [TestCase("Andromeda", 3)]
        [TestCase("Pinwheel", 3)]
        [TestCase("NGC 1300", 5)]
        [TestCase("Messier 82", 4)]
        public void CountFetchOnlyRequestedItems(string gameRegion, int scoresCountExpected) {
            // Form the query predicate.
            // This expression selects all scores for the provided game region.
            Expression<Func<Score, bool>> queryPredicate = score => (score.GameRegion == gameRegion);

            // Fetch the scores.
            Task<int> scoresCountTask = _scoreRepository.CountItemsAsync(
                queryPredicate // the predicate defined above
            );
            int scoresCount = scoresCountTask.Result;

            // Verify that each score's game region matches the provided game region.
            Assert.AreEqual(scoresCount, scoresCountExpected);
        }

        [TestCase("1")]
        [TestCase("25")]
        public void GetExistentItemShouldReturnItem(string id) {
            // Fetch the scores.
            Task<Score> scoreTask = _scoreRepository.GetItemAsync(id);
            Score scoreResult = scoreTask.Result;

            Assert.True(scoreResult.Id == id);
        }

        [TestCase("0")]
        [TestCase("26")]
        public void GetInexistentItemShouldReturnNull(string id) {
            // Fetch the scores.
            Task<Score> scoreTask = _scoreRepository.GetItemAsync(id);
            Score scoreResult = scoreTask.Result;

            Assert.True(scoreResult == null);
        }
    }
}