/**
 * SettingsWindow.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class SettingsWindow : Window<SettingsWindow>
    {
        private readonly Settings settings;
        private readonly string version;

        private GUIStyle versionStyle;

        internal SettingsWindow(Settings settings)
            : base("TAC Part Lister Settings", 220, 260)
        {
            this.settings = settings;
            version = Utilities.GetDllVersion(this);

            this.Resizable = false;
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();
            versionStyle = Utilities.GetVersionStyle();
        }

        protected override void DrawWindowContents(int windowID)
        {
            GUILayout.BeginVertical();

            settings.showDeleteButtons = GUILayout.Toggle(settings.showDeleteButtons, "Show \"Delete\" Buttons");
            settings.showStage = GUILayout.Toggle(settings.showStage, "Show Stage Number");
            settings.showFullMass = GUILayout.Toggle(settings.showFullMass, "Show Full Mass");
            settings.showResourceMass = GUILayout.Toggle(settings.showResourceMass, "Show Resource Mass");
            settings.showEmptyMass = GUILayout.Toggle(settings.showEmptyMass, "Show Empty (Dry) Mass");
            settings.showFullCost = GUILayout.Toggle(settings.showFullCost, "Show Full Cost");
            settings.showResourceCost = GUILayout.Toggle(settings.showResourceCost, "Show Resource Cost");
            settings.showEmptyCost = GUILayout.Toggle(settings.showEmptyCost, "Show Empty Cost");
            settings.includePhysicsLessParts = GUILayout.Toggle(settings.includePhysicsLessParts, "Include Physics-less Parts");
            settings.highlightGridAreas = GUILayout.Toggle(settings.highlightGridAreas, "Highlight Mass & Cost areas");
            settings.showColoredNumbers = GUILayout.Toggle(settings.showColoredNumbers, "Show Colored Numbers");

            GUILayout.EndVertical();

            GUILayout.Space(8);
            GUI.Label(new Rect(4, windowPos.height - 14, windowPos.width - 20, 12), "TAC Part Lister v" + version, versionStyle);
        }
    }
}
