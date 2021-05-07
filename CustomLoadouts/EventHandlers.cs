// -----------------------------------------------------------------------
// <copyright file="EventHandlers.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System;
    using System.Collections.Generic;
    using Exiled.Events.EventArgs;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Contains all of the EventHandlers used by this plugin.
    /// </summary>
    public static class EventHandlers
    {
        private static readonly Random Random = new Random();
        private static readonly HashSet<int> Spawning = new HashSet<int>();

        public static List<Loadout> Loadouts { get; } = new List<Loadout>();

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.OnChangingRole(ChangingRoleEventArgs)"/>
        internal static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!Spawning.Add(ev.Player.Id))
                return;

            foreach (Loadout loadout in Loadouts)
            {
                if (!ev.Player.CheckPermission(loadout.Permission))
                    continue;

                if (loadout.Chance < Random.Next(0, 101))
                    continue;

                if (loadout.RemoveAmmo)
                    ev.Player.Ammo.Clear();

                if (loadout.RemoveItems)
                    ev.Player.Items.Clear();

                foreach (var item in loadout.Items)
                    ev.Player.AddItem(item);
            }

            Spawning.Remove(ev.Player.Id);
        }
    }
}