using System.Collections.Generic;

using CMS.Core;
using CMS.EventLog;

namespace Kentico.Xperience.Algolia.Test
{
    internal class MockEventLogService : EventLogService
    {
        public static List<EventLogData> LoggedEvents => new();


        public override void LogEvent(EventLogData eventLogData)
        {
            LoggedEvents.Add(eventLogData);
        }
    }
}
