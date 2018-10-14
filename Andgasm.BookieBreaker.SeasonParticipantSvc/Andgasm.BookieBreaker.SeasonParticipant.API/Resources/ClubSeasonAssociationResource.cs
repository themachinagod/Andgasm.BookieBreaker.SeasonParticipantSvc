namespace Andgasm.BookieBreaker.SeasonParticipant.API.Resources
{
    public class ClubSeasonAssociationResource
    {
        public string Key
        {
            get
            {
                return $"{ClubKey}-{SeasonKey}";
            }
        }
        public string ClubKey { get; set; }
        public string SeasonKey { get; set; }
        
        public string ClubSeasonCode { get; set; }
        public string StageCode { get; set; }
    }
}
