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
//using NSwag.Annotations;
using System.Globalization;

namespace Andgasm.BB.SeasonParticipant.API.Controllers
{
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
        //[SwaggerResponse(typeof(List<SeasonResource>), IsNullable = false,
        //                 Description = "A collection of SeasonResources.")]
        public async Task<IActionResult> GetAll()
        {
            var d = await  _context.Seasons.Select(x => new SeasonResource()
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
            }).ToListAsync();
            return Ok(d);
        }

        [HttpPost(Name = "CreateSeason")]
        //[SwaggerResponse(typeof(SeasonResource), IsNullable = false,
        //                 Description = "The successfully created Season.")]
        public async Task<IActionResult> Create([FromBody]SeasonResource model)
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
                    await _newSeasonRegistrationBus.SendEvent(BuildNewSeasonEvent(model.TournamentCode, model.SeasonCode, model.StageCode, model.RegionCode, model.CountryKey, model.Name));
                }
                else { Conflict($"The key '{model.Key}' already exists!"); }
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
            jsonpayload.TournamentCode = tournamentcode;
            jsonpayload.SeasonCode = seasoncode;
            jsonpayload.StageCode = stagecode;
            jsonpayload.RegionCode = regioncode;
            jsonpayload.CountryCode = countrycode;
            jsonpayload.SeasonName = seasonname;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
