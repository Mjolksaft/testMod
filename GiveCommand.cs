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
    public class GiveCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;

        public override string Command => "give";

        public override string Description => "Gives an item to a player";

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

            if (args.Length < 1)
            {
                caller.Reply("Usage: give <item> [amount] | give <player> <item> [amount]", Color.Red);
                return;
            }

            int amount = 1;
            if (args.Length > 1 && int.TryParse(args[^1], out int qty) && qty > 0)
            {
                amount = qty;
                args = args[..^1];
            }

            string senderName = caller.Player?.name ?? "Server";
            Player target = null;
            string itemArg = null;
            string detail = null;

            if (args.Length == 1)
            {
                target = caller.Player;
                if (target == null)
                {
                    caller.Reply("Usage: give <player> <item>", Color.Red);
                    return;
                }
                itemArg = args[0];
                detail = itemArg;
            }
            else
            {
                for (int split = 1; split < args.Length; split++)
                {
                    string testName = string.Join(" ", args[..split]);
                    string testItem = string.Join(" ", args[split..]);

                    Player matched = ResolvePlayer(testName);
                    if (matched == null)
                        continue;

                    if (ResolveItem(testItem) != 0)
                    {
                        target = matched;
                        itemArg = testItem;
                        detail = testItem;
                        break;
                    }
                }

                if (target == null)
                {
                    target = caller.Player;
                    if (target != null)
                    {
                        itemArg = string.Join(" ", args);
                        detail = itemArg;
                    }
                    else
                    {
                        caller.Reply("Usage: give <player> <item>", Color.Red);
                        return;
                    }
                }
            }

            int itemType = ResolveItem(itemArg);
            if (itemType == 0)
            {
                caller.Reply($"Item '{itemArg}' not found!", Color.Red);
                return;
            }

            target.QuickSpawnItem(target.GetSource_DropAsItem(), itemType, amount);
            string displayName = Lang.GetItemNameValue(itemType);

            string msg;
            bool isSelf = senderName == target.name || (caller.Player == target);
            if (isSelf)
                msg = $"{target.name} gave themselves {amount} x {displayName}";
            else
                msg = $"{senderName} gave {target.name} {amount} x {displayName}";

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.Green);
            else
                Main.NewText(msg, Color.Green);

            if (caller.Player != null)
                WebLogger.Notify("give", senderName, target.name, $"{detail} {amount}");
        }

        private static Player ResolvePlayer(string name)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }

        private static int ResolveItem(string name)
        {
            if (int.TryParse(name, out int id) && id > 0)
                return id;

            foreach (var kvp in ContentSamples.ItemsByType)
            {
                if (kvp.Value != null && string.Equals(kvp.Value.Name, name, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            return 0;
        }
    }
}
