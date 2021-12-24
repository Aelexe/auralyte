using System;
using System.Collections.Generic;
using System.Linq;

namespace Auralyte.Configuration {
    public static class Util {
        public static Dictionary<int, string> EnumToDictionary<T>() {
            if(!typeof(T).IsEnum) {
                throw new ArgumentException("Type must be an enum");
            }
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(t => (int)(object)t, t => t.ToString());
        }
    }
}
