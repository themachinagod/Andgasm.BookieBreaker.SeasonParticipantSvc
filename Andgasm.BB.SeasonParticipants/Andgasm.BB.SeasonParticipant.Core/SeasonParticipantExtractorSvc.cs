using Andgasm.BB.Harvest;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BB.SeasonParticipant.Core
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
            _logger.LogDebug("SeasonParticipantExtractorSvc is registering to new season events...");
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
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
            _harvester.TournamentKey = payloadvalues.TournamentKey;
            _harvester.SeasonKey = payloadvalues.SeasonKey;
            _harvester.StageKey = payloadvalues.StageKey;
            _harvester.RegionKey = payloadvalues.RegionKey;
            _harvester.CountryKey = payloadvalues.CountryKey;
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
            jsonpayload.TournamentKey = tournamentcode;
            jsonpayload.SeasonKey = seasoncode;
            jsonpayload.StageKey = stagecode;
            jsonpayload.RegionKey = regioncode;
            jsonpayload.CountryKey = countrycode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
