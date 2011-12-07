using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ProcessorEmulator
{
    public class Z80
    {
        //private
        //byte A;
        public byte A { get; private set; }
        //byte F;     //S Z x H x P N C
        /// <summary>サイン。正なら0、負なら1。</summary>
        public bool flagS { get; private set; }
        public bool flagZ { get; private set; }
        /// <summary>ハーフキャリー。bit3からbit4への繰り上がり時に発生。</summary>
        public bool flagH { get; private set; }
        /// <summary>パリティ/オーバーフロー</summary>
        public bool flagP { get; private set; }
        /// <summary>加算命令後0、減算命令後1。</summary>
        public bool flagN { get; private set; }
        public bool flagC { get; private set; }
        public byte B { get; private set; }
        public byte C { get; private set; }
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte H { get; private set; }
        public byte L { get; private set; }
        /// <summary>
        /// フラグレジスタ。SZxHxPNCの順(上位から)。
        /// flagS: サイン。正なら0。負なら1。
        /// flagN: 加算命令後0。減算命令後1。
        /// flagH: ハーフキャリー。bit3からbit4への繰り上がり時に発生。
        /// </summary>
        public byte F
        {
            get
            {
                return (byte)(
                                ((flagS ? 1 : 0) << 7) |
                                ((flagZ ? 1 : 0) << 6) |
                                ((flagH ? 1 : 0) << 4) |
                                ((flagP ? 1 : 0) << 2) |
                                ((flagN ? 1 : 0) << 1) |
                                ((flagC ? 1 : 0) << 0));
            }
            private set
            {
                flagS = (value & 0x80) == 0x80 ? true : false;
                flagZ = (value & 0x40) == 0x40 ? true : false;
                flagH = (value & 0x10) == 0x10 ? true : false;
                flagP = (value & 0x04) == 0x04 ? true : false;
                flagN = (value & 0x02) == 0x02 ? true : false;
                flagC = (value & 0x01) == 0x01 ? true : false;
            }
            //public byte[] MEM {
            //    get { return mem; }
            //}

        }        
        /// <summary>実行中のインストラクション</summary>
        public byte instruction { get; private set; }

        public byte IXL { get; private set; }
        public byte IXH { get; private set; }
        public byte IYL { get; private set; }
        public byte IYH { get; private set; }
        public UInt16 SP { get; private set; }
        public UInt16 PC { get; private set; }

        /// <summary>主メモリ空間のサイズ</summary>
        const int MEMSIZE = 64 * 1024;  //64KB
        /// <summary>MEMSIZEの大きさのメモリ空間</summary>
        private byte[] mem;
        public ReadOnlyCollection<byte> PeepedMEM { get; private set; }
        //ちなみに初期化子は親クラスのコンストラクタの実行に先行するらしい
        //public Array mem { get; set; }
        //public byte[] MEM {
        //    get { return mem; }
        //}

        public Z80(UInt16 initialAddress)
        {
            //mem = new byte[MEMSIZE];
            //PeepedMEM = new ReadOnlyCollection<byte>(mem);
            
            ////Test Execute2
            ////mem[0] = 0xcd;


            //PC = initialAddress;
            //SP = 0x0000;
            //executedInstructions = 0;

            Reset(initialAddress);
            //IXL = 0;
            //IXH = 0;
            //IYL = 0;
            //IYH = 0;
        }

        public void Reset(UInt16 initialAddress)
        {
            mem = null;     // 意味あります？      //->この場合は意味ない
            //GC.Collect（）；GC.WaitForPendingFinalizers();GC.Collect()
            mem = new byte[MEMSIZE];
            PeepedMEM = new ReadOnlyCollection<byte>(mem);

            PC = initialAddress;
            SP = 0x0000;
            executedInstructions = 0;
            A = 0;
            B = 0;
            C = 0;
            D = 0;
            E = 0;
            H = 0;
            L = 0;
            flagS = false;
            flagZ = false;
            flagH = false;
            flagP = false;
            flagN = false;
            flagC = false;
        }

        /// <summary>命令実行数統計</summary>
        public int executedInstructions { get; private set; }

        /// <summary>
        /// メモリの領域にデータをコピーします。
        /// </summary>
        /// <param name="address">コピー先のアドレス</param>
        /// <param name="barray">コピーするデータ</param>
        public void Memset(int address, byte[] barray)
        {
            barray.CopyTo(mem, address);
        }

        /// <summary>
        /// メモリにプログラムデータをコピーした上で、addressから実行を開始します。
        /// </summary>
        /// <param name="address">プログラムの開始番地</param>
        /// <param name="mempart">プログラムデータ</param>
        public /*private*/ void SetAndExecute(int address, byte[] mempart)
        {
            PC =(UInt16)address;
            Memset(address, mempart);
            do
            {
                Execute();
            } while (instruction != 0xff);
        }

        public int Execute2()
        {
            //CPUの状態を入力とし、CPUの状態を出力とするようなサンプルのスタブ
            //入力するCPU状態:
            //レジスタ(の一部)の値
            //メモリ(の一部)の値
            //入力ポート(の一部)から入力を実行した時の値
            //出力するCPU状態:
            //変化したレジスタの(レジスタ番号と)値
            //変化したメモリの(アドレスと)値
            //出力ポートへ出力された(ポート番号と順序と)値
            //レジスタ番号:
            //B:0
            //C:1
            //D:2
            //E:3
            //H:4
            //L:5
            //(HL):6
            //A:7
            //PCH: 8,	PCL: 9
            //SPH: 10,	SPL: 11
            //IXH	IXL
            //IYH	IYL
            var r = new List<System.Tuple<UInt16, Byte>>();
            var m = new List<System.Tuple<UInt16, Byte>>(); //Tupleではなく状態オブジェクトで表せる？
            //TupleだとTest側でリテラルを書けない？
            return 1234;
        }

        public void Execute()
        {
            instruction = mem[PC++];
            executedInstructions++;     //おまけの統計
            //命令のデコード
            //ex)レジスタ<-即値間コピー、レジスタ-レジスタ間コピー、レジスタ-メモリ間コピー、
            //レジスタ<-即値間演算、レジスタ-レジスタ演算、レジスタ-メモリ間演算、

            //http://taku.izumisawa.jp/Msx/ktecho1.htm

            if ((instruction & 0xc6) == 0x04)
            //00000100
            //00000101
            //00010100
            //00010101
            //00100100
            //00100101
            //00110100
            //00110101
            //00001100
            //00001101
            //00011100
            //00011101
            //00101100
            //00101101
            //00111100	INC A
            //00111101	DEC A

            //00xxx10x
            //00111000	レジスタr
            //00000001	INCかDECか
            //11000110これ(0でなければならないbitと1であることを確かめたいbit)とANDをとって
            //00000100これになれば8bitINC,DEC
            {
                //8bit INC or DEC
                INCDEC8bitAndFlag(instruction);
                return;
            }
            
            ////LD    A, n
            //if (instruction == 0x3e)
            //{
            //    A = mem[PC++];
            //    return;
            //}

            if (instruction == 0xc2)
            {
                // C2 JP    NZ, nn
                // C3 JP    nn
                if (flagZ && (instruction & 0x01) == 0) return;
                UInt16 addr = mem[PC++];
                addr |= (UInt16)(mem[PC++] << 8);
                PC = (UInt16)addr;
                //return;
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
                return;
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
                //return;
            }

            if (instruction == 0xc9)
            {
                // RET
                UInt16 addr = mem[SP++];
                addr |= (UInt16)(mem[SP++] << 8);
                PC = (UInt16)addr;
                //return;
            }

            if ((instruction & 0xc0) == 0x40)
            {
                //LD  r,r'
                load8bitrr(instruction);
                //return;
            }
            if ((instruction & 0xc0) == 0x80)
            {
                //ADD, ADC, SUB, SBC, AND, OR, XOR, CPのいずれか
                accumulate(instruction);
                //return;
            }
            //switch (op)
            //{
            //    case 0x40:  //レジスタ間ロード(コピー)命令
            //        load8bitrr(instruction);
            //        break;
            //    case 0x80:  //演算命令
            //        accumulate(instruction);
            //        break;
            //    default:
            //        break;
            //}
            //return;

            if ((instruction & 0xc7) == 0x06)
            //00000110
            //00001110
            //00010110
            //00011110
            //00100110
            //00101110
            //00110110
            //00111110

            //00xxx110
            //11000111これとANDをとって
            //00000110これになるのがLD r, n            
            {
                //LD    r, n
                // レジスタr : オペランド
                int r = (instruction & 0x38) >> 3;     // 0x38 = 0b00111000
                switch (r)
                {
                    case 0:
                        B = mem[PC++];
                        break;
                    case 1:
                        C = mem[PC++];
                        break;
                    case 2:
                        D = mem[PC++];
                        break;
                    case 3:
                        E = mem[PC++];
                        break;
                    case 4:
                        H = mem[PC++];
                        break;
                    case 5:
                        L = mem[PC++];
                        break;
                    case 6:
                        //36 LD (HL),n
                        mem[(((UInt16)H) << 8) + L] = mem[PC++];
                        break;
                    case 7:
                        A = mem[PC++];
                        break;
                    default:
                        break;
                }
                return;
            }
        }

        private void INCDEC8bitAndFlag(byte instruction)
        {
            //8bit INC or DEC
            // レジスタr : オペランド
            int r = (instruction & 0x38) >> 3;     // 0x38 = 0b00111000

            //byte incdec = 0xff;
            //if ((instruction & 0x01) == 0x00) incdec += 2;
            byte incdec;
            if ((instruction & 0x01) == 0x00)
            {
                incdec = 1;
            }
            else
            {
                incdec = 0xff;
            }
            byte data;

            switch (r)
            {
                case 0:
                    data = B += incdec;
                    break;
                case 1:
                    data = C += incdec;
                    break;
                case 2:
                    data = D += incdec;
                    break;
                case 3:
                    data = E += incdec;
                    break;
                case 4:
                    data = H += incdec;
                    break;
                case 5:
                    data = L += incdec;
                    break;
                case 6:
                    //34 INC (HL)
                    //35 DEC (HL)
                    data = mem[(((UInt16)H) << 8) + L];
                    //var data = mem[H << 8 | L];
                    data += incdec;
                    mem[(((UInt16)H) << 8) + L] = data;
                    break;
                case 7:
                    data = A += incdec;
                    break;
                default:
                    data = 0;
                    break;
            }

            flagZ = data == 0x00 ? true : false;
            flagS = data >= 0x80 ? true : false;
            if ((instruction & 0x01) == 0x00)
            {
                flagH = (data & 0x0f) == 0x00 ? true : false;
                flagN = false;
                flagP = data == 0x00 ? true : false;   // over flow
                //キャリーは変化しない。
            }
            else
            {
                flagH = (data & 0x0f) == 0x0f ? true : false;
                flagN = true;
                flagP = data == 0xff ? true : false;   // over flow
                //キャリーは変化しない。
            }
        }

        private void pushpop(byte instruction)
        {
            var qq = (instruction & 0x30) >> 4;    //0b00110000
            //(SP-1)←qqH
            //(SP-2)←qqL
            //SP←SP-2
            //if ((instruction & 0x04) == 0x04)
            if ((instruction & 0xcf) == 0xc5)
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
                        //byte F = (byte)(
                        //        ((flagS ? 1 : 0) << 7) &
                        //        ((flagZ ? 1 : 0) << 6) &
                        //        ((flagH ? 1 : 0) << 4) &
                        //        ((flagP ? 1 : 0) << 2) &
                        //        ((flagN ? 1 : 0) << 1) &
                        //        ((flagC ? 1 : 0) << 0));
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
                        //byte F = mem[SP++];
                        F = mem[SP++];
                        //flagS = (F & 0x80) == 0x80 ? true : false;
                        //flagZ = (F & 0x40) == 0x40 ? true : false;
                        //flagH = (F & 0x10) == 0x10 ? true : false;
                        //flagP = (F & 0x04) == 0x04 ? true : false;
                        //flagN = (F & 0x02) == 0x02 ? true : false;
                        //flagC = (F & 0x01) == 0x01 ? true : false;
                        A = mem[SP++];  //TODO: 確認。AFレジスタはAが上位8ビットだと思う
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
    }
}
