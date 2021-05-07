// -----------------------------------------------------------------------
// <copyright file="Loadouts.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts.Commands
{
    using System;
    using System.Text;
    using CommandSystem;
    using NorthwoodLib.Pools;

    /// <summary>
    /// A command to list all registered loadouts.
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Loadouts : ICommand
    {
        /// <inheritdoc />
        public string Command { get; } = "loadouts";

        /// <inheritdoc />
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public string Description { get; } = "Lists all current loadouts.";

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            foreach (Loadout loadout in EventHandlers.Loadouts)
            {
                stringBuilder.AppendLine().AppendLine($"Permission: {loadout.Permission}")
                    .AppendLine($"Role: {(loadout.Role == RoleType.None ? "All" : loadout.Role.ToString())}")
                    .AppendLine($"Chance: {loadout.Chance}").AppendLine($"RemoveAmmo: {loadout.RemoveAmmo}")
                    .AppendLine($"RemoveItems: {loadout.RemoveItems}").AppendLine($"Items: {string.Join(", ", loadout.Items)}");
            }

            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder);
            return true;
        }
    }
}