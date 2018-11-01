namespace Game
{
    /// <summary>
    /// 节点
    /// </summary>
    public class Node
    {
        public GH gh;
        public Pos pos;
        public Node father;
        public Node(Pos pos, int g, int h, Node father)
        {
            this.pos = pos;
            this.father = father;
            this.gh = new GH() { g = g + (IsSameD ? 0 : 1), h = h };
        }

        public bool IsSameD
        {
            get
            {
                var d = D;
                if (father == null || (d & father.D) == d)
                {
                    return true;
                }
                return false;
            }
        }

        private Direction D
        {
            get
            {
                if (father == null)
                {
                    return Direction.None;
                }
                if (father.pos.x - pos.x == 0 && father.pos.y - pos.y == -1)
                {
                    return Direction.Down;
                }
                else if (father.pos.x - pos.x == -1 && father.pos.y - pos.y == 0)
                {
                    return Direction.Right;
                }
                else if (father.pos.x - pos.x == 0 && father.pos.y - pos.y == 1)
                {
                    return Direction.Up;
                }
                else
                {
                    return Direction.Left;
                }
            }
        }
    }

    /// <summary>
    /// G H 值, G为从其实点到当前点的行动力 H为当前点到目标点的横纵行动力
    /// </summary>
    public struct GH
    {
        public int g;
        public int h;
        public int f { get { return g + h; } }
        public GH(int g, int h)
        {
            this.g = g;
            this.h = h;
        }
    }

    /// <summary>
    /// 方向枚举
    /// </summary>
    public enum Direction
    {
        Up = 1,
        Left = 2,
        Down = 4,
        Right = 8,
        None = Up | Left | Down | Right
    }
}
