﻿using ColossalFramework;
using Harmony;
using RealCity.Util;
using System;
using System.Reflection;

namespace RealCity.CustomData
{
    [HarmonyPatch]
    public static class CustomTransportLine
    {
        public static byte[] WeekDayPlan = new byte[256];
        public static byte[] WeekEndPlan = new byte[256];
        public static byte[] saveData = new byte[512];
        public static ushort lastLineID = 0;

        public static void DataInit()
        {
            for (int i = 0; i < WeekDayPlan.Length; i++)
            {
                WeekDayPlan[i] = 1;
                WeekEndPlan[i] = 2;
            }
        }

        public static void save()
        {
            int i = 0;
            SaveAndRestore.save_bytes(ref i, WeekDayPlan, ref saveData);
            SaveAndRestore.save_bytes(ref i, WeekEndPlan, ref saveData);
        }

        public static void load()
        {
            int i = 0;
            WeekDayPlan = SaveAndRestore.load_bytes(ref i, saveData, WeekDayPlan.Length);
            WeekEndPlan = SaveAndRestore.load_bytes(ref i, saveData, WeekEndPlan.Length);
        }

        public static MethodBase TargetMethod()
        {
            return typeof(TransportLine).GetMethod("CalculateTargetVehicleCount", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Prefix(ref TransportLine __instance, out float __state)
        {
            __state = __instance.m_totalLength;
            if (Loader.isTransportLinesManagerRunning) { return; }
            float budget = __instance.m_totalLength;
            if (IsWeekend(Singleton<SimulationManager>.instance.m_currentGameTime))
            {
                if (WeekEndPlan[FindLineID(__instance)] == 1)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                }
                else if (WeekEndPlan[FindLineID(__instance)] == 2)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                }
                else if (WeekEndPlan[FindLineID(__instance)] == 3)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                }
                else if (WeekEndPlan[FindLineID(__instance)] == 4)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                }
            }
            else
            {
                //PlanA
                if (WeekDayPlan[FindLineID(__instance)] == 1)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetWeekDay) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetWeekDay) * 0.01f * budget);
                    }
                }
                else if (WeekDayPlan[FindLineID(__instance)] == 2)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetWeekEnd) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetWeekEnd) * 0.01f * budget);
                    }
                }
                else if (WeekDayPlan[FindLineID(__instance)] == 3)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetMax) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetMax) * 0.01f * budget);
                    }
                }
                else if (WeekDayPlan[FindLineID(__instance)] == 4)
                {
                    if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 8 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 10)
                    {
                        budget = (int)((RealCity.morningBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 10 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 17)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 17 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 19)
                    {
                        budget = (int)((RealCity.eveningBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 19 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 24)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 0 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 4)
                    {
                        budget = (int)((RealCity.deepNightBudgetMin) * 0.01f * budget);
                    }
                    else if (Singleton<SimulationManager>.instance.m_currentGameTime.Hour >= 4 && Singleton<SimulationManager>.instance.m_currentGameTime.Hour < 8)
                    {
                        budget = (int)((RealCity.otherBudgetMin) * 0.01f * budget);
                    }
                }
            }

            __instance.m_totalLength = budget;
        }

        public static void Postfix(ref TransportLine __instance, ref float __state)
        {
            if (Loader.isTransportLinesManagerRunning) { return; }
            __instance.m_totalLength = __state;
        }

        public static bool IsWeekend(this DateTime dateTime)
        {
            if (dateTime.DayOfWeek != DayOfWeek.Saturday)
            {
                return dateTime.DayOfWeek == DayOfWeek.Sunday;
            }
            return true;
        }

        public static ushort FindLineID(TransportLine transportLine)
        {
            for (int i = 0; i < 256; i++)
            {
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags.IsFlagSet(TransportLine.Flags.Created))
                {
                    if (transportLine.m_lineNumber != 0)
                    {
                        if (transportLine.Info.m_transportType == Singleton<TransportManager>.instance.m_lines.m_buffer[i].Info.m_transportType)
                        {
                            if (transportLine.m_lineNumber == Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber)
                            {
                                return (ushort)i;
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
}
