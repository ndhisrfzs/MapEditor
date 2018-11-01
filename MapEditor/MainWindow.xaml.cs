using Game;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
{
    public enum EditStatus
    {
        SetStart,
        SetEnd,
        Draw,
    }
    ///   (0,0) -> (1, 0)
    ///     |----------> x
    ///     |
    ///(0,1)|
    ///     |
    ///     V   y
    /// 
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int obstruct = 0;
        private string mapName;
        private double width;
        private double height;
        private Grid grid;
        private byte[,] Matrix; //[y,x]
        protected int drawX;
        protected int drawY;
        private int gridwidth;
        private int gridheight;

        //路径数据
        private Pos startPos;
        private Pos endPos;
        private List<Pos> paths;
        public MainWindow()
        {
            InitializeComponent();
            this.AddHandler(InkCanvas.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.ObstructionViewer_MouseLeftButtonDown), true);
            gridwidth = int.Parse(GridWidth.Text);
            gridheight = int.Parse(GridHeight.Text);
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadMap = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "图像文件(*.jpg,*.png)|*.jpg;*.png",
            };
            loadMap.FileOk += new System.ComponentModel.CancelEventHandler(LoadMap_FileOk);
            loadMap.ShowDialog();
        }

        /// <summary>
        /// 加载地图完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadMap_FileOk(object sender, CancelEventArgs e)
        {
            mapName = (sender as OpenFileDialog).FileName;
            Map.Source = new BitmapImage(new Uri(mapName, UriKind.Absolute));
            width = Map.Source.Width;
            height = Map.Source.Height;
            AddGrid();
        }

        /// <summary>
        /// 加网格
        /// </summary>
        private void AddGrid()
        {
            grid = new Grid()
            {
                ShowGridLines = IsShowGrid.IsChecked.Value,
                Width = width,
                Height = height,
            };

            int cols = (int)(grid.Width) / gridwidth + 1;
            int rows = (int)(grid.Height) / gridheight + 1;
            for (int y = 0; y < cols; y++)
            {
                ColumnDefinition col = new ColumnDefinition()
                {
                    Width = new GridLength(gridwidth)
                };
                grid.ColumnDefinitions.Add(col);
            }
            for (int x = 0; x < rows; x++)
            {
                RowDefinition row = new RowDefinition()
                {
                    Height = new GridLength(gridheight)
                };
                grid.RowDefinitions.Add(row);
            }
            ObstructionViewer.Content = grid;

            //初始化地图数据
            //看数据是否为空，来渲染障碍物
            if (Matrix != null)
            {
                for (int x = 0; x <= Matrix.GetUpperBound(1); x++)
                {
                    for (int y = 0; y <= Matrix.GetUpperBound(0); y++)
                    {
                        if (Matrix[y, x] == obstruct)
                        {
                            //为障碍物
                            grid.UnregisterName("rect" + x + "_" + y);
                        }
                    }
                }
            }
            if(startPos != null)
            {
                grid.UnregisterName("startPos");
            }
            if(endPos != null)
            {
                grid.UnregisterName("endPos");
            }
            if(paths != null)
            {
                foreach (var item in paths)
                {
                    grid.UnregisterName("path" + item.x + "_" + item.y);
                }
            }

            startPos = null;
            endPos = null;
            paths = null;
            //初始化障碍物数据
            Matrix = new byte[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Matrix[y, x] = 1;
                }
            }
        }

        public void ObstructionViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            MapScrollViewer.ScrollToVerticalOffset(scrollviewer.VerticalOffset);
            MapScrollViewer.ScrollToHorizontalOffset(scrollviewer.HorizontalOffset);
        }

        /// <summary>
        /// 设置网格颜色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        private void SetMatrixGrid(int x, int y, byte value)
        {
            if (x < 0 || y < 0 || y > Matrix.GetUpperBound(0) || x > Matrix.GetUpperBound(1))
                return;
            //先获取原先这个格子的数据
            int old_value = Matrix[y, x];
            if (value == 0)
            {
                //设置障碍物
                if (old_value == 0)
                {
                    Matrix[y, x] = 1;
                    //先清除障碍物
                    Rectangle temp_rect = grid.FindName("rect" + x + "_" + y) as Rectangle;
                    grid.UnregisterName("rect" + x + "_" + y);
                    grid.Children.Remove(temp_rect);
                }
                else
                {
                    Matrix[y, x] = 0;
                    Rectangle rect = new Rectangle()
                    {
                        Width = gridwidth,
                        Height = gridheight,
                        Fill = new SolidColorBrush(Colors.Gray)
                    };
                    grid.Children.Add(rect);
                    grid.RegisterName("rect" + x + "_" + y, rect);
                    rect.SetValue(Grid.ColumnProperty, x);
                    rect.SetValue(Grid.RowProperty, y);
                }
            }
            else if(value == 2)
            {
                //设置起点
                if(startPos != null)
                {
                    //先清除起点
                    Rectangle temp_rect = grid.FindName("startPos") as Rectangle;
                    grid.UnregisterName("startPos");
                    grid.Children.Remove(temp_rect);
                }
                Rectangle rect = new Rectangle()
                {
                    Width = gridwidth,
                    Height = gridheight,
                    Fill = new SolidColorBrush(Colors.Red)
                };
                grid.Children.Add(rect);
                grid.RegisterName("startPos", rect);
                rect.SetValue(Grid.ColumnProperty, x);
                rect.SetValue(Grid.RowProperty, y);
                startPos = new Pos(x, y);
                SearchPath();
            }
            else if(value == 3 && old_value != 3)
            {
                //设置终点
                if(endPos != null)
                {
                    //先清除终点
                    Rectangle temp_rect = grid.FindName("endPos") as Rectangle;
                    grid.UnregisterName("endPos");
                    grid.Children.Remove(temp_rect);
                }
                Rectangle rect = new Rectangle()
                {
                    Width = gridwidth,
                    Height = gridheight,
                    Fill = new SolidColorBrush(Colors.Blue)
                };
                grid.Children.Add(rect);
                grid.RegisterName("endPos", rect);
                rect.SetValue(Grid.ColumnProperty, x);
                rect.SetValue(Grid.RowProperty, y);
                endPos = new Pos(x, y);
                SearchPath();
            }
            else if(value == 4)
            {
                //画路径
                Rectangle rect = new Rectangle()
                {
                    Width = gridwidth,
                    Height = gridheight,
                    Fill = new SolidColorBrush(Colors.Green)
                };
                grid.Children.Add(rect);
                grid.RegisterName("path" + x + "_" + y, rect);
                rect.SetValue(Grid.ColumnProperty, x);
                rect.SetValue(Grid.RowProperty, y);
                paths.Add(new Pos(x, y));
            }
        }

        private void SearchPath()
        {
            if (startPos != null && endPos != null)
            {
                //先清除旧路径
                if (paths != null && paths.Count > 0)
                {
                    foreach (var p in paths)
                    {
                        Rectangle temp_rect = grid.FindName("path" + p.x + "_" + p.y) as Rectangle;
                        grid.UnregisterName("path" + p.x + "_" + p.y);
                        grid.Children.Remove(temp_rect);
                    }
                }

                paths = new List<Pos>();

                Game.Map.HarbMap = ToMap();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<Pos> newPaths = Game.Map.HarbMap.FindPath(startPos, endPos);
                //var newPaths = Game.AStar.FindPath(Game.Map.HarbMap, startPos, endPos);
                sw.Stop();
                if (newPaths == null)
                {
                    MessageBox.Show("未找到合适路径");
                }
                else
                {
                    foreach (var item in newPaths)
                    {
                        SetMatrixGrid(item.x, item.y, 4);
                    }
                }
                findpathtime.Text = "寻路时间:" + sw.Elapsed;
            }
        }

        private Map ToMap()
        {
            int height = Matrix.GetUpperBound(0) + 1;
            int width = Matrix.GetUpperBound(1) + 1;
            Map map = new Map();
            map.tilewidth = gridwidth;
            map.tileheight = gridheight;
            map.width = width;
            map.height = height;
            map.obstruct = false;
            map.data = new bool[height * width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map.data[y * width + x] = (Matrix[y, x] == obstruct) ? map.obstruct : !map.obstruct;
                }
            }

            return map;
        }

        private void ObstructionViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double a = ObstructionViewer.ActualHeight;
            if (grid == null)
            {
                return;
            }

            System.Windows.Point p = e.GetPosition(ObstructionViewer);
            if (p.X < ObstructionViewer.ViewportWidth && p.Y < ObstructionViewer.ViewportHeight)
            {
                p = e.GetPosition(Map);
                drawX = (int)p.X / gridwidth;
                drawY = (int)p.Y / gridheight;
                if (rb_DrawObstruction.IsChecked.Value)
                {
                    SetMatrixGrid(drawX, drawY, 0);
                }
                else if (rb_SetStartPoint.IsChecked.Value)
                {
                    SetMatrixGrid(drawX, drawY, 2);
                }
                else if (rb_SetEndPoint.IsChecked.Value)
                {
                    SetMatrixGrid(drawX, drawY, 3);
                }
                coordinate.Text = "当前坐标:(" + drawX +","+ drawY + ")";
            }
        }

        private void ObstructionViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(Map);
                int tempX = (int)p.X / gridwidth;
                int tempY = (int)p.Y / gridheight;
                if (tempX != drawX || tempY != drawY)
                {
                    drawX = tempX;
                    drawY = tempY;
                    if (rb_DrawObstruction.IsChecked.Value)
                    {
                        SetMatrixGrid(drawX, drawY, 0);
                    }
                    else if (rb_SetStartPoint.IsChecked.Value)
                    {
                        SetMatrixGrid(drawX, drawY, 2);
                    }
                    else if (rb_SetEndPoint.IsChecked.Value)
                    {
                        SetMatrixGrid(drawX, drawY, 3);
                    }
                    coordinate.Text = "当前坐标:(" + drawX + "," + drawY + ")";
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (GridWidth != null)
            {
                GridWidth.Text = ((int)(sender as Slider).Value).ToString();
                gridwidth = int.Parse(GridWidth.Text);
                AddGrid();
            }
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (GridHeight != null)
            {
                GridHeight.Text = ((int)(sender as Slider).Value).ToString();
                gridheight = int.Parse(GridHeight.Text);
                AddGrid();
            }
        }

        private void IsShowGrid_Click(object sender, RoutedEventArgs e)
        {
            AddGrid();
        }

        private void OutputObstruction_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog()
            {
                FileName = mapName.Remove(mapName.LastIndexOf('.')) + ".map",
            };
            save.FileOk += new System.ComponentModel.CancelEventHandler(Save_FileOk);
            save.ShowDialog();
        }
        void Save_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileDialog mySave = sender as SaveFileDialog;
            string savePath = mySave.FileName;
            Output(savePath);
        }
        private void Output(string savepath)
        {
            Map map = ToMap();
            map.sdata = MergeArray.ToByteArray(map.data);

            var buf = FileHelper.SerializeToBinary(map);
            FileHelper.WriteToFile(savepath, buf, 0, buf.Length);
            MessageBox.Show("导出成功！");
        }

        private void LoadObstruction_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "障碍物逻辑文件（*.map）|*.map",
            };
            open.FileOk += new System.ComponentModel.CancelEventHandler(Open_FileOk);
            open.ShowDialog();
        }

        void Open_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OpenFileDialog open = sender as OpenFileDialog;
            var data = FileHelper.ReadFromFile(open.FileName);
            var map = FileHelper.DeserializeFromBinary<Map>(data);
            map.data = MergeArray.ToBoolArray(map.sdata);

            gridWidthSlider.Value = map.tilewidth;
            gridHeightSlider.Value = map.tileheight;

            AddGrid();

            int height = Matrix.GetUpperBound(0) + 1;
            int width = Matrix.GetUpperBound(1) + 1;

            for (int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(map.data[y * width + x] == map.obstruct)
                    {
                        SetMatrixGrid(x, y, 0);
                    }
                    else
                    {
                        Matrix[y, x] = 1;
                    }
                }
            }
        }
    }
}
