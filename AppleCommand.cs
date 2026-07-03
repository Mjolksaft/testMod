using Terraria;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace testMod
{
    public class AppleCommand : ModCommand
    {
        public override CommandType Type => CommandType.Console;

        public override string Command => "apple";

        public override string Description => "Sends <server> message to chat";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            string text = string.Join(" ", args);

            // If there's a message, broadcast it to chat
            if (!string.IsNullOrEmpty(text))
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"<server> {text}"), Color.White);

                // Also run the apple detection
                if (text.ToLower().Contains("apple"))
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("System detected an apple mention!"), Color.Yellow);
                }
            }

            // Spawn an apple for every active player
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                    player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Apple);
            }
        }
    }
}
