// -----------------------------------------------------------------------
// <copyright file="Loadout.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System.Collections.Generic;

    public class Loadout
    {
        public string Permission { get; set; }

        public RoleType Role { get; set; }

        public float Chance { get; set; }

        public bool RemoveAmmo { get; set; }

        public bool RemoveItems { get; set; }

        public List<ItemType> Items { get; set; }
    }
}