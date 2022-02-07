using System.IO;

namespace BSPFormat {
    public abstract class IBSPLump {
        public abstract IBSPLump Read(BinaryReader stream);
        public abstract IBSPLump Read(BinaryReader stream, int size);

        public static string ReadString(BinaryReader stream, int size) {
            string result = System.Text.Encoding.ASCII.GetString(stream.ReadBytes(size));

            int indexOfTerminator = result.IndexOf('\0');
            if (indexOfTerminator >= 0) {
                result = result.Remove(indexOfTerminator);    
            }

            return result;
        }
    };
}