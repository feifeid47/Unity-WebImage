using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UI.Editor
{
    [CustomEditor(typeof(WebRawImage), true)]
    [CanEditMultipleObjects]
    [InitializeOnLoad]
    public class WebRawImageEditor : RawImageEditor
    {
        [MenuItem("GameObject/UI/WebRawImage", false, 2000)]
        static public void Create(MenuCommand menuCommand)
        {
            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly uiEditorAssembly = null;
            foreach (Assembly assembly in allAssemblies)
            {
                AssemblyName assemblyName = assembly.GetName();
                if ("UnityEditor.UI" == assemblyName.Name)
                {
                    uiEditorAssembly = assembly;
                    break;
                }
            }
            if (null == uiEditorAssembly)
            {
                return;
            }

            Type menuOptionType = uiEditorAssembly.GetType("UnityEditor.UI.MenuOptions");
            var m_miPlaceUIElementRoot = menuOptionType.GetMethod("PlaceUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);
            var createUIElementRoot = typeof(DefaultControls).GetMethod("CreateUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
            var obj = createUIElementRoot.Invoke(null, new object[] { "WebRawImage", new Vector2(100f, 100f), new Type[] { typeof(WebRawImage) } });
            m_miPlaceUIElementRoot.Invoke(null, new object[] { obj, menuCommand });
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();


            var image = target as WebRawImage;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope())
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("url"));
                }
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Refresh").image, "刷新"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    image.Refresh(true);
                    EditorUtility.SetDirty(image.gameObject);
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTexture"));

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();

            var m_UVRect = serializedObject.FindProperty("m_UVRect");
            var m_UVRectContent = EditorGUIUtility.TrTextContent("UV Rect");
            var m_Texture = serializedObject.FindProperty("m_Texture");
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(m_Texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}