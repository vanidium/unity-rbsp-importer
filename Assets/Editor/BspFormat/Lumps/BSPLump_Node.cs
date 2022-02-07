using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Node : IBSPLump {
        public int plane;
        public int[] children;
        public BSPLump_Point3i mins;
        public BSPLump_Point3i maxs;

        public override IBSPLump Read(BinaryReader stream) {
            plane = stream.ReadInt32();
            children = new int[] { stream.ReadInt32(), stream.ReadInt32() };
            mins = BSPLumpFactory<BSPLump_Point3i>.Create(stream);
            maxs = BSPLumpFactory<BSPLump_Point3i>.Create(stream);

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}