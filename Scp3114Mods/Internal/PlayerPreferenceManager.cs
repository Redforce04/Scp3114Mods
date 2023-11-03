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

namespace Scp3114Mods.Internal;

public class PlayerPreferenceManager
{
    public static PlayerPreferenceManager Singleton;
    private CoroutineHandle loadingCoroutine;
    private CoroutineHandle updateCoroutine;
    private List<PreferenceToUpdate> _preferencesToUpdate = new List<PreferenceToUpdate>();
    internal string FileLocation { get; set; } = null!;
    private List<PlayerPreference> _preferences = new List<PlayerPreference>();
    public PlayerPreferenceManager()
    {
        Singleton = this;
        FileLocation = "";
        _loadAllInstances();
    }

    public void Deconstruct()
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

    public float Get3114Preference(Player ply)
    {
        return GetOrCreatePlayerPreference(ply, 5).Preference;
    }

    public void Set3114Preference(Player ply, int preference)
    {
        GetOrCreatePlayerPreference(ply, preference).Preference = preference;
    }

    private PlayerPreference GetOrCreatePlayerPreference(Player ply, int preferenceInt = 5)
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

        return new PlayerPreference(ply.DoNotTrack ? hashId : userId, preferenceInt);
    }

        
    private void _updatePlayerPreference(PlayerPreference preference, int newValue, int lineLoc = -1)
    {
        var val = new PreferenceToUpdate(preference, newValue, lineLoc);
        if (updateCoroutine.IsRunning)
        {

            Timing.CallDelayed(3f, () => { _updateLimitRecursion(val, 1); });
            return;
        }
        Timing.RunCoroutine(_updateCoroutine(val), "Player Preference Update Coroutine");
    }

    private void _updateLimitRecursion(PreferenceToUpdate pref, int iteration)
    {
        if (iteration >= 3)
        {
            Log.Warning($"Cannot update {pref.Preference.PlayerId}");
            Log.Debug("3rd update iteration, killing recursion.");
            return;
        }
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
        internal PlayerPreference(string playerId, int preference, bool saveToFile = true, int lineLoc = -1)
        {
            PlayerId = playerId;
            _preference = preference;
            if (lineLoc != -1)
                _lineLoc = lineLoc;
            PlayerPreferenceManager.Singleton._preferences.Add(this);
            if(saveToFile)
                _savePlayerPreference();
        }

        public override string ToString() => $"{PlayerId}, {Preference},\n";

        private int _lineLoc;
        private int _preference;
        internal string PlayerId { get; } // shouldn't ever be changed.
        internal int Preference
        {
            get => _preference;
            set
            {
                if (value == _preference)
                    return;
                value = _preference;
                    //_updatePlayerPreference();

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
                if (readsSinceDelay > 4)
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
                        new PlayerPreference(cache, value, false, lineNum);
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
            Logging.Debug($"Loaded {_preferences.Count} player preferences.");
        }

        private IEnumerator<float> _updateCoroutine(PreferenceToUpdate preference)
        {
            foreach (var x in _preferences)
            {
                
            }

            yield break;
        }

        private struct PreferenceToUpdate
        {
            internal PreferenceToUpdate(PlayerPreference pref, int newValue, int line = -1)
            {
                Preference = pref;
                NewValue = newValue;
                LineLoc = line;
            }
            internal PlayerPreference Preference;
            internal int LineLoc = -1;
            internal int NewValue = 5;
        }
#endregion
}
