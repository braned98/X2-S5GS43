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
        public int size = 500;       ///500x500
        public Dictionary<long, SubstationEntity> subEnt = new Dictionary<long, SubstationEntity>();
        public Dictionary<long, NodeEntity> nodeEnt = new Dictionary<long, NodeEntity>();
        public Dictionary<long, SwitchEntity> swcEnt = new Dictionary<long, SwitchEntity>();
        public Dictionary<long, LineEntity> lineEnt = new Dictionary<long, LineEntity>();
        public List<LineEntity> lines = new List<LineEntity>();
        public List<Line> drawnLines = new List<Line>();
        public List<Line> openSwcLine = new List<Line>();

        private System.Windows.Point start = new System.Windows.Point();
        private System.Windows.Point diffOffset = new System.Windows.Point();
        private int zoomMax = 100;
        private int zoomCurent = 1;

        int numLn = 0;
        int cnt = 0;

        bool showOpenLines = true;


        public double[,] pointMatrix;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            CallLoad();
        }

        public void CallLoad()
        {
            pointMatrix = new double[size+1, size+1];

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
            //uporedio sam broj cvorova u kolekcijama cvorova(toliko treba da bude i u matrici tj. na canvasu posle) i broj cvorova u matrici
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
            count = 0;
            List<LineEntity> lines = temp.Values.ToList();

            foreach(LineEntity ln in lines)
            {
                int cnt = 0;
                long id1 = ln.FirstEnd;
                long id2 = ln.SecondEnd;

                foreach(KeyValuePair<long, LineEntity> line in temp)
                {
                    if((line.Value.FirstEnd == id1 && line.Value.SecondEnd == id2) || (line.Value.FirstEnd == id2 && line.Value.SecondEnd == id1))
                    {
                        cnt++;
                        if(cnt > 1) //prvi slucaj gde postoje isti cvorovi moze da prodje, svaki naredni se brise jer je visak
                        {           //"Treba ignorisati ponovno iscrtavanje vodova izmedju dva ista čvora."
                            count++;
                            lineEnt.Remove(line.Key);
                        }
                        
                    }
                }

            }


            //ovo sam koristio da nadjem odgovarajuce cvorove za pocetak bfs algoritma, neka dva koja su najblize jedan drugom
            //"Algoritam započeti od neka dva čvora koja imaju najmanju udaljenost na gridu. Naći ih automatski ili ručno."

            /* foreach(LineEntity line in lineEnt.Values)
             {
                 double x = 0;
                 double x2 = 0;
                 double y = 0;
                 double y2 = 0;
                 if (subEnt.ContainsKey(line.FirstEnd))
                 {
                     x = subEnt[line.FirstEnd].X;
                     y = subEnt[line.FirstEnd].Y;
                 }else if (nodeEnt.ContainsKey(line.FirstEnd))
                 {
                     x = nodeEnt[line.FirstEnd].X;
                     y = nodeEnt[line.FirstEnd].Y;
                 }
                 else
                 {
                     x = swcEnt[line.FirstEnd].X;
                     y = swcEnt[line.FirstEnd].Y;
                 }
                 if (subEnt.ContainsKey(line.SecondEnd))
                 {
                     x2 = subEnt[line.SecondEnd].X;
                     y2 = subEnt[line.SecondEnd].Y;
                 }
                 else if (nodeEnt.ContainsKey(line.SecondEnd))
                 {
                     x2 = nodeEnt[line.SecondEnd].X;
                     y2 = nodeEnt[line.SecondEnd].Y;
                 }
                 else 
                 {
                     x2 = swcEnt[line.SecondEnd].X;
                     y2 = swcEnt[line.SecondEnd].Y;
                 }

                 if(Math.Abs(x2 - x) <= 1 && Math.Abs(y2 - y) <= 1)
                 {
                     if(line.FirstEnd == 40828 || line.SecondEnd == 40828)
                     {

                     }

                 }

             }*/

            BFS_Algorithm();
            BFS_Algorithm2();
            
        }

        private void BFS_Algorithm()
        {
            bool[,] visited = new bool[size+1, size+1];

            updateVisited(ref visited);

            LineEntity firstLine = new LineEntity();
            if (lineEnt.ContainsKey(35370))
            {
                firstLine = lineEnt[35370];
            }
            lines = lineEnt.Values.ToList();
            lines.Remove(firstLine);
            lines.Insert(0, firstLine);

            List<LineEntity> deleteLines = new List<LineEntity>();

            
            foreach(LineEntity line in lines)
            {
                Queue<Node> qNodes = new Queue<Node>();
           
                int x = getX(line.FirstEnd);
                int y = getY(line.FirstEnd);

                int x2 = getX(line.SecondEnd);
                int y2 = getY(line.SecondEnd);

                pointMatrix[(int)x2, (int)y2] = 3;  //stavljam vrednost odredista u matrici na 3 kako bi ga algoritam jednostavnije prepoznao
                visited[(int)x2, (int)y2] = false;          //kasnije cu ga vratiti opet na 1, (1 su svi cvorovi, 2 su linije)

                Node source = new Node(x, y);
                Node destination = null;

                bool isOpen = false;

                qNodes.Enqueue(source);

                while(qNodes.Count > 0)
                {
                    Node node = qNodes.Dequeue();

                    if(pointMatrix[node.X, node.Y] == 3)
                    {
                        destination = node;
                        break;
                    }

                                                                                
                    if(node.Y - 1 > 0 && visited[node.X, node.Y - 1] == false) //proveravam susedno polje iznad
                    {
                        Node nNode = new Node(node.X, node.Y - 1);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.Y + 1 < size+1 && visited[node.X, node.Y + 1] == false) //susedno polje ispod
                    {
                        Node nNode = new Node(node.X, node.Y + 1);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.X - 1 > 0 && visited[node.X - 1, node.Y] == false) //susedno polje levo
                    {
                        Node nNode = new Node(node.X - 1, node.Y);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.X + 1 < size+1 && visited[node.X + 1, node.Y] == false) //susedno polje desno
                    {
                        Node nNode = new Node(node.X + 1, node.Y);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }
                    

                }
                pointMatrix[(int)x2, (int)y2] = 1;

                if(destination != null)  //ako je destinacija != null, onda je algoritam nasao put do odredisnog cvora
                {

                    if (checkIfSwitch(line))
                    {
                        if (swcEnt.ContainsKey(line.FirstEnd))
                        {
                            if(swcEnt[line.FirstEnd].Status == "Open")
                            {
                                isOpen = true;
                            }
                        }

                        if (swcEnt.ContainsKey(line.SecondEnd))
                        {
                            if (swcEnt[line.SecondEnd].Status == "Open")
                            {
                                isOpen = true;
                            }
                        }
                    }

                    while (destination.Parent != null) //vracam se do izvora tako sto pratim roditelje svakog cvora i iscrtavam liniju do njega
                    {                                 //izvor nema roditeljski cvor(logicno) pa se kod njega iteracija zavrsava, sto je super
                        Line ln = new Line();
                        ln.X1 = (destination.X * (canvas.Width / size)) + (canvas.Width / (2*size));
                        ln.Y1 = canvas.Height - destination.Y * (canvas.Height / size) - (canvas.Height / (2 * size));
                        ln.X2 = destination.Parent.X * (canvas.Width / size) + (canvas.Width / (2 * size));
                        ln.Y2 =  canvas.Height - destination.Parent.Y * (canvas.Height / size) - (canvas.Height / (2 * size));
                        //pointMatrix[destination.X, destination.Y] = 2;
                        if ((pointMatrix[destination.X, destination.Y] == 0))
                        {
                            pointMatrix[destination.X, destination.Y] = 2;
                        }
                        ln.Fill = Brushes.Blue;
                        ln.Stroke = Brushes.Blue;
                        ln.StrokeThickness = (canvas.Width/size)/5;
                        ln.Uid = line.Id.ToString() + ":" + line.FirstEnd.ToString() + ":" + line.SecondEnd.ToString();
                        ln.ToolTip = "Name: " + line.Name + ", ID: " + line.Id;
                        canvas.Children.Add(ln);
                        if (isOpen)
                        {
                            openSwcLine.Add(ln);
                        }
                        destination = destination.Parent;
                        drawnLines.Add(ln);
                       
                    }
                    deleteLines.Add(line);

                }
                updateVisited(ref visited);
                
            }

            foreach(LineEntity ln in deleteLines)  //brisem sve linije koje sam uspeo nacrtati bez presecanja
            {                                      //ostatak linija ce proci kroz drugi deo bfs algoritma
                lines.Remove(ln);
            }
            
        }
        
        //BFS sa presecanjima
        private void BFS_Algorithm2()
        {
            bool[,] visited = new bool[size+1, size+1];

            updateVisited2(ref visited);
            
            
            foreach (LineEntity line in lines)
            {
                Queue<Node> qNodes = new Queue<Node>();

                int x = getX(line.FirstEnd);
                int y = getY(line.FirstEnd);

                int x2 = getX(line.SecondEnd);
                int y2 = getY(line.SecondEnd);

                pointMatrix[(int)x2, (int)y2] = 3;  //stavljam vrednost odredista u matrici na 3 kako bi ga algoritam jednostavnije prepoznao
                visited[(int)x2, (int)y2] = false;          //kasnije cu ga vratiti opet na 1, (1 su svi cvorovi, 2 su linije)

                Node source = new Node(x, y);
                Node destination = null;

                bool isOpen = false;

                qNodes.Enqueue(source);

                while (qNodes.Count > 0)
                {
                    Node node = qNodes.Dequeue();

                    if (pointMatrix[node.X, node.Y] == 3)
                    {
                        destination = node;
                        break;
                    }


                    if (node.Y - 1 > 0 && visited[node.X, node.Y - 1] == false) //proveravam susedno polje iznad
                    {
                        Node nNode = new Node(node.X, node.Y - 1);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.Y + 1 < size+1 && visited[node.X, node.Y + 1] == false) //susedno polje ispod
                    {
                        Node nNode = new Node(node.X, node.Y + 1);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.X - 1 > 0 && visited[node.X - 1, node.Y] == false) //susedno polje levo
                    {
                        Node nNode = new Node(node.X - 1, node.Y);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }

                    if (node.X + 1 < size+1 && visited[node.X + 1, node.Y] == false) //susedno polje desno
                    {
                        Node nNode = new Node(node.X + 1, node.Y);
                        nNode.Parent = node;
                        qNodes.Enqueue(nNode);
                        visited[nNode.X, nNode.Y] = true;
                    }


                }
                pointMatrix[(int)x2, (int)y2] = 1;

                if (destination != null)  //ako je destinacija != null, onda je algoritam nasao put do odredisnog cvora
                {

                    List<Line> completeLine = new List<Line>();
                    
                    bool isLine = false;
                    bool isFirst = true;

                    if (checkIfSwitch(line))
                    {
                        if (swcEnt.ContainsKey(line.FirstEnd))
                        {
                            if (swcEnt[line.FirstEnd].Status == "Open")
                            {
                                isOpen = true;
                            }
                        }

                        if (swcEnt.ContainsKey(line.SecondEnd))
                        {
                            if (swcEnt[line.SecondEnd].Status == "Open")
                            {
                                isOpen = true;
                            }
                        }
                    }

                    while (destination.Parent != null) //vracam se do izvora tako sto pratim roditelje svakog cvora i iscrtavam liniju do njega
                    {                                 //izvor nema roditeljski cvor(logicno) pa se kod njega iteracija zavrsava, sto je super
                        Line ln = new Line();
                        ln.X1 = (destination.X * (canvas.Width / size)) + (canvas.Width / (2 * size));
                        ln.Y1 = canvas.Height - destination.Y * (canvas.Height / size) - (canvas.Height / (2 * size));
                        ln.X2 = destination.Parent.X * (canvas.Width / size) + (canvas.Width / (2 * size));
                        ln.Y2 = canvas.Height - destination.Parent.Y * (canvas.Height / size) - (canvas.Height / (2 * size));
                        //pointMatrix[destination.X, destination.Y] = 2;
                        ln.Fill = Brushes.Blue;
                        ln.Stroke = Brushes.Blue;
                        ln.StrokeThickness = (canvas.Width / size) / 5;
                        ln.Uid = line.Id.ToString() + ":" + line.FirstEnd.ToString() + ":" + line.SecondEnd.ToString();
                        ln.ToolTip = "Name: " + line.Name + ", ID: " + line.Id;
                        canvas.Children.Add(ln);
                        if (isOpen)
                        {
                            openSwcLine.Add(ln);
                        }
                        completeLine.Add(ln);
                        drawnLines.Add(ln);

                        if(pointMatrix[destination.X, destination.Y] == 2) //ako je tu vec linija, mozda dode do presecanja
                        {
                            if (checkLine())  //idem proveriti da li ima presecanja, ako ima na taj podeok u matrici stavljam 5   
                            {
                                pointMatrix[destination.X, destination.Y] = 5;
                            }
                        }

                        if ((pointMatrix[destination.X, destination.Y] == 0))
                        {
                            pointMatrix[destination.X, destination.Y] = 2;
                        }
                        destination = destination.Parent;

                       
                    }
                }
                updateVisited2(ref visited);
            }
            checkAll();
        }

        private bool checkIfSwitch(LineEntity line)
        {
            if(swcEnt.ContainsKey(line.FirstEnd) || swcEnt.ContainsKey(line.SecondEnd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void checkAll()
        {
            for (int k = 0; k < pointMatrix.GetLength(0); k++)
            {
                for (int l = 0; l < pointMatrix.GetLength(1); l++)
                {
                    if (pointMatrix[k, l] == 5)
                    {
                        double x = k * (canvas.Width / size);
                        double y = l * (canvas.Height / size);

                        Rectangle rect = new Rectangle();
                        rect.Width = (canvas.Width / size) * 0.6;
                        rect.Height = (canvas.Height / size) * 0.6;
                        rect.Fill = Brushes.Transparent;
                        rect.Stroke = Brushes.Red;
                        rect.StrokeThickness = rect.Width / 5;

                        Canvas.SetLeft(rect, x + (canvas.Width / size) * 0.2);
                        Canvas.SetBottom(rect, y + (canvas.Height / size) * 0.2);

                        canvas.Children.Add(rect);
                    }
                }
            }
        }

        private bool checkLine() //u specifikaciji pise da oznacim tacke preseka
                                 //jedino mi je logicno da oznacim kada se dve linije bukvalno presecaju
                                 //nekad ce se linije naci u istom podeoku, ali se mogu mimoici bez presecanja i taj slucaj necu oznaciti kao presecanje
                                 //jedini slucaj koji gledam je kada bukvalno dve linije prelaze jedna preko druge sto mi je i logicno i takvih slucajeva ima mnogo
        {
            Line line2 = drawnLines[drawnLines.Count - 1];
            Line line1 = drawnLines[drawnLines.Count - 2];

            if(line1.X1 != line2.X2 && line1.Y1 != line2.Y2)        //Linija koja se trenutno iscrtava skrece i nema presecanja, moze samo biti mimoilazenja
            {
                return false;
            }

            for(int i=0; i < drawnLines.Count-3; i++)
            {
                if(drawnLines[i].X2 == line2.X1 && drawnLines[i].Y2 == line2.Y1)
                {
                    string lineID = drawnLines[i].Uid.Split(':')[0];
                    
                    Line line3 = drawnLines[i];
                    Line line4 = drawnLines[i + 1];

                    if (line3.X1 != line4.X2 && line3.Y1 != line4.Y2)  //Linija koja je vec tu iscrtana pre skrece i nema presecanja, moze samo biti mimoilazenja
                    {
                        return false;
                    }

                    if(line2.X2 != line4.X2 && line2.Y2 != line4.Y2)      //Doslo je do presecanja!
                    {
                        return true;                                      //vracam true i sada ce u matrici ovaj podeok imati vrednost 5
                    }                                                     //na tom mestu u canvasu ce se iscrtati kvadrat sa crvenim rubom
                }
            }
            return false;
        }
        

        private int getX(long id)
        { 

            if (subEnt.ContainsKey(id))
            {
                return (int)subEnt[id].X;
            }else if (nodeEnt.ContainsKey(id))
            {
                return (int)nodeEnt[id].X;
            }
            else
            {
                return (int)swcEnt[id].X;
            }
        }

        private int getY(long id)
        {
            if (subEnt.ContainsKey(id))
            {
                return (int)subEnt[id].Y;
            }
            else if (nodeEnt.ContainsKey(id))
            {
                return (int)nodeEnt[id].Y;
            }
            else
            {
                return (int)swcEnt[id].Y;
            }
        }

        private void updateVisited(ref bool[,] visited)
        {
            //ako je cvor ili linija na polju to smatram posecenim poljem, zato da linije ne idu kroz cvorove na pocetku bez potrebe
            // i zato da iskoristim to za prvu iteraciju bfs algoritma koja ce nacrtati najblize putanje bez presecanja
            // cvorovi u matrici su mi vrednosti 1
            // linije su vrednosti 2
            
            for (int k = 0; k < pointMatrix.GetLength(0); k++)
            {
                for (int l = 0; l < pointMatrix.GetLength(1); l++)
                {
                    if (pointMatrix[k, l] == 0)
                    {
                        visited[k, l] = false;
                    }
                    else
                    {
                        visited[k, l] = true;
                    }
                }
            }
        }

        private void updateVisited2(ref bool[,] visited)
        {
            for (int k = 0; k < pointMatrix.GetLength(0); k++)
            {
                for (int l = 0; l < pointMatrix.GetLength(1); l++)
                {
                    if (pointMatrix[k, l] == 0 || pointMatrix[k, l] == 2 || pointMatrix[k, l] == 5)  //u drugoj iteraciji moze da se preseca pa vod nije prepreka
                    {
                        visited[k, l] = false;
                    }
                    else
                    {
                        visited[k, l] = true;
                    }
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
                return size;
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
                return size;
            }

            double diff = maxY - minY;
            double num = y - minY;
            double finalNum = diff / num;

            return (int)Math.Round(size / finalNum);
        }

        private void findMaxMin(XmlNodeList nodeList)
        {
            //max i min za potrebe aproksimacije
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
                circle.Width = (canvas.Width/size)*0.8; // smanjujem za 20%   ZBOG BOLJE PREGLEDNOSTI SAMIH TACAKA
                circle.Height = (canvas.Height/size)*0.8;   //DA NE BUDU ZBIJENE JEDNA UZ DRUGU AKO SU JEDNA PORED DRUGE
                circle.Fill = Brushes.Black;
                circle.Uid = sub.Id.ToString();             //ID da kasnije lakse nadjem cvor na kanvasu i menjam mu boju
                circle.ToolTip = "Name: " + sub.Name + ", ID: " + sub.Id;

                Canvas.SetLeft(circle, sub.X* (canvas.Width / size) + 0.1 * (canvas.Width / size)); //pomeram za 10% po x osi
                Canvas.SetBottom(circle, sub.Y* (canvas.Height / size) + 0.1 * (canvas.Width / size)); //po y osi  DA BUDU U CENTRU PODEOKA

                canvas.Children.Add(circle);
            }
        }

        private void DrawNodPoints()
        {
            foreach (NodeEntity sub in nodeEnt.Values)
            {
                Ellipse circle = new Ellipse();
                circle.Width = (canvas.Width / size) * 0.8;
                circle.Height = (canvas.Height / size) * 0.8;
                circle.Fill = Brushes.Orange;
                circle.Uid = sub.Id.ToString();
                circle.ToolTip = "Name: " + sub.Name + ", ID: " + sub.Id;

                Canvas.SetLeft(circle, sub.X *(canvas.Width/size) + 0.1 * (canvas.Width / size));
                Canvas.SetBottom(circle, sub.Y *(canvas.Height/size) + 0.1 * (canvas.Width / size));

                canvas.Children.Add(circle);
            }
        }

        private void DrawSwcPoints()
        {
            

            foreach (SwitchEntity sub in swcEnt.Values)
            {
                Ellipse circle = new Ellipse();
                circle.Width = (canvas.Width / size) * 0.8;
                circle.Height = (canvas.Height / size) * 0.8;
                circle.Fill = Brushes.Green;
                circle.Uid = sub.Id.ToString();                      
                circle.ToolTip = "Name: " + sub.Name + ", ID: " + sub.Id;

                Canvas.SetLeft(circle, sub.X * (canvas.Width / size) + 0.1 * (canvas.Width / size));
                Canvas.SetBottom(circle, sub.Y * (canvas.Height / size) + 0.1 * (canvas.Width / size));

                canvas.Children.Add(circle);
            }
        }

        private void Right_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Line))
            {
                NodeColorWin window = new NodeColorWin(e.OriginalSource);

                window.Owner = this;
                window.ShowDialog();

            }
        }

       /* private void MouseWheelZoom(object sender, MouseWheelEventArgs e)
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
        }*/

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
                        {                                               //ako nije trazim prvu narednu koja ce biti validna
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

        private void LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.X;
            diffOffset.Y = translacija.Y;
        }

        private void LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();
        }

        

        private void MouseWheelAction(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point p = e.MouseDevice.GetPosition(this);
            double scaleX = 1;
            double scaleY = 1;


            if (e.Delta > 0 && zoomCurent < zoomMax)
            {

                skaliranje.CenterX = canvas.Width / 2;
                skaliranje.CenterY = canvas.Height / 2;
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
            else if (e.Delta <= 0 && zoomCurent > 1.0)
            {
                skaliranje.CenterX = canvas.Width / 2;
                skaliranje.CenterY = canvas.Height / 2;
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
        }

        private void MouseMoveAction(object sender, MouseEventArgs e)
        {
            if (canvas.IsMouseCaptured)
            {
                System.Windows.Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100000) / w;
                double translateY = -(offsetY * 100000) / h;
                translacija.X = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                translacija.Y = diffOffset.Y - (translateY / (100 * skaliranje.ScaleX));
            }
        }

        private void DisplaySettingClick(object sender, RoutedEventArgs e)
        {
            DisplaySetting displaySettingWindow = new DisplaySetting(this);
            displaySettingWindow.Owner = Window.GetWindow(this);
            displaySettingWindow.Show();
            
        }

        private void DimensionsClick(object sender, RoutedEventArgs e)
        {
            DimensionSetting dimensionSettingWindow = new DimensionSetting(this);
            dimensionSettingWindow.Show();
        }

        private void HideActive(object sender, RoutedEventArgs e)
        {
            if (showOpenLines)
            {
                foreach (Line ln in openSwcLine)
                {
                    canvas.Children.Remove(ln);
                }
            }
            showOpenLines = false;
        }

        private void ShowActive(object sender, RoutedEventArgs e)
        {
            if (!showOpenLines)
            {
                foreach (Line ln in openSwcLine)
                {
                    canvas.Children.Add(ln);
                }
            }
            showOpenLines = true;
        }
        
      
        
    }
}
