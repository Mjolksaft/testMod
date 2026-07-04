using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace testMod
{
    public class KillCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "kill";

        public override string Usage => "/kill all | /kill <npc_name>";

        public override string Description => "Kills all NPCs or NPCs matching a name.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                caller.Reply("Usage: /kill all | /kill <npc_name>", Color.Red);
                return;
            }

            string targetName = string.Join(" ", args);
            int count = 0;

            if (string.Equals(targetName, "all", System.StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.type != NPCID.None)
                    {
                        npc.StrikeInstantKill();
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
                        count++;
                    }
                }
                caller.Reply($"Killed {count} NPCs.", Color.Green);
            }
            else
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    string npcDisplayName = Lang.GetNPCNameValue(npc.type).ToString();
                    if (string.Equals(npcDisplayName, targetName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        npc.StrikeInstantKill();
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
                        count++;
                    }
                }
                caller.Reply($"Killed {count} NPC(s) named '{targetName}'.", Color.Green);
            }
        }
    }
}
