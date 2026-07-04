using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace testMod
{
    public class SpawnCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "spawn";

        public override string Description => "Teleports you or a player to world spawn";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player target = null;

            if (args.Length == 0)
            {
                if (caller.Player != null)
                {
                    target = caller.Player;
                }
                else
                {
                    caller.Reply("Usage: spawn <player_name> | chat: /spawn", Color.Red);
                    return;
                }
            }
            else
            {
                string playerName = string.Join(" ", args);

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active)
                        continue;

                    if (string.Equals(p.name, playerName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        target = p;
                        break;
                    }
                }

                if (target == null)
                {
                    caller.Reply($"Player '{playerName}' not found!", Color.Red);
                    return;
                }
            }

            Vector2 targetPosition = new Vector2(target.SpawnX * 16f + 8, target.SpawnY * 16f - 50);
            Console.WriteLine(targetPosition);
            target.Teleport(targetPosition, 1);
            target.velocity = Vector2.Zero;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(
                    MessageID.TeleportEntity,
                    -1,
                    -1,
                    null,
                    0,
                    target.whoAmI,
                    targetPosition.X,
                    targetPosition.Y,
                    1
                );
            }

            caller.Reply($"Teleported {target.name} to spawn", Color.Green);
        }
    }
}
