using System;
using System.IO;

namespace BSPFormat {
    public static class BSPLumpFactory<T> where T : class, new() {
        public static T Create(BinaryReader stream) {
            IBSPLump abstractLump = (IBSPLump)Activator.CreateInstance(typeof(T));
            return abstractLump.Read(stream) as T;
        }

        public static T Create(BinaryReader stream, int size) {
            IBSPLump abstractLump = (IBSPLump)Activator.CreateInstance(typeof(T));
            return abstractLump.Read(stream, size) as T;
        }
    }
}