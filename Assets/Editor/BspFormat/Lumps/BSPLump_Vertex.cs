
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace BSPFormat {
    public class BSPLump_Vertex : IBSPLump {
        public BSPLump_Point3f position;
        public float[] st;
        public float[,] lightmap;
        public BSPLump_Point3f normal;
        public byte[,] color;
       
        public override IBSPLump Read(BinaryReader stream) {
            position = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            st = new float[2] { stream.ReadSingle(), stream.ReadSingle() };
            
            lightmap = new float[Constants.MaxLightmaps, 2];
            for (int i = 0; i < Constants.MaxLightmaps; i++) {
                for (int a = 0; a < 2; a++) {
                    lightmap[i, a] = stream.ReadSingle();
                }
            }

            normal = BSPLumpFactory<BSPLump_Point3f>.Create(stream);
            
            color = new byte[Constants.MaxLightmaps, 4];
            for (int i = 0; i < Constants.MaxLightmaps; i++) {
                for (int a = 0; a < 4; a++) {
                    color[i, a] = stream.ReadByte();
                }
            }

            PostProcess();
            
            return this;
        }
        
        public override IBSPLump Read(BinaryReader stream, int size) {
            throw new System.NotImplementedException();
        }

        private void PostProcess() {
            // we need to adjust the position and normal coords to
            // account for radiant vs unity differences 
            float z = position.z;
            float y = position.y;
            position.y = z;
            position.z = -y;
            position.x = -position.x;

            z = normal.z;
            y = normal.y;

            normal.y = z;
            normal.z = -y;
            normal.x = -normal.x;
        }

        public Vector3 u_position() {
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            pos.Scale(Constants.VertexScaleFactor);
            return pos;
        }

        public Vector3 u_normal() {
            Vector3 nor = new Vector3(normal.x, normal.y, normal.z);
            nor.Scale(Constants.VertexScaleFactor);
            return nor;
        }

        public Vector2 u_uv() {
            // we need to invert v
            return new Vector2(st[0], -1 * st[1]);
        }
    }
}