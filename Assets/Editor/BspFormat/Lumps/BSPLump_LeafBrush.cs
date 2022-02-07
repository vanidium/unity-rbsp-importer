using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_LeafBrush : IBSPLump {
        public int brush;
       
        public override IBSPLump Read(BinaryReader stream) {
            brush = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}