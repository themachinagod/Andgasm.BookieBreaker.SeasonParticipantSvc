using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        IConfigurationRoot _settings;

        string _participantsapiroot;
        string _registrationsApiPath;
        #endregion

        #region Properties
        public string SeasonName { get; set; }
        public string StageCode { get; set; }
        public string SeasonCode { get; set; }
        public string TournamentCode { get; set; }
        public string RegionCode { get; set; }
        public string SeasonKey { get; set; }
        public string CountryKey { get; set; }
        public DateTime SeasonStartDate { get; set; }
        public DateTime SeasonEndDate { get; set; }
        #endregion

        #region Contructors
        public SeasonParticipantHarvester(IConfigurationRoot settings, ILogger<SeasonParticipantHarvester> logger, HarvestRequestManager requestmanager)
        {
            _logger = logger;
            _requestmanager = requestmanager;

            _participantsapiroot = settings[Constants.SeasonsDbApiRootKey];
            _registrationsApiPath = settings[Constants.ClubSeasonRegistrationsApiPathKey];
            _settings = settings;
        }
        #endregion

        #region Execution Operations
        public override bool CanExecute()
        {
            
            if (!base.CanExecute()) return false;
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
                        var cl =  CreateSeasonParticipant(cr);
                        clubs.Add(cl);
                    }
                    await HttpRequestFactory.Save(clubs, _participantsapiroot, _registrationsApiPath);
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

        #region Entity Creation Helpers
        private string CreateRequestUrl()
        {
            return string.Format(WhoScoredConstants.SeasonsUrl, RegionCode, TournamentCode, SeasonCode);
        }

        private async Task<HtmlDocument> ExecuteRequest()
        {
            var url = CreateRequestUrl();
            var p = await HarvestHelper.AttemptRequest(url, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", null, LastModeKey, CookieString, null, false, _requestmanager);
            if (p != null) LastModeKey = GetLastModeKey(p.DocumentNode.InnerText);
            return p;
        }

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
            dynamic club = new ExpandoObject();
            club.Name = clubdata[teamNameIndex].ToString();
            club.ClubCode = clubdata[teamIdIndex].ToString();
            club.CountryKey = CountryKey;

            dynamic clubseasonlink = new ExpandoObject();

            clubseasonlink.ClubKey = club.Key;
            clubseasonlink.SeasonKey = SeasonKey;
            clubseasonlink.ClubSeasonCode = club.ClubCode;
            clubseasonlink.StageCode = StageCode;

            dynamic registration = new ExpandoObject();
            registration.ClubModel = club;
            registration.AssociationModel = clubseasonlink;

            return registration;
        }
        #endregion
    }
}
