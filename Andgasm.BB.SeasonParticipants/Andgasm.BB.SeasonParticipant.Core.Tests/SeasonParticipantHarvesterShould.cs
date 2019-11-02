using Andgasm.BB.Harvest;
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Reflection;
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
            //await h.Execute();
        }

        private SeasonParticipantHarvester InitialiseHarvester()
        {
            var e = new Mock<IHarvestRequestManager>();
            var h = new Mock<IHttpRequestManager>();
            var s = new Mock<ApiSettings>();
            e.Setup(x => x.MakeRequest(It.IsAny<string>(), It.IsAny<HarvestRequestContext>(), false))
                .ReturnsAsync(new HarvestRequestResult()
                {
                    InnerHtml = GetTestHtml(),
                    InnerText = "testtext"
                });

            SeasonParticipantHarvester ucs = new SeasonParticipantHarvester(s.Object, new Logger<SeasonParticipantHarvester>(new NullLoggerFactory()), e.Object, h.Object);
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
    }
}
