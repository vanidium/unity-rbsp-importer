using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_BrushSide : IBSPLump {
        public int planeNum;
        public int shaderNum;
        public int drawSurfNum;
       
        public override IBSPLump Read(BinaryReader stream) {
            planeNum = stream.ReadInt32();
            shaderNum = stream.ReadInt32();
            drawSurfNum = stream.ReadInt32();
            
            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}