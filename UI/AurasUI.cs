using Auralyte.Configuration;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using static Auralyte.UI.PluginUI;

namespace Auralyte.UI {

    /// <summary>
    /// Configuration user interface for a list of auras.
    /// </summary>
    public class AurasUI : IPage {
        private readonly List<Aura> auras;
        public IPage parentPage { get; set; }

        // State
        public float width;
        public bool open = true;

        private AuraPageLoader LoadAuraPage;

        public AurasUI(ref List<Aura> auras, AuraPageLoader loadPage) {
            this.auras = auras;
            this.LoadAuraPage = loadPage;
        }

        public void Draw() {
            ImGui.Columns(3, "Auralyte Auras", false);
            float width = ImGui.GetWindowContentRegionWidth();
            ImGui.SetColumnWidth(0, width * 0.1f);
            ImGui.SetColumnWidth(1, width * 0.65f);
            ImGui.SetColumnWidth(2, width * 0.25f);

            ImGui.Separator();

            ImGui.Text("ID");
            ImGui.NextColumn();
            ImGui.Text("Name");
            ImGui.NextColumn();
            ImGui.Text("Actions");
            ImGui.NextColumn();

            ImGui.Separator();
            Aura toShiftUp = null;
            Aura toShiftDown = null;
            Aura toDelete = null;

            foreach(Aura aura in auras) {
                ImGui.PushID($"config#{aura.id}");
                ImGui.Text($"#{aura.id}");
                ImGui.NextColumn();
                ImGui.Text(aura.name);

                ImGui.NextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if(ImGui.Button($"{FontAwesomeIcon.ArrowUp.ToIconString()}##Config{aura.id}")) {
                    toShiftUp = aura;
                }
                ImGui.PopFont();

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if(ImGui.Button($"{FontAwesomeIcon.ArrowDown.ToIconString()}##Config{aura.id}")) {
                    toShiftDown = aura;
                }
                ImGui.PopFont();

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if(ImGui.Button($"{FontAwesomeIcon.PencilAlt.ToIconString()}##Config{aura.id}")) {
                    Auralyte.Config.Save();
                    LoadAuraPage(aura);
                }
                ImGui.PopFont();
                ImGuiEx.SetItemTooltip("Left click to edit.\nRight click to copy.", ImGuiHoveredFlags.None);
                if(ImGui.IsItemHovered()) {
                    if(ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        PluginUI.auraCopy = aura.Clone(true);
                    }
                };

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if(ImGui.Button($"{FontAwesomeIcon.TrashAlt.ToIconString()}##Config{aura.id}")) {
                    toDelete = aura;
                }
                ImGui.PopFont();
                ImGuiEx.SetItemTooltip("Right click and then left click to delete.", ImGuiHoveredFlags.None);
                if(ImGui.IsItemHovered()) {
                    if(ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        PluginUI.auraToDelete = aura;
                    }
                };
                ImGui.Separator();
                ImGui.PopID();
                ImGui.NextColumn();
            }

            if(toShiftUp != null) {
                int index = auras.IndexOf(toShiftUp);
                if(index >= 1) {
                    auras.Remove(toShiftUp);
                    auras.Insert(index - 1, toShiftUp);
                }

                Auralyte.Config.Save();
            }
            if(toShiftDown != null) {
                int index = auras.IndexOf(toShiftDown);
                if(index >= 0 && index < auras.Count - 1) {
                    auras.Remove(toShiftDown);
                    auras.Insert(index + 1, toShiftDown);
                }

                Auralyte.Config.Save();
            } else if(toDelete != null) {
                if(toDelete == PluginUI.auraToDelete) {
                    auras.Remove(toDelete);

                    Auralyte.Config.Save();
                }
                PluginUI.auraToDelete = null;
            }

            ImGui.NextColumn();
            ImGui.NextColumn();
            if(ImGui.Button("Create##Aura", new Vector2(ImGui.GetColumnWidth() - (ImGui.GetStyle().ItemSpacing.X * 2), ImGui.GetFrameHeight()))) {
                Aura newAura = new() { id = auras.Count + 1, name = "", position = Vector2.Zero };
                auras.Add(newAura);
                Auralyte.Config.Save();
                LoadAuraPage(newAura);
            }
            ImGuiEx.SetItemTooltip("Left click to create.\nRight click to create from copy.", ImGuiHoveredFlags.None);
            if(ImGui.IsItemHovered()) {
                if(ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                    if(PluginUI.auraCopy != null) {
                        Aura newAura = PluginUI.auraCopy;
                        PluginUI.auraCopy = null;
                        newAura.id = auras.Count + 1;
                        auras.Add(newAura);
                        Auralyte.Config.Save();
                        LoadAuraPage(newAura);
                    }
                }
            };

            ImGui.Columns();
        }
        public void Dispose() {
        }
    }
}
