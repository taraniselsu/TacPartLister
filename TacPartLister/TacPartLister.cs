using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TacPartLister : MonoBehaviour
{
    private static string windowTitle = "TAC Part Lister";
    private static int windowId = windowTitle.GetHashCode();
    private static int iconId = windowId + 1;

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
    }

    void Start()
    {
        Debug.Log("TAC Part Lister [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");

        string filename = IOUtils.GetFilePathFor(this.GetType(), "resize.png");
        if (File.Exists<TacPartLister>(filename))
        {
            texture = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            string temp = "file://" + filename.Replace("\\", "/").Replace("/KSP_Data/..", "");

            var www = new WWW(temp);
            www.LoadImageIntoTexture(texture);
        }
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
            if (partName.Length > 24)
            {
                partName = partName.Substring(0, 24);
            }
            GUILayout.Label(partName, labelStyle);
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
