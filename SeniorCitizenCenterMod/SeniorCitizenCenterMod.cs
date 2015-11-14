using ICities;
using UnityEngine;


namespace SeniorCitizenCenterMod {

    public class SeniorCitizenCenterMod : LoadingExtensionBase, IUserMod {
        private const bool LOG_BASE = true;

        private GameObject nursingHomeInitializer;

        public string Description {
            get {
                return "Enables functionality for Nursing Home Assets to function as working Nursing Homes.";
            }
        }

        public string Name {
            get {
                return "SeniorCitizenCenterMod";
            }
        }

        public void OnSettingsUI(UIHelperBase helper) {

        }

        public override void OnCreated(ILoading loading) {
            Logger.logInfo(LOG_BASE, "SeniorCitizenCenterMod Created");
            base.OnCreated(loading);

            if (this.nursingHomeInitializer != null) {
                return;
            }

            this.nursingHomeInitializer = new GameObject("SeniorCitizenCenterMod Nursing Homes");
            this.nursingHomeInitializer.AddComponent<NursingHomeInitializer>();
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            this.nursingHomeInitializer?.GetComponent<NursingHomeInitializer>().OnLevelUnloading();
        }

        public override void OnReleased() {
            Logger.logInfo(LOG_BASE, "SeniorCitizenCenterMod Released");
            base.OnReleased();
            if (this.nursingHomeInitializer != null) {
                UnityEngine.Object.Destroy(this.nursingHomeInitializer);
            }
        }
    }
}
