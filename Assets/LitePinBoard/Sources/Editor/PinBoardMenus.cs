#if UNITY_EDITOR

using UnityEditor;

namespace LitePinBoard.Sources.Editor
{
    public static class PinBoardMenus
    {
        [MenuItem("Assets/Pin to Lite Board", false, 20)]
        [MenuItem("GameObject/Pin to Lite Board", false, -50)]
        private static void PinSelected()
        {
            var window = EditorWindow.GetWindow<PinBoardWindow>("Lite Pin Board");
            window.LoadData();
            
            foreach (var obj in Selection.objects)
            {
                window.AddItem(obj);
            }
            
            window.Show();
            window.Repaint();
        }

        [MenuItem("Assets/Pin to Lite Board", true)]
        [MenuItem("GameObject/Pin to Lite Board", true)]
        private static bool PinValidation() => Selection.objects.Length > 0;
    }
}

#endif