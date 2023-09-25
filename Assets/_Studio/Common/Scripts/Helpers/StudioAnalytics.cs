using mixpanel;

namespace Terra.Studio
{
    public static class StudioAnalytics
    {
        public static void Track(string key)
        {
            Mixpanel.Track(key);
            Mixpanel.Flush();
        }
    }
}