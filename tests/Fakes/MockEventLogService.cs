using CMS.Core;
using CMS.EventLog;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class MockEventLogService : EventLogService
    {
        public override void LogEvent(EventLogData eventLogData)
        {
            // Do nothing
        }
    }
}
