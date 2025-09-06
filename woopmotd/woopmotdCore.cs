using System;
using woopmotd.Config;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using woopmotd.Gui;

namespace woopmotd;

public class woopmotdCore : ModSystem
{
    public static ILogger Logger { get; private set; }
    public static string ModId { get; private set; }
    public static ICoreAPI Api { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }
    public static ModConfig Config => ConfigLoader.Config;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Api = api;
        Logger = Mod.Logger;
        ModId = Mod.Info.ModID;
        HarmonyInstance = new Harmony(ModId);
        HarmonyInstance.PatchAll();
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Logger.Notification("Hello from mod: " + api.Side);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        Logger.Notification("Hello from mod client side: " + Lang.Get("woopmotd:modname"));

        // Register network channel and message handler
        var channel = api.Network.RegisterChannel("woopmotd").RegisterMessageType<MotdPacket>().RegisterMessageType<MotdRequestPacket>();
        channel.SetMessageHandler<MotdPacket>((packet) =>
        {
            try
            {
                if (packet?.Vtmls == null || packet.Vtmls.Count == 0) return;
                // Join multiple VTML messages with a blank line
                var combined = string.Join("\n", packet.Vtmls);
                if (string.IsNullOrWhiteSpace(combined)) return;
                var dlg = new GuiMotdDialog(api, "woopmotd:motd-title", combined);
                dlg.TryOpen();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to show MOTD dialog: {0}", ex);
            }
        });

        // Register hotkey (default Shift+Y) to request MOTD from server
        api.Input.RegisterHotKey("woopmotd-motd", "Open MOTD", GlKeys.Y, HotkeyType.CharacterControls, false, false, true);
        api.Input.SetHotKeyHandler("woopmotd-motd", (keyComb) =>
        {
            try
            {
                // Send request to server; in singleplayer it loops back
                api.Network.GetChannel("woopmotd").SendPacket(new MotdRequestPacket());
            }
            catch (Exception ex)
            {
                Logger?.Error("Failed to request MOTD via hotkey: {0}", ex);
            }
            return true;
        });
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        Logger.Notification("Hello from mod server side: " + Lang.Get("woopmotd:modname"));

        // Register network channel
        var channel = api.Network.RegisterChannel("woopmotd").RegisterMessageType<MotdPacket>().RegisterMessageType<MotdRequestPacket>();

        // Server-side handler for client requests to open MOTD
        channel.SetMessageHandler<MotdRequestPacket>((player, packet) =>
        {
            try
            {
                if (!Config.EnableMotd) return;
                var msgs = Config.MotdVtmls ?? Array.Empty<string>();
                var toSend = new System.Collections.Generic.List<string>();
                foreach (var s in msgs)
                {
                    if (!string.IsNullOrWhiteSpace(s)) toSend.Add(s);
                }
                if (toSend.Count == 0) return;
                channel.SendPacket(new MotdPacket { Vtmls = toSend }, player);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to handle MOTD request: {0}", ex);
            }
        });

        // Send MOTD when player is ready
        api.Event.PlayerNowPlaying += (player) =>
        {
            try
            {
                if (!Config.EnableMotd) return;
                var msgs = Config.MotdVtmls ?? Array.Empty<string>();
                var toSend = new System.Collections.Generic.List<string>();
                foreach (var s in msgs)
                {
                    if (!string.IsNullOrWhiteSpace(s)) toSend.Add(s);
                }
                if (toSend.Count == 0) return;
                channel.SendPacket(new MotdPacket { Vtmls = toSend }, player);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send MOTD: {0}", ex);
            }
        };

        // Admin command via ChatCommands: /woopmotd reload
        try
        {
            api.ChatCommands
                .Create("motd")
                .RequiresPrivilege(Privilege.chat)
                .WithDescription("Show the server Message of the Day")
                .HandleWith(Help)
                .BeginSubCommand("help")
                    .WithDescription("Show woopmotd command help")
                    .RequiresPrivilege(Privilege.chat)
                    .HandleWith(Help)
                .EndSubCommand()
                .BeginSubCommand("reload")
                    .WithDescription("Reload woopmotd configuration")
                    .RequiresPrivilege(Privilege.controlserver)
                    .HandleWith(Reload)
                .EndSubCommand();

        }
        catch (Exception ex)
        {
            Logger?.Warning("[woopmotd] Failed to register /motd via ChatCommands: {0}", ex);
        }
    }

    private TextCommandResult Reload(TextCommandCallingArgs args)
    {
        ConfigLoader.RequestReload();
        return TextCommandResult.Success("[woopmotd] Reload requested.");
    }

    private TextCommandResult Help(TextCommandCallingArgs args)
    {
        return TextCommandResult.Success("Shift+Y - Show the server Message of the Day");
    }
    public override void Dispose()
    {
        HarmonyInstance?.UnpatchAll(ModId);
        HarmonyInstance = null;
        Logger = null;
        ModId = null;
        Api = null;
        base.Dispose();
    }
}