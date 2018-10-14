using System;
using System.Collections.Generic;
using Andgasm.BookieBreaker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Andgasm.ServiceBus;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Andgasm.BookieBreaker.SeasonParticipant.API.Models;
using System.Text;

namespace Andgasm.BookieBreaker.SeasonParticipant.API.Controllers
{

    [Route("api/[controller]")]
    public class SeasonsController : Controller
    {
        IBusClient _newSeasonRegistrationBus;
        SeasonParticipantsDb _context;

        public SeasonsController(SeasonParticipantsDb context, Func<string, IBusClient> seasregbusaccessor)
        {
            _context = context;
            _newSeasonRegistrationBus = seasregbusaccessor("NewSeason");
        }

        [HttpGet]
        public IEnumerable<SeasonResource> Get()
        {
            var d = _context.Seasons.Select(x => new SeasonResource()
            {
                Name = x.Name,
                TournamentKey = x.TournamentKey,
                CountryKey = x.CountryKey,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                RegionCode = x.RegionCode,
                TournamentCode = x.TournamentCode,
                SeasonCode = x.SeasonCode,
                StageCode = x.StageCode
            });
            return d.ToList();
        }

        [HttpPost]
        public async Task<bool> StoreSeason([FromBody]SeasonResource model)
        {
            try
            {
                bool dochange = false;
                if (!_context.Seasons.Any(x => x.Key == model.Key))
                {
                    dochange = true;
                    var season = new Season()
                    {
                        Key = model.Key,
                        Name = model.Name,
                        TournamentKey = model.TournamentKey,
                        CountryKey = model.CountryKey,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        RegionCode = model.RegionCode,
                        TournamentCode = model.TournamentCode,
                        SeasonCode = model.SeasonCode,
                        StageCode = model.StageCode
                    };
                    _context.Seasons.Add(season);
                    await _newSeasonRegistrationBus.SendEvent(BuildNewSeasonEvent(model.TournamentCode, model.SeasonCode, model.StageCode, model.Key, model.RegionCode, model.CountryKey, model.StartDate, model.EndDate));
                }
                if (dochange) await _context.SaveChangesAsync();
                return dochange;
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store
                Console.WriteLine($"A primary key violation occured while saving player data: { pkex.Message }");
                return false;
            }
        }

        private static BusEventBase BuildNewSeasonEvent(string tournamentcode, string seasoncode, string stagecode, string seasonkey, string regioncode, string countrykey, DateTime seasonstartdate, DateTime seasonenddate)
        {
            // TODO: temp to demo payload comms
            string jsonpayload = string.Format(@"""tournamentcode"":""{0}"",""seasoncode"":""{1}"",""stagecode"":""{2}"",""seasonkey"":""{3}"",""regioncode"":""{4}"",""countrykey"":""{5}"",""startdate"":""{6}"",""enddate"":""{7}""", tournamentcode, seasoncode, stagecode, seasonkey, regioncode, countrykey, seasonstartdate, seasonenddate);
            var payload = Encoding.UTF8.GetBytes("{" + jsonpayload + "}");
            return new BusEventBase(payload);
        }
    }
}
