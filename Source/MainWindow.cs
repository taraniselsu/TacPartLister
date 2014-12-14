/**
 * MainWindow.cs
 *
 * Thunder Aerospace Corporation's Part Lister for the Kerbal Space Program, by Taranis Elsu
 *
 * (C) Copyright 2013, Taranis Elsu
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 *
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 *
 * Non-commercial - You may not use this work for commercial purposes.
 *
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 *
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using HighlightingSystem;
using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class MainWindow : Window<TacPartLister>
    {
        private readonly Settings settings;
        private readonly SettingsWindow settingsWindow;
        private readonly string version;
        private readonly HashSet<Part> selectedParts = new HashSet<Part>();
        private Vector2 scrollPosition = Vector2.zero;
        private bool showResources = false;

        private GUIStyle labelStyle;
        private GUIStyle labelStyle2;
        private GUIStyle labelStyleRes;
        private GUIStyle labelStyleEmpty;
        private GUIStyle headerStyleTop;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleButtonStyle;
        private GUIStyle versionStyle;
        private GUIStyle gridAreaStyle;

        private double totalFullMass;
        private double totalResourceMass;
        private double totalEmptyMass;
        private double totalFullCost;
        private double totalResourceCost;
        private double totalEmptyCost;

        private List<PartStringInfo> partInfos = new List<PartStringInfo>();
        private Dictionary<string, ResourceInfo> resourceInfos = new Dictionary<string, ResourceInfo>();

        public MainWindow(Settings settings, SettingsWindow settingsWindow)
            : base("TAC Part Lister", 360, Screen.height * 0.6f)
        {
            this.Log("Constructor");
            this.settings = settings;
            this.settingsWindow = settingsWindow;
            version = Utilities.GetDllVersion(this);
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (!newValue)
            {
                settingsWindow.SetVisible(false);
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("", headerStyleTop);
            GUILayout.Label("Part Name", headerStyle);
            foreach (PartStringInfo partInfo in partInfos)
            {
                bool selected = GUILayout.Toggle(selectedParts.Contains(partInfo.part), partInfo.nameLabel, buttonStyle);
                if (selected)
                {
                    if (!selectedParts.Contains(partInfo.part))
                    {
                        selectedParts.Add(partInfo.part);
                        Highlight(partInfo.part);
                    }
                }
                else if (selectedParts.Remove(partInfo.part))
                {
                    ClearHighlight(partInfo.part);
                }
            }
            GUILayout.EndVertical();

            if (settings.showStage)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("", headerStyleTop);
                GUILayout.Label("Stage", headerStyle, GUILayout.ExpandWidth(true));
                foreach (PartStringInfo partInfo in partInfos)
                {
                    GUILayout.Label(partInfo.stage, labelStyle2, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndVertical();
            }

            bool showMassArea = settings.showFullMass || settings.showResourceMass || settings.showEmptyMass;
            if (showMassArea)
            {
                if (settings.highlightGridAreas)
                {
                    GUILayout.BeginVertical(gridAreaStyle, GUILayout.ExpandHeight(true));
                }
                else
                {
                    GUILayout.BeginVertical();
                }

                GUILayout.Label("Mass", headerStyleTop, GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();

                if (settings.showFullMass)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Wet", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.fullMass, labelStyle2, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                if (settings.showResourceMass)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Res.", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.resourceMass, labelStyleRes, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                if (settings.showEmptyMass)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Dry", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.emptyMass, labelStyleEmpty, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            bool showCostArea = settings.showFullCost || settings.showResourceCost || settings.showEmptyCost;
            if (showCostArea)
            {
                if (settings.highlightGridAreas)
                {
                    GUILayout.BeginVertical(gridAreaStyle, GUILayout.ExpandHeight(true));
                }
                else
                {
                    GUILayout.BeginVertical();
                }

                GUILayout.Label("Cost", headerStyleTop, GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();

                if (settings.showFullCost)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Total", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.fullCost, labelStyle2, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                if (settings.showResourceCost)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Res.", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.resourceCost, labelStyleRes, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                if (settings.showEmptyCost)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Part", headerStyle, GUILayout.ExpandWidth(true));
                    foreach (PartStringInfo partInfo in partInfos)
                    {
                        GUILayout.Label(partInfo.emptyCost, labelStyleEmpty, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.Space(2);
            GUILayout.Label("Parts: " + partInfos.Count.ToString(), labelStyle);
            GUILayout.Label("Mass: wet: " + totalFullMass.ToString("#,##0.###") + ", resources: " + totalResourceMass.ToString("#,##0.###") + ",   dry: " + totalEmptyMass.ToString("#,##0.###"), labelStyle);
            GUILayout.Label("Cost: total: " + totalFullCost.ToString("#,##0.##") + ", resources: " + totalResourceCost.ToString("#,##0.##") + ", empty: " + totalEmptyCost.ToString("#,##0.##"), labelStyle);

            showResources = GUILayout.Toggle(showResources, "Show resources", toggleButtonStyle, GUILayout.ExpandWidth(false));
            if (showResources)
            {
                foreach (KeyValuePair<string, ResourceInfo> entry in resourceInfos)
                {
                    GUILayout.Label("  " + entry.Key + "  " + Utilities.FormatValue(entry.Value.amount, 3) + "U  " + Utilities.FormatValue(entry.Value.mass, 3) + "g  " + Utilities.FormatValue(entry.Value.cost, 2) + "K", labelStyle);
                }
            }
            GUILayout.Space(2);

            GUILayout.EndVertical();

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "S", closeButtonStyle))
            {
                settingsWindow.ToggleVisible();
            }
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.wordWrap = false;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 1;

                labelStyle2 = new GUIStyle(GUI.skin.label);
                labelStyle2.wordWrap = false;
                labelStyle2.fontStyle = FontStyle.Normal;
                labelStyle2.normal.textColor = Color.white;
                labelStyle2.alignment = TextAnchor.MiddleRight;

                labelStyleRes = new GUIStyle(labelStyle2);
                labelStyleRes.normal.textColor = Color.yellow + Color.gray * 1.5f;

                labelStyleEmpty = new GUIStyle(labelStyle2);
                labelStyleEmpty.normal.textColor = Color.cyan + Color.gray * 1.5f;

                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.wordWrap = false;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = Color.white;
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.padding.top = 0;

                headerStyleTop = new GUIStyle(headerStyle);
                headerStyleTop.padding.bottom = 0;

                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.wordWrap = false;
                buttonStyle.fontStyle = FontStyle.Normal;
                buttonStyle.normal.textColor = Color.white;
                buttonStyle.alignment = TextAnchor.MiddleLeft;
                buttonStyle.padding = new RectOffset(6, 2, 4, 2);

                toggleButtonStyle = new GUIStyle(GUI.skin.button);
                toggleButtonStyle.wordWrap = false;
                toggleButtonStyle.fontStyle = FontStyle.Normal;
                toggleButtonStyle.normal.textColor = Color.white;
                toggleButtonStyle.alignment = TextAnchor.MiddleCenter;
                toggleButtonStyle.margin.top = 1;
                toggleButtonStyle.margin.bottom = 1;
                toggleButtonStyle.padding.top = 3;
                toggleButtonStyle.padding.bottom = 1;

                versionStyle = Utilities.GetVersionStyle();

                gridAreaStyle = new GUIStyle(GUI.skin.scrollView);
            }
        }

        private static int SortParts(Part p1, Part p2)
        {
            return -p1.orgPos.y.CompareTo(p2.orgPos.y);
        }

        private Dictionary<string, ResourceInfo> GetResources(List<Part> parts)
        {
            Dictionary<string, ResourceInfo> resourceInfos = new Dictionary<string, ResourceInfo>();

            foreach (Part p in parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    ResourceInfo resourceInfo;
                    if (!resourceInfos.TryGetValue(r.resourceName, out resourceInfo))
                    {
                        resourceInfo = new ResourceInfo();
                    }

                    resourceInfo.amount += r.amount;
                    resourceInfo.mass += r.amount * r.info.density * 1000000.0; // mass in grams (g)
                    resourceInfo.cost += r.amount * r.info.unitCost;

                    resourceInfos[r.resourceName] = resourceInfo;
                }
            }

            return resourceInfos;
        }

        private void Highlight(Part part)
        {
            part.SetHighlightType(Part.HighlightType.AlwaysOn);
            part.SetHighlightColor(Color.blue);
            part.SetHighlight(true, false);

            try
            {
                // 3D edge highlighting
                Highlighter h = part.FindModelTransform("model").gameObject.AddComponent<Highlighter>();
                h.SeeThroughOn();
                h.ConstantOn(Color.blue);
            }
            catch (Exception ex)
            {
                this.LogWarning("Exception in Highlight: " + ex.Message);
            }
        }

        private void ClearHighlight(Part part)
        {
            part.SetHighlightDefault();

            try
            {
                // 3D edge highlighting
                Highlighter h = part.FindModelTransform("model").gameObject.GetComponent<Highlighter>();
                if (h != null)
                {
                    h.Die();
                }
            }
            catch (Exception ex)
            {
                this.LogWarning("Exception in ClearHighlight: " + ex.Message);
            }
        }

        private bool IsPhysicsLess(Part part)
        {
            return part.PhysicsSignificance == 1 || part.name == "strutConnector" || part.name == "fuelLine" || part.Modules.Contains("LaunchClamp");
        }

        public void RefreshPartInfos(List<Part> inParts)
        {
            this.Log("RefreshPartInfos - inParts.Count: " + inParts.Count);
            partInfos = new List<PartStringInfo>();

            totalFullMass = 0.0;
            totalResourceMass = 0.0;
            totalEmptyMass = 0.0;
            totalFullCost = 0.0;
            totalResourceCost = 0.0;
            totalEmptyCost = 0.0;

            var parts = new List<Part>(inParts);

            parts.ForEach(part => part.UpdateOrgPosAndRot(part.localRoot));
            parts.Sort(SortParts);

            foreach (Part part in parts)
            {
                PartStringInfo partInfo;
                partInfo.part = part;

                partInfo.nameLabel = part.partInfo.title;
                if (partInfo.nameLabel.Length > 28)
                {
                    partInfo.nameLabel = partInfo.nameLabel.Substring(0, 28);
                }

                partInfo.stage = part.inverseStage.ToString();

                // mass
                if (settings.includePhysicsLessParts || !IsPhysicsLess(part))
                {
                    float fullMass = part.mass + part.GetModuleMass(part.mass) + part.GetResourceMass();
                    partInfo.fullMass = fullMass.ToString("#,##0.###");
                    totalFullMass += fullMass;
                }
                else
                {
                    // the part is "physics-less" in-game, so ignore the mass
                    partInfo.fullMass = "-";
                }

                bool isResourceContainer = part.Resources.list.Any(r => r.maxAmount > 0.001);
                if (isResourceContainer)
                {
                    float resourceMass = part.GetResourceMass();
                    partInfo.resourceMass = resourceMass.ToString("#,##0.###");
                    totalResourceMass += resourceMass;
                }
                else
                {
                    // the part has no resources, or has maxAmount of all resources set to 0 (for
                    // example the electricity generators or consumers of fuel, such as engines)
                    partInfo.resourceMass = "-";
                }

                if (settings.includePhysicsLessParts || !IsPhysicsLess(part))
                {
                    float emptyMass = part.mass + part.GetModuleMass(part.mass);
                    partInfo.emptyMass = emptyMass.ToString("#,##0.###");
                    totalEmptyMass += emptyMass;
                }
                else
                {
                    // the part is "physics-less" in-game, so ignore the mass
                    partInfo.emptyMass = "-";
                }

                // cost
                double maxCostOfResources = part.Resources.list.Sum(r => r.maxAmount * r.info.unitCost);
                double emptyCost = part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost) - maxCostOfResources;
                partInfo.emptyCost = emptyCost.ToString("#,##0.##");
                totalEmptyCost += emptyCost;

                double resourceCost = 0.0;
                if (isResourceContainer)
                {
                    resourceCost = part.Resources.list.Sum(r => r.amount * r.info.unitCost);
                    partInfo.resourceCost = resourceCost.ToString("#,##0.##");
                    totalResourceCost += resourceCost;
                }
                else
                {
                    // the part has no resources
                    partInfo.resourceCost = "-";
                }

                double fullCost = emptyCost + resourceCost;
                partInfo.fullCost = fullCost.ToString("#,##0.##");
                totalFullCost += fullCost;

                partInfos.Add(partInfo);
            }

            resourceInfos = GetResources(parts);
        }

        internal struct PartStringInfo
        {
            internal Part part;
            internal string nameLabel;
            internal string stage;
            internal string fullMass;
            internal string resourceMass;
            internal string emptyMass;
            internal string fullCost;
            internal string resourceCost;
            internal string emptyCost;
        }

        internal struct ResourceInfo
        {
            internal double amount;
            internal double mass;
            internal double cost;
        }
    }
}
