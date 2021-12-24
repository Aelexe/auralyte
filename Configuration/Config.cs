using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;
using Dalamud.Configuration;
using Dalamud.Logging;

namespace Auralyte.Configuration
{
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 1;

        public float FontSize = 17;

        [JsonProperty("auras")] [DefaultValue(null)] public List<Aura> auras = new();

        public string PluginVersion = ".INITIAL";
        [JsonIgnore] public string PrevPluginVersion = string.Empty;
        [JsonIgnore] public bool FirstStart = false;
        [JsonIgnore] public static DirectoryInfo ConfigFolder => Auralyte.PluginInterface.ConfigDirectory;
        [JsonIgnore] private static DirectoryInfo iconFolder;
        [JsonIgnore] private static DirectoryInfo backupFolder;
        [JsonIgnore] private static FileInfo tempConfig;
        [JsonIgnore] public static FileInfo ConfigFile => Auralyte.PluginInterface.ConfigFile;

        public string GetVersion() => PluginVersion;
        public void UpdateVersion() {
            if(PluginVersion != ".INITIAL")
                PrevPluginVersion = PluginVersion;
            PluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        public bool CheckVersion() => PluginVersion == Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        public void CheckDisplayUpdateWindow() {
            if(!string.IsNullOrEmpty(PrevPluginVersion)) {
                //var v = new Version(PrevPluginVersion);
                //if (new Version("1.3.2.0") >= v)
                //    displayUpdateWindow = true;
            }
        }

        /// <summary>
        /// Initialises the configuration as required before use.
        /// </summary>
        public void Initialise() {
            Annotate();
            if(ConfigFolder.Exists) {
                iconFolder = new DirectoryInfo(Path.Combine(ConfigFolder.FullName, "icons"));
                backupFolder = new DirectoryInfo(Path.Combine(ConfigFolder.FullName, "backups"));
                tempConfig = new FileInfo(backupFolder.FullName + "\\temp.json");
            }
        }

        /// <summary>
        /// Annotates the configuration with user input/display values.
        /// </summary>
        public void Annotate() {
            foreach(Aura aura in auras) {
                aura.Annotate();
            }
        }

        /// <summary>
        /// Reindexes every aspect of the config to ensure no overlapping IDs.
        /// </summary>
        public void Reindex() {
            for(int i = 0; i < auras.Count; i++) {
                auras[i].id = i + 1;
                auras[i].Reindex();
            }
        }

        public void Save(bool failed = false) {
            Reindex();
            try {
                Auralyte.PluginInterface.SavePluginConfig(this);
            } catch {
                if(!failed) {
                    PluginLog.LogError("Failed to save! Retrying...");
                    Save(true);
                } else {
                    PluginLog.LogError("Failed to save again :(");
                }
            }
        }
    }
}
