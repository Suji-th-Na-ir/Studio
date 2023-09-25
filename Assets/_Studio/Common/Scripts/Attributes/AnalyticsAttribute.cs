using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AnalyticsTrackEventAttribute : Attribute
    {
        public AnalyticsTrackEventAttribute(string _) { }
    }
}
