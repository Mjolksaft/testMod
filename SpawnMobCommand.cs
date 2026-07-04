using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace testMod
{
    public class SpawnMobCommand : ModCommand
    {
        // Keep Console enabled, but handle the null player safely!
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "spawnmob";

        public override string Description => "Spawns an NPC near a player";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                caller.Reply("Usage: spawnmob <npc_id_or_name> [player_name]", Color.Red);
                return;
            }

            // --- STEP 1: RESOLVE ARGUMENTS ---
            string npcSearchTerm = args[0];
            string playerSearchTerm = null;

            // If there is more than 1 argument, treat the last word as the player's name
            if (args.Length > 1)
            {
                playerSearchTerm = args[args.Length - 1];
                npcSearchTerm = string.Join(" ", args[..^1]);
            }

            // --- STEP 2: MULTIPLAYER CONSOLE SAFE PLAYER TARGETING ---
            Player target = null;

            if (!string.IsNullOrEmpty(playerSearchTerm))
            {
                // Try finding the player explicitly typed in the console
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
                // If run by an active player in-game, target them
                if (caller.Player != null && caller.Player.active)
                {
                    target = caller.Player;
                }
                else
                {
                    // CONSOLE FALLBACK: Loop through and target the first active player online
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

            // Ultimate fallback if the server console runs it when the server is empty
            if (target == null || !target.active)
            {
                caller.Reply("Command failed: There are no active players online to spawn the mob near!", Color.Red);
                return;
            }

            // --- STEP 3: FIND THE NPC ID ---
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

            // --- STEP 4: SPAWN AND BROADCAST MULTIPLAYER PACKET ---
            Vector2 spawnPos = target.position;
            spawnPos.Y -= 48f; // Spawns above their head

            // Since this runs on the server side, get the correct source context
            var source = target.GetSource_FromThis();
            int spawnedIndex = NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, npcId);

            if (spawnedIndex >= 0 && spawnedIndex < Main.maxNPCs)
            {
                // Crucial for dedicated servers: Sync NPC packet data so connected clients can see it
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedIndex);
                }

                string displayName = Lang.GetNPCNameValue(npcId);
                caller.Reply($"Spawned {displayName} near {target.name} successfully!", Color.Green);
            }
            else
            {
                caller.Reply("Failed to spawn NPC. World may have reached its max NPC limit.", Color.Red);
            }
        }
    }
}