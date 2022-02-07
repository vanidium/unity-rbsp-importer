using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Effect : IBSPLump {
        public string name;
        public int brush;
        public int unknown;
       
        public override IBSPLump Read(BinaryReader stream) {
            name = ReadString(stream, 64);
            brush = stream.ReadInt32();
            unknown = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}