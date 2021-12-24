using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Runtime.InteropServices;

namespace Auralyte.Game {
    public static unsafe class Client {

        private static ActionManager* actionManager;

        public static void Initialise() {
            actionManager = ActionManager.Instance();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        public static bool IsGameFocused {
            get {
                var activatedHandle = GetForegroundWindow();
                if(activatedHandle == IntPtr.Zero)
                    return false;

                var procId = Environment.ProcessId;
                _ = GetWindowThreadProcessId(activatedHandle, out var activeProcId);

                return activeProcId == procId;
            }
        }

        public static uint GetAdjustedActionId(uint actionId) {
            return actionManager->GetAdjustedActionId(actionId);
        }

        public static uint GetState(byte actionType, uint actionId) {
            return actionManager->GetActionStatus((ActionType)actionType, actionId);
        }

        public static float GetRecastTime(ActionType actionType, uint actionID) {
            var recast = actionManager->GetRecastTime(actionType, actionID);
            if(recast == 0) {
                return -1;
            }

            return recast;
        }

        public static float GetRecastTime(byte actionType, uint actionID) => GetRecastTime((ActionType)actionType, actionID);

        public static float GetRecastTimeElapsed(ActionType actionType, uint actionID) => actionManager->GetRecastTimeElapsed(actionType, actionID);
        public static float GetRecastTimeElapsed(byte actionType, uint actionID) => GetRecastTimeElapsed((ActionType)actionType, actionID);

        public static bool IsRecastTimerActive(ActionType actionType, uint actionID) => actionManager->IsRecastTimerActive(actionType, actionID);
        public static bool IsRecastTimerActive(byte actionType, uint actionID) => IsRecastTimerActive((ActionType)actionType, actionID);

        public static uint CheckActionResources(ActionType actionType, uint actionID) => actionManager->CheckActionResources(actionType, actionID);
        public static uint CheckActionResources(byte actionType, uint actionID) => CheckActionResources((ActionType)actionType, actionID);

    }

}