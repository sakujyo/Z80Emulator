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
            //pg[0] = 0x3e;       // XOR  A
            //pg[1] = 0x67;       // LD   H, A
            //pg[2] = 0x6F;       // LD   L, A
            //pg[3] = 0x7e;       // LD   A, (HL)


            //LD    A, 0
            int pc = 0x0000;
            pg[pc++] = 0x3e;
            pg[pc++] = 0x00;
            //LD    H, A    ;01100111
            pg[pc++] = 0x67;
            //LD    A, 0xf0
            pg[pc++] = 0x3e;
            pg[pc++] = 0xf0;
            //LD    L, A    ;01101111
            pg[pc++] = 0x6f;
            //LD    A, (HL)    ;01 111 110
            pg[pc++] = 0x7e;
            //LD    B, A    ;01 000 111
            pg[pc++] = 0x47;
            //LD    A, 0xf1
            pg[pc++] = 0x3e;
            pg[pc++] = 0xf1;
            //LD    L, A
            pg[pc++] = 0x6f;
            //LD    A, (HL)
            pg[pc++] = 0x7e;
            //SBC   A, B
            pg[pc++] = 0x98;

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
    }
}
