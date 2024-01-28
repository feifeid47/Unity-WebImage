using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UI.Editor
{
    [CustomEditor(typeof(WebImage), true)]
    [CanEditMultipleObjects]
    [InitializeOnLoad]
    public class WebImageEditor : ImageEditor
    {
        [MenuItem("GameObject/UI/WebImage", false, 2000)]
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
            var obj = createUIElementRoot.Invoke(null, new object[] { "WebImage", new Vector2(100f, 100f), new Type[] { typeof(WebImage) } });
            m_miPlaceUIElementRoot.Invoke(null, new object[] { obj, menuCommand });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var image = target as WebImage;

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSprite"));

            var rect = image.rectTransform;

            var type = typeof(ImageEditor);
            var m_bIsDriven = type.GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            m_bIsDriven.SetValue(this, (rect.drivenByObject as Slider)?.fillRect == rect);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();

            var m_Sprite = serializedObject.FindProperty("m_Sprite");
            AnimBool m_ShowType = type.GetField("m_ShowType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) as AnimBool;
            m_ShowType.target = m_Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowType.faded))
                TypeGUI();

            EditorGUILayout.EndFadeGroup();

            Image.Type imageType = (Image.Type)serializedObject.FindProperty("m_Type").enumValueIndex;
            bool showNativeSize = (imageType == Image.Type.Simple || imageType == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            SetShowNativeSize(showNativeSize, false);

            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;
                var m_Type = serializedObject.FindProperty("m_Type");
                var m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
                var m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}