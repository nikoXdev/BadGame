#if UNITY_EDITOR

using UnityEngine;
using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LitePinBoard.Sources.Editor.Data
{
    [Serializable]
    public sealed class PinBoardItem
    {
        [SerializeField] private string guid;
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private int sceneHandle;
        [SerializeField] private string typeName;

        // Публичные свойства
        public string Guid => guid;
        public string Name => name;
        public string Path => path;
        public int SceneHandle => sceneHandle;
        public Type Type => Type.GetType(typeName);
        
        // Не сериализуемые поля
        [NonSerialized] private Texture icon;
        public Texture Icon 
        {
            get 
            {
                if (icon == null) UpdateIcon();
                return icon;
            }
            set => icon = value;
        }

        public PinBoardItem(Object obj)
        {
            name = obj.name;
            typeName = obj.GetType().AssemblyQualifiedName;
            
            if (obj is GameObject go && go.scene.IsValid())
            {
                sceneHandle = obj.GetInstanceID();
            }
            else
            {
                path = AssetDatabase.GetAssetPath(obj);
                guid = AssetDatabase.AssetPathToGUID(path);
            }
            
            UpdateIcon();
        }

        public void UpdateIcon()
        {
            if (IsSceneObject())
            {
                icon = EditorGUIUtility.IconContent("GameObject Icon").image;
            }
            else if (!string.IsNullOrEmpty(path))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                icon = AssetPreview.GetMiniThumbnail(asset) ?? EditorGUIUtility.IconContent("DefaultAsset Icon").image;
            }
        }

        public bool IsSceneObject() => sceneHandle != 0;
        public bool IsValid() => IsSceneObject() || !string.IsNullOrEmpty(guid);
    }
}

#endif