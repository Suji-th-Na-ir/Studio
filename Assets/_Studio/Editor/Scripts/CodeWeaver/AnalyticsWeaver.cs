using Mewlist.Weaver;

namespace Terra.Studio.RTEditor
{
    public class AnalyticsTrackEventWeaver : IWeaver
    {
        public void Weave(AssemblyInjector assemblyInjector)
        {
            assemblyInjector
            .OnAssembly("com.terra.studio")
            .OnAttribute<AnalyticsTrackEventAttribute>()
            .AfterDo<string>(StudioAnalytics.Track)
            .Inject();
        }
    }
}
