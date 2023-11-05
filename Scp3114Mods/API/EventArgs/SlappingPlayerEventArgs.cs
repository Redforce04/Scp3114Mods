// I made this anyways kek - redforce
// -----------------------------------------------------------------------
// <copyright file="SlappingPlayerEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;

namespace Scp3114Mods.API.EventArgs
{
    /// <summary>
    ///     Contains all information before SCP-3114 slaps a player.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    public sealed class SlappingPlayerEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SlappingPlayerEventArgs" /> class.
        /// </summary>
        /// <param name="instance">
        ///     The <see cref="Scp3114Slap"/> instance which this is being instantiated from.
        /// </param>
        /// <param name="targetHub">
        ///     The <see cref="ReferenceHub"/> of the player who was being targeted.
        /// </param>
        /// <param name="humeShieldToGive">
        ///     The amount of hume shield cooldown that is given to the player.
        /// </param>
        /// <param name="isAllowed">
        ///     Is the event allowed to occur.
        /// </param>
        public SlappingPlayerEventArgs(Scp3114Slap instance, ReferenceHub targetHub, float damageAmount, float humeShieldToGive, bool isAllowed = true)
        {
            Player = Player.Get(instance.Owner);
            Target = Player.Get(targetHub);
            if(Player?.RoleBase is Scp3114Role role)
                Scp3114 = role;
            DamageAmount = damageAmount;
            HumeShieldToGive = humeShieldToGive;
            IsAllowed = isAllowed;
        }

        /// <summary>
        ///     Gets or sets the hume shield amount that will be regenerated for the Scp 3114 <see cref="Player"/>.
        /// </summary>
        public float HumeShieldToGive { get; set; }

        /// <summary>
        ///     The <see cref="Player"/> who is Scp-3114.
        /// </summary>
        public Player Player { get; set; }

        /// <inheritdoc cref="IScp3114Event.Scp3114"/>
        public Scp3114Role Scp3114 { get; }

        /// <summary>
        ///     Gets the <see cref="Player"/> who is being slapped.
        /// </summary>
        public Player Target { get; }
        
        public float DamageAmount { get; set; }

        /// <inheritdoc cref="IDeniableEvent.IsAllowed"/>
        public bool IsAllowed { get; set; }
    }
}
