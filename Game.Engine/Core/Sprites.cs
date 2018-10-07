namespace Game.Engine.Core
{
    public static class Sprites
    {
        private static string[] AllSprites = new[]
        {
            "ship0",
            "ship_green",
            "ship_gray",
            "ship_orange",
            "ship_pink",
            "ship_red",
            "ship_cyan",
            "ship_yellow",
            "ship_flash",
            "bullet_green",
            "bullet_orange",
            "bullet_pink",
            "bullet_red",
            "bullet_cyan",
            "bullet_yellow",
            "fish",
            "bullet",
            "seeker",
            "seeker_pickup",
            "obstacle",
            "arrow"
        };

        public static byte SpriteIndex(string sprite)
        {
            for (byte i = 0; i < AllSprites.Length; i++)
                if (AllSprites[i] == sprite)
                    return i;

            return 0;
        }
    }
}
