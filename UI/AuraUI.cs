using System.Numerics;
using ImGuiNET;
using Dalamud.Interface;
using Auralyte.UI;
using Auralyte.Game;
using System;
using System.Collections.Generic;
using Auralyte.Configuration;
using static Auralyte.UI.PluginUI;

namespace Auralyte.UI {

    /// <summary>
    /// Configuration user interface for an aura.
    /// </summary>
    public class AuraUI : IPage {
        private static readonly float BUTTON_WIDTH = 110;
        private static readonly float WINDOW_INDENT = 30;
        private static readonly float CONDITION_RELATION_WIDTH = 60;
        private static readonly float CONDITION_TYPE_WIDTH = 80;
        private static readonly float CONDITION_TYPE2_WIDTH = 100;
        private static readonly float CONDITION_LOGIC_WIDTH = 60;

        private readonly Aura aura;
        private AurasUI aurasUi;

        // State
        public float width;

        private AuraPageLoader LoadAuraPage;

        public AuraUI(Aura aura, AuraPageLoader loadPage) {
            this.aura = aura;
            this.LoadAuraPage = loadPage;
        }

        public void Dispose() {
        }

        public void Draw() {
            width = ImGui.GetWindowWidth();
            ImGui.Columns(2, "Whatever", false);
            ImGui.SetColumnWidth(0, width * 0.1f);
            ImGui.SetColumnWidth(1, width * 0.5f);

            ImGui.Text("ID");
            ImGui.NextColumn();
            ImGui.Text($"{(aura.id >= 0 ? $"#{aura.id}" : "N/A")}");
            ImGui.NextColumn();

            ImGui.Text("Name");
            ImGui.NextColumn();
            ImGui.InputText("", ref aura.name, 32);
            ImGui.NextColumn();

            ImGui.Text("Position");
            ImGui.NextColumn();
            ImGui.PushItemWidth(ImGui.GetColumnWidth() / 3 - 8);
            ImGui.InputFloat($"##config{aura.id} x", ref aura.position.X, 0, 0, "%g");
            ImGui.SameLine();
            ImGui.InputFloat($"##config{aura.id} y", ref aura.position.Y, 0, 0, "%g");
            ImGui.PopItemWidth();
            ImGui.NextColumn();

            ImGui.Columns(1);
            ImGui.Text("Type");
            if(ImGui.RadioButton("Aura", aura.type == Aura.Type.Aura)) {
                aura.type = Aura.Type.Aura;
            }
            ImGui.SameLine();
            if(ImGui.RadioButton("Group", aura.type == Aura.Type.Group)) {
                aura.type = Aura.Type.Group;
            }

            ImGui.Separator();

            ImGui.BeginTabBar("Aura Tabs", ImGuiTabBarFlags.AutoSelectNewTabs);

            if(aura.type == Aura.Type.Aura) {
                DrawPropertiesTab();
            } else if(aura.type == Aura.Type.Group) {
                DrawAurasTab();
            }

            DrawConditionsTab();

            ImGui.EndTabBar();
            ImGui.Separator();

            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
            if(ImGui.Button("Save", new Vector2(BUTTON_WIDTH, 23))) {
                Auralyte.Config.Save();

                LoadAuraPage(aura.parentAura);
            }
        }

        private void DrawPropertiesTab() {
            if(ImGui.BeginTabItem($"Properties##{aura.id}")) {
                List<PropertySet> propertySets = aura.propertySets;

                PropertySet toDelete = null;

                propertySets.ForEach((propertySet) => {
                    if(DrawPropertySet(propertySet)) {
                        toDelete = propertySet;
                    }
                });

                if(toDelete != null) {
                    propertySets.Remove(toDelete);
                    aura.Reindex();
                }

                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
                if(ImGui.Button("Add Set", new Vector2(BUTTON_WIDTH, 23))) {
                    PropertySet propertySet = new() { id = propertySets.Count + 1 };
                    propertySet.properties.Add(new() { id = 1 });
                    propertySets.Add(propertySet);
                }
                ImGui.EndTabItem();
            }
        }

        private bool DrawPropertySet(PropertySet propertySet) {
            List<Property> properties = propertySet.properties;
            Property propertyToDelete = null;

            properties.ForEach((property) => {
                DrawProperty(property, propertySet);

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
                if(ImGui.Button($"Delete##ps{propertySet.id}.p{property.id}", new Vector2(BUTTON_WIDTH, 23))) {
                    propertyToDelete = property;
                }
            });

            if(propertyToDelete != null) {
                properties.Remove(propertyToDelete);
                propertySet.Reindex();
            }

            ImGui.SetCursorPosX(WINDOW_INDENT);
            if(ImGui.Button($"Add Property##{propertySet.id}", new Vector2(BUTTON_WIDTH, 23))) {
                Property newProperty = new() { id = properties.Count + 1 };
                properties.Add(newProperty);
            }

            List<Condition> conditions = propertySet.conditions;
            Condition conditionToDelete = null;

            ImGui.Columns(2, $"##ps#{propertySet.id}.conditions", false);
            float width = ImGui.GetWindowContentRegionWidth();
            ImGui.SetColumnWidth(0, 30);
            ImGui.SetColumnWidth(1, width  - 30);
            conditions.ForEach((condition) => {
                DrawCondition(condition, $"ps#{propertySet.id}");

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
                if(ImGui.Button($"Delete##ps{propertySet.id}.c{condition.id}", new Vector2(BUTTON_WIDTH, 23))) {
                    conditionToDelete = condition;
                }
            });
            ImGui.Columns();

            if(conditionToDelete != null) {
                conditions.Remove(conditionToDelete);
                propertySet.Reindex();
            }

            ImGui.SetCursorPosX(WINDOW_INDENT);
            if(ImGui.Button($"Add Condition##{propertySet.id}", new Vector2(BUTTON_WIDTH, 23))) {
                Condition newCondition = new() { id = conditions.Count + 1 };
                conditions.Add(newCondition);
            }

            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
            bool delete = ImGui.Button($"Delete Set##ps{propertySet.id}", new Vector2(BUTTON_WIDTH, 23));

            ImGui.Separator();

            return delete;
        }

        private void DrawProperty(Property property, PropertySet parentSet) {
            ImGui.Text("Set");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 8);
            DrawEnumCombo($"##ps{parentSet.id}.p#{property.id}.type", ref property.type, Property.GetTypes());

            if(property.type == -1) {
                return;
            }

            if(property.type == (int)Property.Type.Icon) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                DrawSpellTypeahead($"##ps#{parentSet.id}.p#{property.id}.input", ref property.input, ref property.uintValue);
            } else if(property.type == (int)Property.Type.Size) {
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                ImGui.InputText($"##ps#{parentSet.id}.p#{property.id}.input", ref property.value, 32);
            } else if(property.type == (int)Property.Type.Timer) {
                ImGui.SameLine();

                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 6);
                DrawEnumCombo($"##ps{parentSet.id}.p#{property.id}.type2", ref property.type2, Property.GetTimerTypes());

                ImGui.SameLine();

                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                switch((Property.TimerSubType)property.type2) {
                    case Property.TimerSubType.Cooldown:
                    case Property.TimerSubType.ChargeCooldown:
                        DrawSpellTypeahead($"##ps#{parentSet.id}.p#{property.id}.input", ref property.input, ref property.uintValue);
                        break;
                    case Property.TimerSubType.Buff:
                        DrawBuffTypeahead($"##ps#{parentSet.id}.p#{property.id}.input", ref property.input, ref property.uintValue);
                        break;
                }
            } else if(property.type == (int)Property.Type.Effect) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                DrawEnumCombo($"##ps{parentSet.id}.p#{property.id}.type2", ref property.type2, Property.GetEffectTypes());

                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                switch((Property.EffectType)property.type2) {
                    case Property.EffectType.Glow:
                        DrawEnumCombo($"##ps#{parentSet.id}.p#{property.id}.input", ref property.intValue, Property.GetGlowTypes());
                        break;
                    case Property.EffectType.Spinner:
                        DrawEnumCombo($"##ps#{parentSet.id}.p#{property.id}.input", ref property.intValue, Property.GetSpinnerTypes());
                        break;
                    case Property.EffectType.Greyscale:
                        DrawEnumCombo($"##ps#{parentSet.id}.p#{property.id}.input", ref property.intValue, Property.GetGreyscaleTypes());
                        break;
                    case Property.EffectType.Darken:
                        DrawEnumCombo($"##ps#{parentSet.id}.p#{property.id}.input", ref property.intValue, Property.GetDarkenTypes());
                        break;
                }
            }
        }

        private void DrawCondition(Condition condition, string parentId) {
            if(condition.id == 1) {
                ImGui.Text("If");
            } else {
                ImGui.Text("And");
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(CONDITION_TYPE_WIDTH);

            DrawEnumCombo($"##{parentId}.c#{condition.id}.type", ref condition.type, Condition.GetTypes());

            if(condition.type == -1) {
                // Do nothing.
            } else if(condition.type == (int)Condition.Type.Player) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(CONDITION_TYPE2_WIDTH);
                DrawEnumCombo($"##{parentId}.c#{condition.id}.type2", ref condition.type2, Condition.GetPlayerTypes());

                if(condition.type2 == (int)Condition.PlayerType.Job) {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(CONDITION_LOGIC_WIDTH);
                    DrawEnumCombo($"##{parentId}.c#{condition.id}.logic", ref condition.logic, Condition.GetLogicIsTypes());

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 8);
                    if(ImGuiEx.TypeaheadInput($"##{parentId}.c#{condition.id}.value", ref condition.input, Jobs.GetAllJobNames(), 32)) {
                        Job job = Jobs.GetJobByName(condition.input);
                        if(job != null) {
                            condition.uintValue = job.id;
                        } else {
                            condition.uintValue = 0;
                        }
                    }
                } else if(condition.type2 == (int)Condition.PlayerType.Role) {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(CONDITION_LOGIC_WIDTH);
                    DrawEnumCombo($"##{parentId}.c#{condition.id}.logic", ref condition.logic, Condition.GetLogicIsTypes());

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 8);

                    int.TryParse(condition.value, out int roleId);
                    if(ImGui.BeginCombo($"##{parentId}.c#{condition.id}.value", ((Role)roleId).ToString())) {
                        foreach(Role role in Roles.GetAllRoles()) {
                            if(ImGui.Selectable(role.GetName(), roleId == (int)role)) {
                                condition.intValue = (int)role;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
            } else if(condition.type == (int)Condition.Type.Spell) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(CONDITION_TYPE2_WIDTH);
                DrawEnumCombo($"##{parentId}.c#{condition.id}.type2", ref condition.type2, Condition.GetSpellTypes());

                if(condition.type2 == (int)Condition.SpellType.Known) {
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                    DrawSpellTypeahead($"##{parentId}.c#{condition.id}.value", ref condition.input, ref condition.uintValue);

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(CONDITION_LOGIC_WIDTH);
                    DrawEnumCombo($"##{parentId}.c#{condition.id}.logic", ref condition.logic, Condition.GetLogicIsTypes());

                    ImGui.SameLine();
                    ImGui.Text("known");
                } else if(condition.type2 == (int)Condition.SpellType.Cooldown) {
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                    DrawSpellTypeahead($"##{parentId}.c#{condition.id}.value", ref condition.input, ref condition.uintValue);

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(CONDITION_LOGIC_WIDTH);
                    DrawEnumCombo($"##{parentId}.c#{condition.id}.logic", ref condition.logic, Condition.GetLogicIsTypes());

                    ImGui.SameLine();
                    ImGui.Text("on cooldown");
                }
            } else if(condition.type == (int)Condition.Type.Status) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                if(ImGui.BeginCombo($"##{parentId}.c#{condition.id}.type2", ((Condition.StatusType)condition.type2).GetName())) {
                    foreach(Condition.StatusType type in Enum.GetValues(typeof(Condition.StatusType))) {
                        if(ImGui.Selectable(type.GetName(), (int)type == condition.type2)) {
                            condition.type2 = (int)type;
                        }
                    }
                    ImGui.EndCombo();
                }

                if(condition.type2 == (int)Condition.StatusType.Active) {
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                    DrawBuffTypeahead($"##{parentId}.c#{condition.id}.value", ref condition.input, ref condition.uintValue);

                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 4);
                    ImGui.SameLine();
                    DrawEnumCombo($"##{parentId}.c#{condition.id}.logic", ref condition.logic, Condition.GetLogicIsTypes());

                    ImGui.SameLine();
                    ImGui.Text("active");
                }
            }
            ImGui.NextColumn();
        }

        private void DrawAurasTab() {
            if(aurasUi == null) {
                aurasUi = new AurasUI(ref aura.auras, LoadAuraPage);
            }

            if(ImGui.BeginTabItem($"Auras##{aura.id}")) {
                aurasUi.Draw();

                ImGui.EndTabItem();
            }
        }

        private void DrawConditionsTab() {
            if(ImGui.BeginTabItem($"Conditions##{aura.id}")) {
                List<Condition> conditions = aura.conditions;
                Condition conditionToDelete = null;

                conditions.ForEach((condition) => {
                    DrawCondition(condition, $"conf#{aura.id}");

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - BUTTON_WIDTH);
                    if(ImGui.Button($"Delete##conf{aura.id}.c{condition.id}", new Vector2(BUTTON_WIDTH, 23))) {
                        conditionToDelete = condition;
                    }
                });

                if(conditionToDelete != null) {
                    conditions.Remove(conditionToDelete);
                    aura.Reindex();
                }

                ImGui.SetCursorPosX(WINDOW_INDENT);
                if(ImGui.Button($"Add Condition##conf{aura.id}", new Vector2(BUTTON_WIDTH, 23))) {
                    Condition newCondition = new() { id = conditions.Count + 1 };
                    conditions.Add(newCondition);
                }

                ImGui.EndTabItem();
            }
        }

        private void DrawEnumCombo(string id, ref int value, Dictionary<int, string> type) {
            if(!type.TryGetValue(value, out string displayValue)) {
                displayValue = "";
            }

            if(ImGui.BeginCombo(id, displayValue)) {
                foreach(int key in type.Keys) {
                    if(!type.TryGetValue(key, out string keyDisplayValue)) {
                        keyDisplayValue = "";
                    }

                    if(ImGui.Selectable(keyDisplayValue, key == value)) {
                        value = key;
                    }
                }
                ImGui.EndCombo();
            }
        }

        private void DrawSpellTypeahead(string id, ref string input, ref uint output) {
            if(ImGuiEx.TypeaheadInput(id, ref input, Spells.GetAllSpellNames(), 32)) {
                Spell spell = Spells.GetSpellByNameOrId(input);
                if(spell != null) {
                    output = spell.id;
                } else {
                    output = 0;
                }
            }
        }

        private void DrawBuffTypeahead(string id, ref string input, ref uint output) {
            if(ImGuiEx.TypeaheadInput(id, ref input, Buffs.GetAllBuffNames(), 32)) {
                Status buff = Buffs.GetBuffByIdOrName(input);
                if(buff != null) {
                    output = buff.id;
                } else {
                    output = 0;
                }
            }
        }
    }
}
