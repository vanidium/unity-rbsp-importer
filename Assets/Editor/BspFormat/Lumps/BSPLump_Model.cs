using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Model : IBSPLump {
        public BSPLump_Point3f mins;
        public BSPLump_Point3f maxs;
        public int face;
        public int nFaces;
        public int brush;
        public int nBrushes;
       
        public override IBSPLump Read(BinaryReader stream) {
            mins = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            maxs = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            face = stream.ReadInt32();
            nFaces = stream.ReadInt32();
            brush = stream.ReadInt32();
            nBrushes = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}