using System;
using Vintagestory.API.Common;
using Newtonsoft.Json.Linq;

namespace woopmotd.Config;

public class ConfigLoader : ModSystem
{
    private const string ConfigName = "woopmotd.json";
    public static ModConfig Config { get; private set; }

    private static ICoreAPI apiRef;

    public override void StartPre(ICoreAPI api)
    {
        apiRef = api;
        LoadOrCreate(api);
    }

    private static void LoadOrCreate(ICoreAPI api)
    {
        try
        {
            Config = api.LoadModConfig<ModConfig>(ConfigName);

            if (Config == null)
            {
                Config = new ModConfig();
                woopmotdCore.Logger?.VerboseDebug("Config file not found, creating a new one...");
            }
            else
            {
                // Migrate older configs that had a single string property "MotdVtml"
                try
                {
                    var raw = api.LoadModConfig<object>(ConfigName);
                    if (raw != null)
                    {
                        var jo = raw as JObject ?? JObject.FromObject(raw);
                        var single = jo["MotdVtml"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(single) && (Config.MotdVtmls == null || Config.MotdVtmls.Length == 0))
                        {
                            Config.MotdVtmls = new[] { single };
                            woopmotdCore.Logger?.Notification("[woopmotd] Migrated MotdVtml => MotdVtmls in config.");
                        }
                    }
                }
                catch
                {
                    // ignore migration errors; defaults will be used
                }

                // Ensure non-null
                if (Config.MotdVtmls == null)
                {
                    Config.MotdVtmls = Array.Empty<string>();
                }
            }

            api.StoreModConfig(Config, ConfigName);
        }
        catch (Exception e)
        {
            woopmotdCore.Logger?.Error("Failed to load config, you probably made a typo: {0}", e);
            Config = new ModConfig();
        }
    }

    public static void RequestReload()
    {
        if (apiRef == null)
        {
            woopmotdCore.Logger?.Warning("[woopmotd] Cannot reload config: API not initialized.");
            return;
        }
        try
        {
            apiRef.Event.EnqueueMainThreadTask(() =>
            {
                try
                {
                    LoadOrCreate(apiRef);
                    woopmotdCore.Logger?.Notification("[woopmotd] Config reloaded by command.");
                }
                catch (Exception ex)
                {
                    woopmotdCore.Logger?.Error("[woopmotd] Error reloading config: {0}", ex);
                }
            }, "woopmotdReloadCmd");
        }
        catch (Exception ex)
        {
            woopmotdCore.Logger?.Error("[woopmotd] Failed to schedule reload: {0}", ex);
        }
    }

    public override void Dispose()
    {
        Config = null;
        apiRef = null;
        base.Dispose();
    }

    public override void Start(ICoreAPI api)
    {
        // Properties can be used in json patches like this
        // "condition": { "when": "woopmotd_ExampleProperty", "isValue": "true" }
    }
}