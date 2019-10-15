using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andgasm.BB.SeasonParticipant.API.Models;
using Andgasm.BB.SeasonParticipant.API.Resources;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
//using NSwag.Annotations;

namespace Andgasm.BB.SeasonParticipant.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClubSeasonRegistrationsController : Controller
    {
        #region Fields
        IBusClient _newClubSeasonRegistrationBus;
        SeasonParticipantsDb _context;
        ILogger _logger;
        #endregion

        #region Constructors
        public ClubSeasonRegistrationsController(SeasonParticipantsDb context, Func<string, IBusClient> clubregbus, ILogger<ClubSeasonRegistrationsController> logger)
        {
            _context = context;
            _newClubSeasonRegistrationBus = clubregbus("NewClubSeason");
            _logger = logger;
        }
        #endregion

        [HttpPost(Name = "CreateClubSeasonRegistration")]
        public async Task<IActionResult> CreateBatch([FromBody]List<ClubSeasonRegistrationResource> resources)
        {
            if (resources == null) return BadRequest("A batch of club season registration resources was expected on request body!");
            if (resources.Count < 1) return BadRequest("At least one club season registration resource was expected to be on the request body collection!");
            try
            {
                bool dochange = false;
                foreach (var r in resources)
                {
                    if (!_context.Clubs.Any(x => x.Key == r.ClubKey)) 
                    {
                        dochange = true;
                        var club = new Club()
                        {
                            Key = r.ClubKey,
                            Name = r.ClubName,
                            NickName = r.ClubNickName,
                            CountryKey = r.CountryKey,
                            StadiumName = r.StadiumName,
                        };
                        _context.Clubs.Add(club);
                    }
                    if (!_context.ClubSeasonAssociations.Any(x => x.ClubKey == r.ClubKey && x.SeasonKey == r.SeasonKey))
                    {
                        dochange = true;
                        var association = new ClubSeasonAssociation()
                        {
                            ClubKey = r.ClubKey,
                            SeasonKey = r.SeasonKey
                        };
                        _context.ClubSeasonAssociations.Add(association);
                        await _newClubSeasonRegistrationBus.SendEvent(BuildNewClubSeasonAssociationEvent(r.ClubKey, r.StageKey, r.SeasonKey));
                    }
                }
                if (dochange) await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store
                return Conflict($"A primary key violation occured while saving club season participation data: { pkex.Message }");
            }
        }

        static BusEventBase BuildNewClubSeasonAssociationEvent(string clubcode, string stagecode, string seasoncode)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.SeasonKey = seasoncode;
            jsonpayload.StageKey = stagecode;
            jsonpayload.ClubKey = clubcode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
