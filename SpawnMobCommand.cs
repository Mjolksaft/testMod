using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace testMod
{
    public class SpawnMobCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "spawnmob";

        public override string Description => "Spawns an NPC near a player";

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

            if (args.Length < 1)
            {
                caller.Reply("Usage: spawnmob <npc_id_or_name> [player_name]", Color.Red);
                return;
            }

            string npcSearchTerm = args[0];
            string playerSearchTerm = null;

            if (args.Length > 1)
            {
                playerSearchTerm = args[args.Length - 1];
                npcSearchTerm = string.Join(" ", args[..^1]);
            }

            Player target = null;

            if (!string.IsNullOrEmpty(playerSearchTerm))
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p.active && string.Equals(p.name, playerSearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        target = p;
                        break;
                    }
                }

                if (target == null)
                {
                    caller.Reply($"Player '{playerSearchTerm}' not found or offline.", Color.Red);
                    return;
                }
            }
            else
            {
                if (caller.Player != null && caller.Player.active)
                {
                    target = caller.Player;
                }
                else
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].active)
                        {
                            target = Main.player[i];
                            break;
                        }
                    }
                }
            }

            if (target == null || !target.active)
            {
                caller.Reply("Command failed: There are no active players online to spawn the mob near!", Color.Red);
                return;
            }

            int npcId = 0;
            bool npcFound = false;

            if (int.TryParse(npcSearchTerm, out int id) && id > 0 && id < NPCLoader.NPCCount)
            {
                npcId = id;
                npcFound = true;
            }
            else
            {
                for (int i = 1; i < NPCLoader.NPCCount; i++)
                {
                    string name = Lang.GetNPCNameValue(i);
                    if (!string.IsNullOrEmpty(name) && string.Equals(name, npcSearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        npcId = i;
                        npcFound = true;
                        break;
                    }
                }
            }

            if (!npcFound)
            {
                caller.Reply($"Could not find matching NPC: '{npcSearchTerm}'", Color.Red);
                return;
            }

            Vector2 spawnPos = target.position;
            spawnPos.Y -= 48f;

            var source = target.GetSource_FromThis();
            int spawnedIndex = NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, npcId);

            if (spawnedIndex >= 0 && spawnedIndex < Main.maxNPCs)
            {
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedIndex);

                string displayName = Lang.GetNPCNameValue(npcId);

                string msg = $"{senderName} spawned {displayName} near {target.name}";
                if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.Green);
                else
                    Main.NewText(msg, Color.Green);

                if (caller.Player != null)
                    WebLogger.Notify("spawnmob", senderName, target.name, displayName);
            }
            else
            {
                caller.Reply("Failed to spawn NPC. World may have reached its max NPC limit.", Color.Red);
            }
        }
    }
}