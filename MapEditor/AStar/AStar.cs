using System.Collections.Generic;

namespace Game
{
    public class AStar
    {
        //地图数据
        protected Map map = null;

        private readonly Dictionary<Pos, Node> OpenList = new Dictionary<Pos, Node>();
        private readonly Dictionary<Pos, Node> CloseList = new Dictionary<Pos, Node>();

        //终点位置
        private Pos EndPos;
        //最终节点
        private Node EndNode;

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="map"></param>
        /// <param name="StartPos"></param>
        /// <param name="EndPos"></param>
        /// <returns></returns>
        public static List<Pos> FindPath(Map map, Pos StartPos, Pos EndPos)
        {
            AStar aStar = new AStar();
            return aStar.findPath(map, StartPos, EndPos);
        }

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="StartPos">起点</param>
        /// <param name="EndPos">终点</param>
        /// <returns></returns>
        protected List<Pos> findPath(Map map, Pos StartPos, Pos EndPos)
        {
            this.map = map;

            this.EndPos = EndPos;
            this.EndNode = null;

            if (IsObstruct(EndPos))
                return null;

            //起点终点在同一个点，返回终点位置
            if (EndPos.Equals(StartPos))
                return new List<Pos>() { EndPos };

            //起点处理
            Node startNode = new Node(StartPos, 0, (EndPos - StartPos) * 10, null);
            OpenList.Add(startNode.pos, startNode);

            while (!GoANode()) ;
            if (EndNode == null)
            {
                return null;
            }
            else
            {
                List<Pos> poses = new List<Pos>();
                Node currentNode = EndNode;
                bool isTurner = true;
                do
                {
                    if (isTurner)
                    {
                        poses.Add(currentNode.pos);
                    }
                    isTurner = !currentNode.IsSameD;
                    currentNode = currentNode.father;
                }
                while (currentNode.father != null);

                return poses;
            }
        }

        /// <summary>
        /// 走一个节点
        /// </summary>
        /// <returns></returns>
        private bool GoANode()
        {
            Node minNode = GetMinFNode();
            if (minNode == null || EndNode != null)
                return true;

            CloseList.Add(minNode.pos, minNode);
            OpenList.Remove(minNode.pos);

            RedoGH(minNode.pos.Up, minNode);
            RedoGH(minNode.pos.Right, minNode);
            RedoGH(minNode.pos.Down, minNode);
            RedoGH(minNode.pos.Left, minNode);

            return false;
        }

        /// <summary>
        /// 计算到这个点的最优路径
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="father"></param>
        private void RedoGH(Pos pos, Node father)
        {
            Node node = null;
            OpenList.TryGetValue(pos, out node);
            if (node != null)
            {
                //当前路线的g值比之前路线小时，切换父节点到当前节点
                if (node.gh.g > father.gh.g + 10)
                {
                    node.gh.g = father.gh.g + 10 + (node.IsSameD ? 0 : 1);
                    node.father = father;
                }
            }
            else
            {
                //没有找到时尝试将节点加入到OpenList中
                AddNewNodeToOpenList(father, pos);
            }
        }

        /// <summary>
        /// 增加新的待查询点到Openlist中
        /// </summary>
        /// <param name="father"></param>
        /// <param name="newPos"></param>
        private void AddNewNodeToOpenList(Node father, Pos newPos)
        {
            if (!CloseList.ContainsKey(newPos)          //未检查过的节点
                && !newPos.IsOverflow(map.width, map.height)    //未越界                 
                && !IsObstruct(newPos))                 //不是障碍物
            {
                Node node = new Node(newPos, father.gh.g + 10, (EndPos - newPos) * 10, father);
                OpenList.Add(newPos, node);
                if (newPos.x == EndPos.x && newPos.y == EndPos.y)
                {
                    EndNode = node;
                }
            }
        }

        /// <summary>
        /// 是否为障碍物
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected bool IsObstruct(Pos pos)
        {
            return map.IsObstruct(pos);
        }

        /// <summary>
        /// 获取f值最小的节点
        /// </summary>
        /// <returns></returns>
        private Node GetMinFNode()
        {
            if (OpenList.Count <= 0)
                return null;

            Node minNode = null;
            bool isLine = false;
            int f, mf = 0;
            foreach (var item in OpenList)
            {
                if (minNode == null)
                {
                    minNode = item.Value;
                    mf = minNode.gh.f;
                }
                else
                {
                    f = item.Value.gh.f;
                    if (f < mf)         //查找f值最小的点
                    {
                        isLine = false;
                        minNode = item.Value;
                        mf = minNode.gh.f;
                    }
                    else if (!isLine && f == mf)        //优化：当f相同时，当前点与目标点在一条直线上，优先走这个点
                    {
                        if (Pos.isLine(item.Key, EndPos))
                        {
                            isLine = true;
                            minNode = item.Value;
                            mf = minNode.gh.f;
                        }
                    }
                }
            }

            return minNode;
        }
    }
}
