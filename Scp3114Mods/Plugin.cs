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
public class Scp3114Mods : Plugin<Config>
{
    public override string Author => "Redforce04";
    public override string Name => "Scp3114Mods";
    public override string Prefix => "3114";
    public override Version Version => Version.Parse(VersionName);
    public override PluginPriority Priority => PluginPriority.Default;
#endif
    public const string VersionName = "1.0.1";
    public static Scp3114Mods Singleton;
    
    public Harmony Harmony { get; set; }
    
    public EventHandlers Handlers { get; set; }
    
    [PluginConfig]
    public Config Config;
    public bool EventsRegistered { get; set; } = false;
    internal void _clearCooldownList() => _scp3114Strangles.Clear();

    private CoroutineHandle StrangleCooldownProcessor;
    private bool _killCoroutine;

    [PluginEntryPoint("Scp3114Mods", VersionName, "Modifies the mechanics of Scp3114 to be more balanced.", "Redforce04")]
    public void OnStart()
    {
        
        Singleton = this;
        Log.Info("Scp3114Mods has been initialized." + ( Config.Debug ? " [Debug]" : ""));
        Handlers = new EventHandlers();
        Harmony = new Harmony("me.redforce04.scp3114mods");
        if (!Config.IsEnabled)
            return;
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
        //_scp3114Cooldowns = null;
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
    private List<Scp3114Strangle> _scp3114Strangles { get; set; } = new List<Scp3114Strangle >();

    internal void AddCooldownForPlayer(Scp3114Strangle strangle)
    {
        if (_scp3114Strangles.Contains(strangle))
            return;
        _scp3114Strangles.Add(strangle);
    }



    private IEnumerator<float> ProcessStrangleCooldowns()
    {
        if (Config.Debug)
            Log.Debug("Running Cooldown Coroutine");
        List<Scp3114Strangle> stranglesToRemove = new List<Scp3114Strangle>();
        while (!_killCoroutine)
        {
            if (Config.StranglePartialCooldown <= 0)
            {
                yield return Timing.WaitForSeconds(60);
                continue;
            }
            try
            {
                for (int i = 0; i < _scp3114Strangles.Count(); i++)
                {
                    var strangle = _scp3114Strangles[i];
                    if (strangle is null)
                    {
                        continue;
                    }

                    if (strangle.SyncTarget is null)
                    {
                        float amount = Config.StranglePartialCooldown;
                        if (strangle.Cooldown.Remaining < amount)
                        {
                            if(Config.Debug) Log.Debug("Partial Strangle Finished. Adding cooldown.");
                            strangle.Cooldown.Trigger(amount);
                        }
                        stranglesToRemove.Add(strangle);
                    }
                }

                foreach (var itemToRemove in stranglesToRemove)
                {
                    _scp3114Strangles.Remove(itemToRemove);
                }
                stranglesToRemove.Clear();
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