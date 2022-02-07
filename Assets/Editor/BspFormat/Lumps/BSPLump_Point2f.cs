using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Point2f : IBSPLump {
        public float u, v;
       
        public override IBSPLump Read(BinaryReader stream) {
            u = stream.ReadSingle();
            v = stream.ReadSingle();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}