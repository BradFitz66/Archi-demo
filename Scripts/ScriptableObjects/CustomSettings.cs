using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Archi.Core.Settings
{
    // Create a new type of Settings Asset.
    class MyCustomSettings : ScriptableObject
    {
        public const string k_MyCustomSettingsPath = "Assets/MyCustomSettings.asset";

        [SerializeField]
        private Color handleColor = new Color(1, .3f, 0);
        [SerializeField]
        bool showDebug;

        internal static MyCustomSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MyCustomSettings>(k_MyCustomSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MyCustomSettings>();
                settings.handleColor = new Color(1, .3f, 0);
                settings.showDebug = false;
                
                AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using IMGUI for the drawing framework:
    static class MyCustomSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/Archi", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Archi settings",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = MyCustomSettings.GetSerializedSettings();
                    GUILayout.Label(new GUIContent("Handle Color"));
                    settings.FindProperty("handleColor").colorValue = EditorGUILayout.ColorField(settings.FindProperty("handleColor").colorValue);
                    settings.FindProperty("showDebug").boolValue = EditorGUILayout.Toggle(new GUIContent("Show debug information"),settings.FindProperty("showDebug").boolValue);
                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Color", "Handle" })
            };

            return provider;
        }
    }
}