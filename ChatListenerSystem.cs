using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace testMod
{
    public class ChatReaderSystem : ModSystem
    {
        public override bool HijackGetData(ref byte packetType, ref BinaryReader reader, int playerNumber)
        {

            if (packetType == MessageID.ChatText)
            {
                long startPosition = reader.BaseStream.Position;
                reader.ReadByte(); // skip author ID
                string chatMessage = NetworkText.Deserialize(reader).ToString();
                reader.BaseStream.Position = startPosition;

                CheckForApple(chatMessage);
            }

            return false;
        }

        string previousChatText = "";

        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (msgType == MessageID.ChatText && text != null)
            {
                string chatMessage = text.ToString();
                CheckForApple(chatMessage);
            }

            return false;
        }

        // For single player: detect when chat is submitted
        public override void PostUpdateInput()
        {
            string currentChatText = Main.chatText;

            // If text went from non-empty to empty and Enter was pressed, chat was submitted
            if (!string.IsNullOrEmpty(previousChatText) && string.IsNullOrEmpty(currentChatText) && Main.inputTextEnter)
            {
                CheckForApple(previousChatText);
            }

            previousChatText = currentChatText;
        }

        private void CheckForApple(string message)
        {
            if (message.ToLower().Contains("apple"))
            {
                Main.NewText("System detected an apple mention!", Color.Yellow);

                if (Main.netMode != NetmodeID.Server)
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.Apple);
            }
        }
    }
}