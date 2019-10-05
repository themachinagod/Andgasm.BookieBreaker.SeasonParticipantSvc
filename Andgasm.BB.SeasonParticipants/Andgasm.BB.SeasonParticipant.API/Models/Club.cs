using System.ComponentModel.DataAnnotations;

namespace Andgasm.BB.SeasonParticipant.API.Models
{
    public class Club
    {
        [Key]
        public string Key { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string StadiumName { get; set; }
        public string CountryKey { get; set; }
    }
}
