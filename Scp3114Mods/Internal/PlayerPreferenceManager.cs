// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         PlayerPreferenceManager.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/02/2023 4:29 PM
//    Created Date:     11/02/2023 4:29 PM
// -----------------------------------------

using Exiled.API.Features.Core.Generic;
using PluginAPI.Core;
using System.Security.Cryptography;
using System.Text;
using MEC;
using Scp3114Mods.API;
using UnityEngine;

namespace Scp3114Mods.Internal;

internal class PlayerPreferenceManager
{
    internal static PlayerPreferenceManager Singleton;
    private CoroutineHandle loadingCoroutine;
    private CoroutineHandle updateCoroutine;
    internal string FileLocation { get; set; } = null!;
    private bool init = false;
    private List<PlayerPreference> _preferences = new List<PlayerPreference>();
    internal PlayerPreferenceManager()
    {
        Singleton = this;
        FileLocation = Scp3114Mods.Singleton.Config.PlayerPreferenceFileLocation;
        try
        {
            if(!File.Exists(Scp3114Mods.Singleton.Config.PlayerPreferenceFileLocation))
                File.Create(Scp3114Mods.Singleton.Config.PlayerPreferenceFileLocation).Close();
            Logging.Debug($"Player Preferences Location: {Scp3114Mods.Singleton.Config.PlayerPreferenceFileLocation}");
            _loadAllInstances();
        }
        catch (DirectoryNotFoundException)
        {
            Logging.Error(
                "Cannot find the base directory or file to store the player-preference data. Please check your configs and ensure it can be read/written to, and that it is in a valid location. (For pterodactyl users it must be in [/home/container/*]");
        }
        catch (UnauthorizedAccessException)
        {
            Logging.Error(
                $"Cannot write to the base-directory or file for player-preference data. Ensure that the default location, or location set in configs, can be written/read to.");
        }
        catch (Exception e)
        {
            Logging.Error("Could not load player preference file. Please check your configs and ensure it can be read/written to, and that it is in a valid location. (For pterodactyl users it must be in [/home/container/*]");
            Logging.Debug($"Exception: \n{e}");
        }
    }

    internal void Deconstruct()
    {
        if (loadingCoroutine.IsRunning)
        {
            Timing.KillCoroutines(loadingCoroutine);
        }

        if (updateCoroutine.IsRunning)
        {
            Timing.KillCoroutines(updateCoroutine);
        }
    }

    internal int Get3114Preference(Player ply)
    {
        if (!init)
        {
            Logging.Warn($"Cannot get the player preference! Defaults will be used!");
            return Scp3114Mods.Singleton.Config.DefaultPlayerPreference;
        }
        var preference = GetOrCreatePlayerPreference(ply, Scp3114Mods.Singleton.Config.DefaultPlayerPreference, false);
        if (preference is null)
            return Scp3114Mods.Singleton.Config.DefaultPlayerPreference;
        Logging.Debug($"Player Preference: {preference.Preference}");
        return preference.Preference;
    }

    internal void Set3114Preference(Player ply, int preference)
    {
        if (!init)
        {
            Logging.Warn("Cannot update the player preference!");
            return;
        }
        GetOrCreatePlayerPreference(ply, preference).Preference = preference;
    }

    private PlayerPreference GetOrCreatePlayerPreference(Player ply, int preferenceInt = 5, bool returnNullIfNotFound = false)
    {
        string userId = ply.UserId;
        
        var preference = _preferences.FirstOrDefault(x => x.PlayerId == ply.UserId);
        
        if (preference is not null)
            return preference;
        
        string hashId = GetHashString(ply.UserId);
        
        // only call if necessary.
        preference = _preferences.FirstOrDefault(x => x.PlayerId == hashId);
        
        if (preference is not null)
            return preference;
        if (returnNullIfNotFound) return null;

        return new PlayerPreference(ply.DoNotTrack ? hashId : userId, preferenceInt);
    }

    private void _updateLimitRecursion(PlayerPreference pref, int iteration)
    {
        if (iteration >= 3)
        {
            Log.Warning($"Cannot update {pref.PlayerId}");
            Log.Debug("3rd update iteration, killing recursion.");
            return;
        }
        if (Singleton.updateCoroutine.IsRunning)
        {
            Timing.CallDelayed(3f, () =>
            {
                _updateLimitRecursion(pref, iteration + 1);
            });
            return;
        }
        Timing.RunCoroutine(_updateCoroutine(), "Player Preference Update Coroutine");
    }
    
#region Hashing
    private static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
#endregion
#region PlayerPreference
    private protected class PlayerPreference
    {
        internal PlayerPreference(string playerId, int preference, bool saveToFile = true)
        {
            PlayerId = playerId;
            _preference = preference;
            PlayerPreferenceManager.Singleton._preferences.Add(this);
            if(saveToFile)
                _savePlayerPreference();
        }

        public override string ToString() => $"{PlayerId}, {Preference},\n";

        private int _preference;
        internal string PlayerId { get; } // shouldn't ever be changed.
        internal int Preference
        {
            get => _preference;
            set
            {
                _preference = Mathf.Clamp(value, -5, 5);
                Singleton._updateLimitRecursion(this, 0);
            }
        }
        private void _savePlayerPreference()
        {
            File.AppendAllText(Singleton.FileLocation, this.ToString());
        }
    } 
#endregion
#region Stream Implementations
        private void _loadAllInstances() => this.loadingCoroutine = Timing.RunCoroutine(_instanceLoadingCoroutine(), "Player Preference Instance Loading Coroutine");
        private IEnumerator<float> _instanceLoadingCoroutine()
        {
            
            Logging.Debug("Loading Player Preferences.");
            StreamReader stream = new StreamReader(File.Open(PlayerPreferenceManager.Singleton.FileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            // define these here so we dont reallocate for each iteration
            int errorCount = 0;
            int readsSinceDelay = 0;
            int lineNum = -1;
            string line = "";
            string cache = "";
            int value = 0;
            while ((line = stream.ReadLine()) != null)
            {
                lineNum++;
                // ignore comments
                if (line.StartsWith("#"))
                {
                    // make sure to reset the cache as to not screw something up.
                    cache = "";
                    continue;
                }
                // Makes this more or less "async" - won't hold the thread up for ages aka seconds. Every 4 lines - yield.
                if (readsSinceDelay > 20)
                {
                    readsSinceDelay = 0;
                    yield return Timing.WaitForOneFrame;
                }
                readsSinceDelay++;

                // This may be iterated a lot, so try to optimize as well as possible.
                foreach (var x in line.Split(','))
                {
                    // this is CSV (comma separated values - every other value should be an int)
                    if (cache == "")
                    {
                        cache = x;
                        continue;
                    }
                    // probably a bad line or out of sync.
                    if (!int.TryParse(x.Trim(), out value))
                    {
                        Logging.Debug($"[Player Preference Manager] Caching desync error.");
                        cache = x;
                        continue;
                    }

                    try
                    {
                        var unused = new PlayerPreference(cache, value, false);
                        cache = "";
                    }
                    catch (Exception e)
                    {
                        if (errorCount > 3)
                        {
                            // bad stuff - prevents 80000 errors from a reoccuring issue.
                            stream.Close();
                            yield break;
                        }
                        Logging.Warning("Caught an error while reading player preference.");
                        Logging.Debug($"{e}");
                        errorCount++;
                    }
                }
            }
            stream.Close();
            Logging.Info($"Loaded {_preferences.Count} player preferences.");
            init = true;
        }

        private IEnumerator<float> _updateCoroutine()
        {
            StreamWriter stream = new StreamWriter(File.Open(PlayerPreferenceManager.Singleton.FileLocation, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite));
            int i = 0;
            foreach (var x in _preferences)
            {
                if (i > 20)
                {
                    i = 0;
                    yield return Timing.WaitForOneFrame;
                }
                i++;
                if(x is null)
                    continue;
                
                stream.Write(x.ToString());
            }
            stream.Close();
            yield break;
        }
        #endregion
}
