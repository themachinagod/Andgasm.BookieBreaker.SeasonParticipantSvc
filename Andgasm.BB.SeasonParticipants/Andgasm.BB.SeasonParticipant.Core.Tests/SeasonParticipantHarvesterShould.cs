using Andgasm.BB.Harvest;
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http;
using Andgasm.Http.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Andgasm.BB.SeasonParticipant.Core.Tests
{
    [TestClass]
    public class SeasonParticipantHarvesterShould
    {
        List<ExpandoObject> identifiedClubs = new List<ExpandoObject>();

        [TestMethod]
        public async Task IdentifyAllClubDataFromPayload_WhenAllKeysProvided()
        {
            var h = InitialiseHarvester();
            await h.Execute();

            Assert.AreEqual(20, identifiedClubs.Count);
        }

        [TestMethod]
        public async Task ExtractClubDataFromPayload_WhenAllKeysProvided()
        {
            var h = InitialiseHarvester();
            await h.Execute();

            dynamic c = identifiedClubs.First();
            Assert.AreEqual("Manchester City", c.ClubName);
            Assert.AreEqual("167", c.ClubKey);
        }

        [TestMethod]
        public void BypassExecution_WhenStageKeyMissing()
        {
            var h = InitialiseHarvester(false, true, true, true, true);
            var r = h.CanExecute();
            Assert.AreEqual(false, r);
        }

        [TestMethod]
        public void BypassExecution_WhenCountryKeyMissing()
        {
            var h = InitialiseHarvester(true, false, true, true, true);
            var r = h.CanExecute();
            Assert.AreEqual(false, r);
        }

        [TestMethod]
        public void BypassExecution_WhenRegionKeyMissing()
        {
            var h = InitialiseHarvester(true, true, false, true, true);
            var r = h.CanExecute();
            Assert.AreEqual(false, r);
        }

        [TestMethod]
        public void BypassExecution_WhenSeasonKeyMissing()
        {
            var h = InitialiseHarvester(true, true, true, false, true);
            var r = h.CanExecute();
            Assert.AreEqual(false, r);
        }

        [TestMethod]
        public void BypassExecution_WhenTournamentKeyMissing()
        {
            var h = InitialiseHarvester(true, true, true, true, false);
            var r = h.CanExecute();
            Assert.AreEqual(false, r);
        }

        [TestMethod]
        public async Task BypassExecution_WhenInitialRequestFails()
        {
            var h = InitialiseHarvester(true, true, true, true, true, true);
            await h.Execute();
            Assert.AreEqual(0, identifiedClubs.Count);
        }

        #region Setup
        private SeasonParticipantHarvester InitialiseHarvester(bool setstage = true, bool setcountry = true, bool setregion = true, bool setseason = true, bool settourny = true, bool nullrequest = false)
        {
            var s = new Mock<ApiSettings>();
            var e = new Mock<IHarvestRequestManager>();
            var es = e.Setup(x => x.MakeRequest(It.IsAny<string>(), It.IsAny<HttpRequestContext>(), false));
            if (!nullrequest)
            {
                es.ReturnsAsync(new HarvestRequestResult()
                {
                    InnerHtml = GetTestHtml(),
                    InnerText = "testtext"
                });
            }
            else
            {
                es.Returns(Task.FromResult<IHarvestRequestResult>(null));
            }
            var h = new Mock<IHttpRequestManager>();
            h.Setup(x => x.Post(It.IsAny<List<ExpandoObject>>(), It.IsAny<string>(), null)).Callback<List<ExpandoObject>, string, IHttpRequestContext>((x, y, z) => identifiedClubs = x);

            SeasonParticipantHarvester ucs = new SeasonParticipantHarvester(s.Object, new Logger<SeasonParticipantHarvester>(new NullLoggerFactory()), e.Object, h.Object);
            if (setcountry) ucs.CountryKey = "test";
            if (setregion) ucs.RegionKey = "test";
            if (setseason) ucs.SeasonKey = "test";
            if (setstage) ucs.StageKey = "test";
            if (settourny) ucs.TournamentKey = "test";
            return ucs;
        }

        private string GetTestHtml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Andgasm.BB.SeasonParticipant.Core.Tests.FullResponse.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion
    }
}
