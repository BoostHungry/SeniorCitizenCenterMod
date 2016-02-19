using ICities;
using UnityEngine;


namespace SeniorCitizenCenterMod {

    public class SeniorCitizenCenterMod : LoadingExtensionBase, IUserMod, ISerializableData {
        private const bool LOG_BASE = true;

        private static SeniorCitizenCenterMod instance;

        private GameObject nursingHomeInitializerObj;
        private NursingHomeInitializer nursingHomeInitializer;
        private OptionsManager optionsManager = new OptionsManager();

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

        public static SeniorCitizenCenterMod getInstance() {
            return instance;
        }

        public NursingHomeInitializer getNursingHomeInitializer() {
            return this.nursingHomeInitializer;
        }

        public OptionsManager getOptionsManager() {
            return this.optionsManager;
        }

        public void OnSettingsUI(UIHelperBase helper) {
            this.optionsManager.initialize(helper);
            this.optionsManager.loadOptions();
        }

        public override void OnCreated(ILoading loading) {
            Logger.logInfo(LOG_BASE, "SeniorCitizenCenterMod Created");
            instance = this;
            base.OnCreated(loading);

            if (this.nursingHomeInitializerObj != null) {
                return;
            }
            
            this.nursingHomeInitializerObj = new GameObject("SeniorCitizenCenterMod Nursing Homes");
            this.nursingHomeInitializer = this.nursingHomeInitializerObj.AddComponent<NursingHomeInitializer>();
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            this.nursingHomeInitializer?.OnLevelUnloading();
        }

        public override void OnLevelLoaded(LoadMode mode) {
            Logger.logInfo(LOG_BASE, "SeniorCitizenCenterMod Level Loaded: {0}", mode);
            base.OnLevelLoaded(mode);
            if(mode == LoadMode.LoadGame) {
                this.nursingHomeInitializer?.OnLevelWasLoaded(NursingHomeInitializer.LOADED_LEVEL_GAME);
            }
        }

        public override void OnReleased() {
            Logger.logInfo(LOG_BASE, "SeniorCitizenCenterMod Released");
            base.OnReleased();
            if (this.nursingHomeInitializerObj != null) {
                UnityEngine.Object.Destroy(this.nursingHomeInitializerObj);
            }
        }

        public byte[] LoadData(string id) {
            Logger.logInfo(Logger.LOG_OPTIONS, "Load Data: {0}", id);
            return null;
        }

        public void SaveData(string id, byte[] data) {
            Logger.logInfo(Logger.LOG_OPTIONS, "Save Data: {0} -- {1}", id, data);
        }

        public IManagers managers { get; }
        public string[] EnumerateData() { return null; }
        public void EraseData(string id) { }
        public bool LoadGame(string saveName) { return false; }
        public bool SaveGame(string saveName) { return false; }
    }
}
