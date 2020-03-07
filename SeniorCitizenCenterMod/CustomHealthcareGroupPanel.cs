using System;
using System.Threading;
using ColossalFramework;
using ColossalFramework.UI;

namespace SeniorCitizenCenterMod {
    public class CustomHealthcareGroupPanel : HealthcareGroupPanel {

        private static readonly string SPRITE_BASE = "SubBar";

        public static readonly string HEALTHCARE_NAME = "Healthcare";
        public static readonly string HEALTHCARE_PANEL_NAME = "HealthcarePanel";
        private static readonly string HEALTHCARE_COMPONENT_NAME = "HealthcareDefault";
        private static readonly string HEALTHCARE_MONUMENT_COMPONENT_NAME = "MonumentCategory3";

        private static readonly string NURSING_HOME_NAME = "NursingHome";
        private static readonly string NURSING_HOME_COMPONENT_NAME = "NursingHomeDefault";

        public override ItemClass.Service service {
            get {
                return ItemClass.Service.HealthCare;
            }
        }

        public void initNursingHomes() {
            UIComponent healthCareComponent = this.m_Strip.Find(HEALTHCARE_COMPONENT_NAME);
            if (healthCareComponent == null)
            {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- ERROR Null healthCareComponent");
                throw new Exception("healthCareComponent is null");
            }

            GeneratedScrollPanel healthCarePanel = this.m_Strip.GetComponentInContainer(healthCareComponent, typeof(GeneratedScrollPanel)) as GeneratedScrollPanel;
            if (healthCarePanel == null)
            {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- ERROR Null healthCarePanel");
                throw new Exception("healthCarePanel is null");
            }

            //Destroy the existing component
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying existing Healthcare Panel: {0}", healthCarePanel);
            Destroy(healthCarePanel);

            //Set the new component
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Custom Healthcare Panel");
            UIComponent healthcarePanelContainer = this.m_Strip.tabPages.components[healthCareComponent.zOrder];
            var customHealthcarePanel = healthcarePanelContainer.gameObject.AddComponent<CustomHealthcarePanel>();
            customHealthcarePanel.category = "HealthcareDefault";
            customHealthcarePanel.removeAllChildren();
            customHealthcarePanel.RefreshPanel();

            // Check the Healthcare Mounument Component and either destroy or replace the Panel with a custom one that will exclude Nursing Homes
            UIComponent healthCareMonumentComponent = this.m_Strip.Find(HEALTHCARE_MONUMENT_COMPONENT_NAME);
            if (healthCareMonumentComponent != null)
            {
                var shouldHideTab = SeniorCitizenCenterMod.getInstance().getOptionsManager().getHideTabSelectedValue();
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Should Hide Tab: {0}", shouldHideTab);

                if (shouldHideTab)
                {
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying Healthcare Monument Panel due to options set");
                    healthCareMonumentComponent.Hide();
                }
                else
                {
                    GeneratedScrollPanel healthCareMonumentPanel = this.m_Strip.GetComponentInContainer(healthCareMonumentComponent, typeof(GeneratedScrollPanel)) as GeneratedScrollPanel;

                    // Destroy the existing component
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying existing Healthcare Monument Panel: {0}", healthCareMonumentPanel);
                    Destroy(healthCareMonumentPanel);

                    // Set the new component
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Custom Healthcare Monument Panel");
                    UIComponent healthcareMonumentPanelContainer = this.m_Strip.tabPages.components[healthCareMonumentComponent.zOrder];
                    var customHealthCareMonumentPanel = healthcareMonumentPanelContainer.gameObject.AddComponent<CustomHealthcarePanel>();
                    customHealthCareMonumentPanel.category = "MonumentCategory3";
                    customHealthCareMonumentPanel.RefreshPanel();
                }
            }

            //Create the new tab for the Nursing Home

            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Nursing Home Tab");
            UIComponent nursingHomeComponent = this.SpawnButtonEntry(this.m_Strip, NURSING_HOME_NAME, NURSING_HOME_COMPONENT_NAME, true, null, SPRITE_BASE, true, false);

            // Create the new Nursing Home Panel
            UIComponent nursingHomePanelContainer = this.m_Strip.tabPages.components[nursingHomeComponent.zOrder];
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Setting Panel for: {0}", nursingHomePanelContainer);
            nursingHomePanelContainer.gameObject.AddComponent<NursingHomePanel>();
            nursingHomePanelContainer.name = "nursingHomePanel";

            //// Before finishing, refresh the panel
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Refreshing panels to add back components with the new logic");
            this.RefreshPanel();

            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Done Initing");
        }
    }
}