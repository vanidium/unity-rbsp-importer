using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BSPFormat {
public class BSPFile
{
    private BSPImporterWindow.BSPImporterOptions importerOptions;

    // actual map info
    public BSPLump_Header header;
    public BSPLump_Entity entities;
    public List<BSPLump_Texture> textures;
    public List<BSPLump_Plane> planes;
    public List<BSPLump_Node> nodes;
    public List<BSPLump_Leafs> leafs;
    public List<BSPLump_LeafFace> leafFaces;
    public List<BSPLump_LeafBrush> leafBrushes;
    public List<BSPLump_Model> models;
    public List<BSPLump_Brush> brushes;
    public List<BSPLump_BrushSide> brushSides;
    public List<BSPLump_Vertex> vertices;
    public List<BSPLump_MeshVert> meshVertices;
    public List<BSPLump_Effect> effects;
    public List<BSPLump_Face> faces;
    public List<BSPLump_Lightmap> lightmaps;
    public List<BSPLump_Lightvol> lightvols;
    public BSPLump_Visdata visdata;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="importerOptions"></param>
    public BSPFile(BSPImporterWindow.BSPImporterOptions importerOptions) {
        this.importerOptions = importerOptions;
    }

    private void Log(string tag, string message) {
        importerOptions.BSPLogger($"[L][{tag}] {message}");
    }

    /// <summary>
    /// Reads a bsp file and stores the parsed contents in memory
    /// </summary>
    /// <returns></returns>
    public Task Load() {
        var task = new TaskCompletionSource<object>();
        using (FileStream fStream = File.Open(importerOptions.BSPFileLocation, FileMode.Open)) {
            using (BinaryReader stream = new BinaryReader(fStream, System.Text.Encoding.Default)) {
                if (!LoadHeader(stream)) {
                    Log("BSPLump_Header", $"Failed to read or verify the header: id={header.magic}");
                    return task.Task;
                }
                
                LoadSingleLump<BSPLump_Entity>(stream, header.lumps[Constants.LumpEntities], out entities);
                LoadLump<BSPLump_Texture>(stream, header.lumps[Constants.LumpTextures], out textures);
                LoadLump<BSPLump_Plane>(stream, header.lumps[Constants.LumpPlanes], out planes);
                LoadLump<BSPLump_Node>(stream, header.lumps[Constants.LumpNodes], out nodes);
                LoadLump<BSPLump_Leafs>(stream, header.lumps[Constants.LumpLeafs], out leafs);
                LoadLump<BSPLump_LeafFace>(stream, header.lumps[Constants.LumpLeafFaces], out leafFaces);
                LoadLump<BSPLump_LeafBrush>(stream, header.lumps[Constants.LumpLeafBrushes], out leafBrushes);
                LoadLump<BSPLump_Model>(stream, header.lumps[Constants.LumpModels], out models);
                LoadLump<BSPLump_Brush>(stream, header.lumps[Constants.LumpBrushes], out brushes);
                LoadLump<BSPLump_BrushSide>(stream, header.lumps[Constants.LumpBrushSides], out brushSides);
                LoadLump<BSPLump_Vertex>(stream, header.lumps[Constants.LumpVertexes], out vertices);
                LoadLump<BSPLump_MeshVert>(stream, header.lumps[Constants.LumpMeshVerts], out meshVertices);
                LoadLump<BSPLump_Effect>(stream, header.lumps[Constants.LumpEffects], out effects);
                LoadLump<BSPLump_Face>(stream, header.lumps[Constants.LumpFaces], out faces);
                LoadLump<BSPLump_Lightmap>(stream, header.lumps[Constants.LumpLightmaps], out lightmaps);
                LoadLump<BSPLump_Lightvol>(stream, header.lumps[Constants.LumpLightVols], out lightvols);
                LoadSingleLump<BSPLump_Visdata>(stream, header.lumps[Constants.LumpVisData], out visdata);

                // verbose debug, todo: make this an option?
                Log("BSPLump_Entity", $"\n{entities.textBuffer}");
                foreach (BSPLump_Texture textureLump in textures) {
                    Log("BSPLump_Texture", textureLump.name);
                }

                return task.Task;
            }
        }
    }

    /// <summary>
    /// Loads and verifies the header portion of the bsp file.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>Success flag</returns>
    private bool LoadHeader(BinaryReader stream) {
        header = BSPLumpFactory<BSPLump_Header>.Create(stream);
        return Constants.BSPMagicRaven == header.magic;
    }

    /// <summary>
    /// Loads a single instance of a lump
    /// </summary>
    /// <typeparam name="T">Lump type</typeparam>
    private void LoadSingleLump<T>(BinaryReader stream, BSPLump_DirectoryEntry lumpInfo, out T res) where T : class, new(){
        stream.BaseStream.Seek(lumpInfo.offset, SeekOrigin.Begin);
        res = BSPLumpFactory<T>.Create(stream, lumpInfo.length) as T;

        Log(typeof(T).Name, $"Read (off={lumpInfo.offset} len={lumpInfo.length} count=1)");
    }

    /// <summary>
    /// Loads all instances of a lump; length must be dividable by 4, and the number of read bytes must be equal
    /// to the length of the directory entry for that lump type in the header.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private void LoadLump<T>(BinaryReader stream, BSPLump_DirectoryEntry lumpInfo, out List<T> resArr)  where T : class, new() {
        resArr = new List<T>();
        stream.BaseStream.Seek(lumpInfo.offset, SeekOrigin.Begin);

        long readSize = 0;
        while (readSize < lumpInfo.length) {
            IBSPLump lump = BSPLumpFactory<T>.Create(stream) as IBSPLump;
            resArr.Add(lump as T);

            readSize = stream.BaseStream.Position - lumpInfo.offset;
        }
        
        /* just some sanity checks */
        if (lumpInfo.length % 4 != 0) Log(typeof(T).Name, "Inconsistent length, not dividable by 4");
        if (lumpInfo.length != readSize) Log(typeof(T).Name, $"Incorrect number of bytes read ({readSize})");

        Log(typeof(T).Name, $"Read (off={lumpInfo.offset} len={lumpInfo.length} count={resArr.Count})");
    }
}
}