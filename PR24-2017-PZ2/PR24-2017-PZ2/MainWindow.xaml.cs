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
        int size = 200;       ///200x200
        List<SubstationEntity> subEnt = new List<SubstationEntity>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList;

            
            

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            minX = double.Parse(nodeList[0].SelectSingleNode("X").InnerText);
            maxX = minX;
            minY = double.Parse(nodeList[0].SelectSingleNode("Y").InnerText);
            maxY = minY;

            findMaxMin(nodeList);

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

            DrawPoints();
            
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

        private void DrawPoints()
        {
            foreach(SubstationEntity sub in subEnt)
            {
                Ellipse circle = new Ellipse();
                circle.Width = canvas.Width/200;
                circle.Height = canvas.Height/200;
                circle.Fill = Brushes.Black;

                Canvas.SetLeft(circle, sub.X*4);
                Canvas.SetTop(circle, sub.Y*4);

                canvas.Children.Add(circle);
            }
        }
    }
}
