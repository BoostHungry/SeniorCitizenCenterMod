using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace SeniorCitizenCenterMod {
    public class PanelHelper : ThreadingExtensionBase {

        public const bool LOG_CUSTOM_PANELS = true;
        private const bool LOG_PANEL_HELPER = true;

        public const string INFO_PANEL_NAME = "CityServiceWorldInfoPanel";
        public const string STATS_PANEL_NAME = "LayoutPanel";
        public const string STATS_INFO_PANEL_NAME = "Info";
        public const string INFO_GROUP_PANEL_NAME = "InfoGroupPanel";
        public const string UPKEEP_LABEL_NAME = "Upkeep";

        private bool initialized = false;

        float originalPanelHeight = 0.0f;
        float originalStatsPanelHeight = 0.0f;
        float originalStatsPanelPosition = 0.0f;
        float originalStatsInfoPanelHeight = 0.0f;
        public static Color32 originalUpkeepColor;

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

                    // Get the original color of the Upkeep Label
                    UIComponent infoGroupPanel = infoPanel.Find(PanelHelper.INFO_GROUP_PANEL_NAME);
                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: infoGroupPanel: " + infoGroupPanel);
                    if (infoGroupPanel == null) {
                        return;
                    }

                    UILabel upkeepLabel = infoGroupPanel.Find<UILabel>(PanelHelper.UPKEEP_LABEL_NAME);
                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: upkeepLabel: " + upkeepLabel);
                    if (upkeepLabel == null) {
                        return;
                    }

                    PanelHelper.originalUpkeepColor = upkeepLabel.textColor;
                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: upkeepLabel.textColor: " + upkeepLabel.textColor);

                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Done Initilizing");
                    this.initialized = true;
                }
                return;
            }

            // Check to see if the panel height should be reset
            if (infoPanel != null && !infoPanel.isVisible && Math.Abs(this.originalPanelHeight - infoPanel.height) > 1) {
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Attempting to reset infoPanel height");
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Current infoPanel height: {0}", infoPanel.height);
                infoPanel.height = this.originalPanelHeight;
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Reset panel height back to: {0}", infoPanel.height);

                // Reset the other heights and positions
                UIComponent statsPanel = infoPanel.Find(STATS_PANEL_NAME);
                if (statsPanel != null) {
                    statsPanel.height = originalStatsPanelHeight;
                    Vector3 pos = statsPanel.position;
                    pos.y = originalStatsPanelPosition;
                    statsPanel.position = pos;

                    UIComponent statsInfoPanel = statsPanel.Find(STATS_INFO_PANEL_NAME);
                    if (statsInfoPanel != null) {
                        statsInfoPanel.height = originalStatsInfoPanelHeight;
                    }
                }

                // Also reset the Upkeep Color
                UIComponent infoGroupPanel = infoPanel.Find(PanelHelper.INFO_GROUP_PANEL_NAME);
                if (infoGroupPanel != null) {
                    UILabel upkeepLabel = infoGroupPanel.Find<UILabel>(PanelHelper.UPKEEP_LABEL_NAME);
                    if (upkeepLabel != null) {
                        upkeepLabel.textColor = PanelHelper.originalUpkeepColor;
                        Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: Reset upkeep color back to: {0}", upkeepLabel.textColor);
                    }
                }
            }
        }

        private void printLevels(UIComponent comp, String prevLevels, int depth) {
            if (comp == null) {
                return;
            }

            String level = prevLevels + " -> " + comp.name;
            Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.handleBuildingInfoPanel: components: {0}", level);

            if (depth > 6) {
                return;
            }

            foreach (UIComponent child in comp.components) {
                printLevels(child, level, depth + 1);
            }
        }
    }
}