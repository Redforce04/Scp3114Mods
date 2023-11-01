using HarmonyLib;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
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
    public override PluginPriority Priority => PluginPriority.Default;
#endif
    public const string VersionName = "1.0.3";
    public static Scp3114Mods Singleton = null!;

    public Harmony Harmony { get; set; } = null!;

    public EventHandlers Handlers { get; set; } = null!;
    
#if !EXILED
    [PluginConfig] 
    public Translations Translation;

    [PluginConfig]
    public Config Config = null!;
#endif
    public bool EventsRegistered { get; set; } = false;
    internal void _clearCooldownList() => _scp3114Strangles.Clear();
    
#if !EXILED
    [PluginEntryPoint("Scp3114Mods", VersionName, "Modifies the mechanics of Scp3114 to be more balanced.", "Redforce04")]
#endif
    public void OnStart()
    {
        Singleton = this;
        Log.Info("Scp3114Mods has been initialized." + ( Config.Dbg ? " [Debug]" : ""));
        Handlers = new EventHandlers();
        Harmony = new Harmony("me.redforce04.scp3114mods");
        if (!Config.IsEnabled)
            return;
        RegisterEvents();
    }

    internal void RegisterEvents()
    {
        EventsRegistered = true;
        
        API.Events.StranglingPlayer += Handlers.OnStranglingPlayer;
        EventManager.RegisterEvents(Handlers);
        Harmony.PatchAll();
    }

#if !EXILED
    [PluginUnload]
#endif
    public void OnStop()
    {
        UnregisterEvents();
        Harmony = null;
        Handlers = null;
        //_scp3114Cooldowns = null;
        Singleton = null;
    }

    internal void UnregisterEvents()
    {
        Harmony.UnpatchAll();
        API.Events.StranglingPlayer -= Handlers.OnStranglingPlayer;
        EventManager.UnregisterEvents(Handlers);
        EventsRegistered = false;
    }
    private List<Scp3114Strangle> _scp3114Strangles { get; set; } = new List<Scp3114Strangle >();

    internal void AddCooldownForPlayer(Scp3114Strangle strangle)
    {
        if (_scp3114Strangles.Contains(strangle))
            return;
        _scp3114Strangles.Add(strangle);
    }
}