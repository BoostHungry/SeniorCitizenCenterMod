using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace SeniorCitizenCenterMod {
    public class PanelHelper : ThreadingExtensionBase {

        public const bool LOG_CUSTOM_PANELS = false;
        private const bool LOG_PANEL_HELPER = false;

        public const string INFO_PANEL_NAME = "CityServiceWorldInfoPanel";
        public const string STATS_PANEL_NAME = "StatsPanel";
        public const string STATS_INFO_PANEL_NAME = "Info";

        bool initialized = false;
        private static bool replacedHealthcareGroupPanel = false;

        float originalPanelHeight = 0.0f;
        

        public override void OnBeforeSimulationTick() {
            this.handleBuildingInfoPanel();
        }

        private void handleBuildingInfoPanel() {
            UIComponent infoPanel = UIView.library.Get(INFO_PANEL_NAME);
            if (!this.initialized) {
                // Make sure the component is loaded before attempting initilization 
                if (infoPanel == null) {
                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Can't Inilitize yet because the component is still null");
                    return;
                }

                // Can start initilization
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Attempting Initilization");

                // Init the original panel height
                this.originalPanelHeight = infoPanel.height;
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Original Panel Height Detected: {0}", this.originalPanelHeight);

                // Ensure the original height is > 1 to consider this initilized
                if (this.originalPanelHeight > 1) {
                    // Also set the Stats Panel to a perminently larger and higher configuration
                    UIComponent statsPanel = infoPanel.Find(STATS_PANEL_NAME);
                    if (statsPanel.height < 124) {
                        statsPanel.height = 125f;
                        statsPanel.Find(STATS_INFO_PANEL_NAME).height = 120f;

                        Vector3 position = ((UIPanel) statsPanel).position;
                        position.y = position.y + 40;
                        ((UIPanel) statsPanel).position = position;
                    }

                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Done Initilizing");
                    this.initialized = true;
                }
                return;
            }

            // Check to see if the panel height should be reset
            if (infoPanel != null && !infoPanel.isVisible && Math.Abs(this.originalPanelHeight - infoPanel.height) > 1) {
                infoPanel.height = this.originalPanelHeight;
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Reset panel height back to: {0}", infoPanel.height);
            }
        }

        public static void reset() {
            // Reset the values needed for panel initilization, not everything needs to be re-initilized, but the healthcare menu does
            replacedHealthcareGroupPanel = false;
        }

        public static bool initCustomHealthcareGroupPanel() {

            // Get the Tab Strip, but fetching it before it's initlized can throw an exception
            UITabstrip strip = null;
            try {
                strip = ToolsModifierControl.mainToolbar?.component as UITabstrip;
            } catch {
                // Do nothing
            }

            // Get the other needed components
            UIComponent healthCare = strip?.Find(CustomHealthcareGroupPanel.HEALTHCARE_NAME);
            UIComponent healthcarePanelComp = strip?.tabPages?.Find(CustomHealthcareGroupPanel.HEALTHCARE_PANEL_NAME);
            HealthcareGroupPanel healthcareGroupPanel = healthcarePanelComp?.GetComponent<HealthcareGroupPanel>();

            // Ensure the Healthcare Components are available before initilization
            if (healthCare == null || healthcarePanelComp == null || healthcareGroupPanel == null || !healthCare.isActiveAndEnabled || !healthCare.isVisible) {
                Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Waiting to initilize Healthcare Menu because the components aren't ready");
                return false;
            }

            // Can start initilization
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Initilizing Healthcare Menu");

            // Check the Healthcare Group Panel and replace it with a Custom Healthcare Group Panel
            if (!(healthcareGroupPanel is CustomHealthcareGroupPanel)) {
                if (replacedHealthcareGroupPanel) {
                    Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Waiting to continue initilization of the Healthcare Menu because the Custom Panel isn't fully initilized yet");
                    return false;
                }

                // Destroy the existing group panel
                Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Destroying the existing Healthcare Group Panel: {0}", healthcareGroupPanel);
                UnityEngine.Object.Destroy(healthcareGroupPanel);

                // Create a new custom group panel
                Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Creating the new Custom Healthcare Group Panel");
                healthcarePanelComp.gameObject.AddComponent(typeof (CustomHealthcareGroupPanel));

                // Mark this step as complete and bail to give this step a chance to complete
                replacedHealthcareGroupPanel = true;
                return false;
            }

            // Attempt initilization of the Custom Healthcare Group Panel -- Will take multiple attempts to completely initilize
            Logger.logInfo(LOG_CUSTOM_PANELS, "PanelHelper.initCustomHealthcareGroupPanel -- Attempting initilization of the Custom Healthcare Group Panel");
            return ((CustomHealthcareGroupPanel) healthcareGroupPanel).initNursingHomes();
            
        }
        
    }
}