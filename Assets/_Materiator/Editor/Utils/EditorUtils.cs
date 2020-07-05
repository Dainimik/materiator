using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace Materiator
{
    public static class EditorUtils
    {
        /// <summary>
        /// Creates a new inspector window instance and locks it to inspect the specified target
        /// </summary>
        public static void InspectTarget(Object target)
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
            inspectorInstance.Show();
            var prevSelection = Selection.activeGameObject;
            Selection.activeObject = target;
            var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
            Selection.activeGameObject = prevSelection;
        }

        public static void DrawMessages(List<string> messages, MessageType messageType)
        {
            for (int i = 0; i < messages.Count; i++)
                EditorGUILayout.HelpBox(messages[i], messageType);
        }

        public static int ShowObjectPicker<T>(Object obj = null, bool allowSceneObjects = false, string searchFilter = null) where T : Object
        {
            var currentPickerWindowID = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<T>(obj, allowSceneObjects, searchFilter, currentPickerWindowID);
            return currentPickerWindowID;
        }

        public delegate void DrawDropAreaAction<T>(List<T> obj);
        public static ICollection<T> DrawDropArea<T>(VisualElement dropArea, DrawDropAreaAction<T> Action) where T : Object
        {
            var objs = new List<T>();
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.contentRect.Contains(evt.mousePosition))
                        return null;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                            if (draggedObject.GetType() == typeof(T))
                                objs.Add((T)draggedObject);

                        Action(objs);
                    }
                    break;
            }
            return null;
        }

        public static string DrawSearchbar(string searchString)
        {
            Event e = Event.current;
            var rect = EditorGUILayout.BeginHorizontal();
            Rect cancelButtonRect = new Rect(EditorGUIUtility.currentViewWidth - 20f, rect.y, 20f, 20f);

            if (searchString != "")
                GUI.enabled = !(e.isMouse && cancelButtonRect.Contains(e.mousePosition));
            
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            if (searchString != "")
            {
                if (GUI.Button(cancelButtonRect, "X", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    searchString = "";
                    GUI.FocusControl(null);
                }
            }

            return searchString;
        }

        public static void LoseFocusButton(Rect rect)
        {
            if (GUI.Button(rect, "", GUIStyle.none))
                GUI.FocusControl(null);
        }

        public static void MarkOpenPrefabSceneDirty()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }
}