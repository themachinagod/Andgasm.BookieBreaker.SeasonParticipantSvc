using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Andgasm.BB.Global;
using HtmlAgilityPack;
using Andgasm.BB.Harvest;
using System.Dynamic;
using Andgasm.Http;

namespace Andgasm.BB.SeasonParticipant.Core
{
    public class SeasonParticipantHarvester : DataHarvest
    {
        #region Constants
        const int teamNameIndex = 2;
        const int teamIdIndex = 1;
        #endregion

        #region Fields
        ILogger<SeasonParticipantHarvester> _logger;
        ApiSettings _apisettings;

        string _participantsapiroot;
        string _registrationsApiPath;
        #endregion

        #region Properties
        public string StageKey { get; set; }
        public string SeasonKey { get; set; }
        public string TournamentKey { get; set; }
        public string RegionKey { get; set; }
        public string CountryKey { get; set; }
        #endregion

        #region Contructors
        public SeasonParticipantHarvester(ApiSettings settings, ILogger<SeasonParticipantHarvester> logger, HarvestRequestManager requestmanager)
        {
            _logger = logger;
            _requestmanager = requestmanager;

            _participantsapiroot = settings.SeasonsDbApiRootKey;
            _registrationsApiPath = settings.ClubSeasonRegistrationsApiPath;
            _apisettings = settings;
        }
        #endregion

        #region Execution Operations
        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            if (string.IsNullOrWhiteSpace(StageKey)) return false;
            if (string.IsNullOrWhiteSpace(SeasonKey)) return false;
            if (string.IsNullOrWhiteSpace(TournamentKey)) return false;
            if (string.IsNullOrWhiteSpace(RegionKey)) return false;
            if (string.IsNullOrWhiteSpace(CountryKey)) return false;
            return true;
        }

        public async override Task Execute()
        {
            if (CanExecute())
            {
                _timer.Start();
                HtmlDocument responsedoc = await ExecuteRequest();
                if (responsedoc != null)
                {
                    var clubs = new List<ExpandoObject>();
                    foreach (var cr in ParseClubsFromResponse(responsedoc))
                    {
                        clubs.Add(CreateSeasonParticipant(cr));
                    }                   
                    await HttpRequestFactory.Post(clubs, _participantsapiroot, _registrationsApiPath); // TODO: handle success/fail
                    _logger.LogDebug(string.Format("Stored club season registrations data to database for season '{0}'", SeasonKey));
                }
                else
                {
                    _logger.LogDebug(string.Format("Failed to store & commit club registations for season '{0}'", SeasonKey));
                }
                HarvestHelper.FinaliseTimer(_timer);
            }
        }
        #endregion

        #region Request Helpers
        private string CreateRequestUrl()
        {
            return string.Format(WhoScoredConstants.SeasonsUrl, RegionKey, TournamentKey, SeasonKey);
        }

        private async Task<HtmlDocument> ExecuteRequest()
        {
            // TODO: hardwired cookie for now!!
            // TODO: hardwired accept string for now!!
            var url = CreateRequestUrl();
            var ctx = HarvestHelper.ConstructRequestContext(LastModeKey, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", null,
                                                            CookieString,
                                                            null, false, false, false);
            var p = await _requestmanager.MakeRequest(url, ctx);
            if (p != null) LastModeKey = GetLastModeKey(p.DocumentNode.InnerText);
            return p;
        }
        #endregion

        #region Entity Creation Helpers
        private JArray ParseClubsFromResponse(HtmlDocument response)
        {
            var rawdata = response.DocumentNode.InnerHtml;
            int jsonstartindex = rawdata.IndexOf("DataStore.prime('standings',") + 28;
            int jsonendindex = rawdata.IndexOf("]);", jsonstartindex) + 1;
            var rawjson = "[" + rawdata.Substring(jsonstartindex, jsonendindex - jsonstartindex) + "]";
            var jsondata = JsonConvert.DeserializeObject<JArray>(rawjson);
            return (JArray)jsondata[1];
        }

        private ExpandoObject CreateSeasonParticipant(JToken clubdata)
        {
            dynamic registration = new ExpandoObject();
            registration.ClubKey = clubdata[teamIdIndex].ToString();
            registration.ClubName = clubdata[teamNameIndex].ToString();
            registration.CountryKey = CountryKey;
            registration.SeasonKey = SeasonKey;
            registration.StageKey = StageKey;
            return registration;
        }
        #endregion
    }
}
