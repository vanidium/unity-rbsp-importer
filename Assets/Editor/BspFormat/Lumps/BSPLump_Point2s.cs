using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Point2s : IBSPLump {
        public ushort x, y;
       
        public override IBSPLump Read(BinaryReader stream) {
            x = stream.ReadUInt16();
            y = stream.ReadUInt16();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}