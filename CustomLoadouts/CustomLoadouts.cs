namespace CustomLoadouts
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Exiled.Permissions.Extensions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using YamlDotNet.Serialization;

    public class CustomLoadouts : Plugin<Config>
    {
        public override string Prefix => "customloadouts";
        private JObject _loadouts;
        private bool _debug;
        private readonly Random _random = new Random();
        private readonly HashSet<string> _spawning = new HashSet<string>();
        private bool _verbose;
        private static string _pluginDirectory;

        public override void OnEnabled()
        {
            _pluginDirectory = Path.Combine(Paths.Plugins, "CustomLoadouts");
            if (!Directory.Exists(_pluginDirectory))
                Directory.CreateDirectory(_pluginDirectory);
            string globalFilePath = Path.Combine(_pluginDirectory, "config.yml");
            if (!File.Exists(globalFilePath))
                File.WriteAllText(globalFilePath, Encoding.UTF8.GetString(Resources.Resource1.config));

            Exiled.Events.Handlers.Player.ChangingRole += StartItemsEvent;
            Reload();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= StartItemsEvent;
            base.OnDisabled();
        }

        private void StartItemsEvent(ChangingRoleEventArgs ev)
        {
            if (_spawning.Contains(ev.Player.UserId))
                return;

            _spawning.Add(ev.Player.UserId);
            try
            {
                JProperty[] array = _loadouts.Properties().ToArray();
                foreach (var jproperty in array)
                {
                    if (!ev.Player.CheckPermission($"customloadouts.{jproperty.Name}"))
                        continue;

                    try
                    {
                        JProperty[] array2 = jproperty.Value.Value<JObject>().Properties().ToArray();
                        foreach (var jproperty2 in array2)
                        {
                            if (!string.Equals(ev.NewRole.ToString(), jproperty2.Name,
                                    StringComparison.CurrentCultureIgnoreCase) &&
                                jproperty2.Name.ToUpper() != "ALL") continue;

                            try
                            {
                                foreach (JToken jToken in jproperty2.Value.Children())
                                {
                                    JObject jObject = (JObject) jToken;
                                    JProperty jproperty3 = jObject.Properties().First();
                                    if (!float.TryParse(jproperty3.Name, out float num))
                                    {
                                        Log.Error($"Invalid chance: {jproperty3.Name}");
                                        continue;
                                    }

                                    float rand = _random.Next(1, 101);
                                    if (rand > num)
                                    {
                                        Log.Debug($"{jObject.Path}: Failed random chance. {num} < {rand}", _debug);
                                        continue;
                                    }

                                    Log.Debug($"{jObject.Path}: Succeeded random chance. {num} >= {rand}", _debug);

                                    foreach (JToken jToken2 in jproperty3.Value as JArray)
                                    {
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(jToken2.ToString()) &&
                                                jToken2.ToString().ToUpper() == "REMOVEITEMS")
                                            {
                                                ev.Items.Clear();
                                                if (_verbose)
                                                    Log.Info(
                                                        $"Cleared inventory of {ev.NewRole} {ev.Player.Nickname} ({ev.Player.UserId})");

                                                continue;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error(
                                                $"Error occured while resetting the inventory of {ev.Player?.Nickname}.");
                                            if (_debug)
                                                Log.Error(e);
                                        }

                                        try
                                        {
                                            ev.Items.Add((ItemType) Enum.Parse(typeof(ItemType), jToken2.ToString(),
                                                true));
                                            if (_verbose)
                                                Log.Info(
                                                    $"{ev.NewRole} {ev.Player.Nickname} ({ev.Player.UserId}) was given item {jToken2}.");
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error(
                                                $"Error occured while giving item \"{jToken2}\" to {ev.Player?.Nickname}");
                                            if (_debug)
                                                Log.Error(e);
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

            _spawning.Remove(ev.Player.UserId);
        }

        public override void OnReloaded()
        {
            Reload();
        }

        private void Reload()
        {
            string text = Config.Global
                ? Path.Combine(_pluginDirectory, "config.yml")
                : Path.Combine(_pluginDirectory, Server.Port.ToString(), "config.yml");

            Log.Info($"Loading config {text}...");
            if (!Config.Global)
                if (!Directory.Exists(Path.Combine(_pluginDirectory, Server.Port.ToString())))
                    Directory.CreateDirectory(Path.Combine(_pluginDirectory, Server.Port.ToString()));

            if (!File.Exists(text))
                File.WriteAllText(text, Encoding.UTF8.GetString(Resources.Resource1.config));

            FileStream stream = File.OpenRead(text);
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object obj = deserializer.Deserialize(new StreamReader(stream));
            ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
            string text2 = serializer.Serialize(obj);
            JObject jObject = JObject.Parse(text2);
            _debug = jObject.SelectToken("debug").Value<bool>();
            _verbose = jObject.SelectToken("verbose").Value<bool>();
            _loadouts = jObject.SelectToken("customloadouts").Value<JObject>();
            Log.Info("Config loaded.");
        }
    }
}