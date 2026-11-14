using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace XG
{
    public class SceneOpenInterface : EditorWindow
    {
        private static SceneOpenInterface window;
        private GUIStyle sceneButtonStyle;
        private GUIStyle hintStyle;
        static GUIStyle pillStyle;
        static GUIStyle boxStyle;
        static Texture2D roundedTex;

        private const string SearchControlName = "SceneSearchField";
        private List<string> scenePaths = new List<string>();
        private BuildSceneData buildData = new BuildSceneData();
        private Vector2 scrollPosition;
        private string searchQuery = "";

        [MenuItem("Xplosiveginger/Open Scene"), Shortcut("Window/Open Scene", KeyCode.O, ShortcutModifiers.Alt)]
        public static void ShowWindow()
        {
            window = GetWindow<SceneOpenInterface>("Open Scene");
            window.minSize = new Vector2(640.0f, 320.0f);
            window.ShowModal();
        }

        private void OnEnable()
        {
            sceneButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft
            };

            sceneButtonStyle.padding.left = 8;

            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = EditorStyles.textField.padding,
                    fontStyle = FontStyle.Italic
                };
                hintStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 0.9f);
            }

            RefreshScenes();
        }

        static GUIStyle GetPillStyle()
        {
            if (roundedTex == null) roundedTex = Helper.MakeRoundedTexture(16, 6);

            if (pillStyle == null)
            {
                pillStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    fontSize = 9,                              // smaller text
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(6, 6, 6, 6),       // matches the new radius
                    padding = new RectOffset(8, 8, 2, 2),      // tighter padding
                    margin = new RectOffset(2, 2, 2, 2),
                    stretchWidth = false,                      // don't expand horizontally
                    stretchHeight = false
                };
                pillStyle.normal.background = roundedTex;
                pillStyle.normal.textColor = Color.white;
            }
            return pillStyle;
        }

        static GUIStyle GetBoxStyle()
        {
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 8, 8),   // left, right, top, bottom (inside the box)
                    margin = new RectOffset(10, 10, 2, 2)       // space outside the box
                };
            }
            return boxStyle;
        }

        private void RefreshScenes()
        {
            scenePaths.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scenePaths.Add(path);
            }

            scenePaths.Sort();

            buildData.SetData();
        }

        private void DrawScenesAsList()
        {
            foreach (string path in scenePaths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);

                if (!sceneName.ToLower().Contains(searchQuery.ToLower()))
                    continue;

                bool isInBuild = buildData.HasScene(path, out bool isEnabled);

                Rect box = EditorGUILayout.BeginVertical(GetBoxStyle());
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(sceneName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (isInBuild)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = isEnabled ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.8f, 0.25f, 0.25f);

                    GUILayout.Label("IN BUILD", GetPillStyle());

                    GUI.backgroundColor = prev;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Label(path, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                EditorGUIUtility.AddCursorRect(box, MouseCursor.Link);
                if (Event.current.type == EventType.Repaint && box.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(box, new Color(1f, 1f, 1f, 0.06f));

                if (GUI.Button(box, GUIContent.none, GUIStyle.none))
                {
                    EditorSceneManager.OpenScene(
                        path,
                        OpenSceneMode.Single
                    );

                    window.Close();
                }

                GUILayout.Space(10);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Preview");

            GUI.SetNextControlName(SearchControlName);

            searchQuery = EditorGUILayout.TextField(
                searchQuery
            );

            EditorGUI.FocusTextInControl(SearchControlName);

            if (string.IsNullOrEmpty(searchQuery))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                GUI.Label(r, "Search", hintStyle);
            }

            GUILayout.Label(
                $"Total Scenes: {scenePaths.Count}",
                EditorStyles.miniLabel
            );

            GUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawScenesAsList();
            EditorGUILayout.EndScrollView();
        }

        private class BuildSceneData
        {
            private List<SceneData> sceneDatas = new List<SceneData>();

            public void SetData()
            {
                sceneDatas.Clear();

                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    SceneData newSceneData = new SceneData();

                    newSceneData.path = scene.path;
                    newSceneData.enabled = scene.enabled;

                    sceneDatas.Add(newSceneData);
                }
            }

            public bool HasScene(string path, out bool enabled)
            {
                foreach (SceneData sceneData in sceneDatas)
                {
                    if (sceneData.path == path)
                    {
                        enabled = sceneData.enabled;
                        return true;
                    }
                }

                enabled = false;
                return false;
            }

            private class SceneData
            {
                public string path;
                public bool enabled;
            }
        }
    }
}