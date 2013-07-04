/**
 * TacPartLister.cs
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
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class TacPartLister : MonoBehaviour
    {
        private string configFilename;
        private MainWindow window;
        private Icon<TacPartLister> icon;

        void Awake()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            configFilename = IOUtils.GetFilePathFor(this.GetType(), "TacPartLister.cfg");
            window = new MainWindow();
            icon = new Icon<TacPartLister>(new Rect(Screen.width * 0.8f, Screen.height - 32, 32, 32), "icon.png", "PL",
                "Click to show the Part Lister", OnIconClicked);
        }

        void Start()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
            Load();

            icon.SetVisible(true);
        }

        void OnDestroy()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
            icon.SetVisible(false);
            Save();
        }

        private void Load()
        {
            if (File.Exists<TacPartLister>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                icon.Load(config);
                window.Load(config);
            }
        }

        private void Save()
        {
            ConfigNode config = new ConfigNode();
            icon.Save(config);
            window.Save(config);

            config.Save(configFilename);
        }

        private void OnIconClicked()
        {
            window.ToggleVisible();
        }
    }
}
