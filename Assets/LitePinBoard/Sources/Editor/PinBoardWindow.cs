#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using LitePinBoard.Sources.Editor.Data;

namespace LitePinBoard.Sources.Editor
{
    public class PinBoardWindow : EditorWindow
    {
        private PinBoardData data;
        private Vector2 scrollPos;
        private string searchFilter = "";
        private bool showAssets = true;
        private bool showSceneObjects = true;
        private bool isDragging = false;
        
        private static PinBoardWindow Instance { get; set; }
        
        [MenuItem("Window/Lite Pin Board %&p")]
        public static void ShowWindow()
        {
            Instance = GetWindow<PinBoardWindow>("Lite Pin Board");
            Instance.minSize = new Vector2(300, 200);
            Instance.LoadData();
            Instance.Show();
        }

        private void OnEnable() 
        {
            Instance = this;
            LoadData();
            
            EditorApplication.hierarchyChanged += Repaint;
            EditorApplication.projectChanged += Repaint;
        }

        private void OnDisable() 
        {
            SaveData();
            EditorApplication.hierarchyChanged -= Repaint;
            EditorApplication.projectChanged -= Repaint;
        }

        public void LoadData()
        {
            string path = "Assets/LitePinBoardData.asset";
            data = AssetDatabase.LoadAssetAtPath<PinBoardData>(path);
            
            if (data == null)
            {
                data = CreateInstance<PinBoardData>();
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
            }
        }

        private void SaveData()
        {
            if (data != null)
            {
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
        }

        private void OnGUI()
        {
            HandleDragAndDrop();
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            searchFilter = GUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            
            if (data.items.Count == 0)
                ShowEmptyState();
            else
                DrawItemsList();
            
            GUILayout.EndScrollView();
            
            if (isDragging == true)
            {
                Rect dropArea = new Rect(0, 0, position.width, position.height);
                EditorGUI.DrawRect(dropArea, new Color(0.3f, 0.5f, 0.8f, 0.2f));
            }
        }

        private void ShowEmptyState()
        {
            GUILayout.FlexibleSpace();
            var style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUILayout.Label("Empty", style);
            GUILayout.FlexibleSpace();
        }

        private void DrawItemsList()
        {
            var filtered = data.items
                .Where(item => (showAssets && !item.IsSceneObject()) || (showSceneObjects && item.IsSceneObject()))
                .Where(item => string.IsNullOrEmpty(searchFilter) || item.Name.ToLower().Contains(searchFilter.ToLower()))
                .ToList();

            if (filtered.Count == 0)
            {
                GUILayout.Label("No items match filters", EditorStyles.centeredGreyMiniLabel);
                
                return;
            }

            for (int i = 0; i < filtered.Count; i++)
                DrawPinItem(filtered[i], i);
        }

        private void DrawPinItem(PinBoardItem item, int index)
        {
            GUILayout.BeginHorizontal("box", GUILayout.Height(32));
            
            if (item.Icon != null)
                GUILayout.Label(new GUIContent(item.Icon), GUILayout.Width(24), GUILayout.Height(24));
            else
                GUILayout.Label(EditorGUIUtility.IconContent("DefaultAsset Icon"), GUILayout.Width(24), GUILayout.Height(24));

            GUIStyle nameStyle = new GUIStyle(GUI.skin.button) 
            { 
                alignment = TextAnchor.MiddleLeft, 
                richText = true,
                fontSize = 12
            };

            string displayName = item.Name;
            bool isMissing = IsItemMissing(item);
            
            if (isMissing == true)
            {
                nameStyle.normal.textColor = Color.red;
                displayName = $"<color=red>{item.Name} (Missing)</color>";
            }
            else if (item.IsSceneObject() && !IsSceneObjectLoaded(item))
            {
                displayName = $"<color=gray>{item.Name} (Not in scene)</color>";
            }

            if (GUILayout.Button(displayName, nameStyle, GUILayout.ExpandWidth(true)))
                HighlightOriginal(item);

            if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(24)))
                RemoveItem(item);

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private bool IsItemMissing(PinBoardItem item)
        {
            if (item.IsSceneObject() == true) 
                return false;
            
            return AssetDatabase.AssetPathToGUID(item.Path) != item.Guid;
        }

        private bool IsSceneObjectLoaded(PinBoardItem item)
        {
            if (item.IsSceneObject() == false) 
                return false;
            
            var obj = EditorUtility.InstanceIDToObject(item.SceneHandle);
            return obj != null && obj is GameObject;
        }

        private void HighlightOriginal(PinBoardItem item)
        {
            if (item.IsSceneObject() == true)
            {
                var obj = EditorUtility.InstanceIDToObject(item.SceneHandle);
                
                if (obj is GameObject go)
                {
                    Selection.activeGameObject = go;
                    SceneView.lastActiveSceneView?.FrameSelected();
                    EditorGUIUtility.PingObject(go);
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Found", "Object is not in the current scene", "OK");
                }
            }
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(item.Path);
                
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(asset);
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Found", "Asset not found at path:\n" + item.Path, "OK");
                }
            }
        }

        private void PinSelectedObjects()
        {
            foreach (var obj in Selection.objects)
            {
                AddItem(obj);
            }
        }

        public void AddItem(Object obj)
        {
            if (obj == null) 
                return;
            
            if (data.items.Any(i => i.SceneHandle == obj.GetInstanceID()))
                return;

            var item = new PinBoardItem(obj);
            
            if (item.IsValid() == false)
            {
                Debug.LogWarning($"[LitePinBoard] Cannot pin {obj.name}: not a valid asset or scene object");
                
                return;
            }

            data.items.Add(item);
            
            SaveData();
            Repaint();
        }

        private void RemoveItem(PinBoardItem item)
        {
            data.items.Remove(item);
            
            SaveData();
            Repaint();
        }

        private void HandleDragAndDrop()
        {
            var evt = Event.current;
            
            Rect dropArea = new Rect(0, 0, position.width, position.height);
            
            if (dropArea.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated)
                {
                    isDragging = true;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform)
                {
                    isDragging = false;
                    DragAndDrop.AcceptDrag();
                    
                    foreach (var obj in DragAndDrop.objectReferences)
                        AddItem(obj);
                    
                    evt.Use();
                }
                else if (evt.type == EventType.DragExited)
                {
                    isDragging = false;
                    Repaint();
                }
            }
            else
            {
                if (evt.type == EventType.DragExited)
                    isDragging = false;
            }
        }
    }
}

#endif