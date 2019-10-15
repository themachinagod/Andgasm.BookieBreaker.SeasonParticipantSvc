using System;
using System.ComponentModel.DataAnnotations;

namespace Andgasm.BB.SeasonParticipant.API.Models
{
    public class Season
    {
        [Key]
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TournamentName { get; set; }

        public string CountryKey { get; set; }
        public string RegionKey { get; set; }
        public string TournamentKey { get; set; }
        public string StageKey { get; set; }
    }
}
