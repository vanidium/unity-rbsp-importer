using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_MeshVert : IBSPLump {
        public int offset;
       
        public override IBSPLump Read(BinaryReader stream) {
            offset = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}