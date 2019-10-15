using System;
using System.Collections.Generic;
using Andgasm.BB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Andgasm.ServiceBus;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Andgasm.BB.SeasonParticipant.API.Models;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace Andgasm.BB.SeasonParticipant.API.Controllers
{//
    [Route("api/[controller]")]
    [ApiController]
    public class SeasonsController : Controller
    {
        IBusClient _newSeasonRegistrationBus;
        SeasonParticipantsDb _context;

        public SeasonsController(SeasonParticipantsDb context, Func<string, IBusClient> seasregbusaccessor)
        {
            _context = context;
            _newSeasonRegistrationBus = seasregbusaccessor("NewSeason");
        }

        [HttpGet(Name = "GetAllSeasons")]
        [SwaggerResponse(typeof(List<SeasonResource>), IsNullable = false,
                         Description = "A collection of SeasonResources.")]
        public async Task<IActionResult> GetAll()
        {
            var d = await  _context.Seasons.Select(x => new SeasonResource()
            {
                Name = x.Name,
                TournamentName = x.TournamentName,
                StartDate = x.StartDate,
                EndDate = x.EndDate,

                CountryKey = x.CountryKey,
                TournamentKey = x.TournamentKey,
                RegionKey = x.RegionKey,
                SeasonKey = x.Key,
                StageKey = x.StageKey
            }).ToListAsync();
            return Ok(d);
        }

        [HttpPost(Name = "CreateSeason")]
        [SwaggerResponse(typeof(SeasonResource), IsNullable = false,
                         Description = "The successfully created Season.")]
        public async Task<IActionResult> Create([FromBody]SeasonResource model)
        {
            try
            {
                bool dochange = false;
                if (!_context.Seasons.Any(x => x.Key == model.SeasonKey))
                {
                    dochange = true;
                    var season = new Season()
                    {
                        Key = model.SeasonKey,
                        Name = model.Name,
                        TournamentName = model.TournamentName,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,

                        TournamentKey = model.TournamentKey,
                        CountryKey = model.CountryKey,
                        RegionKey = model.RegionKey,
                        StageKey = model.StageKey
                    };
                    _context.Seasons.Add(season);
                    await _newSeasonRegistrationBus.SendEvent(BuildNewSeasonEvent(model.TournamentKey, model.SeasonKey, model.StageKey, model.RegionKey, model.CountryKey, model.Name));
                }
                else { Conflict($"The key '{model.SeasonKey}' already exists!"); }
                if (dochange) await _context.SaveChangesAsync();
                return Ok(model); 
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store
                return Conflict($"A primary key violation occured while saving player data: { pkex.Message }");
            }
        }

        public static BusEventBase BuildNewSeasonEvent(string tournamentcode, string seasoncode, string stagecode, string regioncode, string countrycode, string seasonname)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.TournamentKey = tournamentcode;
            jsonpayload.SeasonKey = seasoncode;
            jsonpayload.StageKey = stagecode;
            jsonpayload.RegionKey = regioncode;
            jsonpayload.CountryKey = countrycode;
            jsonpayload.SeasonName = seasonname;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
