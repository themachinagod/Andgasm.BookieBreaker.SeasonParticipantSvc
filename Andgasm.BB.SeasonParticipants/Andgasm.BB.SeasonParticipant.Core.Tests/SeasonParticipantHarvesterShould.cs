using Andgasm.BB.Harvest;
using Andgasm.BB.Harvest.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Andgasm.BB.SeasonParticipant.Core.Tests
{
    [TestClass]
    public class SeasonParticipantHarvesterShould
    {
        [TestMethod]
        public async Task Execute()
        {
            var h = InitialiseHarvester();
            await h.Execute();
        }

        private SeasonParticipantHarvester InitialiseHarvester()
        {
            var e = new Mock<IHarvestRequestManager>();
            var s = new Mock<ApiSettings>();
            e.Setup(x => x.MakeRequest(It.IsAny<string>(), It.IsAny<HarvestRequestContext>(), false))
                .ReturnsAsync(new HarvestRequestResult()
                {
                    InnerHtml = "testhtml",
                    InnerText = "testtext"
                });

            SeasonParticipantHarvester ucs = new SeasonParticipantHarvester(s.Object, new Logger<SeasonParticipantHarvester>(new NullLoggerFactory()), e.Object);
            return ucs;
        }
    }
}
