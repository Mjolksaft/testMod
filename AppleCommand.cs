using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace YourModName.Commands
{
    public class GiveCommand : ModCommand
    {
        public override CommandType Type => CommandType.Console;

        public override string Command => "give";

        public override string Description => "Gives an item to a specific player";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 2)
            {
                caller.Reply("Usage: give <playerName> <item>", Color.Red);
                return;
            }

            Player target = null;
            int itemType = 0;

            for (int split = 1; split < args.Length; split++)
            {
                string testName = string.Join(" ", args[..split]);
                string testItem = string.Join(" ", args[split..]);

                Player matched = null;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && string.Equals(player.name, testName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        matched = player;
                        break;
                    }
                }

                if (matched == null)
                    continue;

                if (int.TryParse(testItem, out int id) && id > 0)
                {
                    target = matched;
                    itemType = id;
                    break;
                }

                foreach (var kvp in ContentSamples.ItemsByType)
                {
                    if (kvp.Value != null && string.Equals(kvp.Value.Name, testItem, System.StringComparison.OrdinalIgnoreCase))
                    {
                        target = matched;
                        itemType = kvp.Key;
                        break;
                    }
                }

                if (target != null)
                    break;
            }

            if (target == null)
            {
                caller.Reply($"Could not find a matching player or item in '{string.Join(" ", args)}'", Color.Red);
                return;
            }

            target.QuickSpawnItem(target.GetSource_DropAsItem(), itemType);
            string displayName = Lang.GetItemNameValue(itemType);
            caller.Reply($"Gave {displayName} to {target.name}", Color.Green);
        }
    }
}
