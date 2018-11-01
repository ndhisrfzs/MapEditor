namespace Game
{
    public class MergeArray
    {
        public static byte[] ToByteArray(bool[] data)
        {
            int byteLen = ((data.Length + 7) / 8);
            byte[] buf = new byte[byteLen];
            byte value = 0;
            int i = 0;
            for(i = 0; i < data.Length; i++)
            {
                value |= (byte)(data[i] ? 1 << (7 - (i & 7)) : 0);
                if((i & 7) == 7)
                {
                    buf[i / 8] = value;
                    value = 0;
                }
            }

            if((i & 7) != 0)
            {
                buf[i / 8] = value;
            }

            return buf;
        }

        public static bool[] ToBoolArray(byte[] data)
        {
            int len = data.Length;
            int boolLen = data.Length * 8;
            bool[] buf = new bool[boolLen];
            for(int i = 0; i < boolLen; i++)
            {
                buf[i] = ((data[i / 8] >> (7 - (i & 7))) & 1) == 1;
            }
            return buf;
        }
    }
}
