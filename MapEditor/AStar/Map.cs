using System;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    public partial class Map : Map<bool>
    {
        public static Map HarbMap { get; set; }

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="map"></param>
        /// <param name="StartPos"></param>
        /// <param name="EndPos"></param>
        /// <returns></returns>
        public List<Pos> FindPath(Pos StartPos, Pos EndPos)
        {
            return AStar.FindPath(this, StartPos, EndPos);
        }

        /// <summary>
        /// 是否为障碍物
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public override bool IsObstruct(Pos pos)
        {
            return data[pos.y * width + pos.x].Equals(obstruct);
        }
    }

    [Serializable]
    public abstract class Map<T>
    {
        public int tilewidth;
        public int tileheight;
        public int width;
        public int height;
        public T obstruct;
        public byte[] sdata;
        [NonSerialized]
        public T[] data;

        public void Init(T[] data, int width, int height, T obstruct)
        {
            this.data = data;
            this.width = width;
            this.height = height;
            this.obstruct = obstruct;
        }

        /// <summary>
        /// 是否为障碍物
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract bool IsObstruct(Pos pos);       
        
        //public static int offsetx;
        //public static int offsety;
    }
}
