using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BSPFormat {
    public class BSPGenerator {
        private BSPImporterWindow.BSPImporterOptions importerOptions;
        private int TessellatedWidth {
            get {
                return importerOptions.BSPTessellationLevel + 1;
            }
        }
        private float TessellationDelta {
            get {
                return 1f / importerOptions.BSPTessellationLevel;
            }
        }

        private BSPFile bsp;
        private BSPShaderDirectory shaderDirectory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bspFile"></param>
        /// <param name="shaderDirectory"></param>
        /// <param name="importerOptions"></param>
        public BSPGenerator(BSPFile bspFile, BSPShaderDirectory shaderDirectory, BSPImporterWindow.BSPImporterOptions importerOptions) {
            this.bsp = bspFile;
            this.shaderDirectory = shaderDirectory;
            this.importerOptions = importerOptions;
        }

        private void Log(string tag, string message) {
            importerOptions.BSPLogger($"[G][{tag}] {message}");
        }

        /// <summary>
        /// Generates GameObjects for the BSP
        /// </summary>
        /// <returns></returns>
        public Task Generate() {
            var task = new TaskCompletionSource<object>();
            Log("Generate", $"Generating from BSP{bsp.header.version}");
            
            Log("Generate", "Creating root object...");
            GameObject rootObject = new GameObject("BSP");

            Log("Generate", $"Processing {bsp.faces.Count} faces...");
            foreach (BSPLump_Face face in bsp.faces) {
                // 1=polygon, 2=patch, 3=mesh, 4=billboard
                if (face.surfaceType == 1 || face.surfaceType == 3) {
                    CreatePolygon(rootObject, face);
                } else if (face.surfaceType == 2) {
                    CreateBezierPatch(rootObject, face);
                } else {
                    Log("Generate", $"Unsupported face type: {face.surfaceType}");
                }
            }

            return task.Task;
        }

        /// <summary>
        /// Creates a GameObject for polygon / mesh types
        /// </summary>
        /// <param name="root"></param>
        /// <param name="face"></param>
        private void CreatePolygon(GameObject root, BSPLump_Face face) {
            GameObject meshedObject = new GameObject("PolyMesh");
            meshedObject.transform.parent = root.transform;
            meshedObject.isStatic = true;

            AddPolygonMesh(meshedObject, face);
            AddMaterial(meshedObject, face);
        }

        /// <summary>
        /// Calculates the neccessary info (vertices, normals, uvs, triangle indices) for and creates
        /// a MeshFilter and MeshRenderer. Normals are a bit redundant, because we will call RecalculateNormals
        /// but it will be handy once it will start using the lightmaps, maybe
        /// </summary>
        /// <param name="to">GameObject to attach the new MeshFilter and MeshRenderers to</param>
        /// <param name="face"></param>
        /// <returns></returns>
        private GameObject AddPolygonMesh(GameObject to, BSPLump_Face face) {
            Mesh polyMesh = new Mesh();
            int nVerts = face.numVertices;
            int nIndices = face.numIndices;

            // FIXME: can we just initialize all the arrays
            // with the correct sizes in the mesh directly?
            Vector3[] vertices = new Vector3[nVerts];
            Vector3[] normals = new Vector3[nVerts];
            Vector2[] uvs = new Vector2[nVerts];
            int[] tris = new int[nIndices];

            for (int i = 0; i < nVerts; i++) {
                var current = face.firstVertex + i;
                vertices[i] = bsp.vertices[current].u_position();
                normals[i] = bsp.vertices[current].u_normal();
                uvs[i] = bsp.vertices[current].u_uv();
            }

            for (int i = 0; i < nIndices; i++) {
                var current = face.firstIndex + i;
                tris[i] = bsp.meshVertices[current].offset;
            }

            polyMesh.vertices = vertices;
            polyMesh.normals = normals;
            polyMesh.uv = uvs;
            polyMesh.triangles = tris;

            polyMesh.name = "PolyMesh";
            polyMesh.RecalculateBounds();
            polyMesh.RecalculateNormals();
            polyMesh.Optimize();

            to.AddComponent<MeshFilter>().mesh = polyMesh;
            to.AddComponent<MeshRenderer>();

            return to;
        }

        /// <summary>
        /// Creates a new GameObject for bezier mesh tyes
        /// </summary>
        /// <param name="root"></param>
        /// <param name="face"></param>
        private void CreateBezierPatch(GameObject root, BSPLump_Face face) {
            GameObject meshedObject = new GameObject("BezierMesh");
            meshedObject.transform.parent = root.transform;
            meshedObject.isStatic = true;

            AddBezierMeshes(meshedObject, face);
        }

        /// <summary>
        /// Creates new GameObjects for bezier mesh patches
        /// </summary>
        /// <param name="to"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        private GameObject AddBezierMeshes(GameObject to, BSPLump_Face face) {
            int numPatches = ((face.patchWidth - 1) / 2) * ((face.patchHeight - 1) / 2);

            for (int i = 0; i < numPatches; i++) {
                GameObject patchObject = new GameObject("Patch");
                patchObject.transform.parent = to.transform;
                patchObject.isStatic = true;

                AddBezierPatch(patchObject, face, i);
                AddMaterial(patchObject, face);
            }

            return to;
        }

        /// <summary>
        /// Calculates the neccessary info for bezier meshes and creates the appropriate
        /// MeshFilters and MeshRenderers for a given patch index
        /// </summary>
        /// <param name="to"></param>
        /// <param name="face"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        private GameObject AddBezierPatch(GameObject to, BSPLump_Face face, int patch) {
            int numColumns = (face.patchWidth - 1) / 2;
            int patchX = (patch % numColumns) * 2;
            int patchY = (patch / numColumns) * 2;

            BSPLump_Vertex[,] controlVertexGrid = new BSPLump_Vertex[face.patchWidth, face.patchHeight];

            // read the control vertex grid (flat indices to grid)
            for (int i = 0; i < face.numVertices; i++) {
                int y = i / face.patchWidth;
                int x = i % face.patchWidth;
                controlVertexGrid[x, y] = bsp.vertices[face.firstVertex + i];
            }

            // flatten the 3*3 control vertex grid to a flat array of vertex positions and uvs
            List<Vector3> controlVertices = new List<Vector3>();
            List<Vector2> controlUVs = new List<Vector2>();
            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 3; x++) {
                    BSPLump_Vertex current = controlVertexGrid[patchX + x, patchY + y];
                    controlVertices.Add(current.u_position());
                    controlUVs.Add(current.u_uv());
                }
            }

            // tessellate and transform control points
            List<Vector3>[] controlVertexPoints = new List<Vector3>[3];
            List<Vector2>[] controlUVPoints = new List<Vector2>[3];
            for (int i = 0; i < 3; i++) {
                int a = i + 0;
                int b = i + 3;
                int c = i + 6;
                
                controlVertexPoints[i] = Tessellate<Vector3>(controlVertices[a], controlVertices[b], controlVertices[c]);
                controlUVPoints[i] = Tessellate<Vector2>(controlUVs[a], controlUVs[b], controlUVs[c]);
            }
            
            // run the final tessellation pass to grab all the final vertices and uvs
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i <= importerOptions.BSPTessellationLevel; i++) {
                vertices.AddRange(Tessellate<Vector3>(controlVertexPoints[0][i], controlVertexPoints[1][i], controlVertexPoints[2][i]));
                uvs.AddRange(Tessellate<Vector2>(controlUVPoints[0][i], controlUVPoints[1][i], controlUVPoints[2][i]));
            }

            int patchWidth = importerOptions.BSPTessellationLevel + 1;
            int numOfVertices = patchWidth * patchWidth;
            List<int> tris = new List<int>();
            
            // kind of a stupid way to figure out the triangle indices on the grid; I couldn't think of a better way,
            // but let's mark it as FIXME
            int xs = 1;
            for (int i = 0; i < numOfVertices - patchWidth; i++) {
                IEnumerable<int> calculatedIndices;
                
                if (xs == 1) {
                    calculatedIndices = new List<int> { i, i + patchWidth, i + 1 };
                    xs++;
                } else if (xs == TessellatedWidth) {
                    calculatedIndices = new List<int> { i, i + (patchWidth - 1), i + patchWidth };
                    xs = 1;
                } else {
                    calculatedIndices = new List<int> { i, i + (patchWidth - 1), i + patchWidth, i, i + patchWidth, i + 1 };
                    xs++;
                }

                tris.AddRange(calculatedIndices);
            }

            // create the mesh, push the data
            Mesh bezierMesh = new Mesh();
            bezierMesh.name = "BezierMesh";
            bezierMesh.vertices = vertices.ToArray();
            bezierMesh.uv = uvs.ToArray();
            bezierMesh.triangles = tris.ToArray();

            bezierMesh.RecalculateBounds();
            bezierMesh.RecalculateNormals();
            bezierMesh.Optimize();

            to.AddComponent<MeshFilter>().mesh = bezierMesh;
            to.AddComponent<MeshRenderer>();
            
            return to;
        }

        /// <summary>
        /// Create the final points on the curve by subdividing the vector and figuring out the the actual 
        /// location of those new points on the curve. 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<T> Tessellate<T>(T p0, T p1, T p2) {
            List<T> tessellated = new List<T>();
            
            float d = TessellationDelta;

            tessellated.Add(p0);
            for (int i = 0; i < (importerOptions.BSPTessellationLevel - 1); i++) {
                tessellated.Add(BezierCurve(d, (dynamic)p0, (dynamic)p1, (dynamic)p2));
                d += TessellationDelta;
            }
            tessellated.Add(p2);

            return tessellated;
        }

        /// <summary>
        /// Calculate the location of a point on the bezier curve for t
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Vector2 BezierCurve(float t, Vector2 p0, Vector2 p1, Vector2 p2) {
            Vector2 bezierPoint = new Vector2();

            float a = 1f - t;
            float tt = t * t;

            for (int i = 0; i < 2; i++) {
                bezierPoint[i] = ((a * a) * p0[i]) + (2 * a) * (t * p1[i]) + (tt * p2[i]);
            }

            return bezierPoint;
        }

        /// <summary>
        /// Calculate the point on the bezier curve for t
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
            Vector3 bezierPoint = new Vector3();

            float a = 1f - t;
            float tt = t * t;

            for (int i = 0; i < 3; i++) {
                bezierPoint[i] = ((a * a) * p0[i]) + (2 * a) * (t * p1[i]) + (tt * p2[i]);
            }

            return bezierPoint;
        }

        /// <summary>
        /// Grab the appropriate material from BSPShaderDirectory and apply it to the material 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        private GameObject AddMaterial(GameObject to, BSPLump_Face face)  {
            BSPLump_Texture textureLump = bsp.textures[face.shaderNum];

            Material[] materials = shaderDirectory.GetMaterialsForShaderName(textureLump.name);
            to.GetComponent<Renderer>().materials = materials;

            return to;
        }
    }
}