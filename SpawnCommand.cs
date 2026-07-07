using Terraria;
using Terraria.Chat;
using Terraria.Localization;
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

            // Fix applied here: Use Player.CheckSpawn for multiplayer bed validation
            int spawnX = Main.spawnTileX;
            int spawnY = Main.spawnTileY;

            if (target.SpawnX > 0 && target.SpawnY > 0 && Player.CheckSpawn(target.SpawnX, target.SpawnY))
            {
                spawnX = target.SpawnX;
                spawnY = target.SpawnY;
            }

            if (spawnX <= 0 || spawnY <= 0 || spawnX >= Main.maxTilesX || spawnY >= Main.maxTilesY)
            {
                spawnX = Main.spawnTileX;
                spawnY = Main.spawnTileY;
            }

            Vector2 targetPosition = new Vector2(spawnX * 16f + 8f, spawnY * 16f - target.height);

            // Reset physics states so the player doesn't instantly die from accumulated fall velocity
            target.velocity = Vector2.Zero;
            target.fallStart = (int)(target.position.Y / 16f);

            // Handle Multiplayer Synchronization flawlessly
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                target.Teleport(targetPosition, 1);
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                target.Teleport(targetPosition, 1);

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
            else if (Main.netMode == NetmodeID.MultiplayerClient && caller.Player == target)
            {
                target.Teleport(targetPosition, 1);

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

            string msg;
            if (caller.Player == target)
                msg = $"{target.name} was teleported to spawn";
            else
                msg = $"{senderName} teleported {target.name} to spawn";
            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.Green);
            else
                Main.NewText(msg, Color.Green);

            if (caller.Player != null)
                WebLogger.Notify("spawn", senderName, target.name, "spawn");
        }
    }
}
