using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;
using System.Threading;

namespace SeniorCitizenCenterMod {
    public class NursingHomeAi : PlayerBuildingAI {
        private const bool LOG_PRODUCTION = false;
        private const bool LOG_SIMULATION = false;
        private const bool LOG_RANGE = false;
        private const bool LOG_BUILDING = false;

        private static readonly float[] QUALITY_VALUES = { -50, -25, 10, 40, 70, 125 };

        // TODO: Workout how to have Nursing Homes Coverage display separate from Health Care
        //private static readonly ItemClass NURSING_HOME_ITEM_CLASS = NursingHomeAi.initNewItemClass();

        private Randomizer randomizer = new Randomizer(97);

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int numEducatedWorkers = 5;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int numHighlyEducatedWorkers = 4;

        [CustomizableProperty("Number of Rooms")]
        public int numRooms = 25;
        private float capacityModifier = 1.0f;

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

        public override int GetResourceRate(ushort buildingID, ref Building buildingData, EconomyManager.Resource resource) {
            if (resource == EconomyManager.Resource.Maintenance) {
                int amount = -((int) buildingData.m_productionRate * Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class) / 100 * (this.getTotalMaintenance(ref buildingData) / 100));
                return amount;
            }
            return base.GetResourceRate(buildingID, ref buildingData, resource);
        }

        private int getTotalMaintenance(ref Building buildingData) {
            return this.GetMaintenanceCost() + this.getCustomMaintenanceCost(ref buildingData);
        } 

        private int getCustomMaintenanceCost(ref Building buildingData) {
            int originalAmount = -(this.m_maintenanceCost * 100);

            SeniorCitizenCenterMod mod = SeniorCitizenCenterMod.getInstance();
            if (mod == null) {
                return 0;
            }

            OptionsManager optionsManager = mod.getOptionsManager();
            if (optionsManager == null) {
                return 0;
            }

            int numResidents;
            int numRoomsOccupied;
            this.getOccupancyDetails(ref buildingData, out numResidents, out numRoomsOccupied);
            float capacityModifier = (float) numRoomsOccupied / (float) this.getModifiedCapacity();
            int modifiedAmount = (int) ((float) originalAmount * capacityModifier);

            int amount = 0;
            switch (optionsManager.getIncomeModifier()) {
                case OptionsManager.IncomeValues.FULL_MAINTENANCE:
                    return 0;
                case OptionsManager.IncomeValues.HALF_MAINTENANCE:
                    amount = modifiedAmount / 2;
                    break;
                case OptionsManager.IncomeValues.NO_MAINTENANCE:
                    amount = modifiedAmount;
                    break;
                case OptionsManager.IncomeValues.NORMAL_PROFIT:
                    amount = modifiedAmount * 2;
                    break;
                case OptionsManager.IncomeValues.DOUBLE_DOUBLE:
                    amount = -originalAmount + (modifiedAmount * 4);
                    break;
                case OptionsManager.IncomeValues.DOUBLE_PROFIT:
                    amount = modifiedAmount * 3;
                    break;
            }

            if(amount == 0) {
                return 0;
            }
            
            Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref amount, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
            Logger.logInfo(Logger.LOG_INCOME, "getCustomMaintenanceCost - building: {0} - calculated maintenance amount: {1}", buildingData.m_buildIndex, amount);

            return amount;
        }

        public void handleAdditionalMaintenanceCost(ref Building buildingData) {
            int amount = this.getCustomMaintenanceCost(ref buildingData);
            if (amount == 0) {
                return;
            }

            int productionRate = (int) buildingData.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            amount = amount / 100;
            amount = productionRate * budget / 100 * amount / 100;
            Logger.logInfo(Logger.LOG_INCOME, "getCustomMaintenanceCost - building: {0} - adjusted maintenance amount: {1}", buildingData.m_buildIndex, amount);

            if ((buildingData.m_flags & Building.Flags.Original) == Building.Flags.None && amount != 0) {
                int result = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, amount, this.m_info.m_class);
            }
        }

        protected override void ProduceGoods(ushort buildingId, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount) {
            base.ProduceGoods(buildingId, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);

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
            districtManager.m_districts.m_buffer[(int) district].m_servicePoliciesEffect |= policies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.PetBan | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.SmokingBan | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity);


            // TODO: Ignore Tax Stuff? - Might want to add possabilitiy to collect taxes from Nursing Homes
            //if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialLow) {
            //    if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)) != (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)) {
            //        districtManager.m_districts.m_buffer[(int) district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow);
            //    }
            //} else if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)) != (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)) {
            //    districtManager.m_districts.m_buffer[(int) district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh);
            //}

            // No clue what these are for, setting some policies at the disctrict level?
            districtManager.m_districts.m_buffer[(int) district].m_cityPlanningPoliciesEffect |= cityPlanning & (DistrictPolicies.CityPlanning.HighTechHousing | DistrictPolicies.CityPlanning.HeavyTrafficBan | DistrictPolicies.CityPlanning.EncourageBiking | DistrictPolicies.CityPlanning.BikeBan | DistrictPolicies.CityPlanning.OldTown | DistrictPolicies.CityPlanning.AntiSlip);

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

            // Handle Heating
            int heatingConsumption = 0;
            if (modifiedElectricityConsumption != 0 && districtManager.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation)) {
                if ((policies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None) {
                    heatingConsumption = Mathf.Max(1, modifiedElectricityConsumption * 3 + 8 >> 4);
                } else
                    heatingConsumption = Mathf.Max(1, modifiedElectricityConsumption + 2 >> 2);
            }

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
                int commonConsumptionValue = this.HandleCommonConsumption(buildingID, ref buildingData, ref frameData, ref modifiedElectricityConsumption, ref heatingConsumption, ref waterConsumption, ref modifiedSewageAccumulation, ref garbageAccumulation, policies);

                // TODO: Possibly allow for income
                //modifiedIncomeAccumulation = (modifiedIncomeAccumulation * commonConsumptionValue + 99) / 100;
                //if (modifiedIncomeAccumulation != 0) {
                //    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, modifiedIncomeAccumulation, this.m_info.m_class, taxationPolicies);
                //}
                buildingData.m_flags |= Building.Flags.Active;
            } else {
                // Handle on fire
                modifiedElectricityConsumption = 0;
                heatingConsumption = 0;
                waterConsumption = 0;
                modifiedSewageAccumulation = 0;
                garbageAccumulation = 0;
                buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood | Notification.Problem.Heating);
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
            if ((buildingData.m_problems & Notification.Problem.MajorProblem) != Notification.Problem.None) {
                happiness -= happiness >> 1;
            } else if (buildingData.m_problems != Notification.Problem.None) {
                happiness -= happiness >> 2;
            }
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
            int crimeAccumulation = behaviour.m_crimeAccumulation / (3 * getModifiedCapacity());
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
            
            districtManager.m_districts.m_buffer[(int) district].AddResidentialData(ref behaviour, aliveCount, health, happiness, crimeRate, homeCount, aliveHomeCount, emptyHomeCount, (int) this.m_info.m_class.m_level, modifiedElectricityConsumption, heatingConsumption, waterConsumption, modifiedSewageAccumulation, garbageAccumulation, modifiedIncomeAccumulation, Mathf.Min(100, (int) buildingData.m_garbageBuffer / 50), (int) buildingData.m_waterPollution * 100 / (int) byte.MaxValue, this.m_info.m_class.m_subService);

            // Handle custom maintenance in addition to the standard maintenance handled in the base class
            this.handleAdditionalMaintenanceCost(ref buildingData);

            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            this.HandleFire(buildingID, ref buildingData, ref frameData, policies);
        }

        private static int GetAverageResidentRequirement(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnit = data.m_citizenUnits;
            int counter = 0;
            int requirement1 = 0;
            int requirement2 = 0;
            while ((int) citizenUnit != 0) {
                uint num5 = citizenManager.m_units.m_buffer[citizenUnit].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnit].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    int residentRequirement1 = 0;
                    int residentRequirement2 = 0;
                    for (int index = 0; index < 5; ++index) {
                        uint citizen = citizenManager.m_units.m_buffer[citizenUnit].GetCitizen(index);
                        if ((int) citizen != 0 && !citizenManager.m_citizens.m_buffer[citizen].Dead) {
                            residentRequirement1 += NursingHomeAi.GetResidentRequirement(resource, ref citizenManager.m_citizens.m_buffer[citizen]);
                            ++residentRequirement2;
                        }
                    }
                    if (residentRequirement2 == 0) {
                        requirement1 += 100;
                        ++requirement2;
                    } else {
                        requirement1 += residentRequirement1;
                        requirement2 += residentRequirement2;
                    }
                }
                citizenUnit = num5;
                if (++counter > 524288) {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
            if (requirement2 != 0)
                return (requirement1 + (requirement2 >> 1)) / requirement2;
            return 0;
        }

        private static int GetResidentRequirement(ImmaterialResourceManager.Resource resource, ref Citizen citizen) {
            switch (resource) {
                case ImmaterialResourceManager.Resource.HealthCare:
                    return Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.FireDepartment:
                    return Citizen.GetFireDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.PoliceDepartment:
                    return Citizen.GetPoliceDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.EducationElementary:
                    Citizen.AgePhase agePhase1 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase1 < Citizen.AgePhase.Teen0)
                        return Citizen.GetEducationRequirement(agePhase1);
                    return 0;
                case ImmaterialResourceManager.Resource.EducationHighSchool:
                    Citizen.AgePhase agePhase2 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase2 >= Citizen.AgePhase.Teen0 && agePhase2 < Citizen.AgePhase.Young0)
                        return Citizen.GetEducationRequirement(agePhase2);
                    return 0;
                case ImmaterialResourceManager.Resource.EducationUniversity:
                    Citizen.AgePhase agePhase3 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase3 >= Citizen.AgePhase.Young0)
                        return Citizen.GetEducationRequirement(agePhase3);
                    return 0;
                case ImmaterialResourceManager.Resource.DeathCare:
                    return Citizen.GetDeathCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.PublicTransport:
                    return Citizen.GetTransportRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.Entertainment:
                    return Citizen.GetEntertainmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                default:
                    return 100;
            }
        }

        public override float GetEventImpact(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource, float amount) {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
                return 0.0f;
            switch (resource) {
                case ImmaterialResourceManager.Resource.HealthCare:
                    int residentRequirement1 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local1;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local1);
                    int num1 = ImmaterialResourceManager.CalculateResourceEffect(local1, residentRequirement1, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local1 + Mathf.RoundToInt(amount), residentRequirement1, 500, 20, 40) - num1) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.FireDepartment:
                    int residentRequirement2 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local2;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local2);
                    int num2 = ImmaterialResourceManager.CalculateResourceEffect(local2, residentRequirement2, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local2 + Mathf.RoundToInt(amount), residentRequirement2, 500, 20, 40) - num2) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PoliceDepartment:
                    int residentRequirement3 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local3;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local3);
                    int num3 = ImmaterialResourceManager.CalculateResourceEffect(local3, residentRequirement3, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local3 + Mathf.RoundToInt(amount), residentRequirement3, 500, 20, 40) - num3) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.EducationElementary:
                case ImmaterialResourceManager.Resource.EducationHighSchool:
                case ImmaterialResourceManager.Resource.EducationUniversity:
                    int residentRequirement4 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local4;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local4);
                    int num4 = ImmaterialResourceManager.CalculateResourceEffect(local4, residentRequirement4, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local4 + Mathf.RoundToInt(amount), residentRequirement4, 500, 20, 40) - num4) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.DeathCare:
                    int residentRequirement5 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local5;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local5);
                    int num5 = ImmaterialResourceManager.CalculateResourceEffect(local5, residentRequirement5, 500, 10, 20);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local5 + Mathf.RoundToInt(amount), residentRequirement5, 500, 10, 20) - num5) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PublicTransport:
                    int residentRequirement6 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local6;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local6);
                    int num6 = ImmaterialResourceManager.CalculateResourceEffect(local6, residentRequirement6, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local6 + Mathf.RoundToInt(amount), residentRequirement6, 500, 20, 40) - num6) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.NoisePollution:
                    int local7;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local7);
                    int num7 = local7 * 100 / (int) byte.MaxValue;
                    return Mathf.Clamp((float) (Mathf.Clamp(local7 + Mathf.RoundToInt(amount), 0, (int) byte.MaxValue) * 100 / (int) byte.MaxValue - num7) / 50f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Entertainment:
                    int residentRequirement7 = NursingHomeAi.GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local8;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local8);
                    int num8 = ImmaterialResourceManager.CalculateResourceEffect(local8, residentRequirement7, 500, 30, 60);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local8 + Mathf.RoundToInt(amount), residentRequirement7, 500, 30, 60) - num8) / 30f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Abandonment:
                    int local9;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local9);
                    int num9 = ImmaterialResourceManager.CalculateResourceEffect(local9, 15, 50, 10, 20);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local9 + Mathf.RoundToInt(amount), 15, 50, 10, 20) - num9) / 50f, -1f, 1f);
                default:
                    return base.GetEventImpact(buildingID, ref data, resource, amount);
            }
        }

        public override float GetEventImpact(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource, float amount) {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
                return 0.0f;
            if (resource != NaturalResourceManager.Resource.Pollution)
                return base.GetEventImpact(buildingID, ref data, resource, amount);
            byte groundPollution;
            Singleton<NaturalResourceManager>.instance.CheckPollution(data.m_position, out groundPollution);
            int num = (int) groundPollution * 100 / (int) byte.MaxValue;
            return Mathf.Clamp((float) (Mathf.Clamp((int) groundPollution + Mathf.RoundToInt(amount), 0, (int) byte.MaxValue) * 100 / (int) byte.MaxValue - num) / 50f, -1f, 1f);
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
            Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, notificationWaveEventType, ImmaterialResourceManager.Resource.DeathCare, -NursingHomeAi.QUALITY_VALUES[this.quality], this.operationRadius);
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
            return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, str, LocaleFormatter.Info2, String.Format("Number of Rooms: {0}", getModifiedCapacity())));
        }
        
        public override string GetLocalizedStats(ushort buildingId, ref Building data) {
            int numResidents;
            int numRoomsOccupied;
            this.getOccupancyDetails(ref data, out numResidents, out numRoomsOccupied);

            // Make the panel a little bit bigger to support the stats
            UIComponent infoPanel = UIView.library.Get(PanelHelper.INFO_PANEL_NAME);
            if (infoPanel.height < 349f) {
                infoPanel.height = 350;
            }

            // Update the Upkeep Stats with custom value
            int maintenance = this.GetResourceRate(buildingId, ref data, EconomyManager.Resource.Maintenance);
            
            UIComponent infoGroupPanel = infoPanel.Find(PanelHelper.INFO_GROUP_PANEL_NAME);
            if(infoGroupPanel != null) {
                UILabel upkeepLabel = infoGroupPanel.Find<UILabel>(PanelHelper.UPKEEP_LABEL_NAME);
                if(upkeepLabel != null) {
                    if (maintenance > 0) {
                        String upkeepText = LocaleFormatter.FormatUpkeep(maintenance, false);
                        upkeepLabel.text = upkeepText.Replace("Upkeep", "Profit");
                        upkeepLabel.textColor = Color.green;
                    } else {
                        upkeepLabel.textColor = PanelHelper.originalUpkeepColor;
                    }
                }
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
            stringBuilder.Append(string.Format("Rooms Occupied: {0} of {1}", numRoomsOccupied, getModifiedCapacity()));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Number of Residents: {0}", numResidents));
            return stringBuilder.ToString();
        }

        private void getOccupancyDetails(ref Building data, out int numResidents, out int numRoomsOccupied) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            numResidents = 0;
            numRoomsOccupied = 0;
            int counter = 0;

            // Calculate number of occupied rooms and total number of residents
            while ((int) citizenUnitIndex != 0) {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    bool occupied = false;
                    for (int index = 0; index < 5; ++index) {
                        uint citizenId = citizenManager.m_units.m_buffer[citizenUnitIndex].GetCitizen(index);
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
        }

        public void updateCapacity(float newCapacityModifier) {
            Logger.logInfo(Logger.LOG_OPTIONS, "NursingHomeAI.updateCapacity -- Updating capacity with modifier: {0}", newCapacityModifier);
            // Set the capcityModifier and check to see if the value actually changes
            if (Interlocked.Exchange(ref this.capacityModifier, newCapacityModifier) == newCapacityModifier) {
                // Capcity has already been set to this value, nothing to do
                Logger.logInfo(Logger.LOG_OPTIONS, "NursingHomeAI.updateCapacity -- Skipping capacity change because the value was already set");
                return;
            }
        }

        private int getModifiedCapacity() {
            return (this.capacityModifier > 0 ? (int) (this.numRooms * this.capacityModifier) : this.numRooms);
        }

        public void validateCapacity(ushort buildingId, ref Building data, bool shouldCreateRooms) {
            int numRoomsExpected = this.getModifiedCapacity();
            
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            uint lastCitizenUnitIndex = 0;
            int numRoomsFound = 0;

            // Count the number of rooms
            while ((int) citizenUnitIndex != 0) {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    numRoomsFound++;
                }
                lastCitizenUnitIndex = citizenUnitIndex;
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            Logger.logInfo(Logger.LOG_CAPACITY_MANAGEMENT, "NursingHomeAi.validateCapacity -- Checking Expected Capacity {0} vs Current Capacity {1} for Building {2}", numRoomsExpected, numRoomsFound, buildingId);
            // Check to see if the correct amount of rooms are present, otherwise adjust accordingly
            if (numRoomsFound == numRoomsExpected) {
                return;
            } else if (numRoomsFound < numRoomsExpected) {
                if (shouldCreateRooms) {
                    // Only create rooms after a building is already loaded, otherwise let EnsureCitizenUnits to create them
                    this.createRooms((numRoomsExpected - numRoomsFound), buildingId, ref data, lastCitizenUnitIndex);
                }
            } else {
                this.deleteRooms((numRoomsFound - numRoomsExpected), buildingId, ref data);
            }
        }

        private void createRooms(int numRoomsToCreate, ushort buildingId, ref Building data, uint lastCitizenUnitIndex) {
            Logger.logInfo(Logger.LOG_CAPACITY_MANAGEMENT, "NursingHomeAi.createRooms -- Creating {0} Rooms", numRoomsToCreate);
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            uint firstUnit = 0;
            citizenManager.CreateUnits(out firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingId, (ushort) 0, numRoomsToCreate, 0, 0, 0, 0);
            citizenManager.m_units.m_buffer[lastCitizenUnitIndex].m_nextUnit = firstUnit;
        }

        private void deleteRooms(int numRoomsToDelete, ushort buildingId, ref Building data) {
            Logger.logInfo(Logger.LOG_CAPACITY_MANAGEMENT, "NursingHomeAi.deleteRooms -- Deleting {0} Rooms", numRoomsToDelete);
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            
            // Always start with the second to avoid loss of pointer from the building to the first unit
            uint prevUnit = data.m_citizenUnits;
            uint citizenUnitIndex = citizenManager.m_units.m_buffer[data.m_citizenUnits].m_nextUnit;

            // First try to delete empty rooms
            while (numRoomsToDelete > 0 && (int) citizenUnitIndex != 0) {
                bool deleted = false;
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    if (citizenManager.m_units.m_buffer[citizenUnitIndex].Empty()) {
                        this.deleteRoom(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
                        numRoomsToDelete--;
                        deleted = true;
                    }
                }
                if(!deleted) {
                    prevUnit = citizenUnitIndex;
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            // Check to see if enough rooms were deleted
            if(numRoomsToDelete == 0) {
                return;
            }

            Logger.logInfo(Logger.LOG_CAPACITY_MANAGEMENT, "NursingHomeAi.deleteRooms -- Deleting {0} Occupied Rooms", numRoomsToDelete);
            // Still need to delete more rooms so start deleting rooms with people in them...
            // Always start with the second to avoid loss of pointer from the building to the first unit
            prevUnit = data.m_citizenUnits;
            citizenUnitIndex = citizenManager.m_units.m_buffer[data.m_citizenUnits].m_nextUnit;

            // Delete any rooms still available until the correct number is acheived
            while (numRoomsToDelete > 0 && (int) citizenUnitIndex != 0) {
                bool deleted = false;
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                    this.deleteRoom(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
                    numRoomsToDelete--;
                    deleted = true;
                }
                if (!deleted) {
                    prevUnit = citizenUnitIndex;
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }
        }

        private void deleteRoom(uint unit, ref CitizenUnit data, uint prevUnit) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Update the pointer to bypass this unit
            citizenManager.m_units.m_buffer[prevUnit].m_nextUnit = data.m_nextUnit;

            // Release all the citizens
            this.releaseUnitCitizen(data.m_citizen0, ref data);
            this.releaseUnitCitizen(data.m_citizen1, ref data);
            this.releaseUnitCitizen(data.m_citizen2, ref data);
            this.releaseUnitCitizen(data.m_citizen3, ref data);
            this.releaseUnitCitizen(data.m_citizen4, ref data);

            // Release the Unit
            data = new CitizenUnit();
            citizenManager.m_units.ReleaseItem(unit);
        }

        private void releaseUnitCitizen(uint citizen, ref CitizenUnit data) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            if ((int) citizen == 0) {
                return;
            }
            if ((data.m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) {
                citizenManager.m_citizens.m_buffer[citizen].m_homeBuilding = 0;
            }
            if ((data.m_flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != CitizenUnit.Flags.None) {
                citizenManager.m_citizens.m_buffer[citizen].m_workBuilding = 0;
            }
            if ((data.m_flags & CitizenUnit.Flags.Visit) != CitizenUnit.Flags.None) {
                citizenManager.m_citizens.m_buffer[citizen].m_visitBuilding = 0;
            }
            if ((data.m_flags & CitizenUnit.Flags.Vehicle) == CitizenUnit.Flags.None) {
                return;
            }
            citizenManager.m_citizens.m_buffer[citizen].m_vehicle = 0;
        }

        public override void CreateBuilding(ushort buildingId, ref Building data) {
            Logger.logInfo(LOG_BUILDING, "NursingHomeAI.CreateBuilding -- New Nursing Home Created: {0}", data.Info.name);
            base.CreateBuilding(buildingId, ref data);
            int workCount = this.numUneducatedWorkers + this.numEducatedWorkers + this.numWellEducatedWorkers + this.numHighlyEducatedWorkers;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingId, 0, getModifiedCapacity(), workCount, 0, 0, 0);

            // Ensure quality is within bounds
            if (this.quality < 0) {
                this.quality = 0;
            } else if (this.quality > 5) {
                this.quality = 5;
            }
        }

        public override void BuildingLoaded(ushort buildingId, ref Building data, uint version) {
            Logger.logInfo(LOG_BUILDING, "NursingHomeAI.BuildingLoaded -- Nursing Home Loaded: {0}", data.Info.name);
            base.BuildingLoaded(buildingId, ref data, version);

            // Validate the capacity and adjust accordingly - but don't create new units, that will be done by EnsureCitizenUnits
            float capcityModifier = SeniorCitizenCenterMod.getInstance().getOptionsManager().getCapacityModifier();
            this.updateCapacity(capcityModifier);
            this.validateCapacity(buildingId, ref data, false);

            int workCount = this.numUneducatedWorkers + this.numEducatedWorkers + this.numWellEducatedWorkers + this.numHighlyEducatedWorkers;
            this.EnsureCitizenUnits(buildingId, ref data, getModifiedCapacity(), workCount, 0, 0);
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