/**
 * Settings.cs
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

namespace Tac
{
    internal class Settings
    {
        internal bool showStage { get; set; }
        internal bool showFullMass { get; set; }
        internal bool showResourceMass { get; set; }
        internal bool showEmptyMass { get; set; }
        internal bool showFullCost { get; set; }
        internal bool showResourceCost { get; set; }
        internal bool showEmptyCost { get; set; }
        internal bool highlightGridAreas { get; set; }
        internal bool showColoredNumbers { get; set; }
        internal bool showDeleteButtons { get; set; }

        internal Settings()
        {
            showStage = true;
            showFullMass = true;
            showResourceMass = true;
            showEmptyMass = true;
            showFullCost = true;
            showResourceCost = true;
            showEmptyCost = true;
            highlightGridAreas = true;
            showColoredNumbers = true;
            showDeleteButtons = true;
        }

        internal void Load(ConfigNode config)
        {
            showStage = Utilities.GetValue(config, "showStage", showStage);
            showFullMass = Utilities.GetValue(config, "showFullMass", showFullMass);
            showResourceMass = Utilities.GetValue(config, "showResourceMass", showResourceMass);
            showEmptyMass = Utilities.GetValue(config, "showEmptyMass", showEmptyMass);
            showFullCost = Utilities.GetValue(config, "showFullCost", showFullCost);
            showResourceCost = Utilities.GetValue(config, "showResourceCost", showResourceCost);
            showEmptyCost = Utilities.GetValue(config, "showEmptyCost", showEmptyCost);
            highlightGridAreas = Utilities.GetValue(config, "highlightGridAreas", highlightGridAreas);
            showColoredNumbers = Utilities.GetValue(config, "showColoredNumbers", showColoredNumbers);
            showDeleteButtons = Utilities.GetValue(config, "showDeleteButtons", showDeleteButtons);
        }

        internal void Save(ConfigNode config)
        {
            config.AddValue("showStage", showStage);
            config.AddValue("showFullMass", showFullMass);
            config.AddValue("showResourceMass", showResourceMass);
            config.AddValue("showEmptyMass", showEmptyMass);
            config.AddValue("showFullCost", showFullCost);
            config.AddValue("showResourceCost", showResourceCost);
            config.AddValue("showEmptyCost", showEmptyCost);
            config.AddValue("highlightGridAreas", highlightGridAreas);
            config.AddValue("showColoredNumbers", showColoredNumbers);
            config.AddValue("showDeleteButtons", showDeleteButtons);
        }
    }
}
