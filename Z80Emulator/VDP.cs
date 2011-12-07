using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ProcessorEmulator
{
    public class VDP
    {
        /// <summary>ビデオメモリ空間のサイズ</summary>
        public UInt32 vramsize { get; private set; }
        //const int VRAMSIZE = 256 * 1024;  //256KB
        /// <summary>VRAMSIZEの大きさのビデオメモリ空間</summary>
        private byte[] vram;
        public ReadOnlyCollection<byte> PeepedVRAM { get; private set; }

        //private UInt32 current;
        public byte commandState { get; private set; }

        public byte register1 { get; private set; }
        public byte sourceAddressL { get; private set; }
        public byte sourceAddressH { get; private set; }
        public byte address0 { get; private set; }
        public byte address1 { get; private set; }
        public byte address2 { get; private set; }
        public byte address3 { get; private set; }


        public VDP(UInt32 vramsize)
        {
            Reset(vramsize);
        }

        public void Reset(UInt32 vramsize)
        {
            vram = new byte[vramsize];
            PeepedVRAM = new ReadOnlyCollection<byte>(vram);
            this.vramsize = vramsize;
            address0 = 0;
            address1 = 0;
            address2 = 0;
            address3 = 0;

            //current = 0x00000000;
        }

        public void Accept(byte port, byte data)
        {
            //OUT された1バイトの受理
            //Console.WriteLine("VDP OUT Accepted: {0} <- {1}", port, data);
            switch (port)
            {
                case 0:
                    switch (data)
                    {
                        case 0:
                            //コマンドの受理開始
                            commandState = data;
                            //TODO: VDP Reset
                            //Reset(256 * 1024);
                            break;
                        case 2:
                            //Pixel Write
                            commandState = data;
                            UInt32 address = (UInt32)((address3 << 24) | (address2 << 16) | (address1 << 8) | address0);
                            vram[address++] = register1;
                            address0 = (byte)(address & 0x000000ff);    address >>= 8;
                            address1 = (byte)(address & 0x000000ff);    address >>= 8;
                            address2 = (byte)(address & 0x000000ff);    address >>= 8;
                            address3 = (byte)(address & 0x000000ff);
                            
                            break;
                        default:
                            break;
                    }
                    break;
                case 1:
                    //Pixel DataまたはDMA転送バイト数の設定
                    register1 = data;
                    break;
                case 2:
                    //DMA転送元アドレス下位指定
                    sourceAddressL = data;
                    break;
                case 3:
                    //DMA転送元アドレス上位指定
                    sourceAddressH = data;
                    break;
                case 4:
                    //VRAMアドレス 0指定
                    address0 = data;
                    break;
                case 5:
                    address1 = data;
                    break;
                case 6:
                    address2 = data;
                    break;
                case 7:
                    address3 = data;
                    break;

                default:
                    break;
            }
        }

        public void Refresh()   // Reflect, Accept
        {
        }
    }
}
