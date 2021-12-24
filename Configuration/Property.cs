using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Auralyte.Configuration {

    /// <summary>
    /// A property of an <see cref="Aura"/>.
    /// </summary>
    public class Property {
        private static Dictionary<int, string> types;
        private static Dictionary<int, string> timerTypes;
        private static Dictionary<int, string> effectTypes;
        private static Dictionary<int, string> glowTypes;
        private static Dictionary<int, string> spinnerTypes;
        private static Dictionary<int, string> greyscaleTypes;
        private static Dictionary<int, string> darkenTypes;

        public enum Type {
            Icon = 0,
            Size = 1,
            Timer = 2,
            Effect = 3,
        }
        public enum TimerSubType {
            Cooldown = 0,
            Buff = 1,
            ChargeCooldown = 2
        }
        public enum EffectType {
            Glow = 0,
            Spinner = 1,
            Greyscale = 2,
            Darken = 3
        }
        public enum GlowType {
            On = 0,
            Off = 1,
        }
        public enum SpinnerType {
            On = 0,
            Reverse = 1,
            Off = 2,
        }
        public enum GreyscaleType {
            On = 0,
            OnIconOnly = 1,
            Off = 2,
        }
        public enum DarkenType {
            Dark = 0,
            Darker = 1,
            Darkest = 2,
            Off = 3
        }

        public static Dictionary<int, string> GetTypes() {
            if(types == null) {
                types = Util.EnumToDictionary<Type>();
            }

            return types;
        }

        public static Dictionary<int, string> GetTimerTypes() {
            if(timerTypes == null) {
                timerTypes = Util.EnumToDictionary<TimerSubType>();
            }

            return timerTypes;
        }

        public static Dictionary<int, string> GetEffectTypes() {
            if(effectTypes == null) {
                effectTypes = Util.EnumToDictionary<EffectType>();
            }

            return effectTypes;
        }

        public static Dictionary<int, string> GetGlowTypes() {
            if(glowTypes == null) {
                glowTypes = Util.EnumToDictionary<GlowType>();
            }

            return glowTypes;
        }

        public static Dictionary<int, string> GetSpinnerTypes() {
            if(spinnerTypes == null) {
                spinnerTypes = Util.EnumToDictionary<SpinnerType>();
            }

            return spinnerTypes;
        }

        public static Dictionary<int, string> GetGreyscaleTypes() {
            if(spinnerTypes == null) {
                spinnerTypes = Util.EnumToDictionary<GreyscaleType>();
            }

            return spinnerTypes;
        }

        public static Dictionary<int, string> GetDarkenTypes() {
            if(darkenTypes == null) {
                darkenTypes = Util.EnumToDictionary<DarkenType>();
            }

            return darkenTypes;
        }

        [JsonProperty("id")]                                public int id = -1;
        [JsonProperty("type")]          [DefaultValue(-1)]  public int type = -1;
        [JsonProperty("type2")]         [DefaultValue(-1)]  public int type2 = -1;
        [JsonProperty("value")]         [DefaultValue("")]  public string value = "";
        [JsonProperty("intValue")]      [DefaultValue(-1)]  public int intValue = -1;
        [JsonProperty("uintValue")]     [DefaultValue(0)]   public uint uintValue = 0;
        [JsonIgnore]                                        public string input = "";

        public Property Clone() {
            Property clone = new Property();
            clone.id = id;
            clone.type = type;
            clone.type2 = type2;
            clone.value = value;
            clone.intValue = intValue;
            clone.uintValue = uintValue;
            clone.input = input;

            return clone;
        }

    }
    public static class PropertyExtension {
        public static string ToString(this Property.Type type) {
            switch(type) {
                case Property.Type.Icon:
                    return "Icon";
                case Property.Type.Size:
                    return "Size";
                case Property.Type.Timer:
                    return "Timer";
                case Property.Type.Effect:
                    return "Effect";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.TimerSubType type) {
            switch(type) {
                case Property.TimerSubType.Cooldown:
                    return "Cooldown";
                case Property.TimerSubType.ChargeCooldown:
                    return "Charge Cooldown";
                case Property.TimerSubType.Buff:
                    return "Buff";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.EffectType type) {
            switch(type) {
                case Property.EffectType.Glow:
                    return "Glow";
                case Property.EffectType.Spinner:
                    return "Spinner";
                case Property.EffectType.Greyscale:
                    return "Greyscale";
                case Property.EffectType.Darken:
                    return "Darken";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.GlowType type) {
            switch(type) {
                case Property.GlowType.On:
                    return "On";
                case Property.GlowType.Off:
                    return "Off";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.SpinnerType type) {
            switch(type) {
                case Property.SpinnerType.Off:
                    return "Off";
                case Property.SpinnerType.On:
                    return "On";
                case Property.SpinnerType.Reverse:
                    return "On (Reverse)";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.GreyscaleType type) {
            switch(type) {
                case Property.GreyscaleType.Off:
                    return "Off";
                case Property.GreyscaleType.On:
                    return "On";
                case Property.GreyscaleType.OnIconOnly:
                    return "On (Icon Only)";
                default:
                    return "";
            }
        }

        public static string ToString(this Property.DarkenType type) {
            switch(type) {
                case Property.DarkenType.Off:
                    return "Off";
                case Property.DarkenType.Dark:
                    return "Dark";
                case Property.DarkenType.Darker:
                    return "Darker";
                case Property.DarkenType.Darkest:
                    return "Darkest";
                default:
                    return "";
            }
        }
    }
}
