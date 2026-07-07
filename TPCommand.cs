using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace testMod
{
    public class TPCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "tp";

        public override string Description => "Teleports one player to another";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player != null)
            {
                var gp = caller.Player.GetModPlayer<GamemodePlayer>();
                if (gp.Gamemode != 2)
                {
                    caller.Reply("You don't have permission to use this command! (gamemode 2 required)", Color.Red);
                    return;
                }
            }

            string senderName = caller.Player?.name ?? "Server";

            if (args.Length < 2)
            {
                caller.Reply("Usage: tp <player1> <player2>", Color.Red);
                return;
            }

            Player player1 = null;
            Player player2 = null;

            for (int split = 1; split < args.Length; split++)
            {
                string name1 = string.Join(" ", args[..split]);
                string name2 = string.Join(" ", args[split..]);

                Player p1 = null, p2 = null;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active)
                        continue;

                    if (p1 == null && string.Equals(p.name, name1, System.StringComparison.OrdinalIgnoreCase))
                        p1 = p;
                    if (p2 == null && string.Equals(p.name, name2, System.StringComparison.OrdinalIgnoreCase))
                        p2 = p;
                }

                if (p1 != null && p2 != null)
                {
                    player1 = p1;
                    player2 = p2;
                    break;
                }
            }

            if (player1 == null || player2 == null)
            {
                caller.Reply("Could not find one or both players!", Color.Red);
                return;
            }

            Vector2 targetPosition = player2.position;
            player1.Teleport(targetPosition, 1);
            player1.velocity = Vector2.Zero;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(
                    MessageID.TeleportEntity,
                    -1,
                    -1,
                    null,
                    0,
                    player1.whoAmI,
                    targetPosition.X,
                    targetPosition.Y,
                    1
                );
            }

            string msg = $"{senderName} teleported {player1.name} to {player2.name}";
            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.Green);
            else
                Main.NewText(msg, Color.Green);

            if (caller.Player != null)
                WebLogger.Notify("tp", senderName, player1.name, player2.name);
        }
    }
}