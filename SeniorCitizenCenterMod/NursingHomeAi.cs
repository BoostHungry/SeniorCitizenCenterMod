using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;

namespace SeniorCitizenCenterMod {
    public class NursingHomeAi : PlayerBuildingAI {
        private const bool LOG_PRODUCTION = false;
        private const bool LOG_SIMULATION = false;
        private const bool LOG_RANGE = false;
        private const bool LOG_BUILDING = false;

        private static readonly float[] QUALITY_VALUES = { 50, 25, 10, 40, 70, 125 };

        // TODO: Workout how to have Nursing Homes Coverage display separate from Health Care
        //private static readonly ItemClass NURSING_HOME_ITEM_CLASS = NursingHomeAi.initNewItemClass();

        private Randomizer randomizer = new Randomizer(97);

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int numEducatedWorkers = 5;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int numHighlyEducatedWorkers = 4;

        [CustomizableProperty("Number of Rooms")]
        public int numRooms = 25;

        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int numUneducatedWorkers = 5;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int numWellEducatedWorkers = 5;

        [CustomizableProperty("Operation Radius")]
        public float operationRadius = 500f;

        [CustomizableProperty("Quality (values: 0-5 including 0 and 5)")]
        public int quality = 2;

        public override Color GetColor(ushort buildingId, ref Building data, InfoManager.InfoMode infoMode) {
            // This is a copy from ResidentialBuildingAI
            InfoManager.InfoMode infoModeCopy = infoMode;
            switch (infoModeCopy) {
                case InfoManager.InfoMode.Health:
                    if (this.ShowConsumption(buildingId, ref data) && (int) data.m_citizenCount != 0)
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHealthLevel((int) data.m_health) * 0.2f);
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Density:
                    if (!this.ShowConsumption(buildingId, ref data) || (int) data.m_citizenCount == 0)
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    int num1 = ((int) data.m_citizenCount - (int) data.m_youngs - (int) data.m_adults - (int) data.m_seniors) * 3;
                    int num2 = (int) data.m_youngs + (int) data.m_adults;
                    int num3 = (int) data.m_seniors;
                    if (num1 == 0 && num2 == 0 && num3 == 0)
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    if (num1 >= num2 && num1 >= num3)
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_activeColor;
                    if (num2 >= num3)
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_activeColorB;
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor;
                default:
                    switch (infoModeCopy - 17) {
                        case InfoManager.InfoMode.None:
                            if (this.ShowConsumption(buildingId, ref data)) {
                                return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Color.Lerp(Singleton<ZoneManager>.instance.m_properties.m_zoneColors[2], Singleton<ZoneManager>.instance.m_properties.m_zoneColors[3], 0.5f) * 0.5f, (float) (0.200000002980232 + (double) Math.Max(0, this.quality - 1) * 0.200000002980232));
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        case InfoManager.InfoMode.Water:
                            if (!this.ShowConsumption(buildingId, ref data) || (int) data.m_citizenCount == 0)
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
                            int num4;
                            int num5;
                            if (currentSubMode == InfoManager.SubInfoMode.Default) {
                                num4 = (int) data.m_education1 * 100;
                                num5 = (int) data.m_teens + (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
                            } else if (currentSubMode == InfoManager.SubInfoMode.WaterPower) {
                                num4 = (int) data.m_education2 * 100;
                                num5 = (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
                            } else {
                                num4 = (int) data.m_education3 * 100;
                                num5 = (int) data.m_youngs * 2 / 3 + (int) data.m_adults + (int) data.m_seniors;
                            }
                            if (num5 != 0)
                                num4 = (num4 + (num5 >> 1)) / num5;
                            int num6 = Mathf.Clamp(num4, 0, 100);
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) num6 * 0.01f);
                        default:
                            return this.handleOtherColors(buildingId, ref data, infoMode);
                    }
            }
        }

        private Color handleOtherColors(ushort buildingId, ref Building data, InfoManager.InfoMode infoMode) {
            switch (infoMode) {
                case InfoManager.InfoMode.Happiness:
                    if (this.ShowConsumption(buildingId, ref data)) {
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHappinessLevel((int) data.m_happiness) * 0.25f);
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Garbage:
                    if (this.m_garbageAccumulation == 0)
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    return base.GetColor(buildingId, ref data, infoMode);
                default:
                    return base.GetColor(buildingId, ref data, infoMode);
            }
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation) {
            mode = InfoManager.InfoMode.Health;
            subMode = InfoManager.SubInfoMode.DeathCare;
        }

        protected override void ProduceGoods(ushort buildingId, ref Building buildingData, ref Building.Frame frameData, int productionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount) {
            base.ProduceGoods(buildingId, ref buildingData, ref frameData, productionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);

            // Make sure there are no problems
            if ((buildingData.m_problems & (Notification.Problem.MajorProblem | Notification.Problem.Electricity | Notification.Problem.ElectricityNotConnected | Notification.Problem.Fire | Notification.Problem.NoWorkers | Notification.Problem.Water | Notification.Problem.WaterNotConnected | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff)) != Notification.Problem.None) {
                return;
            }

            // Make sure there are empty rooms available
            uint emptyRoom = this.getEmptyCitizenUnit(buildingId, ref buildingData);
            if (emptyRoom == 0) {
                return;
            }

            // Fetch a Senior Citizen
            SeniorCitizenManager seniorCitizenManager = SeniorCitizenManager.getInstance();
            uint[] familyWithSeniors = seniorCitizenManager.getFamilyWithSenior();
            if (familyWithSeniors == null) {
                // No Family Located
                return;
            }

            Logger.logInfo(LOG_PRODUCTION, "------------------------------------------------------------");
            Logger.logInfo(LOG_PRODUCTION, "NursingHomeAi.ProduceGoods -- Family: {0}", string.Join(", ", Array.ConvertAll(familyWithSeniors, item => item.ToString())));

            // Check move in chance
            NumWorkers numWorkers = this.getNumWorkers(ref behaviour);
            bool shouldMoveIn = MoveInProbabilityHelper.checkIfShouldMoveIn(familyWithSeniors, ref buildingData, ref this.randomizer, this.operationRadius, this.quality, ref numWorkers);

            // Process the seniors and move them in if able to, mark the seniors as done processing regardless
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            foreach (uint familyMember in familyWithSeniors) {
                if (seniorCitizenManager.isSenior(familyMember)) {
                    if (shouldMoveIn) {
                        Logger.logInfo(LOG_PRODUCTION, "NursingHomeAi.ProduceGoods -- Moving In: {0}", familyMember);
                        citizenManager.m_citizens.m_buffer[familyMember].SetHome(familyMember, buildingId, emptyRoom);
                    }
                    seniorCitizenManager.doneProcessingSenior(familyMember);
                }
            }
        }

        private uint getEmptyCitizenUnit(ushort buildingId, ref Building data) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;

            while ((int) citizenUnitIndex != 0) {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    if (citizenManager.m_units.m_buffer[citizenUnitIndex].Empty()) {
                        return citizenUnitIndex;
                    }
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            return 0;
        }

        private NumWorkers getNumWorkers(ref Citizen.BehaviourData workerBehaviourData) {
            NumWorkers numWorkers = new NumWorkers();
            numWorkers.maxNumUneducatedWorkers = this.numUneducatedWorkers;
            numWorkers.numUneducatedWorkers = workerBehaviourData.m_educated0Count;
            numWorkers.maxNumEducatedWorkers = this.numEducatedWorkers;
            numWorkers.numEducatedWorkers = workerBehaviourData.m_educated1Count;
            numWorkers.maxNumWellEducatedWorkers = this.numWellEducatedWorkers;
            numWorkers.numWellEducatedWorkers = workerBehaviourData.m_educated2Count;
            numWorkers.maxNumHighlyEducatedWorkers = this.numHighlyEducatedWorkers;
            numWorkers.numHighlyEducatedWorkers = workerBehaviourData.m_educated3Count;
            return numWorkers;
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) {
            // TODO: Anything to do here?
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) {
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive");
            Citizen.BehaviourData behaviour = new Citizen.BehaviourData();
            int aliveCount = 0;
            int totalCount = 0;
            int homeCount = 0;
            int aliveHomeCount = 0;
            int emptyHomeCount = 0;
            this.GetHomeBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount, ref homeCount, ref aliveHomeCount, ref emptyHomeCount);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- behaviour: {0}", behaviour);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- aliveCount: {0}", aliveCount);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- totalCount: {0}", totalCount);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- homeCount: {0}", homeCount);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- aliveHomeCount: {0}", aliveHomeCount);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- emptyHomeCount: {0}", emptyHomeCount);

            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            byte district = districtManager.GetDistrict(buildingData.m_position);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- district: {0}", district);
            DistrictPolicies.Services policies = districtManager.m_districts.m_buffer[(int) district].m_servicePolicies;
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- policies: {0}", policies);

            DistrictPolicies.Taxation taxationPolicies = districtManager.m_districts.m_buffer[(int) district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanning = districtManager.m_districts.m_buffer[(int) district].m_cityPlanningPolicies;
            DistrictPolicies.Special special = districtManager.m_districts.m_buffer[(int) district].m_specialPolicies;

            // No clue what this is for, setting some policies at the disctrict level?
            districtManager.m_districts.m_buffer[(int) district].m_servicePoliciesEffect |= policies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.PetBan | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.SmokingBan);


            // TODO: Ignore Tax Stuff? - Might want to add possabilitiy to collect taxes from Nursing Homes
            //if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialLow) {
            //    if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)) != (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)) {
            //        districtManager.m_districts.m_buffer[(int) district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow);
            //    }
            //} else if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)) != (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)) {
            //    districtManager.m_districts.m_buffer[(int) district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh);
            //}

            // No clue what these are for, setting some policies at the disctrict level?
            districtManager.m_districts.m_buffer[(int) district].m_cityPlanningPoliciesEffect |= cityPlanning & (DistrictPolicies.CityPlanning.HighTechHousing | DistrictPolicies.CityPlanning.HeavyTrafficBan | DistrictPolicies.CityPlanning.EncourageBiking | DistrictPolicies.CityPlanning.BikeBan | DistrictPolicies.CityPlanning.OldTown);
            districtManager.m_districts.m_buffer[(int) district].m_specialPoliciesEffect |= special & (DistrictPolicies.Special.ProHippie | DistrictPolicies.Special.ProHipster | DistrictPolicies.Special.ProRedneck | DistrictPolicies.Special.ProGangsta | DistrictPolicies.Special.AntiHippie | DistrictPolicies.Special.AntiHipster | DistrictPolicies.Special.AntiRedneck | DistrictPolicies.Special.AntiGangsta | DistrictPolicies.Special.ComeOneComeAll | DistrictPolicies.Special.WeAreTheNorm);

            // Handle Sub Culture -- Not really sure why only when ProHippie is "loaded"
            if (districtManager.IsPolicyLoaded(DistrictPolicies.Policies.ProHippie)) {
                int hippieValue = 0;
                int hipsterValue = 0;
                int redneckValue = 0;
                int gangstaValue = 0;
                if ((special & (DistrictPolicies.Special.ProHippie | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
                    hippieValue += 100;
                if ((special & (DistrictPolicies.Special.AntiHippie | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
                    hippieValue -= 100;
                if ((special & (DistrictPolicies.Special.ProHipster | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
                    hipsterValue += 100;
                if ((special & (DistrictPolicies.Special.AntiHipster | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
                    hipsterValue -= 100;
                if ((special & (DistrictPolicies.Special.ProRedneck | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
                    redneckValue += 100;
                if ((special & (DistrictPolicies.Special.AntiRedneck | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
                    redneckValue -= 100;
                if ((special & (DistrictPolicies.Special.ProGangsta | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
                    gangstaValue += 100;
                if ((special & (DistrictPolicies.Special.AntiGangsta | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
                    gangstaValue -= 100;
                if (hippieValue < 0)
                    hippieValue = 0;
                if (hipsterValue < 0)
                    hipsterValue = 0;
                if (redneckValue < 0)
                    redneckValue = 0;
                if (gangstaValue < 0)
                    gangstaValue = 0;
                int combinedSubCultureValue = Mathf.Max(100, hippieValue + hipsterValue + redneckValue + gangstaValue);
                int modifiedSubCultureValue = new Randomizer((int) buildingID << 16).Int32((uint) combinedSubCultureValue);
                buildingData.SubCultureType = modifiedSubCultureValue >= hippieValue ? (modifiedSubCultureValue >= hippieValue + hipsterValue ? (modifiedSubCultureValue >= hippieValue + hipsterValue + redneckValue ? (modifiedSubCultureValue >= hippieValue + hipsterValue + redneckValue + gangstaValue ? Citizen.SubCulture.Generic : Citizen.SubCulture.Gangsta) : Citizen.SubCulture.Redneck) : Citizen.SubCulture.Hipster) : Citizen.SubCulture.Hippie;
                Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- SubCultureType: {0}", buildingData.SubCultureType);
            }

            // Handle Consumptions
            int electricityConsumption;
            int waterConsumption;
            int sewageAccumulation;
            int garbageAccumulation;
            int incomeAccumulation;
            this.GetConsumptionRates(new Randomizer((int) buildingID), 100, out electricityConsumption, out waterConsumption, out sewageAccumulation, out garbageAccumulation, out incomeAccumulation);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- electricityConsumption: {0}", electricityConsumption);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- waterConsumption: {0}", waterConsumption);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- sewageAccumulation: {0}", sewageAccumulation);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- garbageAccumulation: {0}", garbageAccumulation);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- incomeAccumulation: {0}", incomeAccumulation);

            int modifiedElectricityConsumption = 1 + (electricityConsumption * behaviour.m_electricityConsumption + 9999) / 10000;
            waterConsumption = 1 + (waterConsumption * behaviour.m_waterConsumption + 9999) / 10000;
            int modifiedSewageAccumulation = 1 + (sewageAccumulation * behaviour.m_sewageAccumulation + 9999) / 10000;
            garbageAccumulation = (garbageAccumulation * behaviour.m_garbageAccumulation + 9999) / 10000;
            int modifiedIncomeAccumulation = 0; //TODO: Possibly allow for income: (incomeAccumulation * behaviour.m_incomeAccumulation + 9999) / 10000;

            // Handle Recylcing and Pets
            if (garbageAccumulation != 0) {
                if ((policies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None) {
                    garbageAccumulation = (policies & DistrictPolicies.Services.PetBan) == DistrictPolicies.Services.None ? Mathf.Max(1, garbageAccumulation * 85 / 100) : Mathf.Max(1, garbageAccumulation * 7650 / 10000);
                    modifiedIncomeAccumulation = modifiedIncomeAccumulation * 95 / 100;
                } else if ((policies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None) {
                    garbageAccumulation = Mathf.Max(1, garbageAccumulation * 90 / 100);
                }
            }

            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- modifiedElectricityConsumption: {0}", modifiedElectricityConsumption);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- modifiedWaterConsumption: {0}", waterConsumption);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- modifiedSewageAccumulation: {0}", modifiedSewageAccumulation);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- modifiedGarbageAccumulation: {0}", garbageAccumulation);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- modifiedIncomeAccumulation: {0}", modifiedIncomeAccumulation);

            if ((int) buildingData.m_fireIntensity == 0) {
                int commonConsumptionValue = this.HandleCommonConsumption(buildingID, ref buildingData, ref modifiedElectricityConsumption, ref waterConsumption, ref modifiedSewageAccumulation, ref garbageAccumulation, policies);
                // TODO: Possibly allow for income
                //modifiedIncomeAccumulation = (modifiedIncomeAccumulation * commonConsumptionValue + 99) / 100;
                //if (modifiedIncomeAccumulation != 0) {
                //    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, modifiedIncomeAccumulation, this.m_info.m_class, taxationPolicies);
                //}
                buildingData.m_flags |= Building.Flags.Active;
            } else {
                // Handle on fire
                buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood);
                buildingData.m_flags &= ~Building.Flags.Active;
            }

            // Get the Health
            int health = 0;
            float radius = (float) (buildingData.Width + buildingData.Length) * 2.5f;
            if (behaviour.m_healthAccumulation != 0) {
                if (aliveCount != 0) {
                    health = (behaviour.m_healthAccumulation + (aliveCount >> 1)) / aliveCount;
                }
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviour.m_healthAccumulation, buildingData.m_position, radius);
            }
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- health: {0}", health);

            // Get the Wellbeing
            int wellbeing = 0;
            if (behaviour.m_wellbeingAccumulation != 0) {
                if (aliveCount != 0) {
                    wellbeing = (behaviour.m_wellbeingAccumulation + (aliveCount >> 1)) / aliveCount;
                }
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviour.m_wellbeingAccumulation, buildingData.m_position, radius);
            }
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- wellbeing: {0}", wellbeing);

            // Calculate Happiness
            int happiness = Citizen.GetHappiness(health, wellbeing);
            Logger.logInfo(LOG_SIMULATION, "NursingHomeAi.SimulationStepActive -- happiness: {0}", happiness);

            // TODO: Ignore Tax Stuff for now
            //int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(this.m_info.m_class, taxationPolicies);
            //int num8 = (int) (11 - Citizen.GetWealthLevel(this.m_info.m_class.m_level));
            //if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
            //    ++num8;
            //if (taxRate >= num8 + 4) {
            //    if ((int) buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32U) == 0) {
            //        int num1 = taxRate - num8 >> 2;
            //        buildingData.m_taxProblemTimer = (byte) Mathf.Min((int) byte.MaxValue, (int) buildingData.m_taxProblemTimer + num1);
            //        if ((int) buildingData.m_taxProblemTimer >= 96)
            //            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh | Notification.Problem.MajorProblem);
            //        else if ((int) buildingData.m_taxProblemTimer >= 32)
            //            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
            //    }
            //} else {
            //    buildingData.m_taxProblemTimer = (byte) Mathf.Max(0, (int) buildingData.m_taxProblemTimer - 1);
            //    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
            //}

            // Set Building Details
            buildingData.m_health = (byte) health;
            buildingData.m_happiness = (byte) happiness;
            buildingData.m_citizenCount = (byte) aliveCount;
            buildingData.m_education1 = (byte) behaviour.m_education1Count;
            buildingData.m_education2 = (byte) behaviour.m_education2Count;
            buildingData.m_education3 = (byte) behaviour.m_education3Count;
            buildingData.m_teens = (byte) behaviour.m_teenCount;
            buildingData.m_youngs = (byte) behaviour.m_youngCount;
            buildingData.m_adults = (byte) behaviour.m_adultCount;
            buildingData.m_seniors = (byte) behaviour.m_seniorCount;

            // Handle Sick and Dead
            this.HandleSick(buildingID, ref buildingData, ref behaviour, totalCount);
            this.HandleDead(buildingID, ref buildingData, ref behaviour, totalCount);

            // Handle Crime and Fire Factors
            int crimeAccumulation = behaviour.m_crimeAccumulation / 10;
            if ((policies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None) {
                crimeAccumulation = crimeAccumulation * 3 + 3 >> 2;
            }
            this.HandleCrime(buildingID, ref buildingData, crimeAccumulation, aliveCount);
            int crimeBuffer = (int) buildingData.m_crimeBuffer;
            int crimeRate;
            if (aliveCount != 0) {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveCount, buildingData.m_position, radius);
                // num1
                int fireFactor = (behaviour.m_educated0Count * 30 + behaviour.m_educated1Count * 15 + behaviour.m_educated2Count * 10) / aliveCount + 50;
                if ((int) buildingData.m_crimeBuffer > aliveCount * 40) {
                    fireFactor += 30;
                } else if ((int) buildingData.m_crimeBuffer > aliveCount * 15) {
                    fireFactor += 15;
                } else if ((int) buildingData.m_crimeBuffer > aliveCount * 5) {
                    fireFactor += 10;
                }
                buildingData.m_fireHazard = (byte) fireFactor;
                crimeRate = (crimeBuffer + (aliveCount >> 1)) / aliveCount;
            } else {
                buildingData.m_fireHazard = (byte) 0;
                crimeRate = 0;
            }

            // TODO: Ignore Land Value?
            //if ((cityPlanning & DistrictPolicies.CityPlanning.HighTechHousing) != DistrictPolicies.CityPlanning.None) {
            //    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 25, this.m_info.m_class);
            //    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.LandValue, 50, buildingData.m_position, radius);
            //}

            // TODO: Ignore Building Level?
            //SimulationManager instance2 = Singleton<SimulationManager>.instance;
            //if ((long) ((instance2.m_currentFrameIndex & 3840U) >> 8) == (long) ((int) buildingID & 15) && (int) Singleton<ZoneManager>.instance.m_lastBuildIndex == (int) instance2.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)
            //    this.CheckBuildingLevel(buildingID, ref buildingData, ref frameData, ref behaviour);

            // TODO: Ignore Building Construction?  What about Building Moving?
            //if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) == Building.Flags.None)
            //    return;

            // TODO: Ignore Citizen Moving Operations?
            //if (emptyHomeCount != 0 && (buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None && Singleton<SimulationManager>.instance.m_randomizer.Int32(5U) == 0) {
            //    TransferManager.TransferReason homeReason = this.GetHomeReason(buildingID, ref buildingData, ref Singleton<SimulationManager>.instance.m_randomizer);
            //    if (homeReason != TransferManager.TransferReason.None)
            //        Singleton<TransferManager>.instance.AddIncomingOffer(homeReason, new TransferManager.TransferOffer() {
            //            Priority = Mathf.Max(1, emptyHomeCount * 8 / homeCount),
            //            Building = buildingID,
            //            Position = buildingData.m_position,
            //            Amount = emptyHomeCount
            //        });
            //}

            districtManager.m_districts.m_buffer[(int) district].AddResidentialData(ref behaviour, aliveCount, health, happiness, crimeRate, homeCount, aliveHomeCount, emptyHomeCount, (int) this.m_info.m_class.m_level, modifiedElectricityConsumption, waterConsumption, modifiedSewageAccumulation, garbageAccumulation, modifiedIncomeAccumulation, Mathf.Min(100, (int) buildingData.m_garbageBuffer / 50), (int) buildingData.m_waterPollution * 100 / (int) byte.MaxValue, buildingData.SubCultureType);
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            this.HandleFire(buildingID, ref buildingData, ref frameData, policies);
        }

        public void GetConsumptionRates(Randomizer randomizer, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation) {
            ItemClass itemClass = this.m_info.m_class;
            electricityConsumption = 0;
            waterConsumption = 0;
            sewageAccumulation = 0;
            garbageAccumulation = 0;
            incomeAccumulation = 0;
            switch (this.quality) {
                case 0:
                    electricityConsumption = 20;
                    waterConsumption = 45;
                    sewageAccumulation = 45;
                    garbageAccumulation = 40;
                    // TODO: Possibly allow for income
                    //incomeAccumulation = 70;
                    break;
                case 1:
                    electricityConsumption = 18;
                    waterConsumption = 40;
                    sewageAccumulation = 40;
                    garbageAccumulation = 40;
                    //incomeAccumulation = 100;
                    break;
                case 2:
                    electricityConsumption = 18;
                    waterConsumption = 40;
                    sewageAccumulation = 40;
                    garbageAccumulation = 30;
                    //incomeAccumulation = 130;
                    break;
                case 3:
                    electricityConsumption = 16;
                    waterConsumption = 35;
                    sewageAccumulation = 35;
                    garbageAccumulation = 20;
                    //incomeAccumulation = 160;
                    break;
                case 4:
                    electricityConsumption = 16;
                    waterConsumption = 35;
                    sewageAccumulation = 35;
                    garbageAccumulation = 20;
                    //incomeAccumulation = 200;
                    break;
                case 5:
                    electricityConsumption = 14;
                    waterConsumption = 30;
                    sewageAccumulation = 30;
                    garbageAccumulation = 15;
                    //incomeAccumulation = 200;
                    break;
            }

            if (electricityConsumption != 0)
                electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption + randomizer.Int32(70U)) / 100;
            if (waterConsumption != 0) {
                int waterAndSewageConsumptionModifier = randomizer.Int32(70U);
                waterConsumption = Mathf.Max(100, productionRate * waterConsumption + waterAndSewageConsumptionModifier) / 100;
                if (sewageAccumulation != 0)
                    sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + waterAndSewageConsumptionModifier) / 100;
            } else if (sewageAccumulation != 0)
                sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + randomizer.Int32(70U)) / 100;
            if (garbageAccumulation != 0)
                garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation + randomizer.Int32(70U)) / 100;
            if (incomeAccumulation == 0)
                return;
            incomeAccumulation = productionRate * incomeAccumulation;
        }

        protected override void ManualActivation(ushort buildingId, ref Building buildingData) {
            NotificationEvent.Type notificationEventType = (this.quality >= 2 ? NotificationEvent.Type.Happy : NotificationEvent.Type.Sad);
            NotificationEvent.Type notificationWaveEventType = (notificationEventType == NotificationEvent.Type.Happy ? NotificationEvent.Type.GainHappiness : NotificationEvent.Type.LoseHappiness);
            Vector3 position = buildingData.m_position;
            position.y += this.m_info.m_size.y;
            Singleton<NotificationManager>.instance.AddEvent(notificationEventType, position, 1.5f);
            Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, notificationWaveEventType, ImmaterialResourceManager.Resource.DeathCare, NursingHomeAi.QUALITY_VALUES[this.quality], this.operationRadius);
        }

        protected override void ManualDeactivation(ushort buildingId, ref Building buildingData) {
            NotificationEvent.Type notificationEventType = (this.quality < 2 ? NotificationEvent.Type.Happy : NotificationEvent.Type.Sad);
            NotificationEvent.Type notificationWaveEventType = (notificationEventType == NotificationEvent.Type.Happy ? NotificationEvent.Type.GainHappiness : NotificationEvent.Type.LoseHappiness);
            Vector3 position = buildingData.m_position;
            position.y += this.m_info.m_size.y;
            Singleton<NotificationManager>.instance.AddEvent(notificationEventType, position, 1.5f);
            Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, notificationWaveEventType, ImmaterialResourceManager.Resource.DeathCare, NursingHomeAi.QUALITY_VALUES[this.quality], this.operationRadius);
        }

        public override float GetCurrentRange(ushort buildingId, ref Building data) {
            /* Logging stuff
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            Logger.logInfo(RANGE, "NursingHomeAi.GetPlacementInfoMode -- Stack Trace: {0}", stackTrace.ToString());
            Logger.logInfo(LOG_RANGE, "NursingHomeAi.GetPlacementInfoMode -- m_service: {0}", ReflectionHelper.GetInstanceField(typeof(CoverageManager), Singleton<CoverageManager>.instance, "m_service"));
            Logger.logInfo(LOG_RANGE, "NursingHomeAi.GetPlacementInfoMode -- m_subService: {0}", ReflectionHelper.GetInstanceField(typeof(CoverageManager), Singleton<CoverageManager>.instance, "m_subService"));
            Logger.logInfo(LOG_RANGE, "NursingHomeAi.GetPlacementInfoMode -- m_level: {0}", ReflectionHelper.GetInstanceField(typeof(CoverageManager), Singleton<CoverageManager>.instance, "m_level"));
            Logger.logInfo(LOG_RANGE, "NursingHomeAi.GetPlacementInfoMode -- m_buildingInfo: {0}", ReflectionHelper.GetInstanceField(typeof(CoverageManager), Singleton<CoverageManager>.instance, "m_buildingInfo"));
            Logger.logInfo(LOG_RANGE, "NursingHomeAi.GetPlacementInfoMode -- m_ignoreBuilding: {0}", ReflectionHelper.GetInstanceField(typeof(CoverageManager), Singleton<CoverageManager>.instance, "m_ignoreBuilding"));
            */

            /* TODO: Nursing Homes should be highlighted separate from HealthCare and DeathCare
            // Only handle range when placing a Nursing Home, not when looking at health info
            BuildingInfo buildingInfo = (BuildingInfo) ReflectionHelper.GetInstanceField(typeof (CoverageManager), Singleton<CoverageManager>.instance, "m_buildingInfo");
            if (buildingInfo != null && !(buildingInfo.m_buildingAI is NursingHomeAi)) {
                return 0.0f;
            }
            */

            // Mostly from PlayerBuildingAI
            int num = (int) data.m_productionRate;
            if ((data.m_flags & Building.Flags.Active) == Building.Flags.None)
                num = 0;
            else if ((data.m_flags & Building.Flags.RateReduced) != Building.Flags.None)
                num = Mathf.Min(num, 50);
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            return (float) ((double) PlayerBuildingAI.GetProductionRate(num, budget) * (double) this.operationRadius * 0.00999999977648258);
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingId, ref Building buildingData, ref Citizen.BehaviourData workerBehaviourData, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount) {
            workPlaceCount = workPlaceCount + this.numUneducatedWorkers + this.numEducatedWorkers + this.numWellEducatedWorkers + this.numHighlyEducatedWorkers;
            this.GetWorkBehaviour(buildingId, ref buildingData, ref workerBehaviourData, ref aliveWorkerCount, ref totalWorkerCount);
            this.HandleWorkPlaces(buildingId, ref buildingData, this.numUneducatedWorkers, this.numEducatedWorkers, this.numWellEducatedWorkers, this.numHighlyEducatedWorkers, ref workerBehaviourData, aliveWorkerCount, totalWorkerCount);
        }

        public override string GetLocalizedTooltip() {
            string str = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", (object) (this.GetWaterConsumption() * 16)) + System.Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", (object) (this.GetElectricityConsumption() * 16));
            return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, str, LocaleFormatter.Info2, String.Format("Number of Rooms: {0}", this.numRooms)));
        }
        
        public override string GetLocalizedStats(ushort buildingId, ref Building data) {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            int numResidents = 0;
            int numRoomsOccupied = 0;
            int counter = 0;

            // Calculate number of occupied rooms and total number of residents
            while ((int) citizenUnitIndex != 0) {
                uint nextCitizenUnitIndex = instance.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((instance.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    bool occupied = false;
                    for (int index = 0; index < 5; ++index) {
                        uint citizenId = instance.m_units.m_buffer[citizenUnitIndex].GetCitizen(index);
                        if (citizenId != 0) {
                            occupied = true;
                            numResidents++;
                        }
                    }
                    if (occupied) {
                        numRoomsOccupied++;
                    }
                }
                citizenUnitIndex = nextCitizenUnitIndex;
                if (++counter > 524288) {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            
            // Make the panel a little bit bigger to support the stats
            UIComponent infoPanel = UIView.library.Get(PanelHelper.INFO_PANEL_NAME);
            if (infoPanel.height < 339f) {
                infoPanel.height = 340f;
            }

            UIComponent statsPanel = infoPanel.Find(PanelHelper.STATS_PANEL_NAME);
            if (statsPanel.height < 124f) {
                statsPanel.height = 125f;
            }
            
            // Get Worker Data
            Citizen.BehaviourData workerBehaviourData = new Citizen.BehaviourData();
            int aliveWorkerCount = 0;
            int totalWorkerCount = 0;
            this.GetWorkBehaviour(buildingId, ref data, ref workerBehaviourData, ref aliveWorkerCount, ref totalWorkerCount);

            // Build Stats
            // TODO: Localize!!!
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("Uneducated Workers: {0} of {1}", workerBehaviourData.m_educated0Count, this.numUneducatedWorkers));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Educated Workers: {0} of {1}", workerBehaviourData.m_educated1Count, this.numEducatedWorkers));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Well Educated Workers: {0} of {1}", workerBehaviourData.m_educated2Count, this.numWellEducatedWorkers));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Highly Educated Workers: {0} of {1}", workerBehaviourData.m_educated3Count, this.numHighlyEducatedWorkers));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Nursing Home Quality: {0}", this.quality));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Rooms Occupied: {0} of {1}", numRoomsOccupied, this.numRooms));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Number of Residents: {0}", numResidents));
            return stringBuilder.ToString();
        }

        public override void CreateBuilding(ushort buildingId, ref Building data) {
            Logger.logInfo(LOG_BUILDING, "NursingHomeAI.CreateBuilding -- New Nursing Home Created: {0}", data.Info.name);
            base.CreateBuilding(buildingId, ref data);
            int workCount = this.numUneducatedWorkers + this.numEducatedWorkers + this.numWellEducatedWorkers + this.numHighlyEducatedWorkers;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingId, 0, this.numRooms, workCount, 0, 0, 0);

            // Ensure quality is within bounds
            if (this.quality < 0) {
                this.quality = 0;
            } else if (this.quality > 5) {
                this.quality = 5;
            }
        }

        public override void BuildingLoaded(ushort buildingId, ref Building data, uint version) {
            Logger.logInfo(LOG_BUILDING, "NursingHomeAI.BuildingLoaded -- New Nursing Home Loaded: {0}", data.Info.name);
            base.BuildingLoaded(buildingId, ref data, version);
            int workCount = this.numUneducatedWorkers + this.numEducatedWorkers + this.numWellEducatedWorkers + this.numHighlyEducatedWorkers;
            this.EnsureCitizenUnits(buildingId, ref data, this.numRooms, workCount, 0, 0);
        }

        public override void ReleaseBuilding(ushort buildingId, ref Building data) {
            Logger.logInfo(LOG_BUILDING, "NursingHomeAI.ReleaseBuilding -- Nursing Home Released: {0}", data.Info.name);
            base.ReleaseBuilding(buildingId, ref data);
        }

        public override bool RequireRoadAccess() {
            return true;
        }

        /* TODO: Workout how to have Nursing Homes Coverage display separate from Health Care
        private static ItemClass initNewItemClass() {
            ItemClass newNursingHomeItemClass = new ItemClass();
            newNursingHomeItemClass.m_level = ItemClass.Level.Level2;
            newNursingHomeItemClass.m_service = ItemClass.Service.HealthCare;
            newNursingHomeItemClass.m_subService = ItemClass.SubService.None;
            return newNursingHomeItemClass;
        }
        */
    }
}