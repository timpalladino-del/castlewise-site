namespace InkRun.Core
{
    /// <summary>Names and the short story copy the UI shows. Localize here later.</summary>
    public static class Story
    {
        public const string GameName = "Ink Run";
        public const string HeroName = "Nozomi";
        public const string Volume = "Volume 1";
        public const string Build = "build 25";   // bump on every push so screenshots show which code is running

        /// <summary>Three-panel crash strips per obstacle kind: approach, impact SFX word, aftermath (STORY.md).</summary>
        public static string[] DeathPanels(InkRun.World.ObstacleKind kind)
        {
            switch (kind)
            {
                case InkRun.World.ObstacleKind.Barrier: return new[] { "The sneaker catches the top rail.", "WHAM!", "Flat on the roof. The rail was higher than it looked." };
                case InkRun.World.ObstacleKind.Sign: return new[] { "The board fills the frame.", "GAN!", "He read the sign. Too late." };
                case InkRun.World.ObstacleKind.Wall: return new[] { "Headlights. The horn.", "DON!", "The Nozomi doesn't stop for anyone." };
                default: return new[] { "Toes over the edge. Nothing below.", "HYUUU", "Caught on a laundry line two floors down. Lucky. This time." };
            }
        }

        /// <summary>Three captions shown over the live run when a chapter changes (STORY.md), by the chapter being entered.</summary>
        public static string[] TransitionPanels(int toIndex)
        {
            switch (toIndex)
            {
                case 1: return new[] { "The rooftops run out.", "The Crew is still there. Four of them now.", "Down to the docks. Don't look at the water." };
                case 2: return new[] { "Something moved under the ships. Something big.", "The Crew saw it too. Even they look scared.", "Into the alleys. The lanterns are the only light." };
                default: return new[] { "Dawn again. The city looks the same.", "It isn't. The Shadow is behind the skyline now.", "Keep running, Nozomi. Volume 1 isn't over." };
            }
        }
        public static readonly string[] ColdOpen = { "The city never sleeps. Neither does the Smudge Crew.", "Nozomi has something of theirs.", "Run." };

        public static readonly string[] RevivePanels =
        {
            HeroName + " never saw it coming. The city doesn't wait for anyone.",
            "Somewhere behind him, the gang is laughing. Above them, the shadow gets closer...",
        };
    }
}
