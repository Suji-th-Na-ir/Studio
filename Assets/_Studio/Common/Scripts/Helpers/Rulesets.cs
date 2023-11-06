using System;
using System.Linq;

namespace Terra.Studio
{
    public class Rulesets
    {
        public static readonly Type[] DUPLICATE_IGNORABLES = new Type[]
        {
            typeof(InGameTimer),
            typeof(InGameScoreSystem),
            typeof(PlayerSpawnPoint)
        };

        public static bool CanBeDuplicated(string type)
        {
            var isAvailable = DUPLICATE_IGNORABLES.Any(x => x.Name.Equals(type));
            return !isAvailable;
        }
    }
}