using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Auralyte.Game;
using Auralyte.UI;
using ImGuiNET;
using Newtonsoft.Json;

namespace Auralyte.Configuration {
    public class Aura {
        public enum Type {
            Aura,
            Group
        }

        [JsonProperty("id")]            [DefaultValue(-1)]              public int id = -1;
        [JsonProperty("name")]          [DefaultValue("")]              public string name = "";
        [JsonProperty("position")]      [DefaultValue(null)]            public Vector2 position = Vector2.Zero;
        [JsonProperty("type")]          [DefaultValue(Type.Aura)]       public Type type = Type.Aura;
        [JsonProperty("conditions")]    [DefaultValue(null)]            public List<Condition> conditions = new();

        // Aura Configuration
        [JsonProperty("propertySets")] [DefaultValue(null)] public List<PropertySet> propertySets = new();

        // Group Configuration
        [JsonProperty("auras")] [DefaultValue(null)] public List<Aura> auras = new();

        public void Annotate() {
            foreach(Aura aura in auras) {
                aura.Annotate();
            }

            foreach(PropertySet propertySet in propertySets) {
                foreach(Property property in propertySet.properties) {
                    if(property.type == (int)Property.Type.Icon) {
                        AnnotateSpellById(property.uintValue, ref property.input);
                    } else if(property.type == (int)Property.Type.Timer) {
                        if(property.type2 == (int)Property.TimerSubType.Cooldown) {
                            AnnotateSpellById(property.uintValue, ref property.input);
                        } else if(property.type2 == (int)Property.TimerSubType.Buff) {
                            AnnotateBuffById(property.uintValue, ref property.input);
                        }
                    }
                }

                foreach(Condition condition in propertySet.conditions) {
                    if(condition.type == (int)Condition.Type.Spell) {
                        AnnotateSpellById(condition.uintValue, ref condition.input);
                    } else if(condition.type == (int)Condition.Type.Status) {
                        AnnotateBuffById(condition.uintValue, ref condition.input);
                    } 
                }
            }
        }

        private static void AnnotateSpellById(uint spellId, ref string spellName) {
            Spell spell = Spells.GetSpellById(spellId);

            if(spell != null) {
                spellName = spell.name;
            } else {
                spellName = "";
            }
        }

        private static void AnnotateBuffById(uint buffId, ref string buffName) {
            Status buff = Buffs.GetBuffById(buffId);

            if(buff != null) {
                buffName = buff.name;
            } else {
                buffName = "";
            }
        }

        public void Reindex() {
            for(int i = 0; i < auras.Count; i++) {
                auras[i].id = i + 1;
                auras[i].Reindex();
            }

            for(int i = 0; i < propertySets.Count; i++) {
                propertySets[i].id = i + 1;
            }

            for(int i = 0; i < conditions.Count; i++) {
                conditions[i].id = i + 1;
            }
        }

        public Aura Clone() {
            return Clone(false);
        }
        public Aura Clone(bool isRoot) {
            Aura clone = new();
            clone.name = isRoot ? $"{name} clone" : name;
            clone.position = new Vector2(position.X, position.Y);
            clone.type = type;

            clone.conditions = conditions.Select((Condition condition) => { return condition.Clone(); }).ToList();
            clone.auras = auras.Select((aura) => { return aura.Clone(); }).ToList();
            clone.propertySets = propertySets.Select((PropertySet propertySet) => { return propertySet.Clone(); }).ToList();

            return clone;
        }

        public void Draw() {
            if(conditions.Count > 0) {
                for(int i = 0; i < conditions.Count; i++) {
                    Condition condition = conditions[i];
                    if(!condition.Check()) {
                        return;
                    }
                }
            }

            if(type == Aura.Type.Aura) {
                DrawAura();
            } else if(type == Aura.Type.Group) {
                DrawGroup();
            }
        }

        private void DrawGroup() {
            // Get position of the group is its own position offset from the current position.
            Vector2 groupPosition = ImGui.GetCursorPos() + position;

            auras.ForEach((childAura) => {
                // Position the cursor before each child aura.
                ImGui.SetCursorPos(groupPosition);

                childAura.Draw();
            });
        }

        // TODO: Move this somewhere.
        public class StatusTime {
            public float lastSeen;
            public float currentMax;
        }

        public Dictionary<uint, StatusTime> statusTimes = new();

        private void DrawAura() {
            int iconId = -1; // Default failure bunny.
            int size = -1;
            float timer = -1;
            float timerMax = -1;
            int timerType = -1;
            int charges = -1;
            int glowType = -1;
            int spinnerType = -1;
            int greyscaleType = -1;
            int darkenType = -1;

            propertySets.ForEach((propertySet) => {
                // Check property set conditions.
                foreach(Condition condition in propertySet.conditions) {
                    if(!condition.Check()) {
                        return;
                    }
                }

                // Apply properties.
                propertySet.properties.ForEach((property) => {
                    if(property.type == (int)Property.Type.Icon) {
                        // If icon is already set, stop.
                        if(iconId != -1) {
                            return;
                        }

                        Spell spell = Spells.GetKnownSpellById(property.uintValue);

                        if(spell != null) {
                            iconId = spell.iconId;
                        }
                    } else if(property.type == (int)Property.Type.Size) {
                        // If size is already set, stop.
                        if(size != -1) {
                            return;
                        }
                        _ = int.TryParse(property.value, out size);
                    } else if(property.type == (int)Property.Type.Timer) {
                        // If timer is already set, stop.
                        if(timer != -1 || timerMax != -1) {
                            return;
                        }

                        if(property.type2 == (int)Property.TimerSubType.Cooldown) {
                            Spell spell = Spells.GetKnownSpellById(property.uintValue);

                            if(spell != null) {
                                timer = spell.GetElapsedCooldown();
                                timerMax = spell.GetMaxCooldown();
                                timerType = (int)Property.TimerSubType.Cooldown;
                            }
                        } else if(property.type2 == (int)Property.TimerSubType.ChargeCooldown) {
                            Spell spell = Spells.GetKnownSpellById(property.uintValue);

                            if(spell != null) {
                                timer = spell.GetElapsedCooldown() % spell.GetChargeCooldown();
                                timerMax = spell.GetChargeCooldown();
                                charges = spell.GetCharges();
                                if(charges == 0) {
                                    timerType = (int)Property.TimerSubType.Cooldown;
                                } else {
                                    timerType = (int)Property.TimerSubType.ChargeCooldown;
                                }
                            }
                        } else if(property.type2 == (int)Property.TimerSubType.Buff) {
                            // Get the status by ID.
                            Status buff = Buffs.GetBuffById(property.uintValue);

                            if(buff != null && buff.IsActive()) {
                                timer = buff.GetRemainingTime();
                                timerMax = buff.GetMaxTime();
                            }
                        }
                    } else if(property.type == (int)Property.Type.Effect) {
                        if(property.type2 == (int)Property.EffectType.Glow) {
                            // If glow is already set, stop.
                            if(glowType != -1) {
                                return;
                            }
                            glowType = property.intValue;
                        } else if(property.type2 == (int)Property.EffectType.Spinner) {
                            // If spinner is already set, stop.
                            if(spinnerType != -1) {
                                return;
                            }
                            spinnerType = property.intValue;
                        } else if(property.type2 == (int)Property.EffectType.Greyscale) {
                            // If spinner is already set, stop.
                            if(greyscaleType != -1) {
                                return;
                            }
                            greyscaleType = property.intValue;
                        } else if(property.type2 == (int)Property.EffectType.Darken) {
                            // If darken is already set, stop.
                            if(darkenType != -1) {
                                return;
                            }
                            darkenType = property.intValue;
                        }
                    }
                });
            });

            // Set missing defaults.
            if(iconId == -1) {
                iconId = 2914; // Failure bunny.
            }
            if(size == -1) {
                size = 32;
            }
            if(glowType == -1) {
                glowType = (int)Property.GlowType.Off;
            }
            if(spinnerType == -1) {
                spinnerType = (int)Property.SpinnerType.Off;
            }
            if(greyscaleType == -1) {
                greyscaleType = (int)Property.GreyscaleType.Off;
            }
            if(darkenType == -1) {
                darkenType = (int)Property.DarkenType.Off;
            }

            var settings = new ImGuiEx.AuraSettings { size = size, iconId = iconId, timer = timer, timerMax = timerMax, timerType = timerType, glowType = glowType, spinnerType = spinnerType, greyscaleType = greyscaleType, darkenType = darkenType, charges = charges };

            ImGui.SetCursorPos(ImGui.GetCursorPos() + position);
            ImGui.Dummy(new Vector2(settings.size, settings.size));
            ImGui.GetWindowDrawList().DrawAura(ImGui.GetItemRectMin(), settings);
        }
    }
}
