using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andgasm.BookieBreaker.SeasonParticipant.API.Models;
using Andgasm.BookieBreaker.SeasonParticipant.API.Resources;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andgasm.BookieBreaker.SeasonParticipant.API.Controllers
{
    [Route("api/[controller]")]
    public class ClubSeasonRegistrationsController : Controller
    {
        IBusClient _newClubSeasonRegistrationBus;
        SeasonParticipantsDb _context;

        public ClubSeasonRegistrationsController(SeasonParticipantsDb context, Func<string, IBusClient> clubregbus)
        {
            _context = context;
            _newClubSeasonRegistrationBus = clubregbus("NewClubSeason");
        }

        [HttpPost]
        public async Task<bool> StoreClubSeasonRegistration([FromBody]List<ClubSeasonRegistrationResource> model)
        {
            try
            {
                bool dochange = false;
                foreach (var p in model)
                {
                    if (!_context.Clubs.Any(x => x.Key == p.ClubResource.Key))
                    {
                        dochange = true;
                        var club = new Club()
                        {
                            Key = p.ClubResource.Key,
                            Name = p.ClubResource.Name,
                            NickName = p.ClubResource.NickName,
                            CountryKey = p.ClubResource.CountryKey,
                            StadiumName = p.ClubResource.StadiumName,
                        };
                        _context.Clubs.Add(club);
                    }
                    if (!_context.ClubSeasonAssociations.Any(x => x.Key == p.AssociationResource.Key))
                    {
                        dochange = true;
                        var association = new ClubSeasonAssociation()
                        {
                            Key = p.AssociationResource.Key,
                            ClubKey = p.AssociationResource.ClubKey,
                            SeasonKey = p.AssociationResource.SeasonKey,
                            ClubSeasonCode = p.AssociationResource.ClubSeasonCode,
                        };
                        _context.ClubSeasonAssociations.Add(association);
                        await _newClubSeasonRegistrationBus.SendEvent(BuildNewClubSeasonAssociationEvent(p.AssociationResource.ClubSeasonCode, p.AssociationResource.StageCode, p.AssociationResource.SeasonKey, p.ClubResource.Key));
                    }
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

        public static BusEventBase BuildNewClubSeasonAssociationEvent(string clubseasoncode, string stagecode, string seasonkey, string clubkey)
        {
            // TODO: temp to demo payload comms
            string jsonpayload = string.Format(@"""clubseasoncode"":""{0}"",""stagecode"":""{1}"",""seasonkey"":""{2}"",""clubkey"":""{3}""", clubseasoncode, stagecode, seasonkey, clubkey);
            var payload = Encoding.UTF8.GetBytes("{" + jsonpayload + "}");
            return new BusEventBase(payload);
        }
    }
}
