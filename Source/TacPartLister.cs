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
        private Icon icon;

        void Awake()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            configFilename = IOUtils.GetFilePathFor(this.GetType(), "TacPartLister.cfg");
            window = new MainWindow();
            icon = new Icon(new Rect(Screen.width * 0.8f, Screen.height - 32, 32, 32),
                IOUtils.GetFilePathFor(this.GetType(), "icon.png"), OnIconClicked);
        }

        void Start()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
            Load();
        }

        void OnDestroy()
        {
            Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
            Save();
        }

        void OnGUI()
        {
            icon.OnGUI();
            window.OnGUI();
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
            window.SetVisible(!window.IsVisible());
        }
    }
}
