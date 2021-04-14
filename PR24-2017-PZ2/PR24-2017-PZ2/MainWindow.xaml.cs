using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using PR24_2017_PZ2.Model;



namespace PR24_2017_PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        double _zoomValue = 1.0;
        double minX, maxX;
        double minY, maxY;
        int size = 500;       ///500x500
        Dictionary<long, SubstationEntity> subEnt = new Dictionary<long, SubstationEntity>();
        Dictionary<long, NodeEntity> nodeEnt = new Dictionary<long, NodeEntity>();
        Dictionary<long, SwitchEntity> swcEnt = new Dictionary<long, SwitchEntity>();
        Dictionary<long, LineEntity> lineEnt = new Dictionary<long, LineEntity>();

        double[,] pointMatrix = new double[501, 501];
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList;
            XmlNodeList nodeList2;
            XmlNodeList nodeList3;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            nodeList2 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            nodeList3 = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            minX = double.Parse(nodeList[0].SelectSingleNode("X").InnerText);
            maxX = minX;
            minY = double.Parse(nodeList[0].SelectSingleNode("Y").InnerText);
            maxY = minY;

            findMaxMin(nodeList);
            findMaxMin(nodeList2);
            findMaxMin(nodeList3);

            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                sub.X = CalculateXCoord(sub.X, minX, maxX);
                sub.Y = CalculateYCoord(sub.Y, minY, maxY);

                double newX, newY;

                checkMatrix(sub.X, sub.Y, out newX, out newY);
                sub.X = newX;
                sub.Y = newY;

                pointMatrix[(int)sub.X, (int)sub.Y] = 1;

                subEnt.Add(sub.Id, sub);
                
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeobj = new NodeEntity();
                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                nodeobj.X = CalculateXCoord(nodeobj.X, minX, maxX);
                nodeobj.Y = CalculateYCoord(nodeobj.Y, minY, maxY);

                double newX, newY;

                checkMatrix(nodeobj.X, nodeobj.Y, out newX, out newY);
                nodeobj.X = newX;
                nodeobj.Y = newY;

                pointMatrix[(int)nodeobj.X, (int)nodeobj.Y] = 1;

                nodeEnt.Add(nodeobj.Id, nodeobj);

            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity();
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                switchobj.X = CalculateXCoord(switchobj.X, minX, maxX);
                switchobj.Y = CalculateYCoord(switchobj.Y, minY, maxY);

                double newX, newY;

                checkMatrix(switchobj.X, switchobj.Y, out newX, out newY);
                switchobj.X = newX;
                switchobj.Y = newY;

                pointMatrix[(int)switchobj.X, (int)switchobj.Y] = 1;

                swcEnt.Add(switchobj.Id, switchobj);
            }

            //int count = 0;


            //provera da li su sve tacke rasporedjene, da slucajno nema preklapanja
            /*foreach(double num in pointMatrix)
            {
                if(num == 1)
                {
                    count++;
                }
            }

            int number = subEnt.Count + nodeEnt.Count + swcEnt.Count;
            */

            DrawSubPoints();
            DrawNodPoints();
            DrawSwcPoints();



            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity();

                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    l.IsUnderground = true;
                }
                else
                {
                    l.IsUnderground = false;
                }
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                lineEnt.Add(l.Id, l);

            }

            int count = 0;

            Dictionary<long, LineEntity> temp = new Dictionary<long, LineEntity>();
            
            foreach(LineEntity ln in lineEnt.Values)
            {
                temp.Add(ln.Id, ln);
            }

            foreach(LineEntity line in temp.Values)
            {
                if(!(subEnt.ContainsKey(line.FirstEnd) || nodeEnt.ContainsKey(line.FirstEnd)
                   || swcEnt.ContainsKey(line.FirstEnd)) || !(subEnt.ContainsKey(line.SecondEnd) || nodeEnt.ContainsKey(line.SecondEnd)
                   || swcEnt.ContainsKey(line.SecondEnd)))
                {
                    count++;
                    lineEnt.Remove(line.Id);
                }
            }
            
        }



        private int CalculateXCoord(double x, double minX, double maxX)
        {
            if(x == minX)
            {
                return 0;
            }else if(x == maxX)
            {
                return 200;
            }

            double diff = maxX - minX;
            double num = x - minX;
            double finalNum = diff / num;

            return (int)Math.Round(size / finalNum);
        }

        private int CalculateYCoord(double y, double minY, double maxY)
        {
            if (y == minY)
            {
                return 0;
            }
            else if (y == maxY)
            {
                return 200;
            }

            double diff = maxY - minY;
            double num = y - minY;
            double finalNum = diff / num;

            return (int)Math.Round(size / finalNum);
        }

        private void findMaxMin(XmlNodeList nodeList)
        {
            SubstationEntity sub = new SubstationEntity();

            foreach (XmlNode node in nodeList)
            {
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                if (maxX < sub.X)
                {
                    maxX = sub.X;
                }

                if (minY > sub.Y)
                {
                    minY = sub.Y;
                }

                if (minX > sub.X)
                {
                    minX = sub.X;
                }

                if (maxY < sub.Y)
                {
                    maxY = sub.Y;
                }
            }

        }

        private void DrawSubPoints()
        {
            foreach(SubstationEntity sub in subEnt.Values)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width/size;
                circle.Height = canvas.Height/size;
                circle.Fill = Brushes.Black;

                Canvas.SetLeft(circle, sub.X* (canvas.Width / size));
                Canvas.SetTop(circle, sub.Y* (canvas.Height / size));

                canvas.Children.Add(circle);
            }
        }

        private void DrawNodPoints()
        {
            foreach (NodeEntity sub in nodeEnt.Values)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width / size;
                circle.Height = canvas.Height / size;
                circle.Fill = Brushes.Red;

                Canvas.SetLeft(circle, sub.X *(canvas.Width/size));
                Canvas.SetTop(circle, sub.Y *(canvas.Height/size));

                canvas.Children.Add(circle);
            }
        }

        private void DrawSwcPoints()
        {
            

            foreach (SwitchEntity sub in swcEnt.Values)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width / size;
                circle.Height = canvas.Height / size;
                circle.Fill = Brushes.Green;

                Canvas.SetLeft(circle, sub.X * (canvas.Width / size));
                Canvas.SetTop(circle, sub.Y * (canvas.Height / size));

                canvas.Children.Add(circle);
            }
        }


        private void MouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            

            if (e.Delta > 0)
            {
                _zoomValue += 0.2;
            }
            else
            {
                if(_zoomValue <= 0.5)
                {
                    return;
                }
                _zoomValue -= 0.2;
            }

            System.Windows.Point mousePos = e.GetPosition(canvas);

            ScaleTransform scale = new ScaleTransform(_zoomValue, _zoomValue, mousePos.X, mousePos.Y);
            
            canvas.LayoutTransform = scale;
          
            e.Handled = true;
        }

        private void checkMatrix(double xx, double yy, out double newX, out double newY)
        {
            newX = xx;
            newY = yy;


            int oldx = (int)Math.Round(xx);
            int oldy = (int)Math.Round(yy);

            int x = (int)Math.Round(xx);
            int y = (int)Math.Round(yy);

            if(pointMatrix[x,y] == 0)
            {
                return;
            }

            int num = 1;

            //pronaci najblize slobodno mesto, idem u krug oko pocetne tacke dok ne pronadjem najblizu slobodnu lokaciju
             while(true)
             {
                for (y = oldy - num; y <= oldy + num; y++)
                {
                    for (x = oldx - num; x <= oldx + num; x++)
                    {
                        if (x >= 0 && x <= size && y >= 0 && y <= size) //provera da li je slobodna tacka u dozvoljenim okvirima
                        {
                            if (pointMatrix[x, y] == 0)
                            {
                                newX = x;
                                newY = y;
                                return;
                            }
                        }
                    }
                }
                num = num + 1;
             }

            
        }
    }
}
