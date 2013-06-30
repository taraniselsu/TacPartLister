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
