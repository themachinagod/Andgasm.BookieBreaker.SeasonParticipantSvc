using Andgasm.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.SeasonParticipant.Core
{
    public class SeasonParticipantExtractorSvc : IHostedService
    {
        static ILogger<SeasonParticipantExtractorSvc> _logger;
        static SeasonParticipantHarvester _harvester;
        static IBusClient _newseasonBus;

        public SeasonParticipantExtractorSvc(ILogger<SeasonParticipantExtractorSvc> logger, SeasonParticipantHarvester harvester, IBusClient newseasonClient)
        {
            _harvester = harvester;
            _logger = logger;
            _newseasonBus = newseasonClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //using (var init = new CookieInitialiser(FiddlerVersion.Fiddler2))
            //{
            //    init.Execute();
            //    _harvester.CookieString = init.RealisedCookie;
            //}

            _logger.LogDebug("SeasonParticipantExtractorSvc is registering to new season events...");
            _newseasonBus.RecieveEvents(ExceptionReceivedHandler, ProcessMessagesAsync);
            _logger.LogDebug("SeasonParticipantExtractorSvc is now listening for new season events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc is closing...");
            await _newseasonBus.Close();
            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc has successfully shut down...");
        }

        static async Task ProcessMessagesAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            dynamic payloadvalues = JsonConvert.DeserializeObject<ExpandoObject>(payload);
            _harvester.TournamentCode = payloadvalues.TournamentCode;
            _harvester.SeasonCode = payloadvalues.SeasonCode;
            _harvester.StageCode = payloadvalues.StageCode;
            _harvester.RegionCode = payloadvalues.RegionCode;
            _harvester.CountryCode = payloadvalues.CountryCode;
            await _harvester.Execute();
            await _newseasonBus.CompleteEvent(message.LockToken);
        }

        static Task ExceptionReceivedHandler(IExceptionArgs exceptionReceivedEventArgs)
        {
            _logger.LogDebug($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.Exception;
            _logger.LogDebug("Exception context for troubleshooting:");
            _logger.LogDebug($"- Message: {context.Message}");
            _logger.LogDebug($"- Stack: {context.StackTrace}");
            _logger.LogDebug($"- Source: {context.Source}");
            return Task.CompletedTask;
        }

        // scratch code to manually invoke new season - invoke from startasync to debug without bus
        public static BusEventBase BuildNewSeasonEvent(string tournamentcode, string seasoncode, string stagecode, string regioncode, string countrycode)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.TournamentCode = tournamentcode;
            jsonpayload.SeasonCode = seasoncode;
            jsonpayload.StageCode = stagecode;
            jsonpayload.RegionCode = regioncode;
            jsonpayload.CountryCode = countrycode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
