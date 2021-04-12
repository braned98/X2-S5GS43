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

        double minX, maxX;
        double minY, maxY;
        int size = 1000;       ///1000x1000
        List<SubstationEntity> subEnt = new List<SubstationEntity>();
        List<NodeEntity> nodeEnt = new List<NodeEntity>();
        List<SwitchEntity> swcEnt = new List<SwitchEntity>();

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

                subEnt.Add(sub);
                
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

                nodeEnt.Add(nodeobj);

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

                swcEnt.Add(switchobj);
            }

            DrawSubPoints();
            DrawNodPoints();
            DrawSwcPoints();

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
            foreach(SubstationEntity sub in subEnt)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width/200;
                circle.Height = canvas.Height/200;
                circle.Fill = Brushes.Black;

                Canvas.SetLeft(circle, sub.X* (canvas.Width / size));
                Canvas.SetTop(circle, sub.Y* (canvas.Height / size));

                canvas.Children.Add(circle);
            }
        }

        private void DrawNodPoints()
        {
            foreach (NodeEntity sub in nodeEnt)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width / 200;
                circle.Height = canvas.Height / 200;
                circle.Fill = Brushes.Red;

                Canvas.SetLeft(circle, sub.X *(canvas.Width/size));
                Canvas.SetTop(circle, sub.Y *(canvas.Height/size));

                canvas.Children.Add(circle);
            }
        }

        private void DrawSwcPoints()
        {
            foreach (SwitchEntity sub in swcEnt)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width / 200;
                circle.Height = canvas.Height / 200;
                circle.Fill = Brushes.Green;

                Canvas.SetLeft(circle, sub.X * (canvas.Width / size));
                Canvas.SetTop(circle, sub.Y * (canvas.Height / size));

                canvas.Children.Add(circle);
            }
        }
    }
}
