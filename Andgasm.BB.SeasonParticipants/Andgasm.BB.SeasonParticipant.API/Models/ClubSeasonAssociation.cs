using System.ComponentModel.DataAnnotations;

namespace Andgasm.BB.SeasonParticipant.API.Models
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
    }
}
