using System;
using System.Collections.Generic;
using System.Numerics;
using Auralyte.Configuration;
using Auralyte.Game;
using Auralyte.UI;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace Auralyte {
    public class Auralyte : IDalamudPlugin {
        // Identification
        public static string Id { get { return _name.ToLower(); } }
        private static readonly string _name = "Auralyte";
        public string Name => _name;
        public static string GetName() => _name;

        // Config
        public static Config Config { get; private set; }
        public const float DefaultFontSize = 17;
        public const float MaxFontSize = 64;
        public static ImFontPtr Font { get; private set; }

        // Config GUI
        private readonly PluginUI pluginUi;

        // Dalamud
        [PluginService]
        public static DalamudPluginInterface PluginInterface { get; private set; }

        [PluginService]
        public static ClientState ClientState { get; private set; }

        [PluginService]
        public static CommandManager CommandManager { get; private set; }

        [PluginService]
        public static DataManager DataManager { get; private set; }

        [PluginService]
        public static GameGui GameGui { get; private set; }

        public Auralyte(DalamudPluginInterface pluginInterface) {
            pluginInterface.Inject(this);

            LoadPlugin();

            Config = (Config)PluginInterface.GetPluginConfig() ?? new();
            Config.Initialise();

            CommandManager.AddHandler("/auralyte", new CommandInfo(ToggleConfig) {
                HelpMessage = "Open the configuration menu.",
                ShowInHelp = true
            });

            pluginUi = new PluginUI();
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
            PluginInterface.UiBuilder.Draw += Draw;
        }

        /// <summary>
        /// Loads all assets and data required for the functioning of the plugin.
        /// </summary>
        public void LoadPlugin() {
            Jobs.LoadJobs();
            Spells.LoadSpells();
            Buffs.LoadBuffs();
            TextureDictionary.InitialiseAll();
            Client.Initialise();
        }

        public void ToggleConfig(string command, string argument) => ToggleConfig();

        public void ToggleConfig() {
            pluginUi.Toggle();
        }

        public static float RunTime => (float)PluginInterface.LoadTimeDelta.TotalSeconds;

        private static ImGuiWindowFlags AuraDisplayFlags {
            get {
                return ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMouseInputs;
            }
        }

        private void Draw() {
            // Don't attempt to draw if the local player doesn't exist as it's needed for almost every aura.
            if(ClientState.LocalPlayer == null) {
                return;
            }

            // TODO: Move this somewhere and make it better.
            List<string> interfaces = new() { "AreaMap", "ContentsFinder", "JournalDetail" };
            for(int i = 0; i < interfaces.Count; i++) {
                var addon = GameGui.GetAddonByName(interfaces[i], 1);
                if(addon != IntPtr.Zero) {
                    unsafe {
                        bool isVisible = ((AtkUnitBase*)addon)->IsVisible;
                        if(isVisible) {
                            return;
                        }
                    }
                }
            }
            

            var viewport = ImGuiHelpers.MainViewport.Size;
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(viewport);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.Begin(Auralyte.Id, AuraDisplayFlags);
            ImGui.PopStyleVar();

            ImGuiEx.IncrementAnimationTimer();

            // Draw each aura.
            foreach(Aura aura in Auralyte.Config.auras) {
                ImGui.SetCursorPos(new Vector2(0, 0));
                aura.Draw();
            }
            ImGui.End();

            pluginUi.Draw();
        }

        public static void Debug(string message) => PluginLog.Debug(message);

        public void Dispose() {
            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
            PluginInterface.UiBuilder.Draw -= Draw;
            PluginInterface.Dispose();
            CommandManager.RemoveHandler("/auralyte");
            GC.SuppressFinalize(this);
        }
    }
}
