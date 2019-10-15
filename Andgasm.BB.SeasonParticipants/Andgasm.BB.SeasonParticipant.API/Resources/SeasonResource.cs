using System;

namespace Andgasm.BB.Models
{
    public class SeasonResource
    {
        public string SeasonKey { get; set; }
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
