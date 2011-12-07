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
    public partial class Form1 : Form
    {
        private Z80 p;
        public Form1()
        {
            InitializeComponent();
            p = new Z80(0);        // 実行開始アドレス
        }

        //public /*private*/ void SetAndExecute(int address, byte[] mempart)
        //{
        //    p.Memset(address, mempart);
        //    do
        //    {
        //        p.Execute();
        //    } while (p.instruction != 0xff);
        //}

        public void TestExecute()
        {
            //var program = new byte[256];
            //UInt16 pc = 0x0000;
            //p.Reset(pc);    //Fもクリアされる
            //program[pc++] = 0x06; program[pc++] = 0x11; //LD B, 0x11
            //program[pc++] = 0x0e; program[pc++] = 0x22; //LD C, 0x22
            //program[pc++] = 0x16; program[pc++] = 0x33; //LD D, 0x33
            //program[pc++] = 0x1e; program[pc++] = 0x44; //LD E, 0x44
            //program[pc++] = 0x26; program[pc++] = 0x55; //LD H, 0x55
            //program[pc++] = 0x2e; program[pc++] = 0x66; //LD L, 0x66
            //program[pc++] = 0x3e; program[pc++] = 0x88; //LD A, 0x88
            //program[pc++] = 0xc5;   //PUSH BC
            //program[pc++] = 0xd5;   //PUSH DE
            //program[pc++] = 0xe5;   //PUSH HL
            //program[pc++] = 0xf5;   //PUSH AF

            //program[pc++] = 0xff;   // テスト実行を強制終了
            //p.SetAndExecute(0, program);

            var program = new byte[256];

            UInt16 pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる


            //CALL のテスト
            program[pc++] = 0xcd;   //CALL 0x0010
            program[pc++] = 0x10;
            program[pc++] = 0x00;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            pc = 0x0010;
            program[pc++] = 0xc9;   //RET
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            p.SetAndExecute(0, program);

            //JP nn のテスト
            program[pc++] = 0xc3;   //JP 0x0010
            program[pc++] = 0x10;
            program[pc++] = 0x00;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            pc = 0x0010;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)
            p.SetAndExecute(0, program);


            //pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる
            program[pc++] = 0x06; program[pc++] = 0x11; //LD B, 0x11
            program[pc++] = 0x0e; program[pc++] = 0x22; //LD C, 0x22
            program[pc++] = 0x16; program[pc++] = 0x33; //LD D, 0x33
            program[pc++] = 0x1e; program[pc++] = 0x44; //LD E, 0x44
            program[pc++] = 0x26; program[pc++] = 0x55; //LD H, 0x55
            program[pc++] = 0x2e; program[pc++] = 0x66; //LD L, 0x66
            program[pc++] = 0x3e; program[pc++] = 0x88; //LD A, 0x88
            program[pc++] = 0xc5;   //PUSH BC
            program[pc++] = 0xd5;   //PUSH DE
            program[pc++] = 0xe5;   //PUSH HL
            program[pc++] = 0xf5;   //PUSH AF
            program[pc++] = 0xaf;   //XOR   A
            program[pc++] = 0x47;   //LD B, A
            program[pc++] = 0x4f;   //LD C, A
            program[pc++] = 0x57;   //LD D, A
            program[pc++] = 0x5f;   //LD E, A
            program[pc++] = 0x67;   //LD H, A
            program[pc++] = 0x6f;   //LD L, A
            program[pc++] = 0xe1;   //POP HL
            program[pc++] = 0xd1;   //POP DE
            program[pc++] = 0xc1;   //POP BC
            program[pc++] = 0xf1;   //POP AF
            program[pc++] = 0xff;   // テスト実行を強制終了
            p.SetAndExecute(0, program);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            var pg = new byte[256];
             
            int pc = 0x0000;
            pg[pc++] = 0xcd;    //CALL  0x0006
            pg[pc++] = 0x06;    //
            pg[pc++] = 0x00;    //
            pg[pc++] = 0x3e;    //LD    A, 0x12
            pg[pc++] = 0x12;    //
            pg[pc++] = 0x67;    //LD    H, A        ;01 100 111
                                //0006:
            pg[pc++] = 0x04;    //INC   B
            pg[pc++] = 0x05;    //DEC   B
            pg[pc++] = 0x0C;    //INC   C
            pg[pc++] = 0x0D;    //DEC   C
            pg[pc++] = 0x14;    //INC   D
            pg[pc++] = 0x15;    //
            pg[pc++] = 0x1C;    //INC   E
            pg[pc++] = 0x1D;    //
            pg[pc++] = 0x24;    //INC   H
            pg[pc++] = 0x25;    //
            pg[pc++] = 0x2C;    //INC   L
            pg[pc++] = 0x2D;    //
            pg[pc++] = 0x34;    //INC   (HL)
            pg[pc++] = 0x35;    //DEC   (HL)
            pg[pc++] = 0x3C;    //INC   A
            pg[pc++] = 0x3D;    //DEC   A
            pg[pc++] = 0x3e;    //LD    A, 0x34
            pg[pc++] = 0x34;
            pg[pc++] = 0xc9;    //RET
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
            p.Memset(0, pg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            p.Execute();
            tbInstruction.Text = string.Format("0x{0:X2}", p.instruction);
            tbA.Text = string.Format("0x{0:X2}", p.A);
            tbB.Text = string.Format("0x{0:X2}", p.B);
            tbC.Text = string.Format("0x{0:X2}", p.C);
            tbD.Text = string.Format("0x{0:X2}", p.D);
            tbE.Text = string.Format("0x{0:X2}", p.E);
            tbH.Text = string.Format("0x{0:X2}", p.H);
            tbL.Text = string.Format("0x{0:X2}", p.L);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pg = new byte[256];

            int pc = 0x0000;
            pg[pc++] = 0xc3;    //JP    nn
            pg[pc++] = 0x00;    //JP    nn
            pg[pc++] = 0x00;    //JP    nn

            p.Memset(0, pg);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000*1000; i++)
            {
                p.Execute();
            }
            sw.Stop();

            var ips = 1000 * 1 * 1000 * 1000 / sw.ElapsedMilliseconds;
            Console.WriteLine(ips);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TestExecute();
        }
    }
}
