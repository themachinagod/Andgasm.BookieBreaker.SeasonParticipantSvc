using System;

namespace Andgasm.BookieBreaker.Global
{
    public static class Constants
    {
        public const string SeasonsDbApiRootKey = "ParticipantsDbApiRoot";
        public const string SeasonApiPathKey = "SeasonApiPath";
        public const string ClubSeasonRegistrationsApiPathKey = "ClubSeasonRegistrationsApiPath";

        public const string PlayersDbApiRootKey = "PlayersDbApiRoot";
        public const string PlayerSquadRegistrationsApiPathKey = "PlayerSquadRegistrationsApiPath";

        public const string FixturesDbApiRootKey = "FixturesDbApiRoot";
        public const string FixtureClubAppearancesApiPath = "FixtureClubAppearancesApiPath";
        public const string FixturePlayerAppearancesApiPath = "FixturePlayerAppearancesApiPath";

        public const string MatchEventsDbApiRootKey = "MatchEventsDbApiRoot";
        public const string MatchEventsApiPath = "MatchEventsApiPath";
    }

    public static class WhoScoredConstants
    {
        public const string WhoScoredHost = "WhoScored";
        public const string RootUrl = "www.whoscored.com";

        public const string SeasonsUrl = "https://www.whoscored.com/Regions/{0}/Tournaments/{1}/Seasons/{2}";
        public const string ClubsUrl = "https://www.whoscored.com/Teams/{0}/Show/";
        public const string FixturesUrl = "https://www.whoscored.com/Regions/{0}/Tournaments/{1}/Seasons/{2}/Stages/{3}/Fixtures/";
        public const string MatchesUrl = "https://www.whoscored.com/Matches/{0}/Live/{1}-{2}-{3}-{4}";

        public const string PlayerStatisticsFeedUrl = "https://www.whoscored.com/StatisticsFeed/1/GetPlayerStatistics?category=summary&subcategory=all&statsAccumulationType=0&isCurrent=true&playerId=&teamIds={1}&matchId=&stageId=&tournamentOptions=2&sortBy=Rating&sortAscending=&age=&ageComparisonType=&appearances=&appearancesComparisonType=&field=Overall&nationality=&positionOptions=&timeOfTheGameEnd=&timeOfTheGameStart=&isMinApp=false&page=&includeZeroValues=true&numberOfPlayersToPick=";
        public const string TournamentsStatisticsFeedUrl = "https://www.whoscored.com/tournamentsfeed/{0}/Fixtures/?d={1}{2}&isAggregate=false";
    }
}
