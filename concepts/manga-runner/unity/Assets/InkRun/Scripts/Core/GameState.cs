namespace InkRun.Core
{
    public enum GameState { Idle, Running, Crashed, Revive, Results, Paused }

    /// <summary>Everything that changes during one run. Reset by GameManager.StartRun().</summary>
    public class RunStats
    {
        public float Distance;      // meters
        public int Coins;
        public int Jumps;
        public int Slides;
        public int Smashes;
        public int CloseCalls;      // obstacles cleared in the hero's own lane
        public int Revives;
        public bool ReachedHarbor;
        public float Seconds;
        public bool Assist;
    }
}
