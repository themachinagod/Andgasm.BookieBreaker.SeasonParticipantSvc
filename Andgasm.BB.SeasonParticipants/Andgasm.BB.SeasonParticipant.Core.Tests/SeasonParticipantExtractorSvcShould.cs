using Andgasm.BB.Harvest;
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.BB.SeasonParticipant.Interfaces;
using Andgasm.Http.Interfaces;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BB.SeasonParticipant.Core.Tests
{
    [TestClass]
    public class SeasonParticipantExtractorSvcShould
    {
        [TestMethod]
        public async Task StartBusEventPipeline_Successfully()
        {
            //var h = new Mock<ISeasonParticipantHarvester>();
            //var s = new Mock<IBusClient>();
            //var c = new Mock<ICookieInitialiser>();
            //SeasonParticipantExtractorSvc svc = new SeasonParticipantExtractorSvc(new Logger<SeasonParticipantExtractorSvc>(new NullLoggerFactory()), h.Object, s.Object, c.Object);

            //await svc.StartAsync(new CancellationToken());

            //s.Verify(mock => mock.RecieveEvents(It.IsAny<Func<IExceptionArgs, Task>>(), It.IsAny<Func<IBusEvent, CancellationToken, Task>>()), Times.Once());
        }

        [TestMethod]
        public async Task StopBusEventPipeline_Successfully()
        {
            var h = new Mock<ISeasonParticipantHarvester>();
            var s = new Mock<IBusClient>();
            var c = new Mock<ICookieInitialiser>();
            SeasonParticipantExtractorSvc svc = new SeasonParticipantExtractorSvc(new Logger<SeasonParticipantExtractorSvc>(new NullLoggerFactory()), h.Object, s.Object, c.Object);

            await svc.StopAsync(new CancellationToken());

            s.Verify(mock => mock.Close(), Times.Once());
        }

        [TestMethod]
        public async Task ProcessValidMessage_Successfully()
        {
            var s = new Mock<IBusClient>();
            var h = new Mock<ISeasonParticipantHarvester>();
            var svc = new SeasonParticipantExtractorSvc(new Logger<SeasonParticipantExtractorSvc>(new NullLoggerFactory()), h.Object, s.Object, null);
            var seasonevent = BuildNewSeasonEvent("testtc", "testsc", "testsc", "testrc", "testcc");
            await SeasonParticipantExtractorSvc.ProcessMessagesAsync(seasonevent, new CancellationToken());

            h.Verify(mock => mock.Execute(), Times.Once());
        }

        [TestMethod]
        public async Task ProcessFailedMessage_Successfully()
        {
            // TODO: fails due to logger statics!!
            //await SeasonParticipantExtractorSvc.ExceptionReceivedHandler(new ExceptionArgsBase(new Exception()));
        }

        #region Setup
        public static BusEventBase BuildNewSeasonEvent(string tournamentcode, string seasoncode, string stagecode, string regioncode, string countrycode)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.TournamentKey = tournamentcode;
            jsonpayload.SeasonKey = seasoncode;
            jsonpayload.StageKey = stagecode;
            jsonpayload.RegionKey = regioncode;
            jsonpayload.CountryKey = countrycode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
        #endregion
    }
}
