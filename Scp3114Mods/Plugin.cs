using HarmonyLib;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using Scp3114Mods.API;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Log = PluginAPI.Core.Log;
using PluginPriority = Exiled.API.Enums.PluginPriority;
#if EXILED
using Exiled.API.Features;
using Exiled.API.Interfaces;
#endif

namespace Scp3114Mods;
#if !EXILED
public class Scp3114Mods
{
#else
public class Scp3114Mods : Plugin<Config, Translations>
{
    public override string Author => "Redforce04";
    public override string Name => "Scp3114Mods";
    public override string Prefix => "3114";
    public override Version Version => Version.Parse(VersionName);
    // Like the whole codebase is built on NWApi because I refuse to maintain two separate versions, when its not necessary (the cedmod philosophy).
    public override Version RequiredExiledVersion  => new Version(8, 3, 0); 
    public override PluginPriority Priority => PluginPriority.Default;
#endif
    public const string VersionName = "1.0.3";
    public static Scp3114Mods Singleton = null!;

    public Harmony Harmony { get; set; } = null!;

    public EventHandlers Handlers { get; set; } = null!;
    
#if !EXILED
    [PluginConfig("Translations")] 
    public Translations Translation;

    [PluginConfig] public Config Config;
#endif
    public bool EventsRegistered { get; set; } = false;
    public override void OnEnabled()
    
#if !EXILED
    [PluginEntryPoint("Scp3114Mods", VersionName, "Modifies the mechanics of Scp3114 to be more balanced.",
        "Redforce04")]
    public void OnEnabled()
#endif
    {
        Singleton = this;

        Logging.Info("Scp3114Mods has been initialized." + (Config.Dbg ? " [Debug]" : ""));
        Handlers = new EventHandlers();
        Harmony = new Harmony("me.redforce04.scp3114mods");
        if (!Config.IsEnabled)
            return;
        RegisterEvents();
        base.OnEnabled();
    }

    internal void RegisterEvents()
    {
        EventsRegistered = true;
        
        API.Events.StranglingPlayer += Handlers.OnStranglingPlayer;
        EventManager.RegisterEvents(Handlers);
        Harmony.PatchAll();
    }

#if EXILED
    public override void OnDisabled()
#else
    [PluginUnload]
    public void OnDisabled()
#endif
    {
        UnregisterEvents();
        Harmony = null;
        Handlers = null;
        Singleton = null;
        base.OnDisabled();
    }

    internal void UnregisterEvents()
    {
        Harmony.UnpatchAll();
        API.Events.StranglingPlayer -= Handlers.OnStranglingPlayer;
        EventManager.UnregisterEvents(Handlers);
        EventsRegistered = false;
    }
}