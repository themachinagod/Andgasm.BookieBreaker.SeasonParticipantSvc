using System.ComponentModel.DataAnnotations;

namespace Andgasm.BookieBreaker.SeasonParticipant.API.Models
{
    public class ClubSeasonAssociation
    {
        [Key]
        public string Key
        {
            get
            {
                return $"{ClubKey}-{SeasonKey}";
            }
            set { }
        }
        public string ClubKey { get; set; }
        public string SeasonKey { get; set; }

        public string ClubSeasonCode { get; set; }
    }
}
