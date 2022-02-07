using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Point3f : IBSPLump {
        public float x, y, z;
       
        public override IBSPLump Read(BinaryReader stream) {
            x = stream.ReadSingle();
            y = stream.ReadSingle();
            z = stream.ReadSingle();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}