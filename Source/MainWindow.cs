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
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleButtonStyle;
        private GUIStyle versionStyle;

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
            double totalFullMass = 0.0;
            double totalResourceMass = 0.0;
            double totalEmptyMass = 0.0;
            double totalFullCost = 0.0;
            double totalResourceCost = 0.0;
            double totalEmptyCost = 0.0;

            var parts = new List<Part>(EditorLogic.fetch.ship.parts);
            parts.ForEach(part => part.UpdateOrgPosAndRot(part.localRoot));
            parts.Sort(SortParts);

            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Part Name", headerStyle);
            foreach (Part part in parts)
            {
                string partName = part.partInfo.title;
                if (partName.Length > 28)
                {
                    partName = partName.Substring(0, 28);
                }

                bool selected = GUILayout.Toggle(selectedParts.Contains(part), partName, buttonStyle);
                if (selected)
                {
                    selectedParts.Add(part);
                    part.SetHighlightColor(Color.blue);
                    part.SetHighlight(true);
                }
                else if (selectedParts.Remove(part))
                {
                    part.SetHighlightDefault();
                }
            }
            GUILayout.EndVertical();

            if (settings.showStage)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Stage", headerStyle, GUILayout.ExpandWidth(true));
                foreach (Part part in parts)
                {
                    GUILayout.Label(part.inverseStage.ToString(), labelStyle2, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndVertical();
            }

            if (settings.showFullMass)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Mass", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    if (part.PhysicsSignificance != 1 && part.name != "strutConnector" && part.name != "fuelLine" && !part.Modules.Contains("LaunchClamp"))
                    {
                        var mass = part.mass + part.GetResourceMass();
                        if (settings.showFullMass)
                            GUILayout.Label(mass.ToString("#,##0.###"), labelStyle2, GUILayout.ExpandWidth(true));
                        totalFullMass += mass;
                    }
                    else
                    {
                        // the part is "physics-less" in-game, so ignore the mass
                        if (settings.showFullMass)
                            GUILayout.Label("-", labelStyle2, GUILayout.ExpandWidth(true));
                    }
                }
            if (settings.showFullMass)
                GUILayout.EndVertical();

            if (settings.showResourceMass)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Resource Mass", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    var mass = part.GetResourceMass();
                    if (settings.showResourceMass)
                        GUILayout.Label(mass.ToString("#,##0.###"), labelStyle2, GUILayout.ExpandWidth(true));
                    totalResourceMass += mass;
                }
            if (settings.showResourceMass)
                GUILayout.EndVertical();

            if (settings.showEmptyMass)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Empty Mass", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    if (part.PhysicsSignificance != 1 && part.name != "strutConnector" && part.name != "fuelLine" && !part.Modules.Contains("LaunchClamp"))
                    {
                        var mass = part.mass;
                        if (settings.showEmptyMass)
                            GUILayout.Label(mass.ToString("#,##0.###"), labelStyle2, GUILayout.ExpandWidth(true));
                        totalEmptyMass += mass;
                    }
                    else
                    {
                        // the part is "physics-less" in-game, so ignore the mass
                        if (settings.showEmptyMass)
                            GUILayout.Label("-", labelStyle2, GUILayout.ExpandWidth(true));
                    }
                }
            if (settings.showEmptyMass)
                GUILayout.EndVertical();

            if (settings.showFullCost)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Cost", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    double missingResourcesCost = part.Resources.list.Sum(r => (r.maxAmount - r.amount) * r.info.unitCost);
                    double partCost = part.partInfo.cost + part.GetModuleCosts() - missingResourcesCost;
                    if (settings.showFullCost)
                        GUILayout.Label(partCost.ToString("#,##0.##"), labelStyle2, GUILayout.ExpandWidth(true));
                    totalFullCost += partCost;
                }
            if (settings.showFullCost)
                GUILayout.EndVertical();

            if (settings.showResourceCost)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Resource Cost", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    double resourceCost = part.Resources.list.Sum(r => r.amount * r.info.unitCost);
                    if (settings.showResourceCost)
                        GUILayout.Label(resourceCost.ToString("#,##0.##"), labelStyle2, GUILayout.ExpandWidth(true));
                    totalResourceCost += resourceCost;
                }
            if (settings.showResourceCost)
                GUILayout.EndVertical();

            if (settings.showEmptyCost)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Empty Cost", headerStyle, GUILayout.ExpandWidth(true));
            }
                foreach (Part part in parts)
                {
                    double maxResourceCost = part.Resources.list.Sum(r => r.maxAmount * r.info.unitCost);
                    double emptyPartCost = part.partInfo.cost + part.GetModuleCosts() - maxResourceCost;
                    if (settings.showEmptyCost)
                        GUILayout.Label(emptyPartCost.ToString("#,##0.##"), labelStyle2, GUILayout.ExpandWidth(true));
                    totalEmptyCost += emptyPartCost;
                }
            if (settings.showEmptyCost)
                GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.Space(2);
            GUILayout.Label("Parts: " + parts.Count.ToString(), labelStyle);
            GUILayout.Label("Mass: total: " + totalFullMass.ToString("#,##0.###") + ", resources: " + totalResourceMass.ToString("#,##0.###") + ", empty: " + totalEmptyMass.ToString("#,##0.###"), labelStyle);
            GUILayout.Label("Cost: total: " + totalFullCost.ToString("#,##0.##") + ", resources: " + totalResourceCost.ToString("#,##0.##") + ", empty: " + totalEmptyCost.ToString("#,##0.##"), labelStyle);

            showResources = GUILayout.Toggle(showResources, "Show resources", toggleButtonStyle, GUILayout.ExpandWidth(false));
            if (showResources)
            {
                foreach (KeyValuePair<string, ResourceInfo> entry in GetResources(parts))
                {
                    GUILayout.Label("  " + entry.Key + "  " + Utilities.FormatValue(entry.Value.amount, 3) + "U  " + Utilities.FormatValue(entry.Value.mass, 3) + "g  " + Utilities.FormatValue(entry.Value.cost, 2) + "K", labelStyle);
                }
            }
            GUILayout.Space(2);

            GUILayout.EndVertical();

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "S", closeButtonStyle))
            {
                settingsWindow.SetVisible(true);
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

                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.wordWrap = false;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = Color.white;
                headerStyle.alignment = TextAnchor.MiddleCenter;

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
            }
        }

        private static int SortParts(Part p1, Part p2)
        {
            return -p1.orgPos.y.CompareTo(p2.orgPos.y);
        }

        internal struct ResourceInfo
        {
            internal double amount;
            internal double mass;
            internal double cost;
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
    }
}
