using System;
using System.Collections.Generic;
using System.Text;

namespace Andgasm.BookieBreaker.SeasonParticipant.Core
{
    public class BusSettings
    {
        public string ServiceBusHost { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public string NewClubSeasonAssociationTopicName { get; set; }
        public string NewClubSeasonAssociationSubscriptionName { get; set; }
    }
}
