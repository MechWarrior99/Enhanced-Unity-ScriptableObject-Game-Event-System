using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(GameEvent))]
public class GameEventEditor : Editor
{
    public enum SearchFilterType { All, Class, Method }

    private GameEvent _target;
    private List<StackTrace> _stackTraces;
    private List<GameEventListener> _listeners;
    private Vector2 _timelineScrollPosition = new Vector2();
    private Vector2 _listenersScrollPosition = new Vector2();
    private AdvancedSearchField<SearchFilterType> _timelineSearchFiled = new AdvancedSearchField<SearchFilterType>();
    private string _searchFilter = "";

    private void OnEnable()
    {
        _target = (GameEvent)target;
        _stackTraces = (List<StackTrace>)_target.GetType().GetField("_stackTraces", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_target);
        _listeners = (List<GameEventListener>)_target.GetType().GetField("_listeners", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_target);
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        RegisteredListenersList();
        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2);
        RaisedTimeline();
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        RaiseEventButton();
                
        Repaint();
    }

    private void RegisteredListenersList()
    {
        using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope("ToolbarButton"))
        {
            GUILayout.Label("Registered Listeners");
        }

        if (!Application.isPlaying)
        {
            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope(GameEventStyles.Box, GUILayout.Height(21)))
                GUILayout.Label("All GameEventListeners registered to this event. Populated at runtime.");
        }
        else
        {
            using (GUILayout.ScrollViewScope s = new GUILayout.ScrollViewScope(_listenersScrollPosition, GameEventStyles.Box))
            {
                _listenersScrollPosition = s.scrollPosition;

                if (_listeners.Count == 0)
                    GUILayout.Label("No GameEventListeners are registered to this event.");

                for (int i = 0; i < _listeners.Count; i++)
                {
                    GUIStyle style = i % 2 == 0 ? GameEventStyles.ListBackgroundEven : GameEventStyles.ListBackgroundOdd;
                    if (GUILayout.Button(_listeners[i].gameObject.name, style))
                    {
                        Selection.activeObject = _listeners[i];
                    }
                }
            }
        }
    }

    private void RaisedTimeline()
    {
        // Header
        using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope("ToolbarButton"))
        {
            GUILayout.Space(2);
            if (GUILayout.Button("Clear", "ToolbarButton", GUILayout.Width(40)))
            {
                _stackTraces.Clear();
            }
            GUILayout.Space(5);
            _searchFilter = _timelineSearchFiled.OnGUI(_searchFilter);
        }

        using (GUILayout.ScrollViewScope s = new GUILayout.ScrollViewScope(_timelineScrollPosition, GameEventStyles.Box, GUILayout.MinHeight(300)))
        {
            _timelineScrollPosition = s.scrollPosition;
            for (int i = 0; i < _stackTraces.Count; i++)
            {
                RaisedTimelineItem(i);
            }
        }        
    }

    private void RaisedTimelineItem(int index)
    {
        MethodBase method = _stackTraces[index].GetFrame(1).GetMethod();
        int lineNumber = _stackTraces[index].GetFrame(1).GetFileLineNumber();
        int columnNumber = _stackTraces[index].GetFrame(1).GetFileColumnNumber();

        string name = "<color=teal>" + method.DeclaringType.Name + ":" + "</color>" + "<color=olive>" + method.Name + "():" + lineNumber + "</color>";

        if (_searchFilter != "")
        {
            if (_timelineSearchFiled.FilterType == SearchFilterType.All)
            {
                if (!Contains(name, _searchFilter, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            else if (_timelineSearchFiled.FilterType == SearchFilterType.Class)
            {
                if (!Contains(method.DeclaringType.Name, _searchFilter, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            else if (_timelineSearchFiled.FilterType == SearchFilterType.Method)
            {
                if (!Contains(method.Name, _searchFilter, StringComparison.OrdinalIgnoreCase))
                    return;
            }
        }

        GUIStyle style = index % 2 == 0 ? GameEventStyles.ListBackgroundEven : GameEventStyles.ListBackgroundOdd;
        
        if (GUILayout.Button(name, style))
        {            
            TextAsset scriptAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetScriptPath(method.DeclaringType.Name));
            AssetDatabase.OpenAsset(scriptAsset, lineNumber, columnNumber);
        }
    }

    private void RaiseEventButton()
    {
        // Manually Raise the event.
        if (GUILayout.Button("Raise"))
        {
            _target.Raise();
        }
    }

    private string GetScriptPath (string scriptName)
    {
        string fullName = scriptName + ".cs";
        string[] res = Directory.GetFiles(Application.dataPath, fullName, SearchOption.AllDirectories);

        if (res.Length == 0)
        {
            Debug.Log("found nothing");
            return null;
        }
        
        string path = res[0].Replace("\\", "/");
        path = path.Substring(path.LastIndexOf("Assets"));        
        return path;
    }

    private bool Contains(string source, string toCheck, StringComparison comp)
    {
        return source.IndexOf(toCheck, comp) >= 0;
    }
}
