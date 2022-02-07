using System.IO;

namespace BSPFormat {
    public class BSPLump_DirectoryEntry : IBSPLump {
        public int offset;
        public int length;

        public override IBSPLump Read(BinaryReader stream) {
            offset = stream.ReadInt32();
            length = stream.ReadInt32();

            return this;
        }
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}