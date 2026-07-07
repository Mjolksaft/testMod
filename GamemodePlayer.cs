using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace testMod
{
    public class GamemodePlayer : ModPlayer
    {
        public int Gamemode { get; set; } = 1;

        public override void SaveData(TagCompound tag)
        {
            tag["gamemode"] = Gamemode;
        }

        public override void LoadData(TagCompound tag)
        {
            Gamemode = tag.GetInt("gamemode");
        }
    }
}
