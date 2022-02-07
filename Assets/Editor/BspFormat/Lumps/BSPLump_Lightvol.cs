using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Lightvol : IBSPLump {
        public byte[] ambient;
        public byte[] directional;
        public byte[] dir;

        public override IBSPLump Read(BinaryReader stream) {
            ambient = stream.ReadBytes(3);
            directional = stream.ReadBytes(3);
            dir = stream.ReadBytes(2);

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}