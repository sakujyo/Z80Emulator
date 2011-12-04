using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z80Emulator
{
    class CPU
    {
        //private
        //byte A;
        public byte A { get; private set; }
        //byte F;     //S Z x H x P N C
        public bool flagS { get; private set; }
        public bool flagZ { get; private set; }
        public bool flagH { get; private set; }
        /// <summary>パリティ/オーバーフロー</summary>
        public bool flagP { get; private set; }
        public bool flagN { get; private set; }
        public bool flagC { get; private set; }
        public byte B { get; private set; }
        public byte C { get; private set; }
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte H { get; private set; }
        public byte L { get; private set; }
        /// <summary>実行中のインストラクション</summary>
        public byte instruction { get; private set; }
        //bool flagS;
        //bool flagZ;
        //bool flagH;
        //bool flagP;
        //bool flagN;
        //bool flagC;
        //byte B;
        //byte C;
        //byte D;
        //byte E;
        //byte H;
        //byte L;

        byte IXL;
        byte IXH;
        byte IYL;
        byte IYH;
        UInt16 SP;
        UInt16 PC;

        /// <summary>主メモリ空間のサイズ</summary>
        const int MEMSIZE = 64 * 1024;  //64KB
        /// <summary>MEMSIZEの大きさのメモリ空間</summary>
        byte[] mem = new byte[MEMSIZE];

        /// <summary>命令実行数統計</summary>
        static UInt64 executedInstructions = 0;

        public void memset(int address, byte[] barray)
        {
            barray.CopyTo(mem, address);
        }

        public void Execute()
        {
            instruction = mem[PC++];
            //命令のデコード
            //ex)レジスタ<-即値間、レジスタ-レジスタ間コピー、レジスタ-メモリ間コピー、
            //レジスタ<-即値間演算、レジスタ-レジスタ演算、

            //LD    A, n
            if (instruction == 0x3e)
            {
                A = mem[PC++];
                executedInstructions += 2;
                return;
            }
            
            //LD  r,r'の判定と実行
            var op = instruction & 0xC0;
            switch (op)
            {
                case 0x40:  //レジスタ間ロード(コピー)命令
                    load8bitrr(instruction);
                    break;
                case 0x80:  //演算命令
                    accumulate(instruction);
                    break;
                default:
                    break;
            }
            //if (op == 0x40)
            //{
            //    load8bitrr(instruction);
            //}

            executedInstructions++;     //おまけの統計
        }

        private void accumulate(byte instruction)
        {
            //Aレジスタとの演算対象のレジスタ(bit0, bit1, bit2)
            int arg = instruction & 0x07;               // 0x07 = 0b00000111
            //演算の種類(bit3, bit4, bit5)
            int operation = (instruction & 0x38) >> 3;  // 0x38 = 0b00111000
            byte value;
            switch (arg)
            {
                case 0x0:
                    value = B;
                    break;
                case 0x1:
                    value = C;
                    break;
                case 0x2:
                    value = D;
                    break;
                case 0x3:
                    value = E;
                    break;
                case 0x4:
                    value = H;
                    break;
                case 0x5:
                    value = L;
                    break;
                case 0x6:   //(HL)
                    //data = mem[H * 0x100 + L];
                    value = mem[((UInt16)H) << 8 + L];
                    break;
                case 0x7:
                    value = A;
                    break;
                default:
                    value = 0;
                    break;
            }

            //各種演算
            //Int16 v;
            switch (operation)
            {
                case 0x0:   // ADD
                    //UInt16 v = A; //かけ算考えるとこっちのがいい？
                    //v = (Int16)(A + value);
                    //A = (byte)(v & 0xff);
                    //flagC = (v & 0xff00) == 0;
                    A = arithAndFlag(A, A + value);
                    #region CarryOperationObsolateCode
                    //if ((v & 0xff00) != 0)
                    //{
                    //    setCarryFlag();
                    //}
                    //else
                    //{
                    //    resetCarryFlag();
                    //}
                    #endregion
                    break;
                case 0x1:   // ADC
                    A = arithAndFlag(A, A + value + (flagC ? 1 : 0));
                    //A = (byte)(v & 0xff);
                    //flagC = (v & 0xff00) == 0;
                    break;
                case 0x2:   // SUB
                    A = arithAndFlag(A, A - value);
                    //A = (byte)(v & 0xff);
                    //flagC = (v & 0xff00) == 0;
                    break;
                case 0x3:   // SBC
                    A = arithAndFlag(A, A - value - (flagC ? 1 : 0));
                    break;
                case 0x4:   // AND
                    A = logicalAndFlag(A, A & value);
                    break;
                case 0x5:   // XOR
                    A = logicalAndFlag(A, A ^ value);
                    break;
                case 0x6:   // OR
                    A = logicalAndFlag(A, A | value);
                    break;
                case 0x7:   // CP
                    arithAndFlag(A, A - value);
                    break;
                default:
                    break;
            }
        }

        private byte logicalAndFlag(byte A, int v)
        {
            flagC = false;
            flagZ = v == 0 ? true : false;
            flagS = (v & 0x80) != 0 ? true : false;   // S 結果が負数(-1～-128)なら1(演算結果の第7ビットのコピー)
            return (byte)(v & 0xff);
        }

        private byte arithAndFlag(byte r, int v)
        {
            //A = (byte)(v & 0xff);
            flagC = (v & 0xff00) == 0;
            flagZ = v == 0 ? true : false;
            //flagS = v < 0 ? true : false;           // 負で true?
            flagS = (v & 0x80) != 0 ? true : false;   // S 結果が負数(-1～-128)なら1(演算結果の第7ビットのコピー)
            //flagP パリティ/オーバーフロー    P/V  結果が2の補数として-128～+127を超えたら1
            flagP = ((r & 0x80) != 0) ^ ((v & 0x80) != 0);
            return (byte)(v & 0xff);
        }

        private void setCarryFlag()
        {
            //F |= 0x01;
            flagC = true;
        }

        private void resetCarryFlag()
        {
            //F &= 0xfe;
            flagC = false;
        }

        private void load8bitrr(byte instruction)
        {
            if (instruction == 0x76)
            {
                //HALT
                return;
            }
            //Bレジスタ＝000
            //Cレジスタ＝001
            //Dレジスタ＝010
            //Eレジスタ＝011
            //Hレジスタ＝100
            //Lレジスタ＝101
            //(HL)＝110
            //Aレジスタ＝111
            // レジスタr': ソース
            int rs = instruction & 0x07;            // 0x07 = 0b00000111
            // レジスタr : ディスティネーション
            int rd = (instruction & 0x38) >> 3;     // 0x38 = 0b00111000
            byte data;
            switch (rs)
            {
                case 0x0:
                    data = B;
                    break;
                case 0x1:
                    data = C;
                    break;
                case 0x2:
                    data = D;
                    break;
                case 0x3:
                    data = E;
                    break;
                case 0x4:
                    data = H;
                    break;
                case 0x5:
                    data = L;
                    break;
                case 0x6:   //(HL)
                    //data = mem[H * 0x100 + L];
                    data = mem[(((UInt16)H) << 8) + L];
                    break;
                case 0x7:
                    data = A;
                    break;
                default:
                    data = 0;
                    break;
            }

            switch (rd)
            {
                case 0x0:
                    B = data;
                    break;
                case 0x1:
                    C = data;
                    break;
                case 0x2:
                    D = data;
                    break;
                case 0x3:
                    E = data;
                    break;
                case 0x4:
                    H = data;
                    break;
                case 0x5:
                    L = data;
                    break;
                case 0x6:   //(HL)
                    //data = mem[H * 0x100 + L];
                    mem[(((UInt16)H) << 8) + L] = data;
                    break;
                case 0x7:
                    A = data;
                    break;
                default:
                    break;
            }
        }

        public CPU(UInt16 initialAddress)
        {
            PC = initialAddress;
        }
    }
}
