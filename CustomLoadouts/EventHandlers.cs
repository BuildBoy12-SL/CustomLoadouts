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
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Exiled.Permissions.Extensions;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Contains all of the EventHandlers used by this plugin.
    /// </summary>
    public static class EventHandlers
    {
        private static readonly Random Random = new Random();
        private static readonly HashSet<string> Spawning = new HashSet<string>();

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.OnChangingRole(ChangingRoleEventArgs)"/>
        internal static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (Spawning.Contains(ev.Player.UserId))
            {
                return;
            }

            Spawning.Add(ev.Player.UserId);
            try
            {
                JProperty[] array = Plugin.Loadouts.Properties().ToArray();
                foreach (var jproperty in array)
                {
                    if (!ev.Player.CheckPermission($"customloadouts.{jproperty.Name}"))
                    {
                        continue;
                    }

                    try
                    {
                        JProperty[] array2 = jproperty.Value.Value<JObject>().Properties().ToArray();
                        foreach (var jproperty2 in array2)
                        {
                            if (!string.Equals(ev.NewRole.ToString(), jproperty2.Name, StringComparison.OrdinalIgnoreCase) &&
                                jproperty2.Name.ToUpper() != "ALL")
                            {
                                continue;
                            }

                            try
                            {
                                foreach (JToken jToken in jproperty2.Value.Children())
                                {
                                    JObject jObject = (JObject)jToken;
                                    JProperty jproperty3 = jObject.Properties().First();
                                    if (!float.TryParse(jproperty3.Name, out float num))
                                    {
                                        Log.Error($"Invalid chance: {jproperty3.Name}");
                                        continue;
                                    }

                                    float rand = Random.Next(1, 101);
                                    if (rand > num)
                                    {
                                        Log.Debug($"{jObject.Path}: Failed random chance. {num} < {rand}", Plugin.Instance.Config.Debug);
                                        continue;
                                    }

                                    Log.Debug($"{jObject.Path}: Succeeded random chance. {num} >= {rand}", Plugin.Instance.Config.Debug);

                                    foreach (JToken jToken2 in jproperty3.Value as JArray)
                                    {
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(jToken2.ToString()) || jToken2.ToString().ToUpper() != "REMOVEITEMS")
                                            {
                                                ev.Items.Clear();
                                                Log.Debug($"Cleared inventory of {ev.Player.Role} {ev.Player.Nickname} ({ev.Player.UserId})", Plugin.Instance.Config.Debug);
                                                continue;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error($"Error occured while resetting the inventory of {ev.Player?.Nickname}.\n{e}");
                                        }

                                        if (Enum.TryParse(jToken2.ToString(), true, out ItemType itemType))
                                        {
                                            ev.Items.Add(itemType);
                                            Log.Debug($"{ev.Player.Role} {ev.Player.Nickname} ({ev.Player.UserId}) was given item {jToken2}.", Plugin.Instance.Config.Debug);
                                        }
                                        else
                                        {
                                            Log.Error($"Could not parse {jToken2} as an ItemType.");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Error giving items: {e}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error checking role: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error checking permission: {e}");
            }

            Spawning.Remove(ev.Player.UserId);
        }
    }
}