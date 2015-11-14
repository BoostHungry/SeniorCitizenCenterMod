using UnityEngine;

namespace SeniorCitizenCenterMod {
    internal static class Logger {
        private static readonly string Prefix = "SeniorCitizenCenterMod: ";

        public static void logInfo(bool shouldLog, string message, params object[] args) {
            if (shouldLog) {
                Logger.logInfo(message, args);
            }
        }

        public static void logInfo(string message, params object[] args) {
            Debug.Log(Prefix + string.Format(message, args));
        }

        public static void logWarning(bool shouldLog, string message, params object[] args) {
            if (shouldLog) {
                Logger.logWarning(message, args);
            }
        }

        public static void logWarning(string message, params object[] args) {
            Debug.LogWarning(Prefix + string.Format(message, args));
        }

        public static void logError(bool shouldLog, string message, params object[] args) {
            if (shouldLog) {
                Logger.logError(message, args);
            }
        }

        public static void logError(string message, params object[] args) {
            Debug.LogError(Prefix + string.Format(message, args));
        }
    }
}