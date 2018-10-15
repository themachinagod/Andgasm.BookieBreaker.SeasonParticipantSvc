using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Andgasm.BookieBreaker.Global;
using HtmlAgilityPack;
using Andgasm.BookieBreaker.Harvest;
using Andgasm.BookieBreaker.Harvest.WhoScored;
using System.Dynamic;
using Andgasm.Http;

namespace Andgasm.BookieBreaker.SeasonParticipant.Core
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
        public string StageCode { get; set; }
        public string SeasonCode { get; set; }
        public string TournamentCode { get; set; }
        public string RegionCode { get; set; }
        public string CountryCode { get; set; }
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
            if (string.IsNullOrWhiteSpace(StageCode)) return false;
            if (string.IsNullOrWhiteSpace(SeasonCode)) return false;
            if (string.IsNullOrWhiteSpace(TournamentCode)) return false;
            if (string.IsNullOrWhiteSpace(RegionCode)) return false;
            if (string.IsNullOrWhiteSpace(CountryCode)) return false;
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
                    await HttpRequestFactory.Post(clubs, _participantsapiroot, _registrationsApiPath);
                    _logger.LogDebug(string.Format("Stored club season registrations data to database for season '{0}'", SeasonCode));
                }
                else
                {
                    _logger.LogDebug(string.Format("Failed to store & commit club registations for season '{0}'", SeasonCode));
                }
                HarvestHelper.FinaliseTimer(_timer);
            }
        }
        #endregion

        #region Request Helpers
        private string CreateRequestUrl()
        {
            return string.Format(WhoScoredConstants.SeasonsUrl, RegionCode, TournamentCode, SeasonCode);
        }

        private async Task<HtmlDocument> ExecuteRequest()
        {
            // TODO: hardwired cookie for now!!
            // TODO: hardwired accept string for now!!
            var url = CreateRequestUrl();
            var p = await HarvestHelper.AttemptRequest(url, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", null, LastModeKey, @"_ga=GA1.2.638389433.1532114109; visid_incap_774904=2VHWR4OrQJ6JcpSUOUIzzJ40UlsAAAAAVUIPAAAAAACAaMiFAU1lv1DEH1MajB012/jYOqYUG7V9; incap_ses_197_774904=rvV/J6YaG1KhxcAp2eS7Ap2Fw1sAAAAAVyFGKcp7/RJBIf7GMdyqQw==; _gid=GA1.2.2146653111.1539540384; _gat=1; _gat_subdomainTracker=1; permutive-session=%7B%22session_id%22%3A%228b5b98a0-c36e-4dad-8949-c45e88e4aef9%22%2C%22last_updated%22%3A%222018-10-14T18%3A06%3A44.463Z%22%7D; permutive-id=83bacd56-cd14-4978-9e5a-a9fe66cbc915; _pdfps=%5B%5D; euconsent=BOVozhmOVozhmABABAENBE-AAAAcd7_______9______9uz_Gv_r_f__33e8_39v_h_7_-___m_-33d4-_1vV11yPg1urfIr1NpjQ6OGsA; __gads=ID=d20ed5572668f88a:T=1539540387:S=ALNI_MZxp2s2AxIAFT0AayrZHABc_FJYDw; __ybotpvd=2; ct=GB", null, false, _requestmanager);
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
            registration.ClubCode = clubdata[teamIdIndex].ToString();
            registration.ClubName = clubdata[teamNameIndex].ToString();
            registration.CountryCode = CountryCode;
            registration.SeasonCode = SeasonCode;
            registration.StageCode = StageCode;
            return registration;
        }
        #endregion
    }
}
