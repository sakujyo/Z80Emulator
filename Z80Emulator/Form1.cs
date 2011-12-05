using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Z80Emulator
{
    public partial class Form1 : Form
    {
        private CPU pu;
        public Form1()
        {
            InitializeComponent();
            pu = new CPU(0);        // 実行開始アドレス
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var pg = new byte[256];
             
            int pc = 0x0000;
            pg[pc++] = 0xcd;    //CALL  0x0000
            pg[pc++] = 0x06;    //
            pg[pc++] = 0x00;    //
            pg[pc++] = 0x3e;    //LD    A, 0x12
            pg[pc++] = 0x12;    //
            pg[pc++] = 0x67;    //LD    H, A        ;01 100 111
            pg[pc++] = 0x3e;    //LD    A, 0x34
            pg[pc++] = 0x34;
            pg[pc++] = 0xc9;
            pg[pc++] = 0x6f;    //LD    L, A        ;01 101 111
            pg[pc++] = 0xe5;    //PUSH  HL
            pg[pc++] = 0xaf;    //XOR   A
            pg[pc++] = 0x67;    //LD    H, A        ;01 100 111
            pg[pc++] = 0x6f;    //LD    L, A        ;01 101 111
            pg[pc++] = 0xe1;    //POP   HL

            pg[pc++] = 0x3e;    //LD    A, 0
            pg[pc++] = 0x00;
            pg[pc++] = 0x67;    //LD    H, A        ;01 100 111
            pg[pc++] = 0x3e;    //LD    A, 0xf0
            pg[pc++] = 0xf0;
            pg[pc++] = 0x6f;    //LD    L, A        ;01 101 111
            pg[pc++] = 0x7e;    //LD    A, (HL)     ;01 111 110
            pg[pc++] = 0x47;    //LD    B, A        ;01 000 111
            pg[pc++] = 0x3e;    //LD    A, 0xf1
            pg[pc++] = 0xf1;
            pg[pc++] = 0x6f;    //LD    L, A
            pg[pc++] = 0x7e;    //LD    A, (HL)
            pg[pc++] = 0x98;    //SBC   A, B

            pg[0x00f0] = 0x0a;    // DB   10
            pg[0x00f1] = 0x15;    // DB   20
            pu.memset(0, pg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pu.Execute();
            tbInstruction.Text = string.Format("0x{0:X2}", pu.instruction);
            tbA.Text = string.Format("0x{0:X2}", pu.A);
            tbB.Text = string.Format("0x{0:X2}", pu.B);
            tbC.Text = string.Format("0x{0:X2}", pu.C);
            tbD.Text = string.Format("0x{0:X2}", pu.D);
            tbE.Text = string.Format("0x{0:X2}", pu.E);
            tbH.Text = string.Format("0x{0:X2}", pu.H);
            tbL.Text = string.Format("0x{0:X2}", pu.L);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pg = new byte[256];

            int pc = 0x0000;
            pg[pc++] = 0xc3;    //JP    nn
            pg[pc++] = 0x00;    //JP    nn
            pg[pc++] = 0x00;    //JP    nn

            pu.memset(0, pg);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000*1000; i++)
            {
                pu.Execute();
            }
            sw.Stop();

            var ips = 1000 * 1 * 1000 * 1000 / sw.ElapsedMilliseconds;
            Console.WriteLine(ips);
        }
    }
}
