using System;
using System.ComponentModel.DataAnnotations;

namespace Andgasm.BB.SeasonParticipant.API.Models
{
    public class Season
    {
        [Key]
        public string Key
        {
            get
            {
                return $"{TournamentKey}-{Name}";
            }
            set { }
        }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TournamentKey { get; set; }
        public string CountryKey { get; set; }

        public string RegionCode { get; set; }
        public string TournamentCode { get; set; }
        public string SeasonCode { get; set; }
        public string StageCode { get; set; }
    }
}
