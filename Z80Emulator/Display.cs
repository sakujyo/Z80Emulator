using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessorEmulator
{
    public partial class Display : Form
    {
        private VDP v;
        private Bitmap screen;
        private Color[] palette;

        public Display(VDP v)
        {
            // コンストラクタ
            InitializeComponent();
            this.v = v;
            screen = new Bitmap(512, 384);
            pictureBox1.Image = screen;
            palette = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                int blue = i & 0x03;
                int green = (i >> 2) & 0x03;
                int red = (i >> 4) & 0x03;
                palette[i] = Color.FromArgb(red * 85, green * 85, blue * 85);
                //for (int r = 0; r < 4; r++)
                //{
                //    for (int g = 0; g < 4; g++)
                //    {
                //        for (int b = 0; b < 4; b++)
                //        {
                //            palette[
                //        }
                //    }
                //}
            }
        }

        public void ReDraw()
        {
            //var g = Graphics.FromImage(Screen);   // おまけ。
            for (int y = 0; y < 384; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    //var brightness = v.PeepedVRAM[y * 512 + x];
                    //var pixelcolor = Color.FromArgb(brightness, brightness, brightness);
                    screen.SetPixel(x, y, palette[v.PeepedVRAM[y * 512 + x]]);
                }
            }
            pictureBox1.Refresh();
        }

        private void Display_VisibleChanged(object sender, EventArgs e)
        {
            var a = Visible;
            if (Visible == false) ((Form1)Owner).stopReDraw();
        }
    }
}
