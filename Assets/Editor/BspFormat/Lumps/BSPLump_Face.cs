using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    
    public class BSPLump_Face : IBSPLump {
        public int shaderNum;
        public int fogNum;
        public int surfaceType; // polygon, patch, mesh, billboard
        public int firstVertex;
        public int numVertices;
        public int firstIndex;
        public int numIndices;
        byte[] lightmapStyles, vertexStyles;
        int[] lightmapNum;
        public int[] lightmapX, lightmapY;
        public int lightmapWidth, lightmapHeight;
        public BSPLump_Point3f lightmapOrigin;
        public BSPLump_Point3f[] lightmapVecs;
        public int patchWidth;
        public int patchHeight;
       
        public override IBSPLump Read(BinaryReader stream) {
            shaderNum = stream.ReadInt32();
            fogNum = stream.ReadInt32();
            surfaceType = stream.ReadInt32();
            firstVertex = stream.ReadInt32();
            numVertices = stream.ReadInt32();
            firstIndex = stream.ReadInt32();
            numIndices = stream.ReadInt32();
            
            lightmapStyles = stream.ReadBytes(Constants.MaxLightmaps);
            vertexStyles = stream.ReadBytes(Constants.MaxLightmaps);
            
            lightmapNum = new int[Constants.MaxLightmaps];
            for (int i = 0; i < Constants.MaxLightmaps; i++) {
                lightmapNum[i] = stream.ReadInt32();
            }

            lightmapX = new int[Constants.MaxLightmaps];
            for (int i = 0; i < Constants.MaxLightmaps; i++) {
                lightmapX[i] = stream.ReadInt32();
            }

            lightmapY = new int[Constants.MaxLightmaps];
            for (int i = 0; i < Constants.MaxLightmaps; i++) {
                lightmapY[i] = stream.ReadInt32();
            }
            lightmapWidth = stream.ReadInt32();
            lightmapHeight = stream.ReadInt32();
            
            lightmapOrigin = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            lightmapVecs = new BSPLump_Point3f[] {
                BSPLumpFactory<BSPLump_Point3f>.Create(stream),
                BSPLumpFactory<BSPLump_Point3f>.Create(stream),
                BSPLumpFactory<BSPLump_Point3f>.Create(stream)
            };
            
            patchWidth = stream.ReadInt32();
            patchHeight = stream.ReadInt32();
            
            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}