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
            RotateOnce,
            RotateForever,
            Oscillate,
            OscillateForever,
            IncrementallyRotate,
            IncrementallyRotateForever
            // RotateMultipleTimes
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
