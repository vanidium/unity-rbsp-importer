using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Texture : IBSPLump {
        public string name;
        public int flags;
        public int contents;

        public override IBSPLump Read(BinaryReader stream) {
            name = ReadString(stream, 64);
            flags = stream.ReadInt32();
            contents = stream.ReadInt32();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}