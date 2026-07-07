using System;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace testMod
{
    public class GamemodeCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "gamemode";

        public override string Description => "Sets your gamemode (1 = restricted, 2 = allowed)";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                caller.Reply("Usage: /gamemode <1|2>", Color.Red);
                return;
            }

            if (!int.TryParse(args[0], out int mode) || (mode != 1 && mode != 2))
            {
                caller.Reply("Gamemode must be 1 (restricted) or 2 (allowed)", Color.Red);
                return;
            }

            var gp = caller.Player.GetModPlayer<GamemodePlayer>();
            gp.Gamemode = mode;

            string msg = $"{caller.Player.name} changed to gamemode {mode}";
            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.Yellow);
            else
                Main.NewText(msg, Color.Yellow);

            WebLogger.Notify("gamemode", caller.Player.name, caller.Player.name, mode.ToString());
        }
    }
}
