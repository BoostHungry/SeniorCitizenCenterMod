using System.Collections.Generic;
using System.Reflection;

namespace SeniorCitizenCenterMod {
    public class AiReplacementHelper {
        private const bool LOG_AI_REPLACEMENT = true;

        private readonly Dictionary<string, BuildingAI> replacedAIs;

        public AiReplacementHelper() {
            this.replacedAIs = new Dictionary<string, BuildingAI>();
        }

        public bool replaceBuildingAi<T>(BuildingInfo building) where T : BuildingAI {
            Logger.logInfo(LOG_AI_REPLACEMENT, "AiReplacementHelper.replaceBuildingAi -- Checking Building: {0}", building);
            if (building == null) {
                Logger.logInfo(LOG_AI_REPLACEMENT, "AiReplacementHelper.replaceBuildingAi -- Null Building");
                return false;
            }

            if (this.replacedAIs.ContainsKey(building.name)) {
                Logger.logInfo(LOG_AI_REPLACEMENT, "AiReplacementHelper.replaceBuildingAi -- Did not replace AI for {0}, building AI has already been replaced", building.name);
                return true;
            }


            if (building.m_buildingAI is NursingHomeAi) {
                Logger.logInfo(LOG_AI_REPLACEMENT, "AiReplacementHelper.replaceBuildingAi -- Did not replace AI for {0}, building already running the NursingHomeAi", building.name);
                return false;
            }


            // Replace the AI
            BuildingAI component = building.GetComponent<BuildingAI>();
            T to = building.gameObject.AddComponent<T>();
            this.copyBuildingAIAttributes(component, to);
            this.replacedAIs[building.name] = component;
            building.m_buildingAI = to;
            to.m_info = building;

            Logger.logInfo(LOG_AI_REPLACEMENT, "AiReplacementHelper.replaceBuildingAi -- Successfully replaced {0}'s AI", building.name);
            return true;
        }

        private void copyBuildingAIAttributes<T>(BuildingAI from, T to) {
            FieldInfo[] fieldInfos = typeof(T).BaseType?.GetFields();
            if (fieldInfos == null) {
                return;
            }

            foreach (FieldInfo fieldInfo in fieldInfos) {
                fieldInfo.SetValue(to, fieldInfo.GetValue(@from));
            }
        }
    }
}