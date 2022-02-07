using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Point3i : IBSPLump {
        public float x, y, z;
       
        public override IBSPLump Read(BinaryReader stream) {
            x = stream.ReadInt32();
            y = stream.ReadInt32();
            z = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}