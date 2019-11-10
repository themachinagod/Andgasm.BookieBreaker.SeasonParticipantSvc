using Andgasm.BB.Harvest;
using System;
using System.Threading.Tasks;

namespace Andgasm.BB.SeasonParticipant.Interfaces
{
    public interface ISeasonParticipantHarvester : IDataHarvest
    {
        string CookieString { get; set; }
        string TournamentKey { get; set; }
        string SeasonKey { get; set; }
        string StageKey { get; set; }
        string RegionKey { get; set; }
        string CountryKey { get; set; }
    }
}
