﻿using ColossalFramework;
using ColossalFramework.Math;
using RealCity.Util;
using RealCity.UI;
using Harmony;
using System.Reflection;
using RealCity.CustomData;
using RealCity.CustomAI;
using UnityEngine;
using System;

namespace RealCity.Patch
{
    [HarmonyPatch]
    public class PrivateBuildingAISimulationStepActivePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(PrivateBuildingAI).GetMethod("SimulationStepActive", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Prefix(ref Building buildingData, ref ushort[] __state)
        {
            __state = new ushort[2];
            __state[0] = buildingData.m_customBuffer1;
            __state[1] = buildingData.m_customBuffer2;
        }

        public static void Postfix(ushort buildingID, ref Building buildingData, ref ushort[] __state)
        {
            LimitAndCheckOfficeMoney(buildingData, buildingID);
            ProcessLandFeeNoOffice(buildingData, buildingID);
            LimitCommericalBuildingAccess(buildingID, ref buildingData);
            ProcessBuildingDataFinal(buildingID, ref buildingData);
        }
        
        public static void ProcessBuildingDataFinal(ushort buildingID, ref Building buildingData)
        {
            if (RealCityPrivateBuildingAI.preBuidlingId > buildingID)
            {
                RealCityPrivateBuildingAI.allOfficeHighTechWorkCountFinal = RealCityPrivateBuildingAI.allOfficeHighTechWorkCount;
                RealCityPrivateBuildingAI.allOfficeLevel1WorkCountFinal = RealCityPrivateBuildingAI.allOfficeLevel1WorkCount;
                RealCityPrivateBuildingAI.allOfficeLevel2WorkCountFinal = RealCityPrivateBuildingAI.allOfficeLevel2WorkCount;
                RealCityPrivateBuildingAI.allOfficeLevel3WorkCountFinal = RealCityPrivateBuildingAI.allOfficeLevel3WorkCount;
                RealCityPrivateBuildingAI.profitBuildingCountFinal = RealCityPrivateBuildingAI.profitBuildingCount;
                BuildingData.commBuildingNumFinal = BuildingData.commBuildingNum;
                BuildingData.commBuildingNum = 0;
                RealCityPrivateBuildingAI.profitBuildingCount = 0;
                RealCityPrivateBuildingAI.allOfficeLevel1WorkCount = 0;
                RealCityPrivateBuildingAI.allOfficeLevel2WorkCount = 0;
                RealCityPrivateBuildingAI.allOfficeLevel3WorkCount = 0;
                RealCityPrivateBuildingAI.allOfficeHighTechWorkCount = 0;
            }
            RealCityPrivateBuildingAI.preBuidlingId = buildingID;
            if (buildingData.Info.m_class.m_service == ItemClass.Service.Residential)
            {
                BuildingData.buildingMoney[buildingID] = 0;
            }

            float building_money = BuildingData.buildingMoney[buildingID];

            if (buildingData.Info.m_class.m_service == ItemClass.Service.Industrial)
            {
                if (building_money < 0)
                    RealCityEconomyExtension.industrialLackMoneyCount++;
                else
                    RealCityEconomyExtension.industrialEarnMoneyCount++;
            }

            if (building_money < 0)
                BuildingData.buildingMoneyThreat[buildingID] = 1.0f;

            switch (buildingData.Info.m_class.m_service)
            {
                case ItemClass.Service.Residential:
                    break;

                case ItemClass.Service.Commercial:
                case ItemClass.Service.Industrial:
                    int num = 0; int num1 = 0;
                    float averageBuildingSalary = BuildingUI.CaculateEmployeeOutcome(buildingData, buildingID, out num, out num1);

                    if (MainDataStore.citizenCount > 0.0)
                    {
                        float averageCitySalary = MainDataStore.citizenSalaryTotal / MainDataStore.citizenCount;
                        float salaryFactor = averageBuildingSalary / averageCitySalary;
                        if (salaryFactor > 1.6f)
                            salaryFactor = 1.6f;
                        else if (salaryFactor < 0.0f)
                            salaryFactor = 0.0f;

                        BuildingData.buildingMoneyThreat[buildingID] = (1.0f - salaryFactor / 1.6f);
                    }
                    else
                        BuildingData.buildingMoneyThreat[buildingID] = 1.0f;

                    break;
            }

            if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
            {
                //get all commercial building for resident.
                if (buildingData.m_customBuffer2 > 2000)
                {
                    BuildingData.commBuildingNum++;
                    BuildingData.commBuildingID[BuildingData.commBuildingNum] = buildingID;
                }
                if (BuildingData.buildingMoney[buildingID] < 0)
                {
                    RealCityEconomyExtension.commericalLackMoneyCount++;
                }
                else
                {
                    RealCityEconomyExtension.commericalEarnMoneyCount++;
                }
            }

            if (buildingData.m_problems == Notification.Problem.None)
            {
                //mark no good
                if ((buildingData.Info.m_class.m_service == ItemClass.Service.Commercial) && (RealCity.debugMode))
                {
                    Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoGoods);
                    if (buildingData.m_customBuffer2 < 500)
                    {
                        problem = Notification.AddProblems(problem, Notification.Problem.NoGoods | Notification.Problem.MajorProblem);
                    }
                    else if (buildingData.m_customBuffer2 < 1000)
                    {
                        problem = Notification.AddProblems(problem, Notification.Problem.NoGoods);
                    }
                    else
                    {
                        problem = Notification.Problem.None;
                    }
                    buildingData.m_problems = problem;
                }
            }
        }

        public static void LimitCommericalBuildingAccess(ushort buildingID, ref Building buildingData)
        {
            if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
            {
                Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                int alivevisitCount = 0;
                int totalvisitCount = 0;
                int maxcount = 0;
                var AI = buildingData.Info.m_buildingAI as CommercialBuildingAI;
                GetVisitBehaviour(buildingID, ref buildingData, ref behaviour, ref alivevisitCount, ref totalvisitCount);                
                maxcount = AI.CalculateVisitplaceCount((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), buildingData.m_width, buildingData.m_length);
                if (maxcount <= totalvisitCount)
                {
                    buildingData.m_flags &= ~Building.Flags.Active;
                }
                else if (buildingData.m_customBuffer2 < (MainDataStore.maxGoodPurchase + 100))
                {
                    buildingData.m_flags &= ~Building.Flags.Active;
                }
            }
        }

        public static void GetVisitBehaviour(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = buildingData.m_citizenUnits;
            int num2 = 0;
            do
            {
                if (num == 0)
                {
                    return;
                }
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    instance.m_units.m_buffer[num].GetCitizenVisitBehaviour(ref behaviour, ref aliveCount, ref totalCount);
                }
                num = instance.m_units.m_buffer[num].m_nextUnit;
            }
            while (++num2 <= 524288);
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
        }

        public static void LimitAndCheckOfficeMoney(Building building, ushort buildingID)
        {
            if (BuildingData.buildingMoney[buildingID] > 60000000)
            {
                BuildingData.buildingMoney[buildingID] = 60000000;
            }
            else if (BuildingData.buildingMoney[buildingID] < -60000000)
            {
                BuildingData.buildingMoney[buildingID] = -60000000;
            }

            if (BuildingData.buildingMoney[buildingID] > 0)
            {
                if (building.Info.m_class.m_service == ItemClass.Service.Industrial || building.Info.m_class.m_service == ItemClass.Service.Commercial)
                {
                    if (!(building.Info.m_buildingAI is IndustrialExtractorAI))
                    {
                        RealCityPrivateBuildingAI.profitBuildingCount++;
                        float bossTake = 0;
                        float investToOffice = 0;
                        // boss take and return to office
                        if (building.Info.m_class.m_subService != ItemClass.SubService.IndustrialGeneric  && building.Info.m_class.m_subService != ItemClass.SubService.CommercialHigh && building.Info.m_class.m_subService != ItemClass.SubService.CommercialLow)
                        {
                            if (building.Info.m_class.m_subService == ItemClass.SubService.CommercialLeisure || building.Info.m_class.m_subService == ItemClass.SubService.CommercialTourist)
                            {
                                investToOffice = 0.0025f;
                            } else
                            {
                                investToOffice = 0.0005f;
                            }
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                        {
                            investToOffice = 0.001f;
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                        {
                            investToOffice = 0.0015f;
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                        {
                            investToOffice = 0.002f;
                        }
                        // Boss will to take 
                        if (building.Info.m_class.m_subService != ItemClass.SubService.IndustrialGeneric && building.Info.m_class.m_subService != ItemClass.SubService.CommercialHigh && building.Info.m_class.m_subService != ItemClass.SubService.CommercialLow)
                        {
                            if (building.Info.m_class.m_subService == ItemClass.SubService.CommercialLeisure || building.Info.m_class.m_subService == ItemClass.SubService.CommercialTourist)
                            {
                                bossTake = 0.005f;
                            }
                            else
                            {
                                bossTake = 0.001f;
                            }
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                        {
                            bossTake = 0.002f;
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                        {
                            bossTake = 0.003f;
                        }
                        else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                        {
                            bossTake = 0.004f;
                        }

                        RealCityPrivateBuildingAI.profitBuildingMoney += (long)(BuildingData.buildingMoney[buildingID] * investToOffice);
                        BuildingData.buildingMoney[buildingID] -= (long)(BuildingData.buildingMoney[buildingID] * bossTake);
                    }
                }
            }

            if (building.Info.m_class.m_service == ItemClass.Service.Office)
            {
                Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
                int aliveWorkerCount = 0;
                int totalWorkerCount = 0;
                BuildingUI.GetWorkBehaviour(buildingID, ref building, ref behaviourData, ref aliveWorkerCount, ref totalWorkerCount);
                int allOfficeWorker = RealCityPrivateBuildingAI.allOfficeLevel1WorkCountFinal + RealCityPrivateBuildingAI.allOfficeLevel2WorkCountFinal + RealCityPrivateBuildingAI.allOfficeLevel3WorkCountFinal + RealCityPrivateBuildingAI.allOfficeHighTechWorkCountFinal;
                double a = 0;
                double z = 0;
                double c = allOfficeWorker * MainDataStore.citizenCount << 1;
                if (allOfficeWorker != 0 && (c != 0))
                {
                    a = (RealCityPrivateBuildingAI.profitBuildingMoneyFinal / (double)(c));
                }
                if ((MainDataStore.citizenCount != 0))
                {
                    z = Math.Pow((aliveWorkerCount / (float)MainDataStore.citizenCount), a);
                }

                if (building.Info.m_class.m_subService == ItemClass.SubService.OfficeGeneric)
                {
                    if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                    {
                        if (allOfficeWorker != 0)
                        {
                            BuildingData.buildingMoney[buildingID] = (float)(RealCityPrivateBuildingAI.profitBuildingMoneyFinal * 0.65f * z * (aliveWorkerCount / (float)allOfficeWorker));
                            BuildingData.buildingMoney[buildingID] = (BuildingData.buildingMoney[buildingID] > 0) ? BuildingData.buildingMoney[buildingID] : 0;
                        }
                        else
                        {
                            BuildingData.buildingMoney[buildingID] = 0;
                        }
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                    {
                        if (allOfficeWorker != 0)
                        {
                            BuildingData.buildingMoney[buildingID] = (float)(RealCityPrivateBuildingAI.profitBuildingMoneyFinal * 0.7f * z * (aliveWorkerCount / (float)allOfficeWorker));
                            BuildingData.buildingMoney[buildingID] = (BuildingData.buildingMoney[buildingID] > 0) ? BuildingData.buildingMoney[buildingID] : 0;
                        }
                        else
                        {
                            BuildingData.buildingMoney[buildingID] = 0;
                        }
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                    {
                        if (allOfficeWorker != 0)
                        {
                            BuildingData.buildingMoney[buildingID] = (float)(RealCityPrivateBuildingAI.profitBuildingMoneyFinal * 0.8f * z * (aliveWorkerCount / (float)allOfficeWorker));
                            BuildingData.buildingMoney[buildingID] = (BuildingData.buildingMoney[buildingID] > 0) ? BuildingData.buildingMoney[buildingID] : 0;
                        }
                        else
                        {
                            BuildingData.buildingMoney[buildingID] = 0;
                        }
                    }
                }
                else if (building.Info.m_class.m_subService == ItemClass.SubService.OfficeHightech)
                {
                    if (allOfficeWorker != 0)
                    {
                        BuildingData.buildingMoney[buildingID] = (float)(RealCityPrivateBuildingAI.profitBuildingMoneyFinal * 0.75f * z * (aliveWorkerCount / (float)allOfficeWorker));
                        BuildingData.buildingMoney[buildingID] = (BuildingData.buildingMoney[buildingID] > 0) ? BuildingData.buildingMoney[buildingID] : 0;
                    }
                    else
                    {
                        BuildingData.buildingMoney[buildingID] = 0;
                    }
                }

                ProcessLandFeeOffice(building, buildingID, BuildingData.buildingMoney[buildingID]);
            }
        }

        public static void ProcessLandFeeOffice(Building building, ushort buildingID, float money)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(building.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;

            int landFee = (int)money;
            int taxRate;
            taxRate = Singleton<EconomyManager>.instance.GetTaxRate(building.Info.m_class, taxationPolicies);

            if (instance.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
            {
                if ((servicePolicies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None)
                {
                    landFee = landFee * 95 / 100;
                }
            }
            if ((servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
            {
                landFee = landFee * 95 / 100;
            }

            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num4 = (int)(currentFrameIndex & 4095u);
            if (((num4 >> 8) & 15u) == (buildingID & 15u))
            {
                Singleton<EconomyManager>.instance.AddPrivateIncome(landFee, building.Info.m_class.m_service, building.Info.m_class.m_subService, building.Info.m_class.m_level, taxRate * 100);
            }
        }

        public static void ProcessLandFeeNoOffice(Building building, ushort buildingID)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(building.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;

            int landFee;
            GetLandRentNoOffice(out landFee, building, buildingID);
            int taxRate;
            taxRate = Singleton<EconomyManager>.instance.GetTaxRate(building.Info.m_class, taxationPolicies);
            if (BuildingData.buildingMoney[buildingID] <= 0)
            {
                landFee = 0;
            }
            if (((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None) && (building.Info.m_class.m_subService == ItemClass.SubService.CommercialLeisure))
            {
                landFee = 0;
            }
            landFee = (int)(landFee * ((float)(instance.m_districts.m_buffer[district].GetLandValue() + 50) / 100));

            if (instance.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
            {
                if ((servicePolicies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None)
                {
                    landFee = landFee * 95 / 100;
                }
            }
            if ((servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
            {
                landFee = landFee * 95 / 100;
            }

            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num4 = (int)(currentFrameIndex & 4095u);
            if (((num4 >> 8) & 15u) == (buildingID & 15u))
            {
                Singleton<EconomyManager>.instance.AddPrivateIncome(landFee, building.Info.m_class.m_service, building.Info.m_class.m_subService, building.Info.m_class.m_level, taxRate * 100);
                if ((building.Info.m_class.m_service == ItemClass.Service.Commercial) || (building.Info.m_class.m_service == ItemClass.Service.Industrial))
                {
                    BuildingData.buildingMoney[buildingID] = (BuildingData.buildingMoney[buildingID] - (float)(landFee * taxRate) / 100);
                }
            }
        }

        public static void GetLandRentNoOffice(out int incomeAccumulation, Building building, ushort buildingID)
        {
            ItemClass @class = building.Info.m_class;
            incomeAccumulation = 0;
            Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
            int aliveWorkerCount = 0;
            int totalWorkerCount = 0;
            ItemClass.SubService subService = @class.m_subService;
            switch (subService)
            {
                case ItemClass.SubService.OfficeHightech:
                    BuildingUI.GetWorkBehaviour(buildingID, ref building, ref behaviourData, ref aliveWorkerCount, ref totalWorkerCount);
                    //incomeAccumulation = (MainDataStore.office_high_tech);
                    RealCityPrivateBuildingAI.allOfficeHighTechWorkCount += aliveWorkerCount;
                    break;
                case ItemClass.SubService.OfficeGeneric:
                    BuildingUI.GetWorkBehaviour(buildingID, ref building, ref behaviourData, ref aliveWorkerCount, ref totalWorkerCount);
                    if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                    {
                        //incomeAccumulation = (MainDataStore.office_gen_levell);
                        RealCityPrivateBuildingAI.allOfficeLevel1WorkCount += aliveWorkerCount;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                    {
                        //incomeAccumulation = (MainDataStore.office_gen_level2);
                        RealCityPrivateBuildingAI.allOfficeLevel2WorkCount += aliveWorkerCount;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                    {
                        //incomeAccumulation = (MainDataStore.office_gen_level3);
                        RealCityPrivateBuildingAI.allOfficeLevel3WorkCount += aliveWorkerCount;
                    }
                    break;
                case ItemClass.SubService.IndustrialFarming:
                    incomeAccumulation = MainDataStore.indu_farm;
                    break;
                case ItemClass.SubService.IndustrialForestry:
                    incomeAccumulation = MainDataStore.indu_forest;
                    break;
                case ItemClass.SubService.IndustrialOil:
                    incomeAccumulation = MainDataStore.indu_oil;
                    break;
                case ItemClass.SubService.IndustrialOre:
                    incomeAccumulation = MainDataStore.indu_ore;
                    break;
                case ItemClass.SubService.IndustrialGeneric:
                    if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                    {
                        incomeAccumulation = MainDataStore.indu_gen_level1;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                    {
                        incomeAccumulation = MainDataStore.indu_gen_level2;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                    {
                        incomeAccumulation = MainDataStore.indu_gen_level3;
                    }
                    break;
                case ItemClass.SubService.CommercialHigh:
                    if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                    {
                        incomeAccumulation = MainDataStore.comm_high_level1;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                    {
                        incomeAccumulation = MainDataStore.comm_high_level2;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                    {
                        incomeAccumulation = MainDataStore.comm_high_level3;
                    }
                    break;
                case ItemClass.SubService.CommercialLow:
                    if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                    {
                        incomeAccumulation = MainDataStore.comm_low_level1;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                    {
                        incomeAccumulation = MainDataStore.comm_low_level2;
                    }
                    else if (building.Info.m_class.m_level == ItemClass.Level.Level3)
                    {
                        incomeAccumulation = MainDataStore.comm_low_level3;
                    }
                    break;
                case ItemClass.SubService.CommercialLeisure:
                    incomeAccumulation = MainDataStore.comm_leisure;
                    break;
                case ItemClass.SubService.CommercialTourist:
                    incomeAccumulation = MainDataStore.comm_tourist;
                    break;
                case ItemClass.SubService.CommercialEco:
                    incomeAccumulation = MainDataStore.comm_eco;
                    break;
                default: break;
            }
        }
    }
}