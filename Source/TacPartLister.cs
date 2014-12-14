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
        private Settings settings;
        private SettingsWindow settingsWindow;
        private MainWindow window;
        private ButtonWrapper button;
        private const string lockName = "TACPL_EditorLock";
        private const ControlTypes desiredLock = ControlTypes.EDITOR_SOFT_LOCK | ControlTypes.EDITOR_UI | ControlTypes.EDITOR_LAUNCH;

        void Awake()
        {
            this.Log("Awake");
            configFilename = IOUtils.GetFilePathFor(this.GetType(), "TacPartLister.cfg");
            settings = new Settings();
            settingsWindow = new SettingsWindow(settings);
            window = new MainWindow(settings, settingsWindow);
            button = new ButtonWrapper(new Rect(Screen.width * 0.225f, 0, 32, 32),
                "TacPartLister/Textures/button", "PL",
                "TAC Part Lister", OnIconClicked);
        }

        void Start()
        {
            this.Log("Start");
            Load();

            button.Visible = true;

            GameEvents.onEditorShipModified.Add(OnEditorShipModified);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
        }

        void Update()
        {
            if ((window.IsVisible() && window.Contains(Event.current.mousePosition))
                || (settingsWindow.IsVisible() && settingsWindow.Contains(Event.current.mousePosition)))
            {
                if (InputLockManager.GetControlLock(lockName) != desiredLock)
                {
                    InputLockManager.SetControlLock(desiredLock, lockName);
                }
            }
            else
            {
                if (InputLockManager.GetControlLock(lockName) == desiredLock)
                {
                    InputLockManager.RemoveControlLock(lockName);
                }
            }
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            Save();
            button.Destroy();

            GameEvents.onEditorShipModified.Remove(OnEditorShipModified);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);

            // Make sure we remove our locks
            if (InputLockManager.GetControlLock(lockName) == desiredLock)
            {
                InputLockManager.RemoveControlLock(lockName);
            }
        }

        private void Load()
        {
            if (File.Exists<TacPartLister>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                settings.Load(config);
                button.Load(config);
                window.Load(config);
                settingsWindow.Load(config);
            }
        }

        private void Save()
        {
            ConfigNode config = new ConfigNode();
            settings.Save(config);
            button.Save(config);
            window.Save(config);
            settingsWindow.Save(config);

            config.Save(configFilename);
        }

        private void OnIconClicked()
        {
            window.ToggleVisible();
        }

        private void OnEditorShipModified(ShipConstruct shipConstruct)
        {
            this.Log("OnEditorShipModified - shipConstruct: " + shipConstruct);
            window.RefreshPartInfos(shipConstruct.parts);
            // may be EditorLogic.fetch.ship.parts , not shipConstruct.parts ? I'm not sure. It seems to me that it doesn't matter
        }

        private void OnLevelWasLoaded(GameScenes gameScene)
        {
            this.Log("Game scene loaded: " + gameScene);
            window.RefreshPartInfos(EditorLogic.fetch.ship.parts);
        }
    }
}
