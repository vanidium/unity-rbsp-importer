using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Plane : IBSPLump {
        public BSPLump_Point3f normal;
        public float distance;
       
        public override IBSPLump Read(BinaryReader stream) {
            normal = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            distance = stream.ReadSingle();

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}