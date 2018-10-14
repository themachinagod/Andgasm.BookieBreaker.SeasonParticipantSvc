namespace Andgasm.BookieBreaker.SeasonParticipant.API.Resources
{
    public class ClubResource
    {
        public string Key
        {
            get
            {
                return $"{CountryKey}-{Name}";
            }
        }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string StadiumName { get; set; }
        public string CountryKey { get; set; }

        public string ClubCode { get; set; }
    }
}
