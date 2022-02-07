using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Entity : IBSPLump {
        public string textBuffer;
        public override IBSPLump Read(BinaryReader stream) {
            throw new System.NotImplementedException();
        }

        public override IBSPLump Read(BinaryReader stream, int size) {
            textBuffer = ReadString(stream, size);

            return this;
        }
    }
}