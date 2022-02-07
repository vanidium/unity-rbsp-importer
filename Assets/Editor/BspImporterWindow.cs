using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace BSPFormat {
    public class BSPImporterWindow : EditorWindow
    {
        // TODO: maybe write/read from PlayerPrefs?
        public class BSPImporterOptions {
            /// <summary>
            /// Should point to a .bsp file
            /// </summary>
            /// <value></value>
            public string BSPFileLocation {
                get; set;
            } = "";

            /// <summary>
            /// Should point to the root of the assets directory
            /// (the one that includes /textures, /shaders, /gfx and more)
            /// </summary>
            /// <value></value>
            public string BSPAssetsLocation {
                get; set;
            } = "";

            /// <summary>
            /// A list of file extensions that we consider as valid textures.
            /// During texture loading, when we are attempting to find textures by their names,
            /// the order of these are the 'priority' for the textures. Meaning if you have
            /// textures/something/a.png and textures/something/a.tga, it will pick textures/something/a.png
            /// </summary>
            /// <value></value>
            public List<string> BSPValidTextureFileTypes {
                get; set;
            } = new List<string> { ".jpg", ".png", ".tga" };

            /// <summary>
            /// Number of subdivisions on a bezier curve for bezier face types
            /// </summary>
            /// <value></value>
            public int BSPTessellationLevel {
                get; set;
            } = 5;

            /// <summary>
            /// Logging function that will be used to log things
            /// </summary>
            /// <value></value>
            public Action<string> BSPLogger {
                get; set;
            } = Debug.Log;

            /// <summary>
            /// Will be used when creating new Materials for faces
            /// </summary>
            /// <value></value>
            public string BaseShaderName {
                get; set;
            } = "Standard";
        };
        private List<string> debugLogs = new List<string>();
        private Vector2 debugLogScrollPosition;
        private Texture2D texture;

        private BSPFile bspFile;
        private BSPShaderDirectory shaderDirectory;

        private BSPImporterOptions importerOptions;
        
        [MenuItem("GameObject/Import BSP")]
        static void Run() {
            BSPImporterWindow window = (BSPImporterWindow)EditorWindow.GetWindow(typeof(BSPImporterWindow));
            window.Show();
        }

        private BSPImporterWindow() {
            importerOptions = new BSPImporterOptions();
            importerOptions.BSPLogger = (string l) => debugLogs.Add(l);
        }

        void OnGUI() {
            // main layout
            EditorGUILayout.BeginVertical();
                
                // bsp path
                EditorGUILayout.BeginHorizontal();
                    importerOptions.BSPFileLocation = EditorGUILayout.TextField("BSP", importerOptions.BSPFileLocation);
                    if (GUILayout.Button("...")) {
                        importerOptions.BSPFileLocation = EditorUtility.OpenFilePanel("Select .bsp", "", "bsp");
                    }
                EditorGUILayout.EndHorizontal();
                
                // assests folder path
                EditorGUILayout.BeginHorizontal();
                    importerOptions.BSPAssetsLocation = EditorGUILayout.TextField("Assets", importerOptions.BSPAssetsLocation);
                    if (GUILayout.Button("...")) {
                        importerOptions.BSPAssetsLocation = EditorUtility.OpenFolderPanel("Select assets folder", "", "") + "/";
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                
                // buttons layout
                EditorGUILayout.BeginHorizontal();
                    // load shaders
                    if (GUILayout.Button("Load Shaders")) {
                        ImportShaders();
                    }

                    // load bsp
                    GUI.enabled = shaderDirectory != null;
                    if (GUILayout.Button("Load BSP")) {
                        ImportMap();
                    }
                    GUI.enabled = true;

                    // generate objects
                    GUI.enabled = bspFile != null;
                    if (GUILayout.Button("Generate Objects")) {
                        GenerateMap();
                    }
                    GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // load everything button layout
                EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Load All")) {
                        ImportShaders();
                        ImportMap();
                        GenerateMap();
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // debug area, used to just display the logs
                GUILayout.Label("Debug");
                EditorGUILayout.BeginHorizontal();
                    debugLogScrollPosition = EditorGUILayout.BeginScrollView(debugLogScrollPosition, GUILayout.ExpandHeight(true));
                        GUILayout.TextArea(string.Join("\n", debugLogs), EditorStyles.textField, GUILayout.ExpandHeight(true));
                    EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Initializes the BSPShaderDirectory with data read from .shader files
        /// </summary>
        /// <returns></returns>
        private async void ImportShaders() {
            if (importerOptions.BSPAssetsLocation == null) {
                return;
            }

            shaderDirectory = new BSPShaderDirectory(importerOptions);
            await shaderDirectory.Load();
        }

        /// <summary>
        /// Initializes a BSPFile object by reading the contents of a .bsp file
        /// </summary>
        /// <returns></returns>
        private async void ImportMap() {
            if (importerOptions.BSPFileLocation == null) {
                return;
            }

            bspFile = new BSPFile(importerOptions);
            await bspFile.Load();
        }

        /// <summary>
        /// Generates GameObjects from the previously loaded .bsp file with material and shader settings
        /// from the previously initialized shader directory
        /// </summary>
        /// <returns></returns>
        private async void GenerateMap() {
            await new BSPGenerator(bspFile, shaderDirectory, importerOptions).Generate();
        }
    }
}