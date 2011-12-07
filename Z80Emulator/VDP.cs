using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ProcessorEmulator
{
    public enum ScreenMode
    {
        One,
        Two,
        Three
    }

    public class VDP
    {
        /// <summary>ビデオメモリ空間のサイズ</summary>
        public UInt32 vramsize { get; private set; }
        //const int VRAMSIZE = 256 * 1024;  //256KB
        /// <summary>VRAMSIZEの大きさのビデオメモリ空間</summary>
        private byte[] vram;
        private CPU p;
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


        public ScreenMode screenMode { get; protected set; }


        public VDP(CPU p, UInt32 vramsize)
        {
            this.p = p;
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
            screenMode = ScreenMode.One;
        }

        public void Accept(byte port, byte data)
        {
            //OUT された1バイトの受理
            //Console.WriteLine("VDP OUT Accepted: {0} <- {1}", port, data);
            switch (port)
            {
                case 0:
                    UInt32 address;
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
                            address = (UInt32)((address3 << 24) | (address2 << 16) | (address1 << 8) | address0);
                            vram[address++] = register1;
                            address0 = (byte)(address & 0x000000ff);    address >>= 8;
                            address1 = (byte)(address & 0x000000ff);    address >>= 8;
                            address2 = (byte)(address & 0x000000ff);    address >>= 8;
                            address3 = (byte)(address & 0x000000ff);
                            
                            break;
                        case 3:
                            //DMA Block Transfer
                            address = (UInt32)((address3 << 24) | (address2 << 16) | (address1 << 8) | address0);
                            UInt16 source = (UInt16)((sourceAddressH << 8) | sourceAddressL);
                            //矩形ブロック転送なので、転送サイズ上位下位各4ビットは0で1を意味する(最大16)
                            var height = 1 + ((register1 >> 4) & 0x0f);
                            var width = 1 + (register1 & 0x0f);
                            for (int y = 0; y < height; y++) {
                                for (int x = 0; x < width; x++) {
                                    if ((address / screenWidth(screenMode)) != ((address + x) / screenWidth(screenMode))) continue; //右端から左端にはみ出る場合はスキップ
                                    vram[address + y * screenWidth(screenMode) + x] = p.PeepedMEM[source + y * width + x];
                                }
                            }
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

        public uint screenWidth(ScreenMode screenMode)
        {
            switch (screenMode)
            {
                case ScreenMode.One:
                    return 256;
                case ScreenMode.Two:
                    return 512;
                case ScreenMode.Three:
                    return 512;
                default:
                    return 512;
            }
        }

        public void Refresh()   // Reflect, Accept
        {
        }
    }
}
