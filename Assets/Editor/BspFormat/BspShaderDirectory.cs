using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BSPFormat {
    public class BSPShaderDirectory {
        // https://icculus.org/gtkradiant/documentation/Q3AShader_Manual/ch01/pg1_1.htm
        private class BSPShader {
            /// <summary>
            /// Refers to a hack that ignores the shader passes that would include 'map $lightmap'
            /// since it's not yet doable with the material setup we have for the BSP faces
            /// </summary>
            private bool EnableIgnoreLightmapHack = true;
            
            /// <summary>
            /// List of shader passes or subshaders
            /// </summary>
            /// <typeparam name="BSPShader"></typeparam>
            /// <returns></returns>
            public List<BSPShader> subShaders = new List<BSPShader>();

            /// <summary>
            /// A list of commands and their untouched params. Eg.:
            /// '{"blendMode": "GL_ONE GL_ONE"}'
            /// </summary>
            /// <typeparam name="string"></typeparam>
            /// <typeparam name="string"></typeparam>
            /// <returns></returns>
            public Dictionary<string, string> commands = new Dictionary<string, string>();
            
            /// <summary>
            /// Name of the shader; subshaders will include _ss_{index} in their names
            /// </summary>
            public string name;

            /// <summary>
            /// Creates, names, and pushes a new subshader (or shader pass) to the list of child passes
            /// </summary>
            public void AddNewSubShader() {
                subShaders.Add(new BSPShader { name = $"{this.name}_ss_{subShaders.Count}" });
            }

            /// <summary>
            /// Adds and processes a command for a given subshader (denoted by 'braceCount')
            /// </summary>
            /// <param name="braceCount"></param>
            /// <param name="commandLine"></param>
            public void AddCommand(int braceCount, string commandLine) {
                Dictionary<string, string> commandDicRef = null;
                if (braceCount == 1) commandDicRef = commands;
                if (braceCount > 1) commandDicRef = subShaders[braceCount - 2].commands;

                string[] splitCmd = commandLine.Split(" ", 2);
                string key = splitCmd.Length > 0 ? splitCmd[0] : "";
                string value = splitCmd.Length > 1 ? splitCmd[1] : "";

                commandDicRef.TryAdd(key, value);
            }

            /// <summary>
            /// Called to validate a shader; so we can include / exclude hacks here
            /// </summary>
            /// <returns></returns>
            public bool Validate() {
                if (!EnableIgnoreLightmapHack) return true;

                if (!commands.ContainsKey("map") ||
                    (commands.ContainsKey("map") && commands["map"].StartsWith("$")))
                        return false;

                return true;
            }
        }

        private BSPImporterWindow.BSPImporterOptions importerOptions;
        
        /// <summary>
        /// Verbose logging
        /// </summary>
        private bool VerboseLogging = false;
        
        /// <summary>
        /// If we want to enable 'map' commands in the shaders 
        /// </summary>
        private bool EnableMap = true;
        
        /// <summary>
        /// If we want to enable 'blendMode' commands in the shaders
        /// </summary>
        private bool EnableBlend = false;
        
        /// <summary>
        /// If we should ignore shaders if we are already able to load the texture
        /// (basically only use shaders if the supplied shaderName is not a texture we know of for a face) 
        /// </summary>
        private bool IgnoreShaderIfTextureIsAvailable = true;
        
        /// <summary>
        /// If we should ignore the extension of a texture written in the shader; I had multiple instances
        /// of Jedi Academy shaders telling it's looking for a .tga texture, but in reality, it was a jpg
        /// </summary>
        private bool IgnoreTextureExtensionIfFromShader = true;
        
        /// <summary>
        /// How many passes can use; setting it to IntMax will make it use all the shader passes, thus (for now)
        /// creating a material for each shader pass in the shader
        /// </summary>
        private int MaxShadersToUse = 1;
        
        /// <summary>
        /// In-memory kvs for Materials, so we can reuse them for faces that should look the same
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Material[]"></typeparam>
        /// <returns></returns>
        private Dictionary<string, Material[]> materialStore = new Dictionary<string, Material[]>();
        
        /// <summary>
        /// In-memory kvs for BSPShaders, so we can reuse them for faces that should look the same
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BSPShader"></typeparam>
        /// <returns></returns>
        private Dictionary<string, BSPShader> shaderStore = new Dictionary<string, BSPShader>();

        public BSPShaderDirectory(BSPImporterWindow.BSPImporterOptions importerOptions) {
            this.importerOptions = importerOptions;
        }

        private void Log(string tag, string message) {
            importerOptions.BSPLogger($"[S][{tag}] {message}");
        }

        /// <summary>
        /// Load and parse all .shader files in the Assets/shaders folder
        /// </summary>
        /// <returns></returns>
        public Task Load() {
            var task = new TaskCompletionSource<object>();

            // grab the directory and check if it exists
            DirectoryInfo shadersDirectory = new DirectoryInfo(importerOptions.BSPAssetsLocation + "shaders/");
            if (!shadersDirectory.Exists) return task.Task;

            // grab all .shader files under the shaders directory, call AddShader on all of them
            FileInfo[] files = shadersDirectory.GetFiles($"*.shader", SearchOption.TopDirectoryOnly);
            foreach(FileInfo fInfo in files) {
                AddShader(fInfo);
            }

            // debug
            foreach(KeyValuePair<string, BSPShader> shader in shaderStore) {
                Log("ShaderEntry", shader.Key);
                LogShader(shader.Value);
            }

            return task.Task;
        }

        /// <summary>
        /// To maybe verbose log a shader
        /// </summary>
        /// <param name="shader"></param>
        private void LogShader(BSPShader shader) {
            if (!VerboseLogging) return;

            foreach(KeyValuePair<string, string> shaderCommandsKvp in shader.commands) {
                Log("", $"\t{shaderCommandsKvp.Key} = {shaderCommandsKvp.Value}");
            }

            foreach (BSPShader subShader in shader.subShaders) {
                Log("", "\t-subshader-");
                LogShader(subShader);
            }
        }

        /// <summary>
        /// Parses and adds a BSPShader to the store
        /// TODO: this is based on how I thought the file format was working after
        /// spending 2 minutes of reading .shaders and 5 minutes of reading the documentation,
        /// so I'm assuming I can do a better job here
        /// </summary>
        /// <param name="fileInfo"></param>
        public void AddShader(FileInfo fileInfo) {
            Log("Shader", $"Reading shader: {fileInfo.Name}");

            // for each file
            using (StreamReader stream = new StreamReader(fileInfo.OpenRead())) {
                int braceCount = 0;
                int subSectionCount = 0;
                string lastSection = null;
                while (stream.Peek() >= 0) {
                    // clear indentations, transform tabs to spaces, so we can
                    // actually figure out the key-values consistently
                    string line = stream.ReadLine().Trim().TrimStart('\t').TrimEnd('\t').Replace('\t', ' ');

                    // if its null or whitespace or comment, just skip
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;
                    
                    // begin section
                    if (line.StartsWith("{")) {
                        ++braceCount;
                        if(++subSectionCount > 1 && lastSection != null){
                            BSPShader shader;
                            if (shaderStore.TryGetValue(lastSection, out shader)) {
                                shader.AddNewSubShader();
                            }
                        }
                        
                        continue;
                    }

                     // end section
                    if (line.StartsWith("}")) { braceCount--; continue; }

                    // read the contents if we can
                    if (braceCount == 0) {
                        subSectionCount = 1;
                        if (!shaderStore.ContainsKey(line)) {
                            lastSection = line;
                            shaderStore.Add(lastSection, new BSPShader{ name = lastSection });
                        }
                    }

                    if (braceCount > 0 && lastSection != null) {
                        BSPShader shader;
                        if (shaderStore.TryGetValue(lastSection, out shader)) {
                            shader.AddCommand(subSectionCount, line);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Based on the name, find the full path for a texture, including extension
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        private string FindTextureWithExtension(string mapName) {
            FileInfo assetFile = new FileInfo(mapName);
            DirectoryInfo assetsDirectory = new DirectoryInfo(importerOptions.BSPAssetsLocation);

            string fileName = mapName;
            if (IgnoreTextureExtensionIfFromShader) fileName = mapName.Split('.')[0];

            string fullPath = $"{assetsDirectory.FullName}{fileName}";

            // find the extension if needed
            if (IgnoreTextureExtensionIfFromShader || string.IsNullOrEmpty(assetFile.Extension)) {
                FileInfo[] files = null;
                try{ files = assetsDirectory.GetFiles($"{fileName}.*", SearchOption.TopDirectoryOnly); }
                catch{ Log("Shader", $"Incorrect path for {fileName}"); }

                if (files != null) {
                    foreach(FileInfo fInfo in files) {
                        if (importerOptions.BSPValidTextureFileTypes.Contains(fInfo.Extension)) {
                            fullPath += fInfo.Extension;
                            break;
                        }
                    }
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Find and load a texture
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        private Texture2D GetTexture(string mapName) {
            string fullPath = FindTextureWithExtension(mapName);
            
            Texture2D mainTexture = null;
            if (File.Exists(fullPath)) {
                mainTexture = new Texture2D(0, 0);
                mainTexture.LoadImage(File.ReadAllBytes(fullPath));
            }

            return mainTexture;
        }
        
        /// <summary>
        /// Create a new Material whilst applying the supplied shader
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        private Material GetMaterialForShader(BSPShader shader) {
            Material shaderMaterial = new Material(Shader.Find(importerOptions.BaseShaderName));
            shaderMaterial.name = shader.name;

            // Handle textures
            if (EnableMap && shader.commands.ContainsKey("map")) {
                string textureName = shader.commands["map"];

                Texture2D texture = GetTexture(textureName);
                if (texture != null) shaderMaterial.mainTexture = texture;
            }

            // Handle blendFunc
            if (EnableBlend && shader.commands.ContainsKey("blendFunc")) {
                string bModeStr = shader.commands["blendFunc"];

                if (bModeStr == "add") bModeStr = "GL_ONE GL_ONE";
                if (bModeStr == "filter") bModeStr = "GL_DST_COLOR GL_ZERO";
                if (bModeStr == "blend") bModeStr = "GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA";

                string[] blendMode = bModeStr.Split(' ');
                UnityEngine.Rendering.BlendMode src = Constants.BlendModeMap.GetValueOrDefault(blendMode[0], UnityEngine.Rendering.BlendMode.One);
                UnityEngine.Rendering.BlendMode dst = Constants.BlendModeMap.GetValueOrDefault(blendMode[1], UnityEngine.Rendering.BlendMode.One);

                shaderMaterial.SetInt("_SrcBlend", (int)src);
                shaderMaterial.SetInt("_DstBlend", (int)dst);
                shaderMaterial.SetInt("_ZWrite", 0);
                shaderMaterial.DisableKeyword("_ALPHATEST_ON");
                shaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                shaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                shaderMaterial.renderQueue = 3000;
            }

            return shaderMaterial;
        }
        
        /// <summary>
        /// Create new Materials whils applying the correct shader and shader passes
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        private List<Material> GetMaterialsForShader(BSPShader shader) {
            // initialize it with the main part of the shader
            List<Material> materialsList = new List<Material>();

            // add the main material
            if (shader.Validate()) materialsList.Add(GetMaterialForShader(shader));
           
            // add the subshaders to the list
            foreach (BSPShader subShader in shader.subShaders) {
                if (subShader.Validate() && materialsList.Count < MaxShadersToUse) materialsList.Add(GetMaterialForShader(subShader));
            }

            List<Material> filteredMaterials = new List<Material>();
            foreach(Material material in materialsList) {
                if (material != null) filteredMaterials.Add(material);
            }

            return filteredMaterials;
        }

        /// <summary>
        /// Either return the cached Materials, or create new ones for a shader by it's name
        /// </summary>
        /// <param name="shaderName"></param>
        /// <returns></returns>
        public Material[] GetMaterialsForShaderName(string shaderName) {
            // we already have this cached, so just return it
            if (materialStore.ContainsKey(shaderName)) {
                return materialStore[shaderName];
            }
            
            BSPShader mainShader = null;

            if (shaderStore.ContainsKey(shaderName) && !(IgnoreShaderIfTextureIsAvailable && File.Exists(FindTextureWithExtension(shaderName)))) {
                Log("Shader", $"Using shader: {shaderName}");
                mainShader = shaderStore[shaderName];
            }

            // in case there is no associated shader to this texture, 
            // just handle it as a map -> texture
            if (mainShader == null) {
                mainShader = new BSPShader();
                mainShader.name = shaderName;
                mainShader.AddCommand(1, $"map {shaderName}");
            }

            Material[] materials = GetMaterialsForShader(mainShader).ToArray(); 

            // add it to the store
            materialStore.Add(shaderName, materials);

            return materials;
        }
    }
}