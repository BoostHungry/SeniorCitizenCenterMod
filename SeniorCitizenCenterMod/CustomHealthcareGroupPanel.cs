using System;
using System.Threading;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace SeniorCitizenCenterMod {
    public class CustomHealthcareGroupPanel : HealthcareGroupPanel {

        private static readonly string SPRITE_BASE = "SubBar";

        public static readonly string HEALTHCARE_NAME = "Healthcare";
        public static readonly string HEALTHCARE_PANEL_NAME = "HealthcarePanel";
        private static readonly string HEALTHCARE_COMPONENT_NAME = "HealthcareDefault";
        private static readonly string HEALTHCARE_MONUMENT_COMPONENT_NAME = "MonumentCategory3";

        private static readonly string NURSING_HOME_NAME = "NursingHome";
        private static readonly string NURSING_HOME_COMPONENT_NAME = "NursingHomeDefault";

        private int iteration = 1;
        private bool hasStartedInit = false;
        private bool replacedHealthcarePanel = false;
        private bool replacedHealthcareMonumentPanel = false;
        private bool replacedNursingHomeComponent = false;
        private bool replacedNursingHomePanel = false;

        private bool shouldHideTab = true;

        private int refreshing = 0;

        public override ItemClass.Service service {
            get {
                return ItemClass.Service.HealthCare;
            }
        }

        public void resetInit() {
            replacedHealthcarePanel = false;
            replacedHealthcareMonumentPanel = false;
            replacedNursingHomeComponent = false;
            replacedNursingHomePanel = false;
        }

        public bool initNursingHomes() {
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Starting Iteration: {0}", iteration++);

            /*
             * Multi-step initilization process, perform each step one at a time and wait for it to completely finish before moving on.
             * If an object isn't allowed to initilize complete, interacting with it can crash the game.
             */

            // First refresh the default panels before starting
            if (!this.hasStartedInit) {
                this.internalRefreshPanel(false);
                this.hasStartedInit = true;
            }

            // 1.1) Check the Healthcare Component and replace the Panel with a custom one that will exclude Nursing Homes
            UIComponent healthCareComponent = this.m_Strip.Find(HEALTHCARE_COMPONENT_NAME);
            GeneratedScrollPanel healthCarePanel = this.m_Strip.GetComponentInContainer(healthCareComponent, typeof(GeneratedScrollPanel)) as GeneratedScrollPanel;
            if (!(healthCarePanel is CustomHealthcarePanel)) {
                // Check to make sure this step is only done once, if attempting more than once, then just bail to give the process more time to finish
                if (this.replacedHealthcarePanel) {
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Waiting for replacement of the Healthcare Panel to complete");
                    return false;
                }

                // Destroy the existing component
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying existing Healthcare Panel: {0}", healthCarePanel);
                Destroy(healthCarePanel);

                // Set the new component
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Custom Healthcare Panel");
                UIComponent healthcarePanelContainer = this.m_Strip.tabPages.components[healthCareComponent.zOrder];
                healthcarePanelContainer.gameObject.AddComponent<CustomHealthcarePanel>();

                // Mark this step as complete and bail to ensure this step is allowed to finish
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after finishing setting the Custom Healthcare Panel");
                this.replacedHealthcarePanel = true;
                return false;
            }

            // 1.2) Check the Healthcare Mounument Component and either destroy or replace the Panel with a custom one that will exclude Nursing Homes
            this.shouldHideTab = SeniorCitizenCenterMod.getInstance().getOptionsManager().getHideTabSelectedValue();
            UIComponent healthCareMonumentComponent = this.m_Strip.Find(HEALTHCARE_MONUMENT_COMPONENT_NAME);
            GeneratedScrollPanel healthCareMonumentPanel = this.m_Strip.GetComponentInContainer(healthCareMonumentComponent, typeof(GeneratedScrollPanel)) as GeneratedScrollPanel;
            if (healthCareMonumentComponent == null) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Ignoring the Healthcare Monument Panel because it was not found");
            } else {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Checking on the Healthcare Monument Panel because it was found");
                if (this.shouldHideTab) {
                    if (!this.replacedHealthcareMonumentPanel) {
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying Healthcare Monument Panel due to options set");
                        healthCareMonumentComponent.Hide();

                        // Mark this step as complete and bail to ensure this step is allowed to finish
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after finishing destroying the Healthcare Monument Panel");
                        this.replacedHealthcareMonumentPanel = true;
                        return false;
                    }
                } else {
                    if (!(healthCareMonumentPanel is CustomHealthcarePanel)) {
                        // Check to make sure this step is only done once, if attempting more than once, then just bail to give the process more time to finish
                        if (this.replacedHealthcareMonumentPanel) {
                            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Waiting for replacement of the Healthcare Monument Panel to complete");
                            return false;
                        }

                        // Destroy the existing component
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Destroying existing Healthcare Monument Panel: {0}", healthCareMonumentPanel);
                        Destroy(healthCareMonumentPanel);

                        // Set the new component
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Custom Healthcare Monument Panel");
                        UIComponent healthcarePanelContainer = this.m_Strip.tabPages.components[healthCareMonumentComponent.zOrder];
                        healthcarePanelContainer.gameObject.AddComponent<CustomHealthcarePanel>();


                        // Mark this step as complete and bail to ensure this step is allowed to finish
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after finishing setting the Custom Healthcare Monument Panel");
                        this.replacedHealthcareMonumentPanel = true;
                        return false;
                    }
                }
            }

            // 2) Set the category strings on the custom panels to their default values
            if (healthCarePanel.category != "HealthcareDefault") {
                healthCarePanel.category = "HealthcareDefault";
            }
            if (!this.shouldHideTab) {
                if (healthCareMonumentPanel.category != "MonumentCategory3") {
                    healthCareMonumentPanel.category = "MonumentCategory3";
                }
            }

            // 3) Sometimes there are multiple panels present for some reason.  Continually destroy all legacy panels until init is complete
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Checking for duplicates");
            List<UIComponent> legacyComps = new List<UIComponent>();
            foreach (UIComponent comp in this.m_Strip.components) {
                if (comp.name == HEALTHCARE_COMPONENT_NAME || (!this.shouldHideTab && comp.name == HEALTHCARE_MONUMENT_COMPONENT_NAME)) {
                    GeneratedScrollPanel panel = this.m_Strip.GetComponentInContainer(comp, typeof(GeneratedScrollPanel)) as GeneratedScrollPanel;
                    if (!(panel is CustomHealthcarePanel)) {
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Found duplicate - Destroying: {0}", comp);
                        Destroy(panel);
                        Destroy(comp);
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after destroying an unexpected panel to allow everything to settle");
                        return false;
                    }
                }
            }

            // 4) Check the Nursing Home Component and create it if it's not present
            UIComponent nursingHomeComponent = this.m_Strip.Find(NURSING_HOME_COMPONENT_NAME);
            if (nursingHomeComponent == null) {
                // Check to make sure this step is only done once, if attempting more than once, then just bail to give the process more time to finish
                if (this.replacedNursingHomeComponent) {
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Waiting for replacement of the Nursing Home Component to complete");
                    return false;
                }

                // Create the new tab for the Nursing Home
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Creating new Nursing Home Tab");
                this.SpawnButtonEntry(this.m_Strip, NURSING_HOME_NAME, NURSING_HOME_COMPONENT_NAME, true, null, SPRITE_BASE, true, false);
                
                // Mark this step as complete and bail to ensure this step is allowed to finish
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after finished creating new Nursing Home tab");
                this.replacedNursingHomeComponent = true;
                return false;
            }

            // 5) Check the Nursing Home Panel and create it if it's not present
            GeneratedScrollPanel nursingHomePanel = this.m_Strip.GetComponentInContainer(nursingHomeComponent, typeof(NursingHomePanel)) as GeneratedScrollPanel;
            if (nursingHomePanel == null) {
                // Check to make sure this step is only done once, if attempting more than once, then just bail to give the process more time to finish
                if (this.replacedNursingHomePanel) {
                    Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Waiting for replacement of the Nursing Home Panel to complete");
                    return false;
                }

                // Create the new Nursing Home Panel
                UIComponent nursingHomePanelContainer = this.m_Strip.tabPages.components[nursingHomeComponent.zOrder];
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Setting Panel for: {0}", nursingHomePanelContainer);
                nursingHomePanelContainer.gameObject.AddComponent<NursingHomePanel>();
                nursingHomePanelContainer.name = "nursingHomePanel";

                // Mark this step as complete and bail to ensure this step is allowed to finish
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Bailing after setting Nusing Home Panel");
                this.replacedNursingHomePanel = true;
                return false;
            }

            // Remove all children from the Healthcare Panel so it can be repopulated by the new panel logic -- Note: May take more than one iteration to remove them all
            if (healthCarePanel.childComponents.Count > 0 && ((CustomHealthcarePanel) healthCarePanel).removeAllChildren()) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Attempted to removing unwanted components from the healthCarePanel");
                return false;
            }

            // Remove all children from the Healthcare Monument Panel (if not hidden) so it can be repopulated by the new panel logic -- Note: May take more than one iteration to remove them all
            if (!this.shouldHideTab && healthCareMonumentPanel.childComponents.Count > 0 && ((CustomHealthcarePanel) healthCareMonumentPanel).removeAllChildren()) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Attempted to removing unwanted components from the healthCareMonumentPanel");
                return false;
            }

            // Remove all children from the Healthcare Panel so it can be repopulated by the new panel logic -- Note: May take more than one iteration to remove them all
            if (nursingHomePanel.childComponents.Count > 0 && ((NursingHomePanel) nursingHomePanel).removeAllChildren()) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Attempted to removing unwanted components from the nursingHomePanel");
                return false;
            }

            // Before finishing, refresh the panel
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Refreshing panels to add back components with the new logic");
            this.RefreshPanel();

            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.initNursingHomes -- Done Initing");
            return true;
        }

        protected override bool CustomRefreshPanel() {
            return this.internalRefreshPanel(true);
        }

        private bool internalRefreshPanel(bool refreshCustoms) {
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel");
            if (Interlocked.CompareExchange(ref this.refreshing, 1, 0) == 1) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel -- Can't refresh, already refreshing");
                return true;
            }


            // Refresh the Custom Panels only when specified -- Can't refresh these panels before completing the init process
            if (refreshCustoms) {
                try {
                    // Refresh the Healthcare Panel
                    UIComponent healthcareComp = this.m_Strip.Find(HEALTHCARE_COMPONENT_NAME);
                    GeneratedScrollPanel healthcarePanel = this.m_Strip.GetComponentInContainer(healthcareComp, typeof (CustomHealthcarePanel)) as GeneratedScrollPanel;
                    if (healthcarePanel != null) {
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel -- Refreshing the Healthcare Panel");
                        healthcarePanel.RefreshPanel();
                    }

                    // Refresh the Healthcare Monument Panel if not hidden
                    if (!this.shouldHideTab) {
                        UIComponent healthcareMonumentComp = this.m_Strip.Find(HEALTHCARE_MONUMENT_COMPONENT_NAME);
                        GeneratedScrollPanel healthcareMonumentPanel = this.m_Strip.GetComponentInContainer(healthcareMonumentComp, typeof(CustomHealthcarePanel)) as GeneratedScrollPanel;
                        if (healthcareMonumentPanel != null) {
                            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel -- Refreshing the Healthcare Monument Panel");
                            healthcareMonumentPanel.RefreshPanel();
                        }
                    }


                    // Refresh the Nursing Home Panel
                    UIComponent nursingHomeDefault = this.m_Strip.Find(NURSING_HOME_COMPONENT_NAME);
                    GeneratedScrollPanel nursingHomePanel = this.m_Strip.GetComponentInContainer(nursingHomeDefault, typeof (NursingHomePanel)) as GeneratedScrollPanel;
                    if (nursingHomePanel != null) {
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel -- Refreshing the Nursing Home Panel");
                        nursingHomePanel.RefreshPanel();
                    }
                } catch (Exception e) {
                    Logger.logError(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcareGroupPanel.CustomRefreshPanel -- Exception refreshing the Custom Panels: {0} -- {1}", e, e.StackTrace);
                    this.refreshing = 0;
                    return true;
                }
            }

            // Proceed with the existing logic
            if (this.groupFilter != GeneratedGroupPanel.GroupFilter.None) {
                this.PopulateGroups(this.groupFilter, this.sortingMethod);
            } else if (!string.IsNullOrEmpty(this.serviceName)) {
                this.DefaultGroup(this.serviceName);
            } else {
                this.DefaultGroup(EnumExtensions.Name<ItemClass.Service>(this.service));
            }

            this.refreshing = 0;
            return true;
        }

    }
}