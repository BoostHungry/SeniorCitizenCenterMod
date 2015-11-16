using System;
using ColossalFramework.UI;
using ICities;

namespace SeniorCitizenCenterMod {
    public class PanelHelper : ThreadingExtensionBase {
        private const bool LOG_PANEL_HELPER = false;

        public const string INFO_PANEL_NAME = "CityServiceWorldInfoPanel";
        public const string STATS_PANEL_NAME = "StatsPanel";

        bool initialized = false;
        float originalPanelHeight = 0.0f;

        public override void OnBeforeSimulationTick() {
            UIComponent comp = UIView.library.Get(INFO_PANEL_NAME);
            if (!this.initialized) {
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.OnBeforeSimulationTick: Attempting Initilization");
                if (comp != null) {
                    // Init the original panel height
                    this.originalPanelHeight = comp.height;
                    Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.OnBeforeSimulationTick: Original Panel Height: {0}", this.originalPanelHeight);
                    if (this.originalPanelHeight > 1) {
                        Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.OnBeforeSimulationTick: Done Initilizing");
                        this.initialized = true;
                    }
                }
                return;
            }

            // Check to see if the panel height should be reset
            if (comp != null && !comp.isVisible && Math.Abs(this.originalPanelHeight - comp.height) > 1) {
                comp.height = this.originalPanelHeight;
                Logger.logInfo(LOG_PANEL_HELPER, "PanelHelper.OnBeforeSimulationTick: Reset panel height back to: {0}", comp.height);
            }
        }
    }
}