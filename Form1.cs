using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Krasnyanskaya221327_Lab03_Sem5_Ver1
{
    public partial class MainForm : Form
    {
        public static UDPServer server;
        public static Bitmap bitmap;
        public static Bitmap MiniMap;
        public int threshold;

        static int count = 0;
        static int oldCount = 0;
        private int savedCount = 0;
        private int N = 0;

        private DateTime moveBackStartTime;
        private bool isMovingBack = false;
        private bool isMovingForward = false;
        private TimeSpan moveBackDuration = TimeSpan.FromSeconds(1);
        private int bumpCount = 0;

        public static Visualization visualization = new Visualization();
        private bool isSend = false;
        public int countOfClick = 1;

        int previousForwardSpeed = 0;
        int previousBackwardSpeed = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        public class UDPServer
        {
            public IPAddress IpAddress { get; set; }
            public int LocalPort { get; set; }
            public int RemotePort { get; set; }
            public UdpClient UdpClient { get; set; }
            public IPEndPoint IpEndPoint { get; set; }
            public byte[] Data { get; set; }
            public static Dictionary<string, int> DecodeText;

            public static string DecodeData { get; set; }
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;

            public UDPServer(IPAddress ip, int localPort, int remotePort)
            {
                IpAddress = ip;
                LocalPort = localPort;
                RemotePort = remotePort;
                UdpClient = new UdpClient(LocalPort);
                IpEndPoint = new IPEndPoint(IpAddress, LocalPort);
            }

            public async Task ReceiveDataAsync()
            {
                while (true)
                {
                    var receivedResult = await UdpClient.ReceiveAsync();
                    Data = receivedResult.Buffer;
                    DecodingData(Data);
                }
            }

            public async Task SendDataAsync(byte[] data)
            {
                if (data != null)
                {
                    IPEndPoint pointServer = new IPEndPoint(IpAddress, RemotePort);
                    await UdpClient.SendAsync(data, data.Length, pointServer);
                }
            }

            public async Task SendRobotDataAsync()
            {
                string robotData = Robot.GetCommandsAsJson();
                byte[] dataToSend = Encoding.ASCII.GetBytes(robotData + "\n");
                await SendDataAsync(dataToSend);
            }

            private void DecodingData(byte[] data)
            {
                var message = Encoding.ASCII.GetString(data);
                DecodeText = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);
                var lines = DecodeText.Select(kv => kv.Key + ": " + kv.Value.ToString());
                DecodeData = "IoT: " + string.Join(Environment.NewLine, lines);

                AnalyzeData(DecodeText);
            }

            private void AnalyzeData(Dictionary<string, int> pairs)
            {
                if (pairs.ContainsKey("n"))
                {
                    n = pairs["n"];
                    s = pairs["s"];
                    c = pairs["c"];
                    le = pairs["le"];
                    re = pairs["re"];
                    az = pairs["az"];
                    b = pairs["b"];
                    d0 = pairs["d0"];
                    d1 = pairs["d1"];
                    d2 = pairs["d2"];
                    d3 = pairs["d3"];
                    d4 = pairs["d4"];
                    d5 = pairs["d5"];
                    d6 = pairs["d6"];
                    d7 = pairs["d7"];
                    l0 = pairs["l0"];
                    l1 = pairs["l1"];
                    l2 = pairs["l2"];
                    l3 = pairs["l3"];
                    l4 = pairs["l4"];
                }
                else
                {
                    MessageBox.Show("No data");
                }
            }

        }

        public static class Robot
        {
            public static Dictionary<string, int> Commands = new Dictionary<string, int>
            {
                { "N", 0 },
                { "M", 0 },
                { "F", 0 },
                { "B", 0 },
                { "T", 0 },
            };

            public static bool isInStartZone = false;
            public static bool isInWaitingZone = false;
            public static bool isPalletGet = false;
            public static bool isReadyToPick = false;
            public static bool PickedOrder = false;
            public static bool ReturnedToFinal = false;
            public static bool FinalStateGot = false;
            public static int countOfOrders = 0;
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;
            public static bool isFirstRotateDone = false, isFirstWayDone = false;

            public static void UpdateData(Dictionary<string, int> pairs)
            {
                n = pairs["n"];
                s = pairs["s"];
                c = pairs["c"];
                le = pairs["le"];
                re = pairs["re"];
                az = pairs["az"];
                b = pairs["b"];
                d0 = pairs["d0"];
                d1 = pairs["d1"];
                d2 = pairs["d2"];
                d3 = pairs["d3"];
                d4 = pairs["d4"];
                d5 = pairs["d5"];
                d6 = pairs["d6"];
                d7 = pairs["d7"];
                l0 = pairs["l0"];
                l1 = pairs["l1"];
                l2 = pairs["l2"];
                l3 = pairs["l3"];
                l4 = pairs["l4"];
            }



            public static string GetCommandsAsJson()
            {
                return JsonConvert.SerializeObject(Commands);
            }

            public static void LoadCommandsFromJson(string json)
            {
                var newCommands = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                if (newCommands != null)
                {
                    Commands = newCommands;
                }
            }

            public static void SetCommand(string key, int value)
            {
                if (Commands.ContainsKey(key))
                {
                    Commands[key] = value;
                }
                else
                {
                    throw new ArgumentException("Команда с таким ключом не существует.");
                }
            }

            public static void UpdateDecodeText()
            {
                UDPServer.DecodeText["n"] = Commands["N"];
            }

            public static void SendOldCommands()
            {
                string oldCommands = JsonConvert.SerializeObject(UDPServer.DecodeText, Formatting.None);

                byte[] data = Encoding.ASCII.GetBytes(oldCommands + "\n");

                UdpClient udpCommands = new UdpClient();
                IPEndPoint pointServer = new IPEndPoint(server.IpAddress, server.RemotePort);
                udpCommands.Send(data, data.Length, pointServer);

                string jsonString = JsonConvert.SerializeObject(Commands, Formatting.None);
                byte[] dataForRobot = Encoding.ASCII.GetBytes(jsonString + "\n");

                udpCommands.Send(dataForRobot, dataForRobot.Length, pointServer);
            }

            public static void RotateRight()
            {
                SetCommand("B", 25);
            }

            public static void RotateLeft()
            {
                SetCommand("B", -25);
            }

            public static void MoveStraight()
            {
                SetCommand("F", 100);
            }

            public static void MoveBack()
            {
                SetCommand("F", -100);
            }

            public static void Stop()
            {
                SetCommand("B", 0);
                SetCommand("F", 0);
            }

            public static void MoveBackWhenBump()
            {
                SetCommand("F", -70);
                SetCommand("B", -25);
            }
        }

        public class Visualization
        {
            public Graphics Graphics;
            public Bitmap MiniMap = new Bitmap(410, 280);
            public PointF RobotPosition = new PointF(205, 140); // Позиция робота на экране (фиксированная)
            public float RobotDirection = 1;

            private List<PointF> Trajectory = new List<PointF>();
            private List<PointF> Obstacles = new List<PointF>();
            private List<PointF> PreviousObstacles = new List<PointF>();
            private List<PointF> Walls = new List<PointF>(); // Список для стен
            private List<PointF> ImgWalls = new List<PointF>();

            private int previousLeftEncoder = 0; // Предыдущее значение энкодера левого колеса
            private int previousRightEncoder = 0; // Предыдущее значение энкодера правого колеса

            private PointF FixedRobotPosition = new PointF(205, 140); // Фиксированная точка робота на экране

            public void CalculateDistance(float[] distances)
            {
                // Обновляем текущее направление робота (предполагается, что RobotDirection обновляется при каждом повороте)
                float directionInRadians = RobotDirection * (float)Math.PI / 180;

                // Здесь используем среднее значение расстояний с дальномеров
                float averageDistance = distances.Average();

                // Вычисляем смещение робота по осям с учетом направления
                float deltaX = averageDistance * (float)Math.Cos(directionInRadians);
                float deltaY = averageDistance * (float)Math.Sin(directionInRadians);

                // Обновляем позицию робота
                RobotPosition.X += deltaX;
                RobotPosition.Y += deltaY;

                if (Robot.Commands["F"] == 0)
                {
                    TurnTrajectory(-Robot.Commands["B"], count);
                }
                else
                {
                    Trajectory.Add(new PointF(RobotPosition.X, RobotPosition.Y));
                }
            }

            public static float scale = 2;

            public Bitmap DrawMiniMap()
            {
                // Создаем графический контекст для мини-карты
                Graphics g = Graphics.FromImage(MiniMap);
                g.Clear(Color.Black); // Очищаем мини-карту

                // Центр карты
                float centerX = MiniMap.Width / 2;
                float centerY = MiniMap.Height / 2;
                g.TranslateTransform(centerX, centerY);

                UpdatePosition(UDPServer.re, UDPServer.le, true);

                // Рисуем препятствия
                foreach (var obstacle in Obstacles)
                {
                    PointF adjustedObstacle = new PointF(obstacle.X - RobotPosition.X, obstacle.Y - RobotPosition.Y);
                    g.FillRectangle(Brushes.Red, adjustedObstacle.X * scale - 3, adjustedObstacle.Y * scale - 3, 6, 6);
                }

                // Отрисовываем траекторию с увеличенным масштабом
                // Отрисовываем все точки траектории
                foreach (var point in Trajectory)
                {
                    PointF miniPoint = new PointF(point.X - RobotPosition.X, point.Y - RobotPosition.Y);
                    g.FillEllipse(Brushes.Blue, miniPoint.X * 30 * scale - 3, miniPoint.Y * 30 * scale - 3, 4, 4);
                }

                // Текущее положение робота
                g.FillEllipse(Brushes.Purple, 0 - 5, 0 - 5, 10, 10); // Центрированное отображение робота

                return MiniMap;
            }



            // Функция для масштабирования точки без учета смещения
            private PointF ScalePoint(PointF point, float scale)
            {
                float scaledX = point.X * scale; // Масштабируем координату X
                float scaledY = point.Y * scale; // Масштабируем координату Y
                return new PointF(scaledX, scaledY);
            }

            private void UpdateWalls(float threshold)
            {
                ImgWalls.Clear(); // Очищаем старые стены для рисования на мини-карте

                // Усредняем координаты между предыдущими и текущими препятствиями
                for (int i = 0; i < Obstacles.Count; i++)
                {
                    PointF previous = (i < PreviousObstacles.Count) ? PreviousObstacles[i] : Obstacles[i]; // Если нет предыдущих данных, используем текущие
                    PointF current = Obstacles[i];

                    // Проверяем, не превышает ли расстояние между текущими и предыдущими координатами порог
                    float distance = (float)Math.Sqrt(Math.Pow(current.X - previous.X, 2) + Math.Pow(current.Y - previous.Y, 2));

                    if (distance <= threshold)
                    {
                        // Усредняем координаты
                        PointF wall = new PointF(
                            (previous.X + current.X) / 2,
                            (previous.Y + current.Y) / 2
                        );

                        ImgWalls.Add(wall); // Добавляем усредненные стены для рисования на мини-карте
                        Walls.Add(wall); // Также добавляем в Walls для заливки
                    }
                }

                // Обновляем список предыдущих препятствий для следующего вызова
                PreviousObstacles = new List<PointF>(Obstacles);
            }


            // Метод для перевода координат в масштаб мини-карты
            private PointF ScalePoint(PointF point, float scale, Rectangle miniMapRect)
            {
                return new PointF(
                    miniMapRect.X + (point.X * scale),
                    miniMapRect.Y + (point.Y * scale)
                );
            }

            public bool isTurning = false;

            // Метод DrawRobot с фиксацией траектории
            public Bitmap DrawRobot(int robotAngle)
            {
                Bitmap view = new Bitmap(410, 280);

                using (Graphics g = Graphics.FromImage(view))
                {
                    g.Clear(Color.Black);

                    // Расчет смещения относительно фиксированной позиции робота
                    PointF translation = new PointF(FixedRobotPosition.X - RobotPosition.X, FixedRobotPosition.Y - RobotPosition.Y);

                    if (Walls.Count > 0)
                    {
                        FillSpaceToWalls(g, translation);
                    }

                    // Рисуем препятствия
                    foreach (var point in Obstacles)
                    {
                        PointF translatedPoint = TranslatePoint(point, translation);
                        g.FillRectangle(Brushes.Red, translatedPoint.X - 3, translatedPoint.Y - 3, 6, 6);
                    }

                    UpdatePosition(UDPServer.le, UDPServer.re, true);
                    g.FillEllipse(Brushes.Purple, FixedRobotPosition.X - 5, FixedRobotPosition.Y - 5, 10, 10);
                }

                return view;
            }

            private void FillSpaceToWalls(Graphics g, PointF translation)
            {
                if (Walls != null && Walls.Count > 0)
                {
                    // Получаем углы, под которыми находятся препятствия
                    List<float> angles = new List<float> { 0, 45, 90, 135, 180, -45, -90, -135 };

                    // Проходим по каждому препятствию
                    for (int i = 0; i < Walls.Count; i++)
                    {
                        PointF currentWall = TranslatePoint(Walls[i], translation);

                        // Угол, под которым находится текущее препятствие
                        float angle = angles[i % angles.Count]; // Используем модуль для повторения углов

                        // Определяем следующую стену для создания сектора
                        PointF nextWall;
                        if (i + 1 < Walls.Count)
                        {
                            nextWall = TranslatePoint(Walls[i + 1], translation);
                        }
                        else
                        {
                            // Если это последнее препятствие, замыкаем с первым
                            nextWall = TranslatePoint(Walls[0], translation);
                        }

                        // Создаем точки для заполнения пространства (треугольник)
                        PointF[] points = new PointF[]
                        {
                FixedRobotPosition, // Центр робота
                currentWall,        // Текущая стена
                nextWall            // Следующая стена
                        };

                        // Рисуем заполнение треугольника
                        g.FillPolygon(Brushes.LightBlue, points);
                    }
                }
            }

            private bool IsAtStartingPosition(float threshold = 5.0f)
            {
                // Вычисляем расстояние от текущей позиции робота до начальной позиции
                float distance = (float)Math.Sqrt(Math.Pow(RobotPosition.X - FixedRobotPosition.X, 2) +
                                                  Math.Pow(RobotPosition.Y - FixedRobotPosition.Y, 2));

                // Если расстояние меньше порога, возвращаем true
                return distance < threshold;
            }

            // Метод для перевода точек относительно неподвижной позиции робота
            private PointF TranslatePoint(PointF point, PointF translation)
            {
                return new PointF(point.X + translation.X, point.Y + translation.Y);
            }

            private void DrawWalls(Graphics g, PointF translation)
            {
                foreach (var wall in Walls)
                {
                    PointF translatedWall = TranslatePoint(wall, translation);
                    g.FillRectangle(Brushes.Red, translatedWall.X - 3, translatedWall.Y - 3, 6, 6); // Рисуем стены красным цветом
                }
            }

            public void UpdatePosition(int le, int re, bool updateTrajectory = true)
            {
                const float wheelBase = 0.4f;
                const float wheelDiameter = 0.1f;
                const float distancePerTick = (float)(Math.PI * wheelDiameter / 360);

                float distanceLeft = (le - previousLeftEncoder) * distancePerTick;
                float distanceRight = (re - previousRightEncoder) * distancePerTick;

                if (distanceLeft == 0 && distanceRight == 0) return;

                float distance = (distanceLeft + distanceRight) / 2;
                float angleChange = (distanceRight - distanceLeft) / wheelBase;

                RobotDirection = (RobotDirection + angleChange * (180 / (float)Math.PI)) % 360;
                float directionInRadians = RobotDirection * (float)Math.PI / 180;

                float deltaX = distance * (float)Math.Cos(directionInRadians);
                float deltaY = distance * (float)Math.Sin(directionInRadians);

                RobotPosition.X += deltaX;
                RobotPosition.Y += deltaY;

                if (updateTrajectory)
                {
                    Trajectory.Add(new PointF(RobotPosition.X, RobotPosition.Y));
                }

                previousLeftEncoder = le;
                previousRightEncoder = re;
            }

            private bool isMovingForwardAfterTurn = false; // Флаг для отслеживания движения вперед после поворота
            private int moveForwardDelay = 1000; // Задержка в миллисекундах

            public void TurnTrajectory(float angularSpeed, float turnTime)
            {
                // Преобразуем угловую скорость из градусов в радианы
                float angularSpeedRad = angularSpeed * (float)Math.PI / 180;
                // Вычисляем угол поворота
                float deltaAngle = angularSpeedRad * turnTime;

                // Обновляем направление робота в градусах
                RobotDirection = (RobotDirection + deltaAngle * (180 / (float)Math.PI)) % 360;

                // Если угловая скорость отрицательна, поворот влево, положительная — вправо
                float turnRadius = 1.0f; // Допустим, радиус поворота робота, можно менять при необходимости
                float dx = turnRadius * (float)Math.Sin(deltaAngle);
                float dy = turnRadius * (1 - (float)Math.Cos(deltaAngle));

                // Учитываем направление робота
                float directionRad = RobotDirection * (float)Math.PI / 180;

                // Вычисляем новую позицию робота с учетом текущего направления
                float newX = RobotPosition.X + (float)(Math.Cos(directionRad) * dx - Math.Sin(directionRad) * dy);
                float newY = RobotPosition.Y + (float)(Math.Sin(directionRad) * dx + Math.Cos(directionRad) * dy);

                // Обновляем текущую позицию робота
                RobotPosition = new PointF(newX, newY);

                // Добавляем текущую позицию в траекторию
                Trajectory.Add(new PointF(newX, newY));
            }


            public void DetectObstaclesFromSensors(int threshold, params int[] distances)
            {
                Obstacles.Clear();
                ImgWalls.Clear();
                Walls.Clear();

                // Углы, соответствующие каждому дальномеру
                float[] angles = { 0, 45, 90, 135, 180, -135, -90, -45 };

                for (int i = 0; i < distances.Length; i++)
                {
                    if (distances[i] < threshold)
                    {
                        // Получаем позицию препятствия по данным дальномера и углу
                        PointF obstacle = GetObstaclePosition(distances[i], RobotDirection + angles[i]);

                        if (!Obstacles.Contains(obstacle))
                        {
                            Obstacles.Add(obstacle);
                            ImgWalls.Add(obstacle);
                            Walls.Add(obstacle);
                        }
                    }
                }

                UpdateWalls(10.0f);
            }


            //public void ResetPositionAfterTurn()
            //{
            //    RobotPosition = FixedRobotPosition;
            //    Trajectory.Clear();
            //}

            private PointF GetObstaclePosition(float distance, float angle)
            {
                float angleInRadians = angle * (float)Math.PI / 180;
                float x = RobotPosition.X + distance * (float)Math.Cos(angleInRadians);
                float y = RobotPosition.Y + distance * (float)Math.Sin(angleInRadians);
                return new PointF(x, y);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                textBox1.Text = data.TextBox1;
                textBox2.Text = data.TextBox2;
                textBox3.Text = data.TextBox3;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form form = new ViewFromRobot();
            form.Show();
        }

        public void SplitDataToTextBoxs()
        {
            var message = Encoding.ASCII.GetString(server.Data);
            var text = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);

            foreach (var chr in text)
            {
                if (chr.Key == "d0")
                {
                    textBox4.Text = chr.Value.ToString();
                }
                if (chr.Key == "d1")
                {
                    textBox5.Text = chr.Value.ToString();
                }
                if (chr.Key == "d2")
                {
                    textBox6.Text = chr.Value.ToString();
                }
                if (chr.Key == "d3")
                {
                    textBox7.Text = chr.Value.ToString();
                }
                if (chr.Key == "d4")
                {
                    textBox8.Text = chr.Value.ToString();
                }
                if (chr.Key == "d5")
                {
                    textBox9.Text = chr.Value.ToString();
                }
                if (chr.Key == "d6")
                {
                    textBox10.Text = chr.Value.ToString();
                }
                if (chr.Key == "d7")
                {
                    textBox11.Text = chr.Value.ToString();
                }

                if (chr.Key == "n")
                {
                    textBox12.Text = chr.Value.ToString();
                }
                if (chr.Key == "s")
                {
                    textBox13.Text = chr.Value.ToString();
                }
                if (chr.Key == "c")
                {
                    textBox14.Text = chr.Value.ToString();
                }
                if (chr.Key == "re")
                {
                    textBox15.Text = chr.Value.ToString();
                }
                if (chr.Key == "le")
                {
                    textBox16.Text = chr.Value.ToString();
                }
                if (chr.Key == "az")
                {
                    textBox17.Text = chr.Value.ToString();
                }
                if (chr.Key == "b")
                {
                    textBox18.Text = chr.Value.ToString();
                }
                if (chr.Key == "l0")
                {
                    textBox24.Text = chr.Value.ToString();
                }
                if (chr.Key == "l1")
                {
                    textBox23.Text = chr.Value.ToString();
                }
                if (chr.Key == "l2")
                {
                    textBox22.Text = chr.Value.ToString();
                }
                if (chr.Key == "l3")
                {
                    textBox21.Text = chr.Value.ToString();
                }
                if (chr.Key == "l4")
                {
                    textBox20.Text = chr.Value.ToString();
                }
            }
        }

        public static double CalculateTurnAngle(double V, double L, double deltaTime)
        {
            // Вычисляем угловую скорость ω = (vR - vL) / L
            double omega = V / L;

            // Вычисляем угол поворота θ = ω * deltaTime
            double angleInRadians = omega * deltaTime;

            // Переводим радианы в градусы, если нужно
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            return angleInDegrees;  // Возвращаем угол в градусах
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (UDPServer.DecodeData != null)
            {
                if (checkBox1.Checked)
                {
                    // Получаем текущее значение скорости вперёд и назад
                    int currentForwardSpeed = trackBar1.Value;
                    int currentBackwardSpeed = trackBar2.Value;

                    // Проверяем, изменилась ли скорость вперёд
                    if (currentForwardSpeed != previousForwardSpeed)
                    {
                        if (currentForwardSpeed > 0)
                        {
                            Robot.SetCommand("B", 0); // Обнуляем поворот
                            visualization.isTurning = false; // Завершаем поворот
                        }
                        previousBackwardSpeed = 0; // Обновляем переменную для отслеживания
                    }

                    // Устанавливаем текущие команды
                    Robot.SetCommand("F", currentForwardSpeed);

                    // Если скорость "F" равна 0, то устанавливаем команду для поворота
                    if (currentForwardSpeed == 0)
                    {
                        Robot.SetCommand("B", currentBackwardSpeed);
                    }
                    else
                    {
                        Robot.SetCommand("B", 0); // Обнуляем поворот, если скорость не равна 0
                    }

                    // Обновляем предыдущие скорости
                    previousForwardSpeed = currentForwardSpeed;

                    // Если флаг isSend активен, отправляем команды из текстовых полей
                    if (isSend)
                    {
                        int forwardSpeedFromText = Convert.ToInt32(textBox26.Text);
                        int backwardSpeedFromText = Convert.ToInt32(textBox27.Text);

                        // Проверяем, изменилась ли скорость вперёд
                        if (forwardSpeedFromText != previousForwardSpeed)
                        {
                            if (currentForwardSpeed > 0)
                            {
                                Robot.SetCommand("B", 0); // Обнуляем поворот
                                visualization.isTurning = false; // Завершаем поворот
                                //visualization.ResetPositionAfterTurn();
                            }
                            previousBackwardSpeed = 0; // Обновляем переменную для отслеживания
                        }

                        // Устанавливаем команду движения вперёд
                        Robot.SetCommand("F", forwardSpeedFromText);

                        // Если скорость "F" равна 0, разрешаем поворот
                        if (forwardSpeedFromText == 0)
                        {
                            Robot.SetCommand("B", backwardSpeedFromText); // Устанавливаем поворот
                        }
                        else
                        {
                            Robot.SetCommand("B", 0); // Обнуляем поворот, если скорость не равна 0
                        }
                    }

                    // Обновляем предыдущие скорости
                    previousForwardSpeed = currentForwardSpeed;
                }

                SplitDataToTextBoxs();

                richTextBox1.Text = "\r\n" + "Here is data";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();

                visualization.DetectObstaclesFromSensors(threshold, UDPServer.d0, UDPServer.d1, UDPServer.d2, UDPServer.d3,
                        UDPServer.d4, UDPServer.d5, UDPServer.d6, UDPServer.d7);
                bitmap = visualization.DrawRobot((int)CalculateTurnAngle(Robot.Commands["B"], 1, count / 10));
                MiniMap = visualization.DrawMiniMap();
                count++;
                richTextBox1.Text = "\r\n" + visualization.RobotPosition.X.ToString() + "; " + visualization.RobotPosition.Y.ToString();

                Robot.SetCommand("N", count);

                // Обновляем данные робота
                Robot.UpdateDecodeText();
                Robot.SendOldCommands();
                await server.SendRobotDataAsync();

                textBox19.Text = count.ToString();
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            server = new UDPServer(IPAddress.Parse(textBox3.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox1.Text));
            await server.ReceiveDataAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var data = new
            {
                TextBox1 = textBox1.Text,
                TextBox2 = textBox2.Text,
                TextBox3 = textBox3.Text
            };

            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            label29.Text = trackBar3.Value.ToString();
            threshold = trackBar3.Value;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label25.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label26.Text = trackBar2.Value.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isSend = true;
            countOfClick++;
            if (countOfClick % 2 == 0)
            {
                isSend = false;
            }
        }
    }
}
