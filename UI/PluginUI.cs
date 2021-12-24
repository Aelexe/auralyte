using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface;
using Auralyte.Configuration;

namespace Auralyte.UI {

    /// <summary>
    /// Plugin UI is the initial interface the user accesses the configuration of the plugin through.
    /// </summary>
    public class PluginUI : IDisposable {
        // TODO: Remove before release.
#if DEBUG
        public bool open = true;
#else
        public bool open = false;
#endif

        private readonly AurasUI configsUi = new(ref Auralyte.Config.auras);
        private AuraUI configUi;
        public static Aura auraCopy;
        public static Aura auraToDelete;

        public PluginUI() {
            LoadFromConfig();
        }

        private void LoadFromConfig() {
        }

        public void Dispose() {
        }

        public void Open() { open = true; }

        public void Toggle() { open = !open; }

        public void Draw() {
            if(configsUi.auraUi != null) {
                configUi = configsUi.auraUi;
                configsUi.auraUi = null;
            }

            if(configUi != null) {
                configUi.Draw();
                if(!configUi.open) {
                    configUi = null;
                }
            } else {
                DrawConfigurationUi();
            }
        }

        private void DrawConfigurationUi() {
            if(!open) {
                return;
            }

            ImGui.SetNextWindowSizeConstraints(new Vector2(588, 500), ImGuiHelpers.MainViewport.Size);
            ImGui.Begin("Auralyte Configuration", ref open);

            configsUi.Draw();

            ImGui.End();
        }
    }
}
