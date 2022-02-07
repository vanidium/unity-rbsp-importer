using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_LeafFace : IBSPLump {
        public int face;
       
        public override IBSPLump Read(BinaryReader stream) {
            face = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}