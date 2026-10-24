namespace InkRun.Core
{
    /// <summary>Names and the short story copy the UI shows. Localize here later.</summary>
    public static class Story
    {
        public const string GameName = "Ink Run";
        public const string HeroName = "Nozomi";
        public const string Volume = "Volume 1";
        public const string Build = "build 12";   // bump on every push so screenshots show which code is running

        public static readonly string[] RevivePanels =
        {
            HeroName + " never saw it coming. The city doesn't wait for anyone.",
            "Somewhere behind him, the gang is laughing. Above them, the shadow gets closer...",
        };
    }
}
