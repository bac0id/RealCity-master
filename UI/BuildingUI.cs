﻿using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using System;
using ColossalFramework.Math;
using RealCity.CustomAI;
using RealCity.Util;
using RealCity.CustomData;

namespace RealCity.UI
{
    public class BuildingUI : UIPanel
    {
        public static readonly string cacheName = "BuildingUI";
        private static readonly float SPACING = 15f;
        private static readonly float SPACING22 = 22f;
        private Dictionary<string, UILabel> _valuesControlContainer = new Dictionary<string, UILabel>(16);
        public ZonedBuildingWorldInfoPanel baseBuildingWindow;
        public static bool refeshOnce = false;
        private UILabel buildingMoney;
        private UILabel buildingIncomeBuffer;
        private UILabel buildingOutgoingBuffer;
        private UILabel employFee;
        private UILabel landRent;
        private UILabel buyPrice;
        private UILabel sellPrice;
        private UILabel comsuptionDivide;
        private UILabel sellTax;
        private UILabel profit;
        private UILabel usedcar;

        public override void Update()
        {
            RefreshDisplayData();
            base.Update();
        }

        public override void Awake()
        {
            base.Awake();
            DoOnStartup();
        }

        public override void Start()
        {
            base.Start();
            canFocus = true;
            isInteractive = true;
            isVisible = true;
            opacity = 1f;
            cachedName = cacheName;
            RefreshDisplayData();
            Hide();
        }

        private void DoOnStartup()
        {
            ShowOnGui();
            Hide();          
        }

        private void ShowOnGui()
        {
            buildingMoney = AddUIComponent<UILabel>();
            buildingMoney.text = Localization.Get("BUILDING_MONEY");
            buildingMoney.relativePosition = new Vector3(SPACING, 50f);
            buildingMoney.autoSize = true;

            buildingIncomeBuffer = AddUIComponent<UILabel>();
            buildingIncomeBuffer.text = Localization.Get("MATERIAL_BUFFER");
            buildingIncomeBuffer.relativePosition = new Vector3(SPACING, buildingMoney.relativePosition.y + SPACING22);
            buildingIncomeBuffer.autoSize = true;

            buildingOutgoingBuffer = AddUIComponent<UILabel>();
            buildingOutgoingBuffer.text = Localization.Get("PRODUCTION_BUFFER");
            buildingOutgoingBuffer.relativePosition = new Vector3(SPACING, buildingIncomeBuffer.relativePosition.y + SPACING22);
            buildingOutgoingBuffer.autoSize = true;

            employFee = AddUIComponent<UILabel>();
            employFee.text = Localization.Get("AVERAGE_EMPLOYFEE");
            employFee.relativePosition = new Vector3(SPACING, buildingOutgoingBuffer.relativePosition.y + SPACING22);
            employFee.autoSize = true;

            landRent = AddUIComponent<UILabel>();
            landRent.text = Localization.Get("BUILDING_LANDRENT");
            landRent.relativePosition = new Vector3(SPACING, employFee.relativePosition.y + SPACING22);
            landRent.autoSize = true;

            buyPrice = AddUIComponent<UILabel>();
            buyPrice.text = Localization.Get("BUY_PRICE");
            buyPrice.relativePosition = new Vector3(SPACING, landRent.relativePosition.y + SPACING22);
            buyPrice.autoSize = true;

            sellPrice = AddUIComponent<UILabel>();
            sellPrice.text = Localization.Get("SELL_PRICE");
            sellPrice.relativePosition = new Vector3(SPACING, buyPrice.relativePosition.y + SPACING22);
            sellPrice.autoSize = true;

            comsuptionDivide = AddUIComponent<UILabel>();
            comsuptionDivide.text = Localization.Get("MATERIAL_DIV_PRODUCTION");
            comsuptionDivide.relativePosition = new Vector3(SPACING, sellPrice.relativePosition.y + SPACING22);
            comsuptionDivide.autoSize = true;

            sellTax = AddUIComponent<UILabel>();
            sellTax.text = Localization.Get("SELL_TAX");
            sellTax.relativePosition = new Vector3(SPACING, comsuptionDivide.relativePosition.y + SPACING22);
            sellTax.autoSize = true;

            profit = AddUIComponent<UILabel>();
            profit.text = Localization.Get("PROFIT");
            profit.relativePosition = new Vector3(SPACING, sellTax.relativePosition.y + SPACING22);
            profit.autoSize = true;

            usedcar = AddUIComponent<UILabel>();
            usedcar.text = Localization.Get("CAR_USED");
            usedcar.relativePosition = new Vector3(SPACING, profit.relativePosition.y + SPACING22);
            usedcar.autoSize = true;
        }

        private void RefreshDisplayData()
        {
            if (refeshOnce || (BuildingData.lastBuildingID != WorldInfoPanel.GetCurrentInstanceID().Building))
            {
                if (isVisible)
                {
                    BuildingData.lastBuildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
                    Building buildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[BuildingData.lastBuildingID];
                    if (buildingData.Info.m_class.m_service == ItemClass.Service.Residential)
                    {
                        Hide();
                    }
                    else
                    {
                        float averageEmployeeFee = CaculateEmployeeOutcome(buildingData, BuildingData.lastBuildingID, out int aliveWorkerCount, out int totalWorkerCount);
                        int landRentFee = CaculateLandFee(buildingData, BuildingData.lastBuildingID);
                        string incomeType = RealCityPrivateBuildingAI.GetProductionType(false, BuildingData.lastBuildingID, buildingData);
                        string outgoingType = RealCityPrivateBuildingAI.GetProductionType(true, BuildingData.lastBuildingID, buildingData);
                        float incomePrice = RealCityPrivateBuildingAI.GetPrice(false, BuildingData.lastBuildingID, buildingData);
                        float outgoingPrice = RealCityPrivateBuildingAI.GetPrice(true, BuildingData.lastBuildingID, buildingData);
                        buildingMoney.text = string.Format(Localization.Get("BUILDING_MONEY") + " [{0}]", BuildingData.buildingMoney[BuildingData.lastBuildingID]);
                        buildingIncomeBuffer.text = string.Format(Localization.Get("MATERIAL_BUFFER") + " [{0}]" + " " + incomeType, buildingData.m_customBuffer1);
                        buildingOutgoingBuffer.text = string.Format(Localization.Get("PRODUCTION_BUFFER") + " [{0}]"+ " " + outgoingType, buildingData.m_customBuffer2);
                        employFee.text = Localization.Get("AVERAGE_EMPLOYFEE") + " " + averageEmployeeFee.ToString() + " " + Localization.Get("PROFIT_SHARING");
                        landRent.text = string.Format(Localization.Get("BUILDING_LANDRENT") + " [{0:N2}]", landRentFee / 100f);
                        buyPrice.text = string.Format(Localization.Get("BUY_PRICE") + " " + incomeType + "[{0:N2}]", incomePrice);
                        sellPrice.text = string.Format(Localization.Get("SELL_PRICE") + " " + outgoingType + " [{0:N2}]", outgoingPrice);

                        float consumptionDivider = 0f;
                        if (buildingData.Info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
                        {
                            consumptionDivider = RealCityPrivateBuildingAI.GetComsumptionDivider(buildingData, BuildingData.lastBuildingID) * 4f;
                            comsuptionDivide.text = string.Format(Localization.Get("MATERIAL_DIV_PRODUCTION") + " [1:{0:N2}]", consumptionDivider);
                        }
                        else
                        {
                            if (buildingData.Info.m_buildingAI is IndustrialExtractorAI)
                            {
                                comsuptionDivide.text = string.Format(Localization.Get("MATERIAL_DIV_PRODUCTION") + " N/A");
                            }
                            else
                            {
                                consumptionDivider = RealCityPrivateBuildingAI.GetComsumptionDivider(buildingData, BuildingData.lastBuildingID);
                                comsuptionDivide.text = string.Format(Localization.Get("MATERIAL_DIV_PRODUCTION") + " [1:{0:N2}]", consumptionDivider);
                            }
                        }

                        int m_sellTax = RealCityPrivateBuildingAI.GetTaxRate(buildingData);
                        sellTax.text = string.Format(Localization.Get("SELL_TAX") + " [{0}%]", m_sellTax);

                        if (consumptionDivider == 0f)
                        {
                            profit.text = string.Format(Localization.Get("SELL_TAX") + " N/A");
                        }
                        else
                        {
                            float profitRatio = (outgoingPrice * (1f - m_sellTax / 100f) - (incomePrice / consumptionDivider)) / outgoingPrice;
                            if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
                            {
                                profit.text = string.Format(Localization.Get("PROFIT") + " [{0}%]" + Localization.Get("EXCLUDE_VISIT_INCOME"), (int)(profitRatio * 100f));
                            }
                            else
                            {
                                profit.text = string.Format(Localization.Get("PROFIT") + " [{0}%]", (int)(profitRatio * 100f));
                            }
                        }

                        int usedCar = 0;
                        int num27 = 0;
                        int num28 = 0;
                        int value = 0;
                        int car = 0;
                        if (buildingData.Info.m_class.m_service == ItemClass.Service.Industrial)
                        {
                            TransferManager.TransferReason tempReason = default(TransferManager.TransferReason);
                            if (buildingData.Info.m_buildingAI is IndustrialExtractorAI)
                            {
                                RealCityIndustrialExtractorAI.InitDelegate();
                                var industrialExtractorAI = (IndustrialExtractorAI)buildingData.Info.m_buildingAI;
                                int productionCapacity = industrialExtractorAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(BuildingData.lastBuildingID), buildingData.m_width, buildingData.m_length);
                                car = Mathf.Max(1, productionCapacity / 6);
                                tempReason = RealCityIndustrialExtractorAI.GetOutgoingTransferReason((IndustrialExtractorAI)buildingData.Info.m_buildingAI);
                                RealCityCommonBuildingAI.InitDelegate();
                                RealCityCommonBuildingAI.CalculateOwnVehicles((IndustrialExtractorAI)buildingData.Info.m_buildingAI, BuildingData.lastBuildingID, ref buildingData, tempReason, ref usedCar, ref num27, ref num28, ref value);
                            }
                            else
                            {
                                RealCityIndustrialBuildingAI.InitDelegate();
                                var industrialBuildingAI = (IndustrialBuildingAI)buildingData.Info.m_buildingAI;
                                int productionCapacity = industrialBuildingAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(BuildingData.lastBuildingID), buildingData.m_width, buildingData.m_length);
                                car = Mathf.Max(1, productionCapacity / 6);
                                tempReason = RealCityIndustrialBuildingAI.GetOutgoingTransferReason((IndustrialBuildingAI)buildingData.Info.m_buildingAI);
                                RealCityCommonBuildingAI.InitDelegate();
                                RealCityCommonBuildingAI.CalculateOwnVehicles((IndustrialBuildingAI)buildingData.Info.m_buildingAI, BuildingData.lastBuildingID, ref buildingData, tempReason, ref usedCar, ref num27, ref num28, ref value);
                            }

                            usedcar.text = string.Format(Localization.Get("CAR_USED") + " [{0}/{1}]", usedCar, car);
                        }
                        else if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
                        {
                            Citizen.BehaviourData behaviour = default;
                            int aliveVisitCount = 0;
                            int totalVisitCount = 0;
                            RealCityCommercialBuildingAI.InitDelegate();
                            RealCityCommercialBuildingAI.GetVisitBehaviour((CommercialBuildingAI)(buildingData.Info.m_buildingAI), BuildingData.lastBuildingID, ref buildingData, ref behaviour, ref aliveVisitCount, ref totalVisitCount);
                            var amount = buildingData.m_customBuffer2 / MainDataStore.maxGoodPurchase - totalVisitCount;
                            var commercialBuildingAI = buildingData.Info.m_buildingAI as CommercialBuildingAI;
                            var maxCount = commercialBuildingAI.CalculateVisitplaceCount((ItemClass.Level)buildingData.m_level, new Randomizer(BuildingData.lastBuildingID), buildingData.m_width, buildingData.m_length);
                            usedcar.text = string.Format("FORDEBUG" + " [{0}/{1}/{2}]", totalVisitCount, maxCount, amount);
                        }
                        else
                        {
                            usedcar.text = Localization.Get("CAR_USED") + " 0/0";
                        }

                        BringToFront();
                        refeshOnce = false;
                    }
                }
            }
        }

        public static float CaculateEmployeeOutcome(Building building, ushort buildingID, out int aliveWorkerCount, out int totalWorkerCount)
        {
            float num1 = 0;
            Citizen.BehaviourData behaviour = default;
            aliveWorkerCount = 0;
            totalWorkerCount = 0;
            RealCityCommonBuildingAI.InitDelegate();
            RealCityCommonBuildingAI.GetWorkBehaviour((CommonBuildingAI)building.Info.m_buildingAI, buildingID, ref building, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);

            if (totalWorkerCount > 0)
            {
                if (BuildingData.buildingMoney[buildingID] > 0)
                {
                    float profitShare = 0;
                    switch (building.Info.m_class.m_subService)
                    {
                        case ItemClass.SubService.IndustrialFarming:
                        case ItemClass.SubService.IndustrialForestry:
                            if (building.Info.m_buildingAI is IndustrialExtractorAI)
                                profitShare = MainDataStore.profitShareRatioInduExtractor;
                            else
                                profitShare = MainDataStore.profitShareRatioInduOther;
                            break;
                        case ItemClass.SubService.IndustrialOil:
                        case ItemClass.SubService.IndustrialOre:
                            profitShare = MainDataStore.profitShareRatioInduOther; break;
                        case ItemClass.SubService.IndustrialGeneric:
                            if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                                profitShare = MainDataStore.profitShareRatioInduLevel1;
                            else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                                profitShare = MainDataStore.profitShareRatioInduLevel2;
                            else
                                profitShare = MainDataStore.profitShareRatioInduLevel3;
                            break;
                        case ItemClass.SubService.CommercialHigh:
                        case ItemClass.SubService.CommercialLow:
                            if (building.Info.m_class.m_level == ItemClass.Level.Level1)
                                profitShare = MainDataStore.profitShareRatioCommLevel1;
                            else if (building.Info.m_class.m_level == ItemClass.Level.Level2)
                                profitShare = MainDataStore.profitShareRatioCommLevel2;
                            else
                                profitShare = MainDataStore.profitShareRatioCommLevel3;
                            break;
                        case ItemClass.SubService.CommercialTourist:
                        case ItemClass.SubService.CommercialLeisure:
                            profitShare = MainDataStore.profitShareRatioCommOther; break;
                        case ItemClass.SubService.CommercialEco:
                            profitShare = MainDataStore.profitShareRatioCommECO; break;
                        case ItemClass.SubService.OfficeGeneric:
                        case ItemClass.SubService.OfficeHightech:
                            profitShare = 1f; break;
                    }
                    num1 = (int)(BuildingData.buildingMoney[buildingID] * profitShare / totalWorkerCount);
                }
                else
                {
                    num1 = 0;
                }
            }
            return num1;
        }

        public int CaculateLandFee(Building building, ushort buildingID)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(building.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;

            int landFee;
            GetLandRent(building, buildingID, out landFee);
            int taxRate;
            taxRate = Singleton<EconomyManager>.instance.GetTaxRate(building.Info.m_class, taxationPolicies);

            if (instance.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
            {
                if ((servicePolicies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None)
                {
                    taxRate = taxRate * 95 / 100;
                }
            }
            if ((servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
            {
                taxRate = taxRate * 95 / 100;
            }

            if (((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None) && (building.Info.m_class.m_subService == ItemClass.SubService.CommercialLeisure))
            {
                landFee = 0;
            }

            if (BuildingData.buildingMoney[buildingID] > 0)
            {
                if ((building.Info.m_class.m_service == ItemClass.Service.Commercial) || (building.Info.m_class.m_service == ItemClass.Service.Industrial))
                {
                    return taxRate * landFee;
                }
                else if (building.Info.m_class.m_service == ItemClass.Service.Office)
                {
                    Citizen.BehaviourData behaviourData = default;
                    int aliveWorkerCount = 0;
                    int totalWorkerCount = 0;
                    RealCityCommonBuildingAI.InitDelegate();
                    RealCityCommonBuildingAI.GetWorkBehaviour((OfficeBuildingAI)building.Info.m_buildingAI, buildingID, ref building, ref behaviourData, ref aliveWorkerCount, ref totalWorkerCount);
                    return totalWorkerCount * 10 * taxRate;
                }
            }

            return 0;
        }

        public void GetLandRent(Building building, ushort buildingID, out int incomeAccumulation)
        {
            ItemClass @class = building.Info.m_class;
            incomeAccumulation = 0;
            ItemClass.SubService subService = @class.m_subService;
            switch (subService)
            {
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
