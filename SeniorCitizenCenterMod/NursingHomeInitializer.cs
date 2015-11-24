using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using UnityEngine;

namespace SeniorCitizenCenterMod {
    public class NursingHomeInitializer : MonoBehaviour {
        private const bool LOG_INITIALIZER = true;

        private const int LOADED_LEVEL_GAME = 6;
        private const int LOADED_LEVEL_ASSET_EDITOR = 19;

        private const String MEDICAL_CLINIC_NAME = "Medical Clinic";

        private static readonly Queue<IEnumerator> ACTION_QUEUE = new Queue<IEnumerator>();
        private static readonly object QUEUE_LOCK = new object();

        private readonly AiReplacementHelper aiReplacementHelper = new AiReplacementHelper();
        private int attemptingInitialization;

        private bool initialized;
        private int loadedLevel;

        private void Awake() {
            // Specify that this object should not be destroyed
            // Without this statement this object would be cleaned up very quickly
            DontDestroyOnLoad(this);
        }

        private void Start() {
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer Starting");
        }

        private void OnLevelWasLoaded(int level) {
            this.loadedLevel = level;
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.OnLevelWasLoaded: {0}", level);
        }

        public void OnLevelUnloading() {
            this.loadedLevel = -1;
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.OnLevelUnloading: {0}", this.loadedLevel);
        }

        private void Update() {
            if (!this.initialized) {
                // Still need initilization, check to see if already attempting initilization
                // Note: Not sure if it's possible for this method to be called more than once at a time, but locking just in case
                if (Interlocked.CompareExchange(ref this.attemptingInitialization, 1, 0) == 0) {
                    this.attemptInitialization();
                }
            }
        }

        private void attemptInitialization() {
            // Check to see if initilization can start
            if (PrefabCollection<BuildingInfo>.LoadedCount() <= 0) {
                this.attemptingInitialization = 0;
                return;
            }

            // Wait for the Medical Clinic to load since all new Nursing Homes will copy its values
            if (PrefabCollection<BuildingInfo>.FindLoaded(MEDICAL_CLINIC_NAME) == null) {
                this.attemptingInitialization = 0;
                return;
            }

            // Start loading
            Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.attemptInitialization Attempting Initialization");
            Singleton<LoadingManager>.instance.QueueLoadingAction(ActionWrapper(() => {
                try {
                    if (this.loadedLevel == LOADED_LEVEL_GAME) {
                        // Reset the PanelHelper and initilize the Healthcare Menu
                        PanelHelper.reset();
                        this.StartCoroutine(this.initHealthcareMenu());
                    }
                    if (this.loadedLevel == LOADED_LEVEL_GAME || this.loadedLevel == LOADED_LEVEL_ASSET_EDITOR) {
                        this.StartCoroutine(this.initNursingHomes(PrefabCollection<BuildingInfo>.FindLoaded(MEDICAL_CLINIC_NAME)));
                        AddQueuedActionsToLoadingQueue();
                    }
                } catch (Exception e) {
                    Logger.logError("Error loading prefabs: {0}", e.Message);
                }
            }));

            // Set initilized
            this.initialized = true;
            this.attemptingInitialization = 0;
        }

        private IEnumerator initHealthcareMenu() {
            while (!Singleton<LoadingManager>.instance.m_loadingComplete) {
                if (PanelHelper.initCustomHealthcareGroupPanel()) {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator initNursingHomes(BuildingInfo buildingToCopyFrom) {
            uint index = 0U;
            while (!Singleton<LoadingManager>.instance.m_loadingComplete) {
                for (; (long) PrefabCollection<BuildingInfo>.LoadedCount() > (long) index; ++index) {
                    BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                    if (buildingInfo != null && buildingInfo.name.EndsWith("_Data") && buildingInfo.name.Contains("NH123")) {
                        this.aiReplacementHelper.replaceBuildingAi<NursingHomeAi>(buildingInfo, buildingToCopyFrom);
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private static IEnumerator ActionWrapper(Action a) {
            a();
            yield break;
        }

        private static void AddQueuedActionsToLoadingQueue() {
            LoadingManager instance = Singleton<LoadingManager>.instance;
            object obj = typeof(LoadingManager).GetFieldByName("m_loadingLock").GetValue(instance);

            while (!Monitor.TryEnter(obj, SimulationManager.SYNCHRONIZE_TIMEOUT)) {
            }
            try {
                FieldInfo fieldByName = typeof(LoadingManager).GetFieldByName("m_mainThreadQueue");
                Queue<IEnumerator> queue1 = (Queue<IEnumerator>) fieldByName.GetValue(instance);
                if (queue1 == null) {
                    return;
                }
                Queue<IEnumerator> queue2 = new Queue<IEnumerator>(queue1.Count + 1);
                queue2.Enqueue(queue1.Dequeue());
                do
                    ; while (!Monitor.TryEnter(QUEUE_LOCK, SimulationManager.SYNCHRONIZE_TIMEOUT));
                try {
                    while (ACTION_QUEUE.Count > 0) {
                        queue2.Enqueue(ACTION_QUEUE.Dequeue());
                    }
                } finally {
                    Monitor.Exit(QUEUE_LOCK);
                }
                while (queue1.Count > 0) {
                    queue2.Enqueue(queue1.Dequeue());
                }
                fieldByName.SetValue(instance, queue2);
            } finally {
                Monitor.Exit(obj);
            }
        }
    }

    public static class TypeExtensions {
        public static IEnumerable<FieldInfo> GetAllFieldsFromType(this Type type) {
            if (type == null) {
                return Enumerable.Empty<FieldInfo>();
            }
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            if (type.BaseType != null) {
                return type.GetFields(bindingAttr).Concat(type.BaseType.GetAllFieldsFromType());
            }
            return type.GetFields(bindingAttr);
        }

        public static FieldInfo GetFieldByName(this Type type, string name) {
            return type.GetAllFieldsFromType().Where(p => p.Name == name).FirstOrDefault();
        }
    }
}