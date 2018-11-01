using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game
{
    public class FileHelper
    {
        /// <summary>
        /// 对象序列化到byte
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeToBinary(object obj)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, obj);
                    byte[] data = stream.ToArray();
                    stream.Close();
                    return data;
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }

        /// <summary>
        /// 反序列化到对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object DeserializeFromBinary(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                object obj = bf.Deserialize(stream);

                stream.Close();

                return obj;
            }
        }
        public static T DeserializeFromBinary<T>(byte[] data)
        {
            return (T)DeserializeFromBinary(data);
        }
        /// <summary>
        /// 写数据到文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="Buf">数据</param>
        /// <param name="start">数据开始位置</param>
        /// <param name="end">数据结束位置</param>
        public static void WriteToFile(string path, byte[] Buf, int start, int end)
        {
            //声明FileStream对象
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))//初始化FileStream对象
                {
                    using (BinaryWriter bw = new BinaryWriter(fs)) //创建BinaryWriter对象
                    {
                        //写入文件
                        bw.Write(Buf, start, end);
                    }
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 从文件读数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadFromFile(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int byteLength = (int)fs.Length;
                byte[] bytes = new byte[byteLength];
                fs.Read(bytes, 0, byteLength);
                return bytes;
            }
        }
    }
}
