using ICities;
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;
using System;
using ColossalFramework;

namespace SeniorCitizenCenterMod {
    public class OptionsManager {

        private static readonly string[] CAPACITY_LABELS = new string[] { "x0.5", "x1.0", "x1.5", "x2.0", "x2.5", "x3.0" };
        private static readonly float[] CAPACITY_VALUES = new float[] { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };

        private UIDropDown capacityDropDown;
        private float capacityModifier = -1.0f;

        public void initialize(UIHelperBase helper) {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.initialize -- Initializing Menu Options");
            UIHelperBase group = helper.AddGroup("Nursing Home Settings");
            this.capacityDropDown = (UIDropDown) group.AddDropdown("Capacity Modifier", CAPACITY_LABELS, 1, handleCapacityChange);
            group.AddSpace(5);
            group.AddButton("Save", saveOptions);
            //group.AddSlider("Capacity Modifier", 0.5f, 5.0f, 0.5f, 1.0f, handleCapacityChange);
        }

        private void handleCapacityChange(int newSelection) {
            // Do nothing until Save is pressed
        }

        public void updateCapacity() {
            this.updateCapacity(this.capacityModifier);
        }

        public float getCapacityModifier() {
            return this.capacityModifier;
        }

        public void updateCapacity(float targetValue) {
            try {
                SeniorCitizenCenterMod seniorCitizenCenterMod = SeniorCitizenCenterMod.getInstance();
                if (seniorCitizenCenterMod == null || seniorCitizenCenterMod.getNursingHomeInitializer() == null) {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.updateCapacity -- Skipping capacity update because a game is not loaded yet");
                    return;
                }

                NursingHomeInitializer nursingHomeInitializer = SeniorCitizenCenterMod.getInstance().getNursingHomeInitializer();
                if (nursingHomeInitializer.getLoadedLevel() != NursingHomeInitializer.LOADED_LEVEL_GAME) {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.updateCapacity -- Skipping capacity update because a game is not loaded yet");
                    return;
                }
            } catch (Exception e) {
                Logger.logError(Logger.LOG_OPTIONS, "OptionsManager.updateCapacity -- Skipping capacity update because a game is not loaded yet -- Exception: {0}", e.Message);
            }

            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.updateCapacity -- Updating capacity with modifier: {0}", targetValue);
            for (uint index = 0; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                if (buildingInfo != null && buildingInfo.m_buildingAI is NursingHomeAi) {
                    ((NursingHomeAi) buildingInfo.m_buildingAI).updateCapacity(targetValue);
                }
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            for (ushort i=0; i < buildingManager.m_buildings.m_buffer.Length; i++) {
                if (buildingManager.m_buildings.m_buffer[i].Info != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI is NursingHomeAi) {
                    ((NursingHomeAi) buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI).validateCapacity(i, ref buildingManager.m_buildings.m_buffer[i], true);
                }
            }
        }

        private void saveOptions() {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Saving Options");
            OptionsManager.Options options = new OptionsManager.Options();
            options.capacityModifierSelectedIndex = -1;

            if(this.capacityDropDown != null) {
                int capacitySelectedIndex = this.capacityDropDown.selectedIndex;
                options.capacityModifierSelectedIndex = capacitySelectedIndex;
                if (capacitySelectedIndex >= 0) {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Capacity Modifier Set to: {0}", CAPACITY_VALUES[capacitySelectedIndex]);
                    this.capacityModifier = CAPACITY_VALUES[capacitySelectedIndex];
                    this.updateCapacity(CAPACITY_VALUES[capacitySelectedIndex]);
                }
            }

            try {
                using (StreamWriter streamWriter = new StreamWriter("SeniorCitizenCenterModOptions.xml")) {
                    new XmlSerializer(typeof(OptionsManager.Options)).Serialize(streamWriter, options);
                }
            } catch (Exception e) {
                Logger.logError(Logger.LOG_OPTIONS, "Error saving options: {0} -- {1}", e.Message, e.StackTrace);
            }

        }

        public void loadOptions() {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Options");
            OptionsManager.Options options = new OptionsManager.Options();
            try {
                using (StreamReader streamReader = new StreamReader("SeniorCitizenCenterModOptions.xml")) {
                    options = (OptionsManager.Options) new XmlSerializer(typeof(OptionsManager.Options)).Deserialize(streamReader);
                }
            } catch (FileNotFoundException ex) {
                // Options probably not serialized yet, just return
                return;
            } catch (Exception e) {
                Logger.logError(Logger.LOG_OPTIONS, "Error loading options: {0} -- {1}", e.Message, e.StackTrace);
                return;
            }

            if (options.capacityModifierSelectedIndex != -1) {
                Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Capacity Modifier to: x{0}", CAPACITY_VALUES[options.capacityModifierSelectedIndex]);
                capacityDropDown.selectedIndex = options.capacityModifierSelectedIndex;
                this.capacityModifier = CAPACITY_VALUES[options.capacityModifierSelectedIndex];
            }
        }

        public struct Options {
            public int capacityModifierSelectedIndex;
        }
    }
}
