using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using System.Linq;

namespace Auralyte.Game {

    /// <summary>
    /// Utility class to help track the max duration of statuses with varying max durations (like Sprint).
    /// </summary>
    public class StatusTime {
        public float lastSeen;
        public float currentMax;
    }

    /// <summary>
    /// A status.
    /// </summary>
    public class Status {
        public uint id;
        public string name;

        public static readonly Dictionary<uint, StatusTime> statusTimes = new();

        private Dalamud.Game.ClientState.Statuses.Status GetStatus() {
            StatusList statusList = Auralyte.ClientState.LocalPlayer.StatusList;
            var status = statusList.FirstOrDefault((status) => {
                return status.StatusId == id;
            });

            return status;
        }

        public float GetRemainingTime() {
            var status = GetStatus();

            if(status == null) {
                return 0;
            }

            if(!statusTimes.TryGetValue(id, out StatusTime statusTime)) {
                statusTime = new StatusTime() { lastSeen = status.RemainingTime, currentMax = status.RemainingTime };
                statusTimes[id] = statusTime;
            } else {
                if(status.RemainingTime > statusTime.lastSeen) {
                    statusTime.currentMax = status.RemainingTime;
                }
                statusTime.lastSeen = status.RemainingTime;
            }

            return statusTime.currentMax - status.RemainingTime;
        }

        /// <summary>
        /// Returns the max duration of a status.<br/>
        /// This value will only be accurate if called in the same frame as <see cref="GetRemainingTime"/>
        /// </summary>
        /// <returns></returns>
        public float GetMaxTime() {
            if(!statusTimes.TryGetValue(id, out StatusTime statusTime)) {
                return 0;
            }

            return statusTime.currentMax;
        }

        /// <summary>
        /// Returns whether the status is currently active on the player.
        /// </summary>
        /// <returns></returns>
        public bool IsActive() {
            return GetStatus() != null;
        }
    }

    /// <summary>
    /// A utility class to load and access buffs.
    /// </summary>
    public static class Buffs {
        private static readonly List<Status> buffs = new();

        /// <summary>
        /// Array of buff to retrieve, as there does not seems to be a way to narrow down Lumina's statuses to only the ones players can apply.
        /// </summary>
        private static readonly uint[] validBuffs = new uint[] {
            //// General
            50, // Sprint

            //// Roles
            // Tank/Melee/Ranged
            1209, // Arms Length

            // Healer/Caster
            167, // Swiftcast
            1204, // Lucid Dreaming
            160, // Surecast
            
            // Tank,
            1191, // Rampart

            // Melee
            84, // Bloodbath
            1250, // True North

            ////// Classes
            //// Tanks
            // Paladin
            76, // Fight or Flight
            79, // Iron Will
            1856, // Sheltron
            74, // Sentinel
            80, // Cover
            82, // Hallowed Ground 
            726, // Divine Veil (Pre-shield)
            727, // Divine Veil (Shield)
            1368, // Requiescat
            1175, // Passage of Arms
            2674, // Holy Sheltron

            // Warrior
            86, // Berserk
            91, // Defiance
            87, // Thrill of Battle 
            89, // Vengeance
            1304, // Holmgang
            1992, // Nascent Chaos
            735, // Raw Intuition
            2681, // Equilibrium
            1457, // Shake it Off
            1177, // Inner Release
            1857, // Nascent Flash
            2678, // Bloodwhetting
            
            // Dark Knight
            743, // Grit
            751, // Darkside
            747, // Shadow Wall
            746, // Dark Mind
            810, // Living Dead
            811, // Walking Dead
            749, // Salted Earth
            1972, // Delirium
            1178, // Blackest Knight
            752, // Dark Arts
            1894, // Dark Missionary
            2682, // Oblation

            // Gunbreaker
            1381, // No Mercy
            1898, // Brutal Shell
            1832, // Camouflage
            392, // Royal Guard
            1834, // Nebula
            1835, // Aurora
            1836, // Superbolide
            1839, // Heart of Light
            1840, // Heart of Stone
            2683, // Heart of Corundum
            2684, // Clarity of Corundum
            2684, // Catharsis of Corundum

            //// Healers
            // White Mage
            157, // Presence of Mind
            158, // Regen
            150, // Medica II
            739, // Asylum
            1217, // Thin Air
            1218, // Divine Benison
            1219, // Confession
            1872, // Temperance
            2708, // Aqua Veil
            2709, // Liturgy of the Bell

            // Scholar
            315, // Whispering Dawn
            297, // Galvanize
            // Succor?
            317, // Fey Illumination
            299, // Sacred Soil,
            792, // Emergency Tactics
            791, // Dissipation
            1220, // Excogiation
            1896, // Recitation
            2710, // Protraction
            2712, // Expedience,
            2711, // Desperate Measures
            
            // Astrologian
            841, // Lightspeed
            835, // Aspected Benefict
            836, // Aspected Helios
            845, // Synastry
            1878, // Divination
            2714, // Harmony of Spirit
            2715, // Harmony of Body
            2716, // Harmony of Mind
            848, // Collective Unconscious (or 2283?)
            956, // Wheel of Fortune
            1879, // Opposition?
            1224, // Earthly Dominance
            1248, // Giant Dominance
            1890, // Horoscope
            1891, // Horoscope Helios
            1892, // Neutral Sect
            2717, // Exaltation
            2718, // Macrocosmos

            // Sage
            2604, // Kardia
            2605, // Kardion
            2617, // Physis
            2620, // Physis II
            2606, // Eukrasia
            2607, // Eukrasian Diagnosis
            2608, // Differential Diagnosis
            2607, // Eukrasian Diagnosis
            2608, // Differential Diagnosis
            2609, // Differential Prognosis
            2610, // Soteria
            2618, // Kerachole
            2611, // Zoe
            2619, // Taurochole
            2612, // Haima
            3003, // Holos?
            2613, // Panhaima
            2643, // Panhaimatinon
            2622, // Krasis

            //// Melee
            // Monk
            3001, // Disciplined Fist
            102, // Mantra
            110, // Perfect Balance
            2513, // Formless Fist
            1179, // Riddle of Earth
            1181, // Riddle of Fire
            1185, // Brotherhood
            1182, // Meditative Brotherhood
            2687, // Riddle of Wind
            2514, // Six-sided Star

            // Dragoon
            116, // Life Surge
            2720, // Power Surge
            802, // Sharper Fang and Claw
            1864, // Lance Charge
            1243, // Dive Ready
            803, // Enhanced Wheeling Thrust
            786, // Battle Littany
            1183, // Right Eye
            1184, // Left Eye
            1863, // Draconian Fire

            // Ninja
            488, // Shade Shift?2011
            1952, // Hide
            496, // Mudra
            501, // Doton
            507, // Suiton
            497, // Kassatsu
            1186, // Ten Chi Jin
            1954, // Bunshin? 2010
            2690, // Forked Raiju Ready
            2691, // Fleeting Raiju Ready

            // Samurai
            1298, // Fugetsu
            1232, // Third Eye
            1299, // Fuka
            1233, // Meikyo Shisui
            1229, // Hissatsu: Kaiten
            1236, // Enhanced Enpi
            1231, // Meditate
            1865, // Meditation?2176
            2959, // Ogi Namikiri Ready

            // Reaper
            2595, // Threshold
            2596, // Crest of Time Borrowed
            2598, // Crest of Time Returned
            2587, // Soul Reaver
            2589, // Enhanced Gallows
            2588, // Enhanced Gibbet
            2599, // Arcane Circle
            2600, // Circle of Sacrifice
            2601, // Bloodsown Circle
            2592, // Immortal Sacrifice
            2593, // Enshrouded
            2592, // Enhanced Cross Reaping
            2590, // Enhanced Void Reaping
            2594, // Soulsow

            //// Ranged
            // Bard
            122, // Straight Shot Ready
            125, // Raging Strikes
            2217, // Mage's Ballad
            866, // The Warden's Paean
            128, // Barrage
            2218, // Army's Paeon
            141, // Battle Voice
            2216, // The Wanderer's Minuet
            1934, // Troubadour
            1202, // Nature's Minne
            3002, // Shadowbite Ready
            2692, // Blast Arrow Ready
            2964, // Radiant Finale

            // Machinist
            851, // Reassemble
            688, // Hypercharge
            1946, // Wildfire
            1951, // Tactician

            // Dancer
            2693, // Flourishing Symmetry
            2694, // Flourishing Flow
            1818, // Standard Step
            1821, // Standard Finish?2105
            1820, // Threefold Fan Dance
            1826, // Shield Samba
            1823, // Closed Position
            1825, // Devilment
            1819, // Technical Step
            1822, // Technical Finish
            1827, // Improvisation
            2696, // Rising Rhythm
            2697, // Improvised Finish

            //// Caster
            // Black Mage
            00000, // Fire Proc
            163, // Thundercloud
            2960, // Enhanced Flare
            168, // Manaward
            737, // Ley Lines
            867, // Sharpcast
            1211, // Triplecast
            165, // Firestarter

            // Summoner
            2702, // Radiant Aegis
            1212, // Further Ruin
            2703, // Searing Light

            // Red Mage
            1249, // Dualcast
            1234, // Verfire Ready
            1235, // Verstone Ready
            1248, // Acceleration
            2282, // Embolden
            1971, // Manaification
            2707, // Magick Barrier
        };

        /// <summary>
        /// Load all buffs from the Lumina generated sheets.
        /// </summary>
        public static void LoadBuffs() {
            var loadedStatuses = Auralyte.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>().Where((status) => {
                return validBuffs.Contains(status.RowId);
            }).ToList();

            foreach(var status in loadedStatuses) {
                buffs.Add(new() {
                    id = status.RowId,
                    name = status.Name
                });
            }
        }

        public static List<string> GetAllBuffNames() {
            return buffs.Select((buff) => { return buff.name; }).ToList();
        }

        public static Status GetBuffById(uint id) {
            Status buff = buffs.Find((buff) => { return buff.id == id; });

            return buff;
        }

        public static Status GetBuffByName(string name) {
            Status buff = buffs.Find((buff) => { return buff.name == name; });

            return buff;
        }

        public static Status GetBuffByIdOrName(string nameOrId) {
            Status buff = null;

            if(uint.TryParse(nameOrId, out uint buffId)) {
                buff = GetBuffById(buffId);
            }

            if(buff == null) {
                buff = GetBuffByName(nameOrId);
            }

            return buff;
        }

        public static Status SearchBuffByName(string partialName) {
            Status buff = buffs.Find((buff) => { return buff.name.StartsWith(partialName); });

            return buff;
        }

    }
}
