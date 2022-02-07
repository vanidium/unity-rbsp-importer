using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Visdata : IBSPLump {
        public int nVecs;
        public int szVecs;
        public byte[] vecs;

        public override IBSPLump Read(BinaryReader stream) {
            throw new System.NotImplementedException();
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            nVecs = stream.ReadInt32();
            szVecs = stream.ReadInt32();
            vecs = stream.ReadBytes(nVecs * szVecs);
            
            return this;
        }
    }
}