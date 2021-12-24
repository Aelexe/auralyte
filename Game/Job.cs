using System.Collections.Generic;
using System.Linq;

namespace Auralyte.Game {

    /// <summary>
    /// A job.
    /// </summary>
    public class Job {
        public uint id;
        public string name;

        /// <summary>
        /// Returns whether a job matches the specified role.
        /// </summary>
        /// <param name="role">Role to check for.</param>
        /// <returns>Whether the job is that role.</returns>
        public bool IsRole(Role role) {
            switch(role) {
                case Role.Tank:
                    return id == 19 || id == 21 || id == 32 || id == 37;
                case Role.Healer:
                    return id == 24 || id == 28 || id == 33 || id == 40;
                case Role.Melee:
                    return id == 20 || id == 22 || id == 30 || id == 34 || id == 39;
                case Role.Ranged:
                    return id == 23 || id == 31 || id == 38;
                case Role.Caster:
                    return id == 25 || id == 27 || id == 35 || id == 36;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// A utility class to load and access jobs.
    /// </summary>
    public static class Jobs {
        private static List<Job> jobs = new List<Job>();

        /// <summary>
        /// Array of jobs to retrieve, to limit the scope of Auralyte (for the time being at least).
        /// </summary>
        private static uint[] validJobs = new uint[] { 1, 2, 3, 4, 5, 6, 7, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40 };

        /// <summary>
        /// Load all jobs from the Lumina generated sheets.
        /// </summary>
        public static void LoadJobs() {
            List<Lumina.Excel.GeneratedSheets.ClassJob> loadedJobs = Auralyte.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().Where((job) => { return validJobs.Contains(job.RowId); }).ToList();

            foreach(var loadedJob in loadedJobs) {
                jobs.Add(new() {
                    id = loadedJob.RowId,
                    name = loadedJob.NameEnglish
                });
            }
        }

        public static List<string> GetAllJobNames() {
            return jobs.Select((job) => { return job.name; }).ToList();
        }

        public static Job GetJobById(uint id) {
            Job job = jobs.Find((job) => { return job.id == id; });

            return job;
        }

        public static Job GetJobByName(string name) {
            Job job = jobs.Find((job) => { return job.name == name; });

            return job;
        }

        public static Job SearchJobByName(string partialName) {
            Job job = jobs.Find((job) => { return job.name.StartsWith(partialName); });

            return job;
        }
    }
}
