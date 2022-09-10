﻿// <copyright file="ExpandedWaterManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using ColossalFramework;
    using UnityEngine;
    using static WaterManager;
    using static WaterManagerPatches;

    /// <summary>
    /// Custom electricty manager components for 81-tile operation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:Static readonly fields should begin with upper-case letter", Justification = "Dotnet runtime style")]
    internal static class ExpandedWaterManager
    {
        // Expanded arrays.
        private static readonly ExpandedPulseUnit[] s_waterPulseUnits = new ExpandedPulseUnit[32768];
        private static readonly ExpandedPulseUnit[] s_sewagePulseUnits = new ExpandedPulseUnit[32768];
        private static readonly ExpandedPulseUnit[] s_heatingPulseUnits = new ExpandedPulseUnit[32768];

        /// <summary>
        /// Gets the expanded water pulse unit array.
        /// </summary>
        internal static ExpandedPulseUnit[] WaterPulseUnits => s_waterPulseUnits;

        /// <summary>
        /// Gets the expanded sewage pulse unit array.
        /// </summary>
        internal static ExpandedPulseUnit[] SewagePulseUnits => s_sewagePulseUnits;

        /// <summary>
        /// Gets the expanded heating pulse unit array.
        /// </summary>
        internal static ExpandedPulseUnit[] HeatingPulseUnits => s_heatingPulseUnits;

        internal static void SimulationStepImpl(
            WaterManager instance,
            int subStep,
            Cell[] m_waterGrid,
            ref int m_waterPulseGroupCount,
            ref int m_waterPulseUnitStart,
            ref int m_waterPulseUnitEnd,
            ref int m_sewagePulseGroupCount,
            ref int m_sewagePulseUnitStart,
            ref int m_sewagePulseUnitEnd,
            ref int m_heatingPulseGroupCount,
            ref int m_heatingPulseUnitStart,
            ref int m_heatingPulseUnitEnd,
            ref int m_processedCells,
            ref int m_conductiveCells,
            ref bool m_canContinue,
            PulseGroup[] m_waterPulseGroups,
            PulseGroup[] m_sewagePulseGroups,
            PulseGroup[] m_heatingPulseGroups)
        {
            if (subStep == 0 || subStep == 1000)
            {
                return;
            }
            NetManager netManager = Singleton<NetManager>.instance;
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num = (int)(currentFrameIndex & 0xFF);
            if (num < 128)
            {
                if (num == 0)
                {
                    m_waterPulseGroupCount = 0;
                    m_waterPulseUnitStart = 0;
                    m_waterPulseUnitEnd = 0;
                    m_sewagePulseGroupCount = 0;
                    m_sewagePulseUnitStart = 0;
                    m_sewagePulseUnitEnd = 0;
                    m_heatingPulseGroupCount = 0;
                    m_heatingPulseUnitStart = 0;
                    m_heatingPulseUnitEnd = 0;
                    m_processedCells = 0;
                    m_conductiveCells = 0;
                    m_canContinue = true;
                }
                int num2 = num * 32768 >> 7;
                int num3 = ((num + 1) * 32768 >> 7) - 1;
                PulseGroup pulseGroup = default;
                ExpandedPulseUnit pulseUnit = default;
                PulseGroup pulseGroup2 = default;
                ExpandedPulseUnit pulseUnit2 = default;
                PulseGroup pulseGroup3 = default;
                ExpandedPulseUnit pulseUnit3 = default;
                for (int i = num2; i <= num3; i++)
                {
                    Node node = instance.m_nodeData[i];
                    if (netManager.m_nodes.m_buffer[i].m_flags != 0)
                    {
                        NetInfo info = netManager.m_nodes.m_buffer[i].Info;
                        if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2)
                        {
                            int water = ((node.m_waterPulseGroup != ushort.MaxValue) ? 1 : 0);
                            int sewage = ((node.m_sewagePulseGroup != ushort.MaxValue) ? 1 : 0);
                            int heating = ((node.m_heatingPulseGroup != ushort.MaxValue) ? 1 : 0);
                            UpdateNodeWater(i, water, sewage, heating);
                            m_conductiveCells += 2;
                            node.m_waterPulseGroup = ushort.MaxValue;
                            node.m_sewagePulseGroup = ushort.MaxValue;
                            node.m_heatingPulseGroup = ushort.MaxValue;
                            if ((node.m_curWaterPressure != 0 || node.m_collectWaterPressure != 0) && m_waterPulseGroupCount < 1024)
                            {
                                pulseGroup.m_origPressure = node.m_curWaterPressure;
                                pulseGroup.m_curPressure = node.m_curWaterPressure;
                                pulseGroup.m_collectPressure = node.m_collectWaterPressure;
                                pulseGroup.m_mergeCount = 0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_node = (ushort)i;
                                node.m_waterPulseGroup = (ushort)m_waterPulseGroupCount;
                                m_waterPulseGroups[m_waterPulseGroupCount++] = pulseGroup;
                                if (pulseGroup.m_origPressure != 0)
                                {
                                    pulseUnit.m_group = (ushort)(m_waterPulseGroupCount - 1);
                                    pulseUnit.m_node = (ushort)i;
                                    pulseUnit.m_x = 0;
                                    pulseUnit.m_z = 0;
                                    s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                                    if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                                    {
                                        m_waterPulseUnitEnd = 0;
                                    }
                                }
                            }
                            if ((node.m_curSewagePressure != 0 || node.m_collectSewagePressure != 0) && m_sewagePulseGroupCount < 1024)
                            {
                                pulseGroup2.m_origPressure = node.m_curSewagePressure;
                                pulseGroup2.m_curPressure = node.m_curSewagePressure;
                                pulseGroup2.m_collectPressure = node.m_collectSewagePressure;
                                pulseGroup2.m_mergeCount = 0;
                                pulseGroup2.m_mergeIndex = ushort.MaxValue;
                                pulseGroup2.m_node = (ushort)i;
                                node.m_sewagePulseGroup = (ushort)m_sewagePulseGroupCount;
                                m_sewagePulseGroups[m_sewagePulseGroupCount++] = pulseGroup2;
                                if (pulseGroup2.m_origPressure != 0)
                                {
                                    pulseUnit2.m_group = (ushort)(m_sewagePulseGroupCount - 1);
                                    pulseUnit2.m_node = (ushort)i;
                                    pulseUnit2.m_x = 0;
                                    pulseUnit2.m_z = 0;
                                    s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit2;
                                    if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                                    {
                                        m_sewagePulseUnitEnd = 0;
                                    }
                                }
                            }
                            if (node.m_curHeatingPressure != 0 && m_heatingPulseGroupCount < 1024)
                            {
                                pulseGroup3.m_origPressure = node.m_curHeatingPressure;
                                pulseGroup3.m_curPressure = node.m_curHeatingPressure;
                                pulseGroup3.m_collectPressure = 0u;
                                pulseGroup3.m_mergeCount = 0;
                                pulseGroup3.m_mergeIndex = ushort.MaxValue;
                                pulseGroup3.m_node = (ushort)i;
                                pulseUnit3.m_group = (ushort)m_heatingPulseGroupCount;
                                pulseUnit3.m_node = (ushort)i;
                                pulseUnit3.m_x = 0;
                                pulseUnit3.m_z = 0;
                                node.m_heatingPulseGroup = (ushort)m_heatingPulseGroupCount;
                                m_heatingPulseGroups[m_heatingPulseGroupCount++] = pulseGroup3;
                                s_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit3;
                                if (++m_heatingPulseUnitEnd == s_heatingPulseUnits.Length)
                                {
                                    m_heatingPulseUnitEnd = 0;
                                }
                            }
                        }
                        else
                        {
                            node.m_waterPulseGroup = ushort.MaxValue;
                            node.m_sewagePulseGroup = ushort.MaxValue;
                            node.m_heatingPulseGroup = ushort.MaxValue;
                            node.m_extraWaterPressure = 0;
                            node.m_extraSewagePressure = 0;
                            node.m_extraHeatingPressure = 0;
                        }
                    }
                    else
                    {
                        node.m_waterPulseGroup = ushort.MaxValue;
                        node.m_sewagePulseGroup = ushort.MaxValue;
                        node.m_heatingPulseGroup = ushort.MaxValue;
                        node.m_extraWaterPressure = 0;
                        node.m_extraSewagePressure = 0;
                        node.m_extraHeatingPressure = 0;
                    }
                    node.m_curWaterPressure = 0;
                    node.m_curSewagePressure = 0;
                    node.m_curHeatingPressure = 0;
                    node.m_collectWaterPressure = 0;
                    node.m_collectSewagePressure = 0;
                    instance.m_nodeData[i] = node;
                }
                int num4 = num * ExpandedWaterGridResolution >> 7;
                int num5 = ((num + 1) * ExpandedWaterGridResolution >> 7) - 1;
                for (int j = num4; j <= num5; j++)
                {
                    int num6 = j * ExpandedWaterGridResolution;
                    for (int k = 0; k < ExpandedWaterGridResolution; k++)
                    {
                        Cell cell = m_waterGrid[num6];
                        cell.m_waterPulseGroup = ushort.MaxValue;
                        cell.m_sewagePulseGroup = ushort.MaxValue;
                        cell.m_heatingPulseGroup = ushort.MaxValue;
                        if (cell.m_conductivity >= 96)
                        {
                            m_conductiveCells += 2;
                        }
                        if (cell.m_tmpHasWater != cell.m_hasWater)
                        {
                            cell.m_hasWater = cell.m_tmpHasWater;
                        }
                        if (cell.m_tmpHasSewage != cell.m_hasSewage)
                        {
                            cell.m_hasSewage = cell.m_tmpHasSewage;
                        }
                        if (cell.m_tmpHasHeating != cell.m_hasHeating)
                        {
                            cell.m_hasHeating = cell.m_tmpHasHeating;
                        }
                        cell.m_tmpHasWater = false;
                        cell.m_tmpHasSewage = false;
                        cell.m_tmpHasHeating = false;
                        m_waterGrid[num6] = cell;
                        num6++;
                    }
                }
                return;
            }
            int num7 = (num - 127) * m_conductiveCells >> 7;
            if (num == 255)
            {
                num7 = 1000000000;
            }
            while (m_canContinue && m_processedCells < num7)
            {
                m_canContinue = false;
                int waterPulseUnitEnd = m_waterPulseUnitEnd;
                int sewagePulseUnitEnd = m_sewagePulseUnitEnd;
                int heatingPulseUnitEnd = m_heatingPulseUnitEnd;
                while (m_waterPulseUnitStart != waterPulseUnitEnd)
                {
                    ExpandedPulseUnit pulseUnit4 = s_waterPulseUnits[m_waterPulseUnitStart];
                    if (++m_waterPulseUnitStart == s_waterPulseUnits.Length)
                    {
                        m_waterPulseUnitStart = 0;
                    }
                    pulseUnit4.m_group = GetRootWaterGroup(pulseUnit4.m_group, m_waterPulseGroups);
                    uint num8 = m_waterPulseGroups[pulseUnit4.m_group].m_curPressure;
                    if (pulseUnit4.m_node == 0)
                    {
                        int num9 = pulseUnit4.m_z * ExpandedWaterGridResolution + pulseUnit4.m_x;
                        Cell cell2 = m_waterGrid[num9];
                        if (cell2.m_conductivity != 0 && !cell2.m_tmpHasWater && num8 != 0)
                        {
                            int num10 = Mathf.Clamp(-cell2.m_currentWaterPressure, 0, (int)num8);
                            num8 -= (uint)num10;
                            cell2.m_currentWaterPressure += (short)num10;
                            if (cell2.m_currentWaterPressure >= 0)
                            {
                                cell2.m_tmpHasWater = true;
                                cell2.m_pollution = instance.m_nodeData[m_waterPulseGroups[pulseUnit4.m_group].m_node].m_pollution;
                            }
                            m_waterGrid[num9] = cell2;
                            m_waterPulseGroups[pulseUnit4.m_group].m_curPressure = num8;
                        }
                        if (num8 != 0)
                        {
                            m_processedCells++;
                            continue;
                        }
                        s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit4;
                        if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                        {
                            m_waterPulseUnitEnd = 0;
                        }
                    }
                    else if (num8 != 0)
                    {
                        m_processedCells++;
                        NetNode netNode = netManager.m_nodes.m_buffer[pulseUnit4.m_node];
                        if (netNode.m_flags == NetNode.Flags.None || netNode.m_buildIndex >= (currentFrameIndex & 0xFFFFFF80u))
                        {
                            continue;
                        }
                        byte pollution = instance.m_nodeData[m_waterPulseGroups[pulseUnit4.m_group].m_node].m_pollution;
                        instance.m_nodeData[pulseUnit4.m_node].m_pollution = pollution;
                        if (netNode.m_building != 0)
                        {
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[netNode.m_building].m_waterPollution = pollution;
                        }
                        ConductWaterToCells(pulseUnit4.m_group, netNode.m_position.x, netNode.m_position.z, 100f, m_waterGrid, ref m_waterPulseUnitEnd, ref m_canContinue);
                        for (int l = 0; l < 8; l++)
                        {
                            ushort segment = netNode.GetSegment(l);
                            if (segment != 0)
                            {
                                ushort startNode = netManager.m_segments.m_buffer[segment].m_startNode;
                                ushort endNode = netManager.m_segments.m_buffer[segment].m_endNode;
                                ushort num11 = ((startNode != pulseUnit4.m_node) ? startNode : endNode);
                                ConductWaterToNode(num11, ref netManager.m_nodes.m_buffer[num11], pulseUnit4.m_group, instance.m_nodeData, m_waterPulseGroups, m_waterPulseGroupCount, ref m_waterPulseUnitEnd, ref m_canContinue);
                            }
                        }
                    }
                    else
                    {
                        s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit4;
                        if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                        {
                            m_waterPulseUnitEnd = 0;
                        }
                    }
                }
                while (m_sewagePulseUnitStart != sewagePulseUnitEnd)
                {
                    ExpandedPulseUnit pulseUnit5 = s_sewagePulseUnits[m_sewagePulseUnitStart];
                    if (++m_sewagePulseUnitStart == s_sewagePulseUnits.Length)
                    {
                        m_sewagePulseUnitStart = 0;
                    }
                    pulseUnit5.m_group = GetRootSewageGroup(pulseUnit5.m_group, m_sewagePulseGroups);
                    uint num12 = m_sewagePulseGroups[pulseUnit5.m_group].m_curPressure;
                    if (pulseUnit5.m_node == 0)
                    {
                        int num13 = pulseUnit5.m_z * ExpandedWaterGridResolution + pulseUnit5.m_x;
                        Cell cell3 = m_waterGrid[num13];
                        if (cell3.m_conductivity != 0 && !cell3.m_tmpHasSewage && num12 != 0)
                        {
                            int num14 = Mathf.Clamp(-cell3.m_currentSewagePressure, 0, (int)num12);
                            num12 -= (uint)num14;
                            cell3.m_currentSewagePressure += (short)num14;
                            if (cell3.m_currentSewagePressure >= 0)
                            {
                                cell3.m_tmpHasSewage = true;
                            }
                            m_waterGrid[num13] = cell3;
                            m_sewagePulseGroups[pulseUnit5.m_group].m_curPressure = num12;
                        }
                        if (num12 != 0)
                        {
                            m_processedCells++;
                            continue;
                        }
                        s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit5;
                        if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                        {
                            m_sewagePulseUnitEnd = 0;
                        }
                    }
                    else if (num12 != 0)
                    {
                        m_processedCells++;
                        NetNode netNode2 = netManager.m_nodes.m_buffer[pulseUnit5.m_node];
                        if (netNode2.m_flags == NetNode.Flags.None || netNode2.m_buildIndex >= (currentFrameIndex & 0xFFFFFF80u))
                        {
                            continue;
                        }
                        ConductSewageToCells(pulseUnit5.m_group, netNode2.m_position.x, netNode2.m_position.z, 100f, m_waterGrid, ref m_sewagePulseUnitEnd, ref m_canContinue);
                        for (int m = 0; m < 8; m++)
                        {
                            ushort segment2 = netNode2.GetSegment(m);
                            if (segment2 != 0)
                            {
                                ushort startNode2 = netManager.m_segments.m_buffer[segment2].m_startNode;
                                ushort endNode2 = netManager.m_segments.m_buffer[segment2].m_endNode;
                                ushort num15 = ((startNode2 != pulseUnit5.m_node) ? startNode2 : endNode2);
                                ConductSewageToNode(num15, ref netManager.m_nodes.m_buffer[num15], pulseUnit5.m_group, instance.m_nodeData, m_sewagePulseGroups, m_sewagePulseGroupCount, ref m_sewagePulseUnitEnd, ref m_canContinue);
                            }
                        }
                    }
                    else
                    {
                        s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit5;
                        if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                        {
                            m_sewagePulseUnitEnd = 0;
                        }
                    }
                }
                while (m_heatingPulseUnitStart != heatingPulseUnitEnd)
                {
                    ExpandedPulseUnit pulseUnit6 = s_heatingPulseUnits[m_heatingPulseUnitStart];
                    if (++m_heatingPulseUnitStart == s_heatingPulseUnits.Length)
                    {
                        m_heatingPulseUnitStart = 0;
                    }
                    pulseUnit6.m_group = GetRootHeatingGroup(pulseUnit6.m_group, m_heatingPulseGroups);
                    uint num16 = m_heatingPulseGroups[pulseUnit6.m_group].m_curPressure;
                    if (pulseUnit6.m_node == 0)
                    {
                        int num17 = pulseUnit6.m_z * ExpandedWaterGridResolution + pulseUnit6.m_x;
                        Cell cell4 = m_waterGrid[num17];
                        if (cell4.m_conductivity2 != 0 && !cell4.m_tmpHasHeating && num16 != 0)
                        {
                            int num18 = Mathf.Clamp(-cell4.m_currentHeatingPressure, 0, (int)num16);
                            num16 -= (uint)num18;
                            cell4.m_currentHeatingPressure += (short)num18;
                            if (cell4.m_currentHeatingPressure >= 0)
                            {
                                cell4.m_tmpHasHeating = true;
                            }
                            m_waterGrid[num17] = cell4;
                            m_heatingPulseGroups[pulseUnit6.m_group].m_curPressure = num16;
                        }
                        if (num16 != 0)
                        {
                            m_processedCells++;
                            continue;
                        }
                        s_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit6;
                        if (++m_heatingPulseUnitEnd == s_heatingPulseUnits.Length)
                        {
                            m_heatingPulseUnitEnd = 0;
                        }
                    }
                    else if (num16 != 0)
                    {
                        m_processedCells++;
                        NetNode netNode3 = netManager.m_nodes.m_buffer[pulseUnit6.m_node];
                        if (netNode3.m_flags == NetNode.Flags.None || netNode3.m_buildIndex >= (currentFrameIndex & 0xFFFFFF80u))
                        {
                            continue;
                        }
                        ConductHeatingToCells(pulseUnit6.m_group, netNode3.m_position.x, netNode3.m_position.z, 100f, m_waterGrid, ref m_heatingPulseUnitEnd, ref m_canContinue);
                        for (int n = 0; n < 8; n++)
                        {
                            ushort segment3 = netNode3.GetSegment(n);
                            if (segment3 != 0)
                            {
                                NetInfo info2 = netManager.m_segments.m_buffer[segment3].Info;
                                if (info2.m_class.m_service == ItemClass.Service.Water && info2.m_class.m_level == ItemClass.Level.Level2)
                                {
                                    ushort startNode3 = netManager.m_segments.m_buffer[segment3].m_startNode;
                                    ushort endNode3 = netManager.m_segments.m_buffer[segment3].m_endNode;
                                    ushort num19 = ((startNode3 != pulseUnit6.m_node) ? startNode3 : endNode3);
                                    ConductHeatingToNode(num19, ref netManager.m_nodes.m_buffer[num19], pulseUnit6.m_group, instance.m_nodeData, m_heatingPulseGroups, m_heatingPulseGroupCount, ref m_heatingPulseUnitEnd, ref m_canContinue);
                                }
                            }
                        }
                    }
                    else
                    {
                        s_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit6;
                        if (++m_heatingPulseUnitEnd == s_heatingPulseUnits.Length)
                        {
                            m_heatingPulseUnitEnd = 0;
                        }
                    }
                }
            }
            if (num != 255)
            {
                return;
            }
            for (int num20 = 0; num20 < m_waterPulseGroupCount; num20++)
            {
                PulseGroup pulseGroup4 = m_waterPulseGroups[num20];
                if (pulseGroup4.m_mergeIndex != ushort.MaxValue && pulseGroup4.m_collectPressure != 0)
                {
                    PulseGroup pulseGroup5 = m_waterPulseGroups[pulseGroup4.m_mergeIndex];
                    pulseGroup4.m_curPressure = (uint)((ulong)((long)pulseGroup5.m_curPressure * (long)pulseGroup4.m_collectPressure) / (ulong)pulseGroup5.m_collectPressure);
                    if (pulseGroup4.m_collectPressure < pulseGroup4.m_curPressure)
                    {
                        pulseGroup4.m_curPressure = pulseGroup4.m_collectPressure;
                    }
                    pulseGroup5.m_curPressure -= pulseGroup4.m_curPressure;
                    pulseGroup5.m_collectPressure -= pulseGroup4.m_collectPressure;
                    m_waterPulseGroups[pulseGroup4.m_mergeIndex] = pulseGroup5;
                    m_waterPulseGroups[num20] = pulseGroup4;
                }
            }
            for (int num21 = 0; num21 < m_waterPulseGroupCount; num21++)
            {
                PulseGroup pulseGroup6 = m_waterPulseGroups[num21];
                if (pulseGroup6.m_mergeIndex != ushort.MaxValue && pulseGroup6.m_collectPressure == 0)
                {
                    PulseGroup pulseGroup7 = m_waterPulseGroups[pulseGroup6.m_mergeIndex];
                    uint curPressure = pulseGroup7.m_curPressure;
                    curPressure = ((pulseGroup7.m_collectPressure < curPressure) ? (curPressure - pulseGroup7.m_collectPressure) : 0u);
                    pulseGroup6.m_curPressure = (uint)((ulong)((long)curPressure * (long)pulseGroup6.m_origPressure) / (ulong)pulseGroup7.m_origPressure);
                    pulseGroup7.m_curPressure -= pulseGroup6.m_curPressure;
                    pulseGroup7.m_origPressure -= pulseGroup6.m_origPressure;
                    m_waterPulseGroups[pulseGroup6.m_mergeIndex] = pulseGroup7;
                    m_waterPulseGroups[num21] = pulseGroup6;
                }
            }
            for (int num22 = 0; num22 < m_waterPulseGroupCount; num22++)
            {
                PulseGroup pulseGroup8 = m_waterPulseGroups[num22];
                if (pulseGroup8.m_curPressure != 0)
                {
                    Node node2 = instance.m_nodeData[pulseGroup8.m_node];
                    node2.m_extraWaterPressure += (ushort)Mathf.Min((int)pulseGroup8.m_curPressure, 32767 - node2.m_extraWaterPressure);
                    instance.m_nodeData[pulseGroup8.m_node] = node2;
                }
            }
            for (int num23 = 0; num23 < m_sewagePulseGroupCount; num23++)
            {
                PulseGroup pulseGroup9 = m_sewagePulseGroups[num23];
                if (pulseGroup9.m_mergeIndex != ushort.MaxValue && pulseGroup9.m_collectPressure != 0)
                {
                    PulseGroup pulseGroup10 = m_sewagePulseGroups[pulseGroup9.m_mergeIndex];
                    pulseGroup9.m_curPressure = (uint)((ulong)((long)pulseGroup10.m_curPressure * (long)pulseGroup9.m_collectPressure) / (ulong)pulseGroup10.m_collectPressure);
                    if (pulseGroup9.m_collectPressure < pulseGroup9.m_curPressure)
                    {
                        pulseGroup9.m_curPressure = pulseGroup9.m_collectPressure;
                    }
                    pulseGroup10.m_curPressure -= pulseGroup9.m_curPressure;
                    pulseGroup10.m_collectPressure -= pulseGroup9.m_collectPressure;
                    m_sewagePulseGroups[pulseGroup9.m_mergeIndex] = pulseGroup10;
                    m_sewagePulseGroups[num23] = pulseGroup9;
                }
            }
            for (int num24 = 0; num24 < m_sewagePulseGroupCount; num24++)
            {
                PulseGroup pulseGroup11 = m_sewagePulseGroups[num24];
                if (pulseGroup11.m_mergeIndex != ushort.MaxValue && pulseGroup11.m_collectPressure == 0)
                {
                    PulseGroup pulseGroup12 = m_sewagePulseGroups[pulseGroup11.m_mergeIndex];
                    uint curPressure2 = pulseGroup12.m_curPressure;
                    if (pulseGroup12.m_collectPressure >= curPressure2)
                    {
                        curPressure2 = 0u;
                    }
                    else
                    {
                        curPressure2 -= pulseGroup12.m_collectPressure;
                    }
                    pulseGroup11.m_curPressure = (uint)((ulong)((long)pulseGroup12.m_curPressure * (long)pulseGroup11.m_origPressure) / (ulong)pulseGroup12.m_origPressure);
                    pulseGroup12.m_curPressure -= pulseGroup11.m_curPressure;
                    pulseGroup12.m_origPressure -= pulseGroup11.m_origPressure;
                    m_sewagePulseGroups[pulseGroup11.m_mergeIndex] = pulseGroup12;
                    m_sewagePulseGroups[num24] = pulseGroup11;
                }
            }
            for (int num25 = 0; num25 < m_sewagePulseGroupCount; num25++)
            {
                PulseGroup pulseGroup13 = m_sewagePulseGroups[num25];
                if (pulseGroup13.m_curPressure != 0)
                {
                    Node node3 = instance.m_nodeData[pulseGroup13.m_node];
                    node3.m_extraSewagePressure += (ushort)Mathf.Min((int)pulseGroup13.m_curPressure, 32767 - node3.m_extraSewagePressure);
                    instance.m_nodeData[pulseGroup13.m_node] = node3;
                }
            }
            for (int num26 = 0; num26 < m_heatingPulseGroupCount; num26++)
            {
                PulseGroup pulseGroup14 = m_heatingPulseGroups[num26];
                if (pulseGroup14.m_mergeIndex != ushort.MaxValue)
                {
                    PulseGroup pulseGroup15 = m_heatingPulseGroups[pulseGroup14.m_mergeIndex];
                    pulseGroup14.m_curPressure = (uint)((ulong)((long)pulseGroup15.m_curPressure * (long)pulseGroup14.m_origPressure) / (ulong)pulseGroup15.m_origPressure);
                    pulseGroup15.m_curPressure -= pulseGroup14.m_curPressure;
                    pulseGroup15.m_origPressure -= pulseGroup14.m_origPressure;
                    m_heatingPulseGroups[pulseGroup14.m_mergeIndex] = pulseGroup15;
                    m_heatingPulseGroups[num26] = pulseGroup14;
                }
            }
            for (int num27 = 0; num27 < m_heatingPulseGroupCount; num27++)
            {
                PulseGroup pulseGroup16 = m_heatingPulseGroups[num27];
                if (pulseGroup16.m_curPressure != 0)
                {
                    Node node4 = instance.m_nodeData[pulseGroup16.m_node];
                    node4.m_extraHeatingPressure += (ushort)Mathf.Min((int)pulseGroup16.m_curPressure, 32767 - node4.m_extraHeatingPressure);
                    instance.m_nodeData[pulseGroup16.m_node] = node4;
                }
            }
        }


        private static void ConductHeatingToCell(ref Cell cell, ushort group, int x, int z, ref int m_heatingPulseUnitEnd, ref bool m_canContinue)
        {
            if (cell.m_conductivity2 >= 96 && cell.m_heatingPulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                s_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                if (++m_heatingPulseUnitEnd == s_heatingPulseUnits.Length)
                {
                    m_heatingPulseUnitEnd = 0;
                }
                cell.m_heatingPulseGroup = group;
                m_canContinue = true;
            }
        }

        private static void ConductHeatingToCells(ushort group, float worldX, float worldZ, float radius, Cell[] m_waterGrid, ref int m_heatingPulseUnitEnd, ref bool m_canContinue)
        {
            int num = Mathf.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            int num4 = Mathf.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                float num6 = ((float)i + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = num; j <= num3; j++)
                {
                    float num7 = ((float)j + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldX;
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        int num8 = i * ExpandedWaterGridResolution + j;
                        ConductHeatingToCell(ref m_waterGrid[num8], group, j, i, ref m_heatingPulseUnitEnd, ref m_canContinue);
                    }
                }
            }
        }

        private static void ConductHeatingToNode(ushort nodeIndex, ref NetNode node, ushort group, Node[] m_nodeData, PulseGroup[] m_heatingPulseGroups, int m_heatingPulseGroupCount, ref int m_heatingPulseUnitEnd, ref bool m_canContinue)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service != ItemClass.Service.Water || info.m_class.m_level != ItemClass.Level.Level2)
            {
                return;
            }
            if (m_nodeData[nodeIndex].m_heatingPulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = 0;
                pulseUnit.m_z = 0;
                s_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                if (++m_heatingPulseUnitEnd == s_heatingPulseUnits.Length)
                {
                    m_heatingPulseUnitEnd = 0;
                }
                m_nodeData[nodeIndex].m_heatingPulseGroup = group;
                m_canContinue = true;
            }
            else
            {
                ushort rootHeatingGroup = GetRootHeatingGroup(m_nodeData[nodeIndex].m_heatingPulseGroup, m_heatingPulseGroups);
                if (rootHeatingGroup != group)
                {
                    MergeHeatingGroups(group, rootHeatingGroup, m_heatingPulseGroups, m_heatingPulseGroupCount);
                    m_nodeData[nodeIndex].m_heatingPulseGroup = group;
                    m_canContinue = true;
                }
            }
        }

        private static void ConductSewageToCell(ref Cell cell, ushort group, int x, int z, ref int m_sewagePulseUnitEnd, ref bool m_canContinue)
        {
            if (cell.m_conductivity >= 96 && cell.m_sewagePulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                {
                    m_sewagePulseUnitEnd = 0;
                }
                cell.m_sewagePulseGroup = group;
                m_canContinue = true;
            }
        }

        private static void ConductSewageToCells(ushort group, float worldX, float worldZ, float radius, Cell[] m_waterGrid, ref int m_sewagePulseUnitEnd, ref bool m_canContinue)
        {
            int num = Mathf.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            int num4 = Mathf.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                float num6 = ((float)i + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = num; j <= num3; j++)
                {
                    float num7 = ((float)j + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldX;
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        int num8 = i * ExpandedWaterGridResolution + j;
                        ConductSewageToCell(ref m_waterGrid[num8], group, j, i, ref m_sewagePulseUnitEnd, ref m_canContinue);
                    }
                }
            }
        }

        private static void ConductSewageToNode(ushort nodeIndex, ref NetNode node, ushort group, Node[] m_nodeData, PulseGroup[] m_sewagePulseGroups, int m_sewagePulseGroupCount, ref int m_sewagePulseUnitEnd, ref bool m_canContinue)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service != ItemClass.Service.Water || info.m_class.m_level > ItemClass.Level.Level2)
            {
                return;
            }
            if (m_nodeData[nodeIndex].m_sewagePulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = 0;
                pulseUnit.m_z = 0;
                s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                {
                    m_sewagePulseUnitEnd = 0;
                }
                m_nodeData[nodeIndex].m_sewagePulseGroup = group;
                m_canContinue = true;
                return;
            }
            ushort rootSewageGroup = GetRootSewageGroup(m_nodeData[nodeIndex].m_sewagePulseGroup, m_sewagePulseGroups);
            if (rootSewageGroup == group)
            {
                return;
            }
            MergeSewageGroups(group, rootSewageGroup, m_sewagePulseGroups, m_sewagePulseGroupCount);
            if (m_sewagePulseGroups[rootSewageGroup].m_origPressure == 0)
            {
                ExpandedPulseUnit pulseUnit2 = default;
                pulseUnit2.m_group = group;
                pulseUnit2.m_node = nodeIndex;
                pulseUnit2.m_x = 0;
                pulseUnit2.m_z = 0;
                s_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit2;
                if (++m_sewagePulseUnitEnd == s_sewagePulseUnits.Length)
                {
                    m_sewagePulseUnitEnd = 0;
                }
            }
            m_nodeData[nodeIndex].m_sewagePulseGroup = group;
            m_canContinue = true;
        }

        private static void ConductWaterToCell(ref Cell cell, ushort group, int x, int z, ref int m_waterPulseUnitEnd, ref bool m_canContinue)
        {
            if (cell.m_conductivity >= 96 && cell.m_waterPulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                {
                    m_waterPulseUnitEnd = 0;
                }
                cell.m_waterPulseGroup = group;
                m_canContinue = true;
            }
        }

        private static void ConductWaterToCells(ushort group, float worldX, float worldZ, float radius, Cell[] m_waterGrid, ref int m_waterPulseUnitEnd, ref bool m_canContinue)
        {
            int num = Mathf.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            int num4 = Mathf.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                float num6 = ((float)i + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = num; j <= num3; j++)
                {
                    float num7 = ((float)j + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE - worldX;
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        int num8 = i * ExpandedWaterGridResolution + j;
                        ConductWaterToCell(ref m_waterGrid[num8], group, j, i, ref m_waterPulseUnitEnd, ref m_canContinue);
                    }
                }
            }
        }

        private static void ConductWaterToNode(ushort nodeIndex, ref NetNode node, ushort group, Node[] m_nodeData, PulseGroup[] m_waterPulseGroups, int m_waterPulseGroupCount, ref int m_waterPulseUnitEnd, ref bool m_canContinue)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service != ItemClass.Service.Water || info.m_class.m_level > ItemClass.Level.Level2)
            {
                return;
            }
            if (m_nodeData[nodeIndex].m_waterPulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = 0;
                pulseUnit.m_z = 0;
                s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                {
                    m_waterPulseUnitEnd = 0;
                }
                m_nodeData[nodeIndex].m_waterPulseGroup = group;
                m_canContinue = true;
                return;
            }
            ushort rootWaterGroup = GetRootWaterGroup(m_nodeData[nodeIndex].m_waterPulseGroup, m_waterPulseGroups);
            if (rootWaterGroup == group)
            {
                return;
            }
            MergeWaterGroups(group, rootWaterGroup, m_nodeData, m_waterPulseGroups, m_waterPulseGroupCount);
            if (m_waterPulseGroups[rootWaterGroup].m_origPressure == 0)
            {
                ExpandedPulseUnit pulseUnit2 = default;
                pulseUnit2.m_group = group;
                pulseUnit2.m_node = nodeIndex;
                pulseUnit2.m_x = 0;
                pulseUnit2.m_z = 0;
                s_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit2;
                if (++m_waterPulseUnitEnd == s_waterPulseUnits.Length)
                {
                    m_waterPulseUnitEnd = 0;
                }
            }
            m_nodeData[nodeIndex].m_waterPulseGroup = group;
            m_canContinue = true;
        }

        private static ushort GetRootHeatingGroup(ushort group, PulseGroup[] m_heatingPulseGroups)
        {
            for (ushort mergeIndex = m_heatingPulseGroups[group].m_mergeIndex; mergeIndex != ushort.MaxValue; mergeIndex = m_heatingPulseGroups[group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private static ushort GetRootSewageGroup(ushort group, PulseGroup[] m_sewagePulseGroups)
        {
            for (ushort mergeIndex = m_sewagePulseGroups[group].m_mergeIndex; mergeIndex != ushort.MaxValue; mergeIndex = m_sewagePulseGroups[group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private static ushort GetRootWaterGroup(ushort group, PulseGroup[] m_waterPulseGroups)
        {
            for (ushort mergeIndex = m_waterPulseGroups[group].m_mergeIndex; mergeIndex != ushort.MaxValue; mergeIndex = m_waterPulseGroups[group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private static void MergeHeatingGroups(ushort root, ushort merged, PulseGroup[] m_heatingPulseGroups, int m_heatingPulseGroupCount)
        {
            PulseGroup pulseGroup = m_heatingPulseGroups[root];
            PulseGroup pulseGroup2 = m_heatingPulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_heatingPulseGroupCount; i++)
                {
                    if (m_heatingPulseGroups[i].m_mergeIndex == merged)
                    {
                        m_heatingPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_heatingPulseGroups[i].m_origPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount++;
            pulseGroup2.m_mergeIndex = root;
            m_heatingPulseGroups[root] = pulseGroup;
            m_heatingPulseGroups[merged] = pulseGroup2;
        }

        private static void MergeSewageGroups(ushort root, ushort merged, PulseGroup[] m_sewagePulseGroups, int m_sewagePulseGroupCount)
        {
            PulseGroup pulseGroup = m_sewagePulseGroups[root];
            PulseGroup pulseGroup2 = m_sewagePulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_sewagePulseGroupCount; i++)
                {
                    if (m_sewagePulseGroups[i].m_mergeIndex == merged)
                    {
                        m_sewagePulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_sewagePulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= m_sewagePulseGroups[i].m_collectPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount++;
            pulseGroup2.m_mergeIndex = root;
            m_sewagePulseGroups[root] = pulseGroup;
            m_sewagePulseGroups[merged] = pulseGroup2;
        }

        private static void MergeWaterGroups(ushort root, ushort merged, Node[] m_nodeData, PulseGroup[] m_waterPulseGroups, int m_waterPulseGroupCount)
        {
            PulseGroup pulseGroup = m_waterPulseGroups[root];
            PulseGroup pulseGroup2 = m_waterPulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if (pulseGroup2.m_origPressure != 0)
            {
                m_nodeData[pulseGroup.m_node].m_pollution = (byte)(m_nodeData[pulseGroup.m_node].m_pollution + m_nodeData[pulseGroup2.m_node].m_pollution + 1 >> 1);
            }
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_waterPulseGroupCount; i++)
                {
                    if (m_waterPulseGroups[i].m_mergeIndex == merged)
                    {
                        m_waterPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_waterPulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= m_waterPulseGroups[i].m_collectPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount++;
            pulseGroup2.m_mergeIndex = root;
            m_waterPulseGroups[root] = pulseGroup;
            m_waterPulseGroups[merged] = pulseGroup2;
        }

        private static void UpdateNodeWater(int nodeID, int water, int sewage, int heating)
        {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            NetManager netManager = Singleton<NetManager>.instance;
            bool flag = false;
            NetNode.Flags flags = netManager.m_nodes.m_buffer[nodeID].m_flags;
            if ((flags & NetNode.Flags.Transition) != 0)
            {
                netManager.m_nodes.m_buffer[nodeID].m_flags &= ~NetNode.Flags.Transition;
                return;
            }
            ushort building = netManager.m_nodes.m_buffer[nodeID].m_building;
            if (building != 0)
            {
                BuildingManager buildingManager = Singleton<BuildingManager>.instance;
                if (buildingManager.m_buildings.m_buffer[building].m_waterBuffer != water)
                {
                    buildingManager.m_buildings.m_buffer[building].m_waterBuffer = (ushort)water;
                    flag = currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating;
                }
                if (buildingManager.m_buildings.m_buffer[building].m_sewageBuffer != sewage)
                {
                    buildingManager.m_buildings.m_buffer[building].m_sewageBuffer = (ushort)sewage;
                    flag = currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating;
                }
                if (buildingManager.m_buildings.m_buffer[building].m_heatingBuffer != heating)
                {
                    buildingManager.m_buildings.m_buffer[building].m_heatingBuffer = (ushort)heating;
                    flag = currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating;
                }
                if (flag)
                {
                    buildingManager.UpdateBuildingColors(building);
                }
            }
            NetNode.Flags flags2 = flags & ~(NetNode.Flags.Water | NetNode.Flags.Sewage | NetNode.Flags.Heating);
            if (water != 0)
            {
                flags2 |= NetNode.Flags.Water;
            }
            if (sewage != 0)
            {
                flags2 |= NetNode.Flags.Sewage;
            }
            if (heating != 0)
            {
                flags2 |= NetNode.Flags.Heating;
            }
            if (flags2 != flags)
            {
                netManager.m_nodes.m_buffer[nodeID].m_flags = flags2;
                flag = currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating;
            }
            if (!flag)
            {
                return;
            }
            netManager.UpdateNodeColors((ushort)nodeID);
            for (int i = 0; i < 8; i++)
            {
                ushort segment = netManager.m_nodes.m_buffer[nodeID].GetSegment(i);
                if (segment != 0)
                {
                    netManager.UpdateSegmentColors(segment);
                }
            }
        }

        /// <summary>
        /// Expanded game PulseUnit struct to handle coordinate ranges outside byte limits.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Uses game names")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Game struct")]
        internal struct ExpandedPulseUnit
        {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
        }
    }
}
