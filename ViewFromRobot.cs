using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Krasnyanskaya221327_Lab03_Sem5_Ver1.MainForm;

namespace Krasnyanskaya221327_Lab03_Sem5_Ver1
{
    public partial class ViewFromRobot : Form
    {
        public static int width = 0;
        public static int height = 0;

        public ViewFromRobot()
        {
            InitializeComponent();
            width = pictureBox1.Width;
            height = pictureBox1.Height;
        }

        private void ViewFromRobot_Load(object sender, EventArgs e)
        {
            if (MainForm.server.Data != null)
            {
                timer1.Enabled = true;
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.White;
            pictureBox1.Image = MainForm.bitmap;
            pictureBox1.Invalidate();

            
            pictureBox2.BackColor = Color.White;
            pictureBox2.Image = MainForm.visualization.DetectObstaclesFromSensors(MainForm.threshold, UDPServer.d0, 
                UDPServer.d1, UDPServer.d2, UDPServer.d3,
                UDPServer.d4, UDPServer.d5, UDPServer.d6, UDPServer.d7);
            pictureBox2.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MainForm.Visualization.scale = (float)(trackBar1.Value * 0.1);
        }
    }
}
