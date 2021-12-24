using Auralyte.Game;
using Dalamud.Game.ClientState.Statuses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Auralyte.Configuration {
    public class Condition {
        private static Dictionary<int, string> types;
        private static Dictionary<int, string> playerTypes;
        private static Dictionary<int, string> spellTypes;
        private static Dictionary<int, string> statusTypes;
        private static Dictionary<int, string> logicIsTypes;

        public enum Type {
            Player = 1,
            Spell = 2,
            //Cooldown = 3,
            Status = 4
        }
        public enum PlayerType {
            Job = 0,
            Role = 1
        }
        public enum SpellType {
            Known = 0,
            Cooldown = 1
        }
        public enum StatusType {
            Active,
        }
        public enum Logic {
            True,
            False
        }

        public static Dictionary<int, string> GetTypes() {
            if(types == null) {
                types = Util.EnumToDictionary<Type>();
            }

            return types;
        }

        public static Dictionary<int, string> GetPlayerTypes() {
            if(playerTypes == null) {
                playerTypes = Util.EnumToDictionary<PlayerType>();
            }

            return playerTypes;
        }

        public static Dictionary<int, string> GetSpellTypes() {
            if(spellTypes == null) {
                spellTypes = Util.EnumToDictionary<SpellType>();
            }

            return spellTypes;
        }

        public static Dictionary<int, string> GetStatusTypes() {
            if(statusTypes == null) {
                statusTypes = Util.EnumToDictionary<StatusType>();
            }

            return statusTypes;
        }
        public static Dictionary<int, string> GetLogicIsTypes() {
            if(logicIsTypes == null) {
                logicIsTypes = Enum.GetValues(typeof(Condition.Logic)).Cast<Condition.Logic>().ToDictionary(t => (int)(object)t, t => t.GetIsName());
            }

            return logicIsTypes;
        }

        [JsonProperty("id")]            [DefaultValue(-1)]          public int id = -1;
        [JsonProperty("type")]          [DefaultValue(-1)]          public int type = -1;
        [JsonProperty("type2")]         [DefaultValue(-1)]          public int type2 = -1;
        [JsonProperty("logic")]         [DefaultValue(-1)]          public int logic = -1;
        [JsonProperty("value")]         [DefaultValue("")]          public string value = "";
        [JsonProperty("intValue")]      [DefaultValue(-1)]          public int intValue = -1;
        [JsonProperty("uintValue")]     [DefaultValue(0)]           public uint uintValue = 0;
        [JsonIgnore]                                                public string input = "";

        public Condition Clone() {
            Condition clone = new Condition();
            clone.id = id;
            clone.type = type;
            clone.type2 = type2;
            clone.logic = logic;
            clone.value = value;
            clone.intValue = intValue;
            clone.uintValue = uintValue;
            clone.input = input;

            return clone;
        }
        public bool Check() {
            if(type == (int)Condition.Type.Player) {
                if(type2 == (int)Condition.PlayerType.Job) {
                    bool isJob = Auralyte.ClientState.LocalPlayer.ClassJob.Id == uintValue;

                    if(logic == (int)Condition.Logic.False) {
                        isJob = !isJob;
                    }

                    return isJob;
                } else if(type2 == (int)Condition.PlayerType.Role) {
                    bool isRole = Jobs.GetJobById(Auralyte.ClientState.LocalPlayer.ClassJob.Id).IsRole((Role)intValue);

                    if(logic == (int)Condition.Logic.False) {
                        isRole = !isRole;
                    }

                    return isRole;
                }
            } else if(type == (int)Condition.Type.Spell) {
                Spell spell = Spells.GetKnownSpellById(uintValue);

                if(type2 == (int)Condition.SpellType.Known) {
                    bool isKnown = spell?.IsKnown() ?? false;

                    if(logic == (int)Condition.Logic.False) {
                        isKnown = !isKnown;
                    }

                    return isKnown;
                } else if(type2 == (int)Condition.SpellType.Cooldown) {
                    bool isOnCooldown = spell?.IsOnCooldown() ?? false;

                    if(logic == (int)Condition.Logic.False) {
                        isOnCooldown = !isOnCooldown;
                    }

                    return isOnCooldown;
                }
            } else if (type == (int)Condition.Type.Status) {
                if(type2 == (int)Condition.StatusType.Active) {
                    bool statusExists = false;

                    var buff = Buffs.GetBuffById(uintValue);

                    if(buff != null && buff.IsActive()) {
                        statusExists = true;
                    }

                    if(logic == (int)Condition.Logic.False) {
                        statusExists = !statusExists;
                    }

                    return statusExists;
                }
            }

            return true;
        }
    }

    public static class ConditionExtension {
        public static string ToString(this Condition.Type type) {
            switch(type) {
                case Condition.Type.Player:
                    return "Player";
                case Condition.Type.Spell:
                    return "Spell";
                case Condition.Type.Status:
                    return "Status";
                default:
                    return "";
            }
        }

        public static string GetName(this Condition.PlayerType type) {
            switch(type) {
                case Condition.PlayerType.Job:
                    return "Job";
                case Condition.PlayerType.Role:
                    return "Role";
                default:
                    return "";
            }
        }

        public static string GetName(this Condition.SpellType type) {
            switch(type) {
                case Condition.SpellType.Known:
                    return "Known";
                case Condition.SpellType.Cooldown:
                    return "Cooldown";
                default:
                    return "";
            }
        }

        public static string GetName(this Condition.StatusType type) {
            switch(type) {
                case Condition.StatusType.Active:
                    return "Active";
                default:
                    return "";
            }
        }

        public static string GetIsName(this Condition.Logic logic) {
            switch(logic) {
                case Condition.Logic.True:
                    return "is";
                case Condition.Logic.False:
                    return "is not";
                default:
                    return "";
            }
        }

        public static string GetDoesName(this Condition.Logic logic) {
            switch(logic) {
                case Condition.Logic.True:
                    return "does";
                case Condition.Logic.False:
                    return "doesn't";
                default:
                    return "";
            }
        }
    }
}
