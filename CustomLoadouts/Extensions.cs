// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System;
    using Exiled.API.Enums;

    /// <summary>
    /// Misc. extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the default amount of ammo in a magazine for an <see cref="AmmoType"/>.
        /// </summary>
        /// <param name="ammoType">The <see cref="AmmoType"/> variant to check the size for.</param>
        /// <returns>The size of the magazine.</returns>
        public static uint GetMagazineSize(this AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Nato556:
                    return 25;
                case AmmoType.Nato762:
                    return 35;
                case AmmoType.Nato9:
                    return 15;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ammoType));
            }
        }
    }
}