﻿using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class MainWindow
    {
        private static string windowTitle = "TAC Part Lister";
        private static int windowId = windowTitle.GetHashCode();

        private Rect windowPos;
        private bool showWindow;

        private GUIStyle labelStyle;
        private GUIStyle labelStyle2;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle iconStyle;

        private Vector2 scrollPosition;

        private bool windowMouseDown;

        private HashSet<Part> selectedParts;

        private Texture2D texture;

        public MainWindow()
        {
            Debug.Log("TAC Part Lister/MainWindow [" + this.GetHashCode().ToString("X") + "][" + Time.time + "]: Constructor");

            windowPos = new Rect(Screen.width * 0.7f, 50.0f, Screen.width * 0.2f, Screen.height * 0.6f);
            showWindow = false;
            scrollPosition = Vector2.zero;

            windowMouseDown = false;

            selectedParts = new HashSet<Part>();

            texture = Utilities.LoadImage(IOUtils.GetFilePathFor(typeof(TacPartLister), "resize.png"));
        }

        public bool IsVisible()
        {
            return showWindow;
        }

        public void SetVisible(bool newValue)
        {
            showWindow = newValue;
        }

        public void OnGUI()
        {
            if (showWindow)
            {
                GUI.skin = HighLogic.Skin;
                ConfigureStyles();

                windowPos = Utilities.EnsureVisible(windowPos);
                windowPos = GUILayout.Window(windowId, windowPos, DrawWindow, windowTitle, GUILayout.MinWidth(64), GUILayout.MinHeight(64));
            }
        }

        private void DrawWindow(int windowId)
        {
            double totalMass = 0.0;
            double totalCost = 0.0;

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

            GUILayout.BeginVertical();
            GUILayout.Label("Stage", headerStyle);
            foreach (Part part in parts)
            {
                GUILayout.Label(part.inverseStage.ToString(), labelStyle2);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Mass", headerStyle);
            foreach (Part part in parts)
            {
                var mass = part.mass + part.GetResourceMass();
                GUILayout.Label(mass.ToString("#,##0.###"), labelStyle2);
                totalMass += mass;
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Cost", headerStyle);
            foreach (Part part in parts)
            {
                GUILayout.Label(part.partInfo.cost.ToString("#,##0.#"), labelStyle2);
                totalCost += part.partInfo.cost;
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            var resizeRect = new Rect(windowPos.width - 16, windowPos.height - 16, 16, 16);
            var content = ((texture != null) ? new GUIContent(texture, "Drag to resize the window.") : new GUIContent("Resize", "Drag to resize the window."));
            GUI.Label(resizeRect, content, iconStyle);

            GUILayout.Label("Parts: " + parts.Count.ToString() + ", Mass: " + totalMass.ToString("#,##0.###") + ", Cost: " + totalCost.ToString("#,##0.#"), labelStyle);

            GUILayout.EndVertical();

            HandleWindowEvents(resizeRect);

            GUI.DragWindow();
        }

        public void Load(ConfigNode config)
        {
            windowPos.x = Utilities.GetValue(config, "window.x", windowPos.x);
            windowPos.y = Utilities.GetValue(config, "window.y", windowPos.y);
            windowPos.width = Utilities.GetValue(config, "window.width", windowPos.width);
            windowPos.height = Utilities.GetValue(config, "window.height", windowPos.height);
        }

        public void Save(ConfigNode config)
        {
            config.AddValue("window.x", windowPos.x);
            config.AddValue("window.y", windowPos.y);
            config.AddValue("window.width", windowPos.width);
            config.AddValue("window.height", windowPos.height);
        }

        private void ConfigureStyles()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.wordWrap = false;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                labelStyle2 = new GUIStyle(GUI.skin.label);
                labelStyle2.wordWrap = false;
                labelStyle2.fontStyle = FontStyle.Normal;
                labelStyle2.normal.textColor = Color.white;
                labelStyle2.alignment = TextAnchor.MiddleCenter;

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

                iconStyle = new GUIStyle(GUI.skin.button);
                iconStyle.alignment = TextAnchor.MiddleCenter;
                iconStyle.padding = new RectOffset(1, 1, 1, 1);
            }
        }

        private Rect HandleWindowEvents(Rect resizeRect)
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (theEvent.type == EventType.MouseDown && !windowMouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
                {
                    windowMouseDown = true;
                    theEvent.Use();
                }
                else if (theEvent.type == EventType.MouseDrag && windowMouseDown && theEvent.button == 0)
                {
                    windowPos.width += theEvent.delta.x;
                    windowPos.height += theEvent.delta.y;
                    theEvent.Use();
                }
                else if (theEvent.type == EventType.MouseUp && windowMouseDown && theEvent.button == 0)
                {
                    windowMouseDown = false;
                    theEvent.Use();
                }
            }
            return resizeRect;
        }

        private static int SortParts(Part p1, Part p2)
        {
            return -p1.orgPos.y.CompareTo(p2.orgPos.y);
        }
    }
}
