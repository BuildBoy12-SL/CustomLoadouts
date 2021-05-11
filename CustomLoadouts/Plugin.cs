// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CustomLoadouts.Properties;
    using Exiled.API.Features;
    using Newtonsoft.Json.Linq;
    using YamlDotNet.Serialization;

    /// <summary>
    /// The main plugin class.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        private static readonly Plugin InstanceValue = new Plugin();
        private static readonly string ConfigDirectory = Path.Combine(Paths.Configs, "CustomLoadouts");
        private static readonly string FileDirectory = Path.Combine(ConfigDirectory, "config.yml");

        private Plugin()
        {
        }

        /// <inheritdoc/>
        public override string Author { get; } = "Build";

        /// <inheritdoc/>
        public override string Prefix { get; } = "customloadouts";

        /// <summary>
        /// Gets a static instance of the <see cref="Plugin"/> class.
        /// </summary>
        internal static Plugin Instance { get; } = InstanceValue;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnChangingRole;
            Exiled.Events.Handlers.Server.ReloadedConfigs += OnReloadedConfigs;
            LoadLoadouts();
            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            EventHandlers.Loadouts.Clear();
            Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnChangingRole;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= OnReloadedConfigs;
            base.OnDisabled();
        }

        private void OnReloadedConfigs()
        {
            EventHandlers.Loadouts.Clear();
            LoadLoadouts();
        }

        private void LoadLoadouts()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                string localDirectory = Path.Combine(ConfigDirectory, Server.Port.ToString());
                if (!Config.Global && !Directory.Exists(localDirectory))
                    Directory.CreateDirectory(localDirectory);

                string path = Config.Global
                    ? FileDirectory
                    : Path.Combine(localDirectory, "config.yml");

                if (!File.Exists(path))
                    File.WriteAllText(path, Encoding.UTF8.GetString(Resources.Config));

                FileStream stream = File.OpenRead(path);
                IDeserializer deserializer = new DeserializerBuilder().Build();
                object yamlObject = deserializer.Deserialize(new StreamReader(stream));
                if (yamlObject == null)
                {
                    Log.Error("Unable to deserialize loadouts!");
                    OnDisabled();
                    return;
                }

                ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
                string jsonString = serializer.Serialize(yamlObject);
                JObject json = JObject.Parse(jsonString);

                JObject configs = json.SelectToken("customloadouts")?.Value<JObject>();
                if (configs == null)
                {
                    Log.Error("Unable to read loadouts!");
                    OnDisabled();
                    return;
                }

                JProperty[] groups = configs.Properties().ToArray();
                foreach (JProperty node in groups)
                {
                    JProperty[] array = node.Value.Value<JObject>().Properties().ToArray();
                    foreach (JProperty group in array)
                    {
                        bool allRoles = string.Equals(group.Name, "all", StringComparison.OrdinalIgnoreCase);
                        if (!Enum.TryParse(group.Name, true, out RoleType roleType))
                        {
                            if (!allRoles)
                            {
                                Log.Warn($"Unable to parse {group.Name} into a {nameof(RoleType)}.");
                                continue;
                            }

                            roleType = RoleType.None;
                        }

                        foreach (JToken bundle in group.Value.Children())
                        {
                            JObject jObject = (JObject)bundle;
                            JProperty jProperty = jObject.Properties().First();
                            if (!float.TryParse(jProperty.Name, out float chance))
                            {
                                Log.Error($"Invalid chance: {jProperty.Name}");
                                continue;
                            }

                            bool removeItems = false;
                            List<ItemType> items = new List<ItemType>();
                            foreach (JToken jToken in jProperty.Value as JArray)
                            {
                                string name = jToken.ToString();

                                if (string.Equals(name, "removeitems", StringComparison.OrdinalIgnoreCase))
                                {
                                    removeItems = true;
                                    continue;
                                }

                                if (!Enum.TryParse(name, true, out ItemType itemType))
                                {
                                    Log.Warn($"Could not parse {itemType} into an {nameof(ItemType)}.");
                                    continue;
                                }

                                items.Add(itemType);
                            }

                            EventHandlers.Loadouts.Add(new Loadout
                            {
                                Permission = "customloadouts." + node.Name,
                                Chance = chance,
                                Role = roleType,
                                RemoveItems = removeItems,
                                Items = items,
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error while generating loadouts: {e}");
                OnDisabled();
            }
        }
    }
}