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

            Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomBasePanel.RefreshPanel Done");
        }

        public bool removeAllChildren() {
            bool didDestroy = false;
            foreach (UIComponent comp in this.childComponents) {
                if (comp != null && comp is UIButton) {
                    object obj = ((UIButton) comp).objectUserData;
                    if (obj != null && obj is BuildingInfo && !this.IsServiceValid((BuildingInfo) obj)) {
                        Logger.logInfo(PanelHelper.LOG_CUSTOM_PANELS, "CustomHealthcarePanel.RefreshPanel -- Destroying Child Component: {0}", comp);
                        this.GetComponentInChildren<UIScrollablePanel>().RemoveUIComponent(comp);
                        Destroy(comp.gameObject);
                        didDestroy = true;
                    }
                }
            }

            return didDestroy;
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