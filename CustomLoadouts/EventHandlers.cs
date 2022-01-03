// -----------------------------------------------------------------------
// <copyright file="EventHandlers.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.Events.EventArgs;
    using Exiled.Loader;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Contains all of the EventHandlers used by this plugin.
    /// </summary>
    public class EventHandlers
    {
        /// <summary>
        /// Gets all currently parsed <see cref="Loadout"/> objects.
        /// </summary>
        public static List<Loadout> Loadouts { get; } = new List<Loadout>();

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.OnChangingRole(ChangingRoleEventArgs)"/>
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            foreach (Loadout loadout in Loadouts)
            {
                if (!ev.Player.CheckPermission(loadout.Permission))
                    continue;

                if (loadout.Role != ev.NewRole && loadout.Role != RoleType.None)
                    continue;

                if (loadout.Chance < Loader.Random.Next(0, 101))
                    continue;

                if (loadout.RemoveItems)
                    ev.Items.Clear();

                foreach (ItemType item in loadout.Items)
                    ev.Items.Add(item);

                foreach (KeyValuePair<AmmoType, ushort> ammo in loadout.Ammo)
                    ev.Ammo[ammo.Key.GetItemType()] += ammo.Value;
            }
        }
    }
}