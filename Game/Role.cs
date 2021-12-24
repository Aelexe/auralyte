using System;
using System.Collections.Generic;
using System.Linq;

namespace Auralyte.Game {
    public enum Role {
        Tank = 0,
        Healer = 1,
        Melee = 2,
        Ranged = 3,
        Caster = 4
    }

    public static class Roles {
        private static List<Role> roles;
        private static List<String> roleNames;

        public static List<Role> GetAllRoles() {
            if(roles == null) {
                roles = new();

                foreach(Role role in Enum.GetValues(typeof(Role))) {
                    roles.Add(role);
                }
            }

            return roles;
        }

        public static List<string> GetAllRoleNames() {
            if(roleNames == null) {
                roleNames = new();

                foreach(Role role in Enum.GetValues(typeof(Role))) {
                    roleNames.Add(role.GetName());
                }
            }

            return roleNames;
        }

        public static string GetName(this Role role) {
            switch(role) {
                case Role.Tank:
                    return "Tank";
                case Role.Healer:
                    return "Healer";
                case Role.Melee:
                    return "Melee";
                case Role.Ranged:
                    return "Ranged";
                case Role.Caster:
                    return "Caster";
                default:
                    return "";
            }
        }
    }
}
