namespace Terra.Studio
{
    public class GlobalEnums
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Direction
        {
            Clockwise,
            AntiClockwise
        }

        public enum RotationType
        {
            Oscillate,
            RotateOnce,
            RotateMultipleTimes
        }

        public enum RepeatType
        {
            Forever,
            XTimes
        }

        public enum BroadcastAt
        {
            Never,
            AtEveryInterval,
            End
        }
    }
}
