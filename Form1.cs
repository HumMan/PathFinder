using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PathFinder
{
    public partial class Form1 : Form
    {
        TMap map;
        bool left_button_down=false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            map = new TMap(panel1.CreateGraphics(),panel1.Size.Width,50);
            map.Draw();
            //timer1.Enabled = true;
            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 0;
            UpdateInfo();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = "Кнопка мыши - рисование";
            map.OnMouseMove (panel1.PointToClient(Cursor.Position));
            if (left_button_down)
                map.OnMouseDown();
            map.Draw();
            UpdateInfo();
        }
        private void Form2_Resize(object sender, EventArgs e)
        {
            map.Draw();
        }
        void UpdateInfo()
        {
            label5.Text = Convert.ToString(map.GetPathCellsCount());
            label6.Text = Convert.ToString(map.GetPathLength());
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            map.Draw();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            map.SetEditMode(TMap.EditMode.SET_START);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            map.SetEditMode(TMap.EditMode.SET_END);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            map.SetEditMode(TMap.EditMode.SET_WALL);
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            map.SetEditMode(TMap.EditMode.CLEAR);
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            left_button_down = true;
            map.OnMouseDown();
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            left_button_down = false;
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                TMap.EditMode temp=map.GetEditMode();
                map = new TMap(panel1.CreateGraphics(),
                    panel1.Size.Width,
                    Convert.ToInt32(comboBox1.Text));
                map.SetEditMode(temp);
                if (comboBox2.Text != "")
                    map.SetBrushSize(Convert.ToInt32(comboBox2.Text));
            }
        }

        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox2.Text != "")
                map.SetBrushSize(Convert.ToInt32(comboBox2.Text));
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }
    }
}
