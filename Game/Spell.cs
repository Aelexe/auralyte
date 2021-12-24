using Dalamud.Game.ClientState;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Auralyte.Game {

    /// <summary>
    /// A spell (action), called such to avoid conflicts with System.Action.
    /// </summary>
    public class Spell {
        public enum Type : byte {
            None = 0,
            Spell = 1,
            General = 5
        }

        private enum SpellState {
            Ready = 0,
            InvalidTarget = 572,
            Unknown = 573,
            Busy = 579,
            Casting = 580,
            Using = 1262,
        }

        public uint id;
        public Type type;
        public string name;
        public int iconId;
        public int maxCharges;
        private bool wasKnown;

        public bool IsKnown() {
            uint state = Client.GetState((byte)type, id);

            switch((SpellState)state) {
                case SpellState.Ready:
                case SpellState.InvalidTarget:
                    wasKnown = true;
                    return true;
                case SpellState.Unknown:
                    return false;
                case SpellState.Busy:
                case SpellState.Casting:
                case SpellState.Using:
                default:
                    return wasKnown;
            }
        }

        public float GetElapsedCooldown() {
            return Client.GetRecastTimeElapsed((byte)type, id);
        }

        public float GetMaxCooldown() {
            return Client.GetRecastTime((byte)type, id);
        }

        public float GetElapsedChargeCooldown() {
            return GetElapsedCooldown() % GetChargeCooldown();
        }

        public float GetChargeCooldown() {
            return GetMaxCooldown() / GetMaxCharges();
        }

        public bool IsOnCooldown() {
            if(GetMaxCharges() > 1) {
                return GetCharges() == 0;
            }

            return GetElapsedCooldown() > 0;
        }

        public int GetCharges() {
            if(GetElapsedCooldown() == 0) {
                return GetMaxCharges();
            }

            float cooldownPerCharge = GetMaxCooldown() / GetMaxCharges();

            return (int)Math.Floor(GetElapsedCooldown() / cooldownPerCharge);
        }

        public int GetMaxCharges() {
            int level = Auralyte.ClientState.LocalPlayer.Level;

            if(id == 3614) {
                if(level >= 78) {
                    return 2;
                }
            }

            return maxCharges;
        }
    }

    /// <summary>
    /// A utility class to load and access spells.
    /// </summary>
    public static class Spells {
        private static readonly List<Spell> spells = new();
        private static List<string> spellNames;

        private static readonly List<uint> _generalSpellIds = new() {
            4, // Sprint
            8, // Return
        };

        /// <summary>
        /// Load all spells from the Lumina generated sheets.
        /// </summary>
        public static void LoadSpells() {
            List<Lumina.Excel.GeneratedSheets.Action> loadedActions = Auralyte.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().Where((action) => {
                return action.IsPlayerAction && ((action.ClassJob.Value != null && Jobs.GetJobById(action.ClassJob.Value.RowId) != null) || action.IsRoleAction);
            }).ToList();
            List<Lumina.Excel.GeneratedSheets.GeneralAction> loadedGeneralActions = Auralyte.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GeneralAction>().Where((action) => { return _generalSpellIds.Contains(action.RowId); }).ToList();

            foreach(var loadedAction in loadedActions) {
                Spell newSpell = new() {
                    id = loadedAction.RowId,
                    type = Spell.Type.Spell,
                    name = loadedAction.Name,
                    iconId = Convert.ToInt32(loadedAction.Icon),
                    maxCharges = loadedAction.MaxCharges
                };

                spells.Add(newSpell);
            }

            foreach(var loadedAction in loadedGeneralActions) {
                spells.Add(new() {
                    id = loadedAction.RowId,
                    type = Spell.Type.General,
                    name = loadedAction.Name,
                    iconId = loadedAction.Icon
                });
            }
        }

        public static List<string> GetAllSpellNames() {
            if(spellNames == null) {
                spellNames = spells.Select((spell) => { return spell.name; }).ToList();
                spellNames.Sort((string name1, string name2) => {
                    return name1.Length - name2.Length;
                });
            }
            return spellNames;
        }

        public static Spell GetSpellById(uint id) {
            Spell spell = spells.Find((spell) => { return spell.id == id; });

            return spell;
        }

        public static Spell GetSpellByName(string name) {
            Spell spell = spells.Find((spell) => { return spell.name == name; });

            return spell;
        }

        public static Spell GetSpellByNameOrId(string nameOrId) {
            Spell spell = null;

            if(uint.TryParse(nameOrId, out uint spellId)) {
                spell = GetSpellById(spellId);
            }

            if(spell == null) {
                spell = GetSpellByName(nameOrId);
            }

            return spell;
        }

        public static Spell GetKnownSpellById(uint spellId) {
            spellId = Client.GetAdjustedActionId(spellId);
            Spell spell = GetSpellById(spellId);

            return spell;
        }

        public static Spell GetKnownSpellByName(string name) {
            Spell spell = spells.Find((spell) => { return spell.name == name; });

            if(spell != null) {
                uint adjustedId = Client.GetAdjustedActionId(spell.id);

                if(adjustedId != spell.id) {
                    spell = GetSpellById(adjustedId);
                }
            }

            return spell;
        }

        public static Spell GetKnownSpellByIdOrName(string nameOrId) {
            Spell spell = null;

            if(uint.TryParse(nameOrId, out uint spellId)) {
                spell = GetKnownSpellById(spellId);
            }

            if(spell == null) {
                spell = GetKnownSpellByName(nameOrId);
            }

            return spell;
        }
    }
}
