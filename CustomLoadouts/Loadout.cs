// -----------------------------------------------------------------------
// <copyright file="Loadout.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System.Collections.Generic;
    using Exiled.API.Enums;

    /// <summary>
    /// A container to be used to pack and unpack loadouts to be given to players when they spawn.
    /// </summary>
    public class Loadout
    {
        /// <summary>
        /// Gets or sets the permission required to get the loadout.
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Gets or sets the role that this applies to.
        /// </summary>
        public RoleType Role { get; set; }

        /// <summary>
        /// Gets or sets the chance to get the loadout.
        /// </summary>
        public float Chance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items should be removed from the player.
        /// </summary>
        public bool RemoveItems { get; set; }

        /// <summary>
        /// Gets or sets the collection of items to be given to the player.
        /// </summary>
        public List<ItemType> Items { get; set; }

        /// <summary>
        /// Gets or sets the collection of ammo to be given to the player.
        /// </summary>
        public Dictionary<AmmoType, ushort> Ammo { get; set; }
    }
}