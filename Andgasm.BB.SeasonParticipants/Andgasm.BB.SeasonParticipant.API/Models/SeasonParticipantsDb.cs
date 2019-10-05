using Microsoft.EntityFrameworkCore;

namespace Andgasm.BB.SeasonParticipant.API.Models
{
    public class SeasonParticipantsDb : DbContext
    {
        public SeasonParticipantsDb(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Season> Seasons { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubSeasonAssociation> ClubSeasonAssociations { get; set; }
        
        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}