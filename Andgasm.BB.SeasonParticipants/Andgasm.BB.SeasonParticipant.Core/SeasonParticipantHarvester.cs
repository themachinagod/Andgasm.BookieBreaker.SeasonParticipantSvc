using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Andgasm.BB.Global;
using Andgasm.BB.Harvest;
using System.Dynamic;
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http.Interfaces;
using Andgasm.BB.SeasonParticipant.Interfaces;
using System.Net;
using Andgasm.Http;

namespace Andgasm.BB.SeasonParticipant.Core
{
    public class SeasonParticipantHarvester : DataHarvest, ISeasonParticipantHarvester
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

        IHttpRequestManager _httpmanager;
        #endregion

        #region Properties
        public string StageKey { get; set; }
        public string SeasonKey { get; set; }
        public string TournamentKey { get; set; }
        public string RegionKey { get; set; }
        public string CountryKey { get; set; }
        #endregion

        #region Contructors
        public SeasonParticipantHarvester(ApiSettings settings, ILogger<SeasonParticipantHarvester> logger, IHarvestRequestManager harvestrequestmanager, IHttpRequestManager httpmanager) : base(harvestrequestmanager)
        {
            _logger = logger;
            _httpmanager = httpmanager;

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
                IHarvestRequestResult responsedoc = await ExecuteRequest();
                if (responsedoc != null)
                {
                    var clubs = new List<ExpandoObject>();
                    foreach (var cr in ParseClubsFromResponse(responsedoc))
                    {
                        clubs.Add(CreateSeasonParticipant(cr));
                    }
                    await _httpmanager.Post(clubs, $"{_participantsapiroot}/api/{_registrationsApiPath}"); // TODO: handle success/fail
                    _logger.LogDebug(string.Format("Stored club season registrations data to database for season '{0}'", SeasonKey));
                }
                else
                {
                    _logger.LogDebug(string.Format("Failed to store & commit club registations for season as no response was recieved from endpoint: '{0}'", SeasonKey));
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

        private async Task<IHarvestRequestResult> ExecuteRequest()
        {
            var url = CreateRequestUrl();
            var ctx = ConstructRequestContext();
            var p = await _requestmanager.MakeRequest(url, ctx);
            if (p != null) LastModeKey = GetLastModeKey(p.InnerText);
            return p;
        }

        private HttpRequestContext ConstructRequestContext()
        {
            var ctx = new HttpRequestContext();
            ctx.Method = "GET";
            ctx.Accept = "";
            ctx.AddHeader("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
            ctx.AddHeader("Accept-Language", "en-GB,en-US;q=0.7,en;q=0.3");
            ctx.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
            ctx.AddHeader("Accept-Encoding", "gzip, deflate");
            ctx.AddHeader("Host", "www.whoscored.com");
            //ctx.AddCookie("Cookie", CookieString);
            ctx.Timeout = 120000;
            return ctx;
        }
        #endregion

        #region Entity Creation Helpers
        private JArray ParseClubsFromResponse(IHarvestRequestResult response)
        {
            var rawdata = response.InnerHtml;
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
