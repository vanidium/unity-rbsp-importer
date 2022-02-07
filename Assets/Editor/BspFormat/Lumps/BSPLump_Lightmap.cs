using System.IO;
using System.Collections.Generic;

namespace BSPFormat {
    public class BSPLump_Lightmap : IBSPLump {
        public byte[,,] map;

        public override IBSPLump Read(BinaryReader stream) {
            map = new byte[128, 128, 3];

            for (int x = 0; x < 128; x++) {
                for (int y = 0; y < 128; y++) {
                    for (int c = 0; c < 3; c++) {
                        map[x,y,c] = stream.ReadByte();
                    }
                }
            }

            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }
    }
}