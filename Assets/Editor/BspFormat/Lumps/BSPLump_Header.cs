using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Header : IBSPLump {
        public string magic;
        public int version;
        public List<BSPLump_DirectoryEntry> lumps;

        public override IBSPLump Read(BinaryReader stream) {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            magic = ReadString(stream, 4);
            version = stream.ReadInt32();
            
            lumps = new List<BSPLump_DirectoryEntry>();
            for (int i = 0; i < 17; i++) {
                lumps.Add(BSPLumpFactory<BSPLump_DirectoryEntry>.Create(stream));
            }

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}