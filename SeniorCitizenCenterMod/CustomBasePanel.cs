using ColossalFramework.UI;
using UnityEngine;

namespace SeniorCitizenCenterMod {
    public class CustomBasePanel : GeneratedScrollPanel {

        public override ItemClass.Service service {
            get {
                return ItemClass.Service.HealthCare;
            }
        }

        public override void RefreshPanel() {
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomBasePanel.RefreshPanel");

            // Refresh the panel
            base.RefreshPanel();
            this.PopulateAssets(GeneratedScrollPanel.AssetFilter.Building);

            if (PanelHelper.LOG_CUSTOM_PANELS) {
                foreach (UIComponent comp in this.childComponents) {
                    Logger.logInfo("CustomHealthcarePanel.RefreshPanel -- Child Component Found: {0}", comp);
                }
            }

            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomBasePanel.RefreshPanel Done");
        }

        public void removeAllChildren() {
            foreach (UIComponent comp in this.childComponents) {
                Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomBasePanel.removeAllChildren -- Removing Child Comp: {0}", comp);
                this.GetComponentInChildren<UIScrollablePanel>().RemoveUIComponent(comp);
                Destroy(comp.gameObject);
            }
        }

        protected override void OnButtonClicked(UIComponent comp) {
            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomBasePanel.OnButtonClicked -- Component Clicked: {0}", comp);
            BuildingInfo buildingInfo = comp.objectUserData as BuildingInfo;
            if (!((Object) buildingInfo != (Object) null))
                return;
            BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool>();
            if (!((Object) buildingTool != (Object) null))
                return;
            buildingTool.m_prefab = buildingInfo;
            buildingTool.m_relocate = 0;
        }

    }
}