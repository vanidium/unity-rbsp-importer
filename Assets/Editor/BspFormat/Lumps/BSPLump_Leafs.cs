using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
        public class BSPLump_Leafs : IBSPLump {
        public int cluster;
        public int area;
        public BSPLump_Point3i mins;
        public BSPLump_Point3i maxs;
        public int leafFace;
        public int nLeafFaces;
        public int leafBrush;
        public int nLeafBrushes;

        public override IBSPLump Read(BinaryReader stream) {
            cluster = stream.ReadInt32();
            area = stream.ReadInt32();
            mins = BSPLumpFactory<BSPLump_Point3i>.Create(stream);
            maxs = BSPLumpFactory<BSPLump_Point3i>.Create(stream);
            leafFace = stream.ReadInt32();
            nLeafFaces = stream.ReadInt32();
            leafBrush = stream.ReadInt32();
            nLeafBrushes = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}