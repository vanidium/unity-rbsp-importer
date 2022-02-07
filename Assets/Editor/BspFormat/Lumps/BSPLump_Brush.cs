using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Brush : IBSPLump {
        public int brushSide;
        public int nBrushSides;
        public int texture;
       
        public override IBSPLump Read(BinaryReader stream) {
            brushSide = stream.ReadInt32();
            nBrushSides = stream.ReadInt32();
            texture = stream.ReadInt32();
            
            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}