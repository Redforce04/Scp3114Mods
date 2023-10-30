using HarmonyLib;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using UnityEngine;

namespace Scp3114Mods;

public class Scp3114Mods
{
    public const string Version = "1.0.1";
    public static Scp3114Mods Singleton;
    
    public Harmony Harmony { get; set; }
    
    public EventHandlers Handlers { get; set; }
    
    [PluginConfig]
    public Config Config;

    private bool _killCoroutine;
    private CoroutineHandle StrangleCooldownProcessor;
    public bool EventsRegistered { get; set; } = false;
    internal void _clearCooldownList() => _scp3114Cooldowns.Clear();

    [PluginEntryPoint("Scp3114Mods", Version, "Modifies the mechanics of Scp3114 to be more balanced.", "Redforce04")]
    public void OnStart()
    {
        if (!Config.IsEnabled)
            return;
        Singleton = this;
        Log.Info("Scp3114Mods has been initialized." + ( Config.Debug ? " [Debug]" : ""));
        _scp3114Cooldowns = new Dictionary<int, float>();
        Handlers = new EventHandlers();
        Harmony = new Harmony("me.redforce04.scp3114mods");
        RegisterEvents();
    }

    internal void RegisterEvents()
    {
        EventsRegistered = true;
        _killCoroutine = false;
        EventManager.RegisterEvents(Handlers);
        Harmony.PatchAll();
        if (Config.StrangleCooldown > -1)
        {
            StrangleCooldownProcessor = Timing.RunCoroutine(ProcessStrangleCooldowns(),"StrangleCooldownProcessor");
        }
    }

    [PluginUnload]
    public void OnStop()
    {
        UnregisterEvents();
        Harmony = null;
        Handlers = null;
        _scp3114Cooldowns = null;
        Singleton = null;
    }

    internal void UnregisterEvents()
    {
        _killCoroutine = true;
        Harmony.UnpatchAll();
        EventManager.UnregisterEvents(Handlers);
        Timing.CallDelayed(1f, () =>
        {
            if (StrangleCooldownProcessor.IsRunning)
            {
                Timing.KillCoroutines(StrangleCooldownProcessor);
                if (Config.Debug)
                    Log.Debug("Killed Cooldown Coroutine via coroutine kill");   
            }
        });
        EventsRegistered = false;
    }
    private Dictionary<int, float> _scp3114Cooldowns { get; set; } = new Dictionary<int, float>();

    internal void AddCooldownForPlayer(Player ply, bool partial = true)
    {
        if (Config.Debug)
            Log.Debug($"Adding Cooldown for {ply.Nickname}");
        if (Config.StrangleCooldown < 0)
            return;

        float amount = partial ? Config.StranglePartialCooldown : Config.StrangleCooldown;
        
        if (!_scp3114Cooldowns.ContainsKey(ply.PlayerId))
            _scp3114Cooldowns.Add(ply.PlayerId, amount);
        else
            _scp3114Cooldowns[ply.PlayerId] = amount > _scp3114Cooldowns[ply.PlayerId] ? amount : _scp3114Cooldowns[ply.PlayerId];
    }

    internal bool PlayerCanStrangle(Player ply, out float cooldownRemaining)
    {
        cooldownRemaining = 0;
        if (ply.IsBypassEnabled)
            return true;

        if (!_scp3114Cooldowns.ContainsKey(ply.PlayerId))
        {
            _scp3114Cooldowns.Add(ply.PlayerId, 0);
            return true;
        }

        if (_scp3114Cooldowns[ply.PlayerId] <= 0)
            return true;

        cooldownRemaining = _scp3114Cooldowns[ply.PlayerId];
        return false;
    }

    private IEnumerator<float> ProcessStrangleCooldowns()
    {
        if (Config.Debug)
            Log.Debug("Running Cooldown Coroutine");
        while (!_killCoroutine)
        {
            try
            {
                Dictionary<int, float> newVals = new Dictionary<int, float>();
                foreach (var kvp in _scp3114Cooldowns)
                {
                    if (_scp3114Cooldowns[kvp.Key] > 0)
                        newVals.Add(kvp.Key, Mathf.Clamp(kvp.Value - 1f, 0, Config.StrangleCooldown));

                    //if (Config.Debug)
                    //    Log.Debug($"Processing player {i}: {kvp.Value}");
                }

                this._scp3114Cooldowns = newVals;

            }
            catch (Exception e)
            {
                Log.Error("Scp3114Mods has caught an error at UpdateCooldowns");
                if (Scp3114Mods.Singleton.Config.Debug)
                {
                    Log.Debug($"Exception: \n{e}");
                }
            }

            yield return Timing.WaitForSeconds(1f);
        }
            
        if (Config.Debug)
            Log.Debug("Killed Cooldown Coroutine via bool");
    }
}