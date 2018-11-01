using System;

namespace Game
{
    /// <summary>
    /// 位置
    /// </summary>
    public class Pos
    {
        public int x;
        public int y;
        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Pos()
        {

        }

        public Pos Up { get { return new Pos(this.x, this.y + 1); } }
        public Pos Down { get { return new Pos(this.x, this.y - 1); } }
        public Pos Left { get { return new Pos(this.x - 1, this.y); } }
        public Pos Right { get { return new Pos(this.x + 1, this.y); } }

        /// <summary>
        /// 是否越界
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool IsOverflow(int width, int height)
        {
            return x < 0 || x >= width || y < 0 || y >= height;
        }

        public static int operator -(Pos lp, Pos rp)
        {
            return Math.Abs(rp.x - lp.x) + Math.Abs(rp.y - lp.y);
        }

        /// <summary>
        /// 是否在一条线上
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="rp"></param>
        /// <returns></returns>
        public static bool isLine(Pos lp, Pos rp)
        {
            return lp.x == rp.x || lp.y == rp.y;
        }

        public override bool Equals(object obj)
        {
            Pos p = obj as Pos;
            if (p == null) return false;

            if (x != p.x || y != p.y)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= x.GetHashCode();
            hash ^= y.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return "X:" + x + " Y:" + y;
        }
    }
}
