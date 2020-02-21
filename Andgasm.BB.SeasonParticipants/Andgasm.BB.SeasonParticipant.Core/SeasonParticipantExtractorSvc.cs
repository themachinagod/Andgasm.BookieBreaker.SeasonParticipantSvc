using Andgasm.BB.Harvest;
using Andgasm.BB.SeasonParticipant.Interfaces;
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
        static ISeasonParticipantHarvester _harvester;
        static IBusClient _newseasonBus;
        static ICookieInitialiser _cookiesvc;

        public SeasonParticipantExtractorSvc(ILogger<SeasonParticipantExtractorSvc> logger, ISeasonParticipantHarvester harvester, IBusClient newseasonClient, ICookieInitialiser cookieinit)
        {
            _harvester = harvester;
            _logger = logger;
            _newseasonBus = newseasonClient;
            _cookiesvc = cookieinit;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SeasonParticipantExtractorSvc is registering to new season events...");
            _harvester.CookieString = await _cookiesvc.GetCookieFromRootDirectives();
            //_newseasonBus.RecieveEvents(ExceptionReceivedHandler, ProcessMessagesAsync);
            await ProcessMessagesAsync(BuildNewSeasonEvent("2", "7361", "16368", "252", "gb-eng"), new CancellationToken());
            _logger.LogDebug("SeasonParticipantExtractorSvc is now listening for new season events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc is closing...");
            await _newseasonBus.Close();
            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc has successfully shut down...");
        }

        public static async Task ProcessMessagesAsync(IBusEvent message, CancellationToken c)
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

        public static Task ExceptionReceivedHandler(IExceptionArgs exceptionReceivedEventArgs)
        {
            _logger.LogDebug($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.Exception;
            _logger.LogDebug("Exception context for troubleshooting:");
            _logger.LogDebug($"- Message: {context.Message}");
            _logger.LogDebug($"- Stack: {context.StackTrace}");
            _logger.LogDebug($"- Source: {context.Source}");
            return Task.CompletedTask;
        }

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
