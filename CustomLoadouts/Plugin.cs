// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomLoadouts
{
    using System.IO;
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

        private static readonly string PluginDirectory = Path.Combine(Paths.Configs, "CustomLoadouts");

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

        /// <summary>
        /// Gets all of the loadouts to be assigned to players.
        /// </summary>
        internal static JObject Loadouts { get; private set; }

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            string folderPath = Path.Combine(Paths.Plugins, "CustomLoadouts");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string globalFilePath = Path.Combine(folderPath, "config.yml");
            if (!File.Exists(globalFilePath))
            {
                File.WriteAllText(globalFilePath, Encoding.UTF8.GetString(Resources.Config));
            }

            Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnChangingRole;
            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnChangingRole;
            base.OnDisabled();
        }

        /// <inheritdoc/>
        public override void OnReloaded()
        {
            Reload();
        }

        private void Reload()
        {
            string text = Config.Global
                ? Path.Combine(PluginDirectory, "config.yml")
                : Path.Combine(PluginDirectory, Server.Port.ToString(), "config.yml");

            Log.Info($"Loading config {text}...");
            if (!Config.Global)
            {
                if (!Directory.Exists(Path.Combine(PluginDirectory, Server.Port.ToString())))
                {
                    Directory.CreateDirectory(Path.Combine(PluginDirectory, Server.Port.ToString()));
                }
            }

            if (!File.Exists(text))
            {
                File.WriteAllText(text, Encoding.UTF8.GetString(Resources.Config));
            }

            FileStream stream = File.OpenRead(text);
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object obj = deserializer.Deserialize(new StreamReader(stream));
            ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
            string text2 = serializer.Serialize(obj);
            JObject jObject = JObject.Parse(text2);
            Loadouts = jObject.SelectToken("customloadouts").Value<JObject>();
            Log.Info("Config loaded.");
        }
    }
}