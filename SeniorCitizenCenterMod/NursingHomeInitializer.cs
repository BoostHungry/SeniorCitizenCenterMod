using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;

namespace SeniorCitizenCenterMod {
    public class NursingHomeInitializer {
        private const bool LOG_CUSTOM_PANELS = true;
        private const bool LOG_INITIALIZER = true;

        public const int LOADED_LEVEL_GAME = 6;
        public const int LOADED_LEVEL_ASSET_EDITOR = 19;

        private const String MEDICAL_CLINIC_NAME = "Medical Clinic";
        private int loadedLevel = -1;

        private readonly string SPRITE_BASE = "SubBar";
        
        private readonly string HEALTHCARE_NAME = "Healthcare";
        private readonly string HEALTHCARE_PANEL_NAME = "HealthcarePanel";
        private readonly string HEALTHCARE_COMPONENT_NAME = "HealthcareDefault";
        private readonly string HEALTHCARE_MONUMENT_COMPONENT_NAME = "MonumentCategory3";

        private readonly string NURSING_HOME_NAME = "NursingHome";
        private readonly string NURSING_HOME_COMPONENT_NAME = "NursingHomeDefault";

        private readonly AiReplacementHelper aiReplacementHelper = new AiReplacementHelper();

        public void OnLevelWasLoaded(int level) {
            this.loadedLevel = level;
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.OnLevelWasLoaded: {0}", level);
        }

        public void OnLevelUnloading() {
            this.loadedLevel = -1;
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.OnLevelUnloading: {0}", this.loadedLevel);
        }

        public void AttemptInitialization() {
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.attemptInitialization -- Attempting Initialization");

            UITabstrip strip = ToolsModifierControl.mainToolbar?.component as UITabstrip;
            if (strip == null)
            {
                throw new Exception("strip is null");
            }

            UIComponent healthCare = strip?.Find(HEALTHCARE_NAME);
            if (healthCare == null)
            {
                throw new Exception("healthCare is null");
            }

            UIComponent healthcarePanelComp = strip?.tabPages?.Find(HEALTHCARE_PANEL_NAME);
            if (healthcarePanelComp == null)
            {
                throw new Exception("healthcarePanelComp is null");
            }

            HealthcareGroupPanel healthcareGroupPanel = healthcarePanelComp?.GetComponent<HealthcareGroupPanel>();
            if (healthcareGroupPanel == null)
            {
                throw new Exception("healthcareGroupPanel is null");
            }

            // Ensure the Healthcare Components are available before initilization
            if (healthCare == null || healthcarePanelComp == null || healthcareGroupPanel == null || !healthCare.isActiveAndEnabled || !healthCare.isVisible)
            {
                Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Waiting to initilize Healthcare Menu because the components aren't ready");
                throw new Exception("Ensure the Healthcare Components are available before initilization fucked up");
            }

            ReplaceAisAndUpdateCapacity();

            // Can start initilization
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Initilizing Healthcare Menu");

            var healthcareDefault = healthcareGroupPanel.Find(HEALTHCARE_COMPONENT_NAME);
            if (healthcareDefault == null)
            {
                throw new Exception("healthcareDefault is null");
            }

            // Destroy the existing group panel
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Destroying the existing Healthcare Group Panel: {0}", healthcareGroupPanel);
            UnityEngine.Object.Destroy(healthcareGroupPanel);

            // Create a new custom group panel
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Creating the new Custom Healthcare Group Panel");
            var customHealthCareGroupPanel = healthcarePanelComp.gameObject.AddComponent<CustomHealthcareGroupPanel>();

            // Attempt initilization of the Custom Healthcare Group Panel -- Will take multiple attempts to completely initilize
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Attempting initilization of the Custom Healthcare Group Panel");
            customHealthCareGroupPanel.initNursingHomes();
        }

        private void ReplaceAisAndUpdateCapacity() {
            var medicalBuildingInfo = findMedicalBuildingInfo();
            var capcityModifier = SeniorCitizenCenterMod.getInstance().getOptionsManager().getCapacityModifier();
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(i);

                // Check for replacement of AI
                if (buildingInfo != null && buildingInfo.name.EndsWith("_Data") && buildingInfo.name.Contains("NH123"))
                {
                    this.aiReplacementHelper.replaceBuildingAi<NursingHomeAi>(buildingInfo, medicalBuildingInfo);
                }

                // Check for updating capacity - Existing NHs will be updated on-load, this will set the data used for placing new homes
                if (buildingInfo != null && buildingInfo.m_buildingAI is NursingHomeAi nursingHomeAi)
                {
                    nursingHomeAi.updateCapacity(capcityModifier);
                }
            }
        }

        private BuildingInfo findMedicalBuildingInfo() {
            // First check for the known Medical Clinic
            BuildingInfo medicalBuildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(MEDICAL_CLINIC_NAME);
            if (medicalBuildingInfo != null) {
                return medicalBuildingInfo;
            }

            // Attempt to find a suitable medical building that can be used as a template
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.findMedicalBuildingInfo -- Couldn't find the Medical Clinic asset, attempting to search for any Building with a HospitalAi");
            for (uint i=0; (long) PrefabCollection<BuildingInfo>.LoadedCount() > (long) i; ++i) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (buildingInfo != null && buildingInfo.GetService() == ItemClass.Service.HealthCare && !buildingInfo.m_buildingAI.IsWonder() && buildingInfo.m_buildingAI is HospitalAI) {
                    Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.findMedicalBuildingInfo -- Using the {0} as a template instead of the Medical Clinic", buildingInfo);
                    return buildingInfo;
                }
            }

            throw new Exception("Could not find the BuildingInfo of a medical building.");
        }
    }
}