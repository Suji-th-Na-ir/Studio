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
            typeof(PlayerSpawnPoint),
            typeof(PlayerHealth)
        };

        public static readonly Type[] NON_DELETABLES = new Type[]
        {
            typeof(PlayerSpawnPoint),
            typeof(PlayerHealth),
            typeof(InGameScoreSystem),
        };

        public static bool CanBeDuplicated(string type)
        {
            var isAvailable = DUPLICATE_IGNORABLES.Any(x => x.Name.Equals(type));
            return !isAvailable;
        }

        public static bool CanBeDeleted(BaseBehaviour type)
        {
            var canBeDeleted = !NON_DELETABLES.Any(x => x == type.GetType());
            return canBeDeleted;
        }

        public static bool CanBeDeleted(BaseBehaviour[] types)
        {
            var areNonDeletable = types.Any(x => NON_DELETABLES.Contains(x.GetType()));
            return !areNonDeletable;
        }
    }
}