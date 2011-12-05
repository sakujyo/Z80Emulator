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
        //static UInt64 executedInstructions = 0;

        public void memset(int address, byte[] barray)
        {
            barray.CopyTo(mem, address);
        }

        public void Execute()
        {
            instruction = mem[PC++];
            executedInstructions++;     //おまけの統計
            //命令のデコード
            //ex)レジスタ<-即値間コピー、レジスタ-レジスタ間コピー、レジスタ-メモリ間コピー、
            //レジスタ<-即値間演算、レジスタ-レジスタ演算、レジスタ-メモリ間演算、

            //http://taku.izumisawa.jp/Msx/ktecho1.htm
            
            //LD    A, n
            if (instruction == 0x3e)
            {
                A = mem[PC++];
                return;
            }

            if (instruction == 0xc2)
            {
                // C2 JP    NZ, nn
                // C3 JP    nn
                if (flagZ && (instruction & 0x01) == 0) return;
                UInt16 addr = mem[PC++];
                addr |= (UInt16)(mem[PC++] << 8);
                PC = (UInt16)addr;
            }

            if ((instruction & 0xcb) == 0xc1)
            //11000001  C1
            //11010001  C5
            //11100001  D1
            //11110001  D5
            //11000101  E1
            //11010101  E5
            //11100101  F1
            //11110101  F5

            //11000001	1でなければならない
            //00110100	1か0かを問わない
            //11001011これとANDをとって
            //11000001これになればC1,C5,D1,D5,E1,E5,F1,F5のいずれか
            {
                //PUSH, POP
                pushpop(instruction);
            }

            if (instruction == 0xcd)
            {
                // CALL nn
                UInt16 addr = mem[PC++];
                addr |= (UInt16)(mem[PC++] << 8);
                mem[--SP] = (byte)((PC & 0xff00) >> 8);
                mem[--SP] = (byte)((PC & 0xff));
                PC = (UInt16)addr;
                //PC = (UInt16)(mem[PC] | (mem[PC + 1] << 8));
            }

            if (instruction == 0xc9)
            {
                // RET
                UInt16 addr = mem[SP++];
                addr |= (UInt16)(mem[SP++] << 8);
                PC = (UInt16)addr;
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
        }

        private void pushpop(byte instruction)
        {
            var qq = (instruction & 0x30) >> 4;    //0b00110000
            //(SP-1)←qqH
            //(SP-2)←qqL
            //SP←SP-2
            if ((instruction & 0x04) == 0x04)
            {
                //PUSH
                switch (qq)
                {
                    case 0:
                        mem[--SP] = B;
                        mem[--SP] = C;
                        break;
                    case 1:
                        mem[--SP] = D;
                        mem[--SP] = E;
                        break;
                    case 2:
                        mem[--SP] = H;
                        mem[--SP] = L;
                        break;
                    case 3:
                        mem[--SP] = A;
                        byte F = (byte)(
                                ((flagS ? 1 : 0) << 7) &
                                ((flagZ ? 1 : 0) << 6) &
                                ((flagH ? 1 : 0) << 4) &
                                ((flagP ? 1 : 0) << 2) &
                                ((flagN ? 1 : 0) << 1) &
                                ((flagC ? 1 : 0) << 0));
                        mem[--SP] = F;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //POP
                switch (qq)
                {
                    case 0:
                        C = mem[SP++];
                        B = mem[SP++];
                        break;
                    case 1:
                        E = mem[SP++];
                        D = mem[SP++];
                        break;
                    case 2:
                        L = mem[SP++];
                        H = mem[SP++];
                        break;
                    case 3:
                        byte F = mem[SP++];
                        flagS = (F & 0x80) == 0x80 ? true : false;
                        flagZ = (F & 0x40) == 0x40 ? true : false;
                        flagH = (F & 0x10) == 0x10 ? true : false;
                        flagP = (F & 0x04) == 0x04 ? true : false;
                        flagN = (F & 0x02) == 0x02 ? true : false;
                        flagC = (F & 0x01) == 0x01 ? true : false;
                        A = mem[SP++];
                        break;
                    default:
                        break;
                }
            }
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
            SP = 0x0000;
        }

        public int executedInstructions { get; set; }
    }
}
