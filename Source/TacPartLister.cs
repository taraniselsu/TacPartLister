using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TacPartLister : MonoBehaviour
{
    private static string windowTitle = "TAC Part Lister";
    private static int windowId = windowTitle.GetHashCode();
    private static int iconId = windowId + 1;
    private static string configFilename = "TacPartLister.cfg";

    private Rect iconPos;
    private Rect windowPos;
    private bool showWindow;

    private GUIStyle labelStyle;
    private GUIStyle labelStyle2;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle iconStyle;

    private Vector2 scrollPosition;

    private bool iconMouseDown;
    private bool iconMouseWasDragged;
    private bool windowMouseDown;
    private bool windowMouseWasDragged;

    private Texture2D texture;

    private HashSet<Part> selectedParts;

    public class PartListerTest : KSP.Testing.UnitTest
    {
        public PartListerTest()
        {
            Debug.Log("TAC Part Lister [][" + Time.time + "]: Test creation");
            GameObject gameObject = new GameObject("TacPartLister", typeof(TacPartLister));
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    void Awake()
    {
        Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
        iconPos = new Rect(Screen.width * 0.8f, Screen.height - 32, 32, 32);
        windowPos = new Rect(Screen.width * 0.7f, 50.0f, Screen.width * 0.2f, Screen.height * 0.6f);
        showWindow = false;
        scrollPosition = Vector2.zero;

        iconMouseDown = false;
        iconMouseWasDragged = false;
        windowMouseDown = false;
        windowMouseWasDragged = false;

        selectedParts = new HashSet<Part>();
    }

    void Start()
    {
        Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");

        Load();
        LoadImages();
    }

    void OnDestroy()
    {
        Save();
    }

    void OnGUI()
    {
        if (HighLogic.LoadedSceneIsEditor && EditorLogic.fetch != null)
        {
            GUI.skin = HighLogic.Skin;
            ConfigureStyles();

            iconPos.y = Screen.height - 32;
            iconPos = EnsureVisible(iconPos);
            GUI.Label(iconPos, new GUIContent("PL", "Click to show the Part Lister"), iconStyle);

            HandleIconEvents();

            if (showWindow)
            {
                windowPos = EnsureVisible(windowPos);
                windowPos = GUILayout.Window(windowId, windowPos, DrawWindow, windowTitle, GUILayout.MinWidth(200), GUILayout.MinHeight(150));
            }
        }
    }

    void DrawWindow(int windowId)
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

        var resizeRect = new Rect(windowPos.width - 32, windowPos.height - 32, 32, 32);
        var content = ((texture != null) ? new GUIContent(texture, "Drag to resize the window.") : new GUIContent("Resize", "Drag to resize the window."));
        GUI.Label(resizeRect, content, iconStyle);

        GUILayout.Label("Parts: " + parts.Count.ToString() + ", Mass: " + totalMass.ToString("#,##0.###") + ", Cost: " + totalCost.ToString("#,##0.#"), labelStyle);

        GUILayout.EndVertical();

        HandleWindowEvents(resizeRect);

        GUI.DragWindow();
    }

    private void Load()
    {
        if (File.Exists<TacPartLister>(configFilename))
        {
            ConfigNode config = ConfigNode.Load(configFilename);

            float newFloat;
            if (config.HasValue("iconPos.x") && float.TryParse(config.GetValue("iconPos.x"), out newFloat))
            {
                iconPos.x = newFloat;
            }
            if (config.HasValue("iconPos.y") && float.TryParse(config.GetValue("iconPos.y"), out newFloat))
            {
                iconPos.y = newFloat;
            }

            if (config.HasValue("windowPos.x") && float.TryParse(config.GetValue("windowPos.x"), out newFloat))
            {
                windowPos.x = newFloat;
            }
            if (config.HasValue("windowPos.y") && float.TryParse(config.GetValue("windowPos.y"), out newFloat))
            {
                windowPos.y = newFloat;
            }
            if (config.HasValue("windowPos.width") && float.TryParse(config.GetValue("windowPos.width"), out newFloat))
            {
                windowPos.width = newFloat;
            }
            if (config.HasValue("windowPos.height") && float.TryParse(config.GetValue("windowPos.height"), out newFloat))
            {
                windowPos.height = newFloat;
            }
        }
    }

    private void Save()
    {
        ConfigNode config = new ConfigNode();

        config.AddValue("iconPos.x", iconPos.x);
        config.AddValue("iconPos.y", iconPos.y);

        config.AddValue("windowPos.x", windowPos.x);
        config.AddValue("windowPos.y", windowPos.y);
        config.AddValue("windowPos.width", windowPos.width);
        config.AddValue("windowPos.height", windowPos.height);

        config.Save(configFilename);
    }

    private void LoadImages()
    {
        string imageFilename = "resize.png";
        if (File.Exists<TacPartLister>(imageFilename))
        {
            var bytes = File.ReadAllBytes<TacPartLister>(imageFilename);
            texture = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);
        }
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
        }
    }

    private void HandleIconEvents()
    {
        var theEvent = Event.current;
        if (theEvent != null)
        {
            if (theEvent.type == EventType.MouseDown && !iconMouseDown && theEvent.button == 0 && iconPos.Contains(theEvent.mousePosition))
            {
                iconMouseDown = true;
                theEvent.Use();
            }
            else if (theEvent.type == EventType.MouseDrag && iconMouseDown && theEvent.button == 0)
            {
                iconMouseWasDragged = true;
                iconPos.x += theEvent.delta.x;
                iconPos.y += theEvent.delta.y;
                theEvent.Use();
            }
            else if (theEvent.type == EventType.MouseUp && iconMouseDown && theEvent.button == 0)
            {
                if (!iconMouseWasDragged)
                {
                    showWindow = !showWindow;
                }

                iconMouseDown = false;
                iconMouseWasDragged = false;
                theEvent.Use();
            }
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
                windowMouseWasDragged = true;
                windowPos.width += theEvent.delta.x;
                windowPos.height += theEvent.delta.y;
                theEvent.Use();
            }
            else if (theEvent.type == EventType.MouseUp && windowMouseDown && theEvent.button == 0)
            {
                windowMouseDown = false;
                windowMouseWasDragged = false;
                theEvent.Use();
            }
        }
        return resizeRect;
    }

    private static Rect EnsureVisible(Rect pos)
    {
        const int min = 16;

        if ((pos.x + pos.width) < min)
        {
            pos.x = min - pos.width;
        }
        if (pos.x > (Screen.width - min))
        {
            pos.x = Screen.width - min;
        }
        if ((pos.y + pos.height) < min)
        {
            pos.y = min - pos.height;
        }
        if (pos.y > (Screen.height - min))
        {
            pos.y = Screen.height - min;
        }

        return pos;
    }

    private static int SortParts(Part p1, Part p2)
    {
        return -p1.orgPos.y.CompareTo(p2.orgPos.y);
    }
}
