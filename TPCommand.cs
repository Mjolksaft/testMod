using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID; // REQUIRED FOR MessageID

namespace testMod
{
    public class TPCommand : ModCommand
    {
        // Keep this as CommandType.Console if you want to run it from the dedicated server window.
        // Change to CommandType.Chat if you want admins to use it inside the game chat.
        public override CommandType Type => CommandType.Console;

        public override string Command => "tp";

        public override string Description => "Teleports one player to another";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
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

            // 1. Set target position to player2's exact current location
            Vector2 targetPosition = player2.position;

            // 2. Move player1 on the server side
            player1.Teleport(targetPosition, 1);
            player1.velocity = Vector2.Zero; // Prevent momentum from carrying over

            // 3. BROADCAST TO CLIENTS: Tell the entire server that player1 has moved
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(
                    MessageID.TeleportEntity,
                    -1,               // -1 sends the packet to ALL connected clients
                    -1,               // Ignore no one
                    null,
                    0,                // 0 specifies that the entity type is a Player
                    player1.whoAmI,   // The index of the player being teleported
                    targetPosition.X,
                    targetPosition.Y,
                    1                 // Teleport visual style (1 = Rod of Discord effect)
                );
            }

            caller.Reply($"Teleported {player1.name} to {player2.name}", Color.Green);
        }
    }
}