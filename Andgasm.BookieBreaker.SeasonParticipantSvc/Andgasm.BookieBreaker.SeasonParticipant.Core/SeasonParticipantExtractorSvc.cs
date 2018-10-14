﻿using Andgasm.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc is registering to new season events...");
            _newseasonBus.RecieveEvents(ExceptionReceivedHandler, ProcessMessagesAsync);
            _logger.LogDebug("SeasonParticipantExtractorSvc.Svc is now listening for new season events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SquadRegistrationExtractor.Svc is closing...");
            await _newseasonBus.Close();
            _logger.LogDebug("SquadRegistrationExtractor.Svc has successfully shut down...");
        }

        static async Task ProcessMessagesAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            var payloadvalues = JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
            _harvester.TournamentCode = payloadvalues["tournamentcode"];
            _harvester.SeasonCode = payloadvalues["seasoncode"];
            _harvester.StageCode = payloadvalues["stagecode"];
            _harvester.RegionCode = payloadvalues["regioncode"];
            _harvester.SeasonKey = payloadvalues["seasonkey"];
            _harvester.CountryKey = payloadvalues["countrykey"];
            _harvester.SeasonStartDate = Convert.ToDateTime(payloadvalues["startdate"]);
            _harvester.SeasonEndDate = Convert.ToDateTime(payloadvalues["enddate"]);
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
    }
}