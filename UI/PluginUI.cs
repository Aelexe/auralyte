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

        private readonly AurasUI rootAurasUi;

        public static Aura auraCopy;
        public static Aura auraToDelete;
        private IPage currentPage;

        public delegate void AuraPageLoader(Aura aura = null);

        public PluginUI() {
            rootAurasUi = new(ref Auralyte.Config.auras, new AuraPageLoader(LoadAuraPage));
            currentPage = rootAurasUi;
        }

        public void Dispose() {
        }

        public void Open() {
            open = true;
        }

        public void Toggle() {
            open = !open;
            if(open) {
                currentPage = rootAurasUi;
            }
        }

        private void LoadAuraPage(Aura aura = null) {
            if(aura != null) {
                currentPage = new AuraUI(aura, LoadAuraPage);
            } else {
                currentPage = rootAurasUi;
            }
        }

        public void Draw() {
            if(open && currentPage != null) {
                ImGui.SetNextWindowSizeConstraints(new Vector2(588, 500), ImGuiHelpers.MainViewport.Size);
                ImGui.Begin("Auralyte Configuration", ref open);

                currentPage.Draw();

                ImGui.End();
            }
        }
    }

    public interface IPage : IDisposable {
        void Draw();
    }
}
