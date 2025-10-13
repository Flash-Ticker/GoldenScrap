using Oxide.Core.Plugins;
using System;

namespace Oxide.Plugins
{
    [Info("GoldenScrap", "RustFlash", "1.4.2")]
    [Description("Allows admins to give players Golden Scrap via console or RCON")]
    class GoldenScrap : RustPlugin
    {
        private const ulong ScrapSkinId = 3239303050;
        private const string PermissionAdmin = "goldenscrap.admin";

        void Init()
        {
            permission.RegisterPermission(PermissionAdmin, this);
            cmd.AddConsoleCommand("inventory.give.goldenscrap", this, "GiveGoldenScrapCommand");
            cmd.AddChatCommand("goldenscrap", this, "GoldenScrapChatCommand"); 
        }

        [ConsoleCommand("inventory.give.goldenscrap")]
        private void GiveGoldenScrapCommand(ConsoleSystem.Arg arg)
        {
            bool isServerOrRcon = arg.Connection == null;
            
            if (!isServerOrRcon)
            {
                var player = arg.Connection.player as BasePlayer;
                if (player == null) return;

                if (!permission.UserHasPermission(player.UserIDString, PermissionAdmin))
                {
                    SendReply(arg, "<color=#FFD700>[GoldenScrap]</color> <color=red>You don't have permission to use this command.</color>");
                    return;
                }
            }

            string targetIdentifier;
            int amount;

            if (isServerOrRcon)
            {
                if (arg.Args == null || arg.Args.Length < 2)
                {
                    Puts("Usage: inventory.give.goldenscrap \"PlayerName/SteamID\" Amount");
                    return;
                }
                
                targetIdentifier = arg.Args[0];
                
                if (!int.TryParse(arg.Args[1], out amount) || amount <= 0)
                {
                    Puts("Invalid amount. Must be a positive number.");
                    return;
                }
            }
            else
            {
                if (arg.Args == null || arg.Args.Length < 3)
                {
                    SendReply(arg, "<color=#FFD700>[GoldenScrap]</color> <color=red>Usage: inventory.give.goldenscrap \"PlayerName/SteamID\" GoldenScrap Amount</color>");
                    return;
                }
                
                targetIdentifier = arg.Args[0];
                
                if (!arg.Args[1].Equals("GoldenScrap", StringComparison.OrdinalIgnoreCase))
                {
                    SendReply(arg, "<color=#FFD700>[GoldenScrap]</color> <color=red>Second argument must be 'GoldenScrap'.</color>");
                    return;
                }
                
                if (!int.TryParse(arg.Args[2], out amount) || amount <= 0)
                {
                    SendReply(arg, "<color=#FFD700>[GoldenScrap]</color> <color=red>Invalid amount. Must be a positive number.</color>");
                    return;
                }
            }

            GiveGoldenScrapToPlayer(targetIdentifier, amount, isServerOrRcon ? null : arg);
        }

        private void GoldenScrapChatCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PermissionAdmin))
            {
                player.ChatMessage("<color=#FFD700>[GoldenScrap]</color> <color=red>You don't have permission to use this command.</color>");
                return;
            }

            if (args == null || args.Length < 2)
            {
                player.ChatMessage("<color=#FFD700>[GoldenScrap]</color> <color=red>Usage: /goldenscrap <PlayerName/SteamID> <Amount></color>");
                player.ChatMessage("<color=#FFD700>[GoldenScrap]</color> <color=orange>Example: /goldenscrap 76561198253131280 1000</color>");
                return;
            }

            string targetIdentifier = args[0];
            
            if (!int.TryParse(args[1], out int amount) || amount <= 0)
            {
                player.ChatMessage("<color=#FFD700>[GoldenScrap]</color> <color=red>Invalid amount. Must be a positive number.</color>");
                return;
            }

            GiveGoldenScrapToPlayer(targetIdentifier, amount, player);
        }

        private void GiveGoldenScrapToPlayer(string targetIdentifier, int amount, object source)
        {
            var targetPlayer = BasePlayer.Find(targetIdentifier);
            if (targetPlayer == null)
            {
                if (ulong.TryParse(targetIdentifier, out ulong steamId))
                {
                    targetPlayer = BasePlayer.FindByID(steamId);
                }
                
                if (targetPlayer == null)
                {
                    var errorMsg = $"<color=#FFD700>[GoldenScrap]</color> <color=red>No player found with name or SteamID '{targetIdentifier}'.</color>";
                    
                    if (source is BasePlayer player)
                        player.ChatMessage(errorMsg);
                    else if (source is ConsoleSystem.Arg arg)
                        SendReply(arg, errorMsg);
                    else
                        Puts($"No player found with name or SteamID '{targetIdentifier}'.");
                    
                    return;
                }
            }

            var item = ItemManager.CreateByName("scrap", amount, ScrapSkinId);
            if (item != null)
            {
                targetPlayer.inventory.GiveItem(item);
                
                string successMessage = $"<color=#FFD700>[GoldenScrap]</color> Given <color=#FFD700>{amount} Golden Scrap</color> to <color=#4AF626>{targetPlayer.displayName}</color>.";
                
                if (source is BasePlayer player)
                    player.ChatMessage(successMessage);
                else if (source is ConsoleSystem.Arg arg)
                    SendReply(arg, successMessage);
                else
                    Puts($"Given {amount} Golden Scrap to {targetPlayer.displayName}.");
                
                targetPlayer.ChatMessage($"<color=#FFD700>╔═══════════════════════════</color>\n<color=#FFD700>║  GOLDEN SCRAP DELIVERY</color>\n<color=#FFD700>║</color>\n<color=#FFD700>║  You received:</color>\n<color=#FFD700>║  <color=white>{amount} Golden Scrap</color></color>\n<color=#FFD700>║</color>\n<color=#FFD700>║  <color=#4AF626>Enjoy your riches!</color></color>\n<color=#FFD700>╚═══════════════════════════</color>");
            }
            else
            {
                var errorMsg = "<color=#FFD700>[GoldenScrap]</color> <color=red>Error creating the item.</color>";
                
                if (source is BasePlayer player)
                    player.ChatMessage(errorMsg);
                else if (source is ConsoleSystem.Arg arg)
                    SendReply(arg, errorMsg);
                else
                    Puts("Error creating the item.");
            }
        }

        private void SendReply(ConsoleSystem.Arg arg, string message)
        {
            var player = arg.Connection?.player as BasePlayer;
            if (player != null)
            {
                player.ChatMessage(message);
            }
        }
    }
}