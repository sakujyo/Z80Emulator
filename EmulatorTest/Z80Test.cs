using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProcessorEmulator;

namespace EmulatorTest
{
    [TestFixture]
    public class Z80Test
    {
        //副作用をテストしたいときはどうしましょうか。。。
        //        member x.measureCPUTime (func : obj * Constraints.IResolveConstraint * string -> unit) (lazyarg : Lazy<'a>) arg2 arg3 =
        //            let cp = System.Diagnostics.Process.GetCurrentProcess()
        //            let t1 = cp.TotalProcessorTime
        ////            printfn "CPU Time 1: %A" (t1)
        //            func (lazyarg.Force(), arg2, arg3)
        //            let t2 = cp.TotalProcessorTime
        ////            printfn "CPU Time 2: %A" (t2)
        //            printfn "%s" arg3
        //            printfn "Test CPU Time: %A" (t2 - t1)

        //        static member TestData0000 =
        //            [|
        ////                [| 1, 1234 |];
        //                [| 2, 2468 |];
        //            |]
        //        [<Test;Description("Problem 0 Test")>]
        //        [<TestCaseSource("TestData0000")>]
        //        member x.Problem_0000 (data : int * int) =
        //            let input, result = data in
        //            x.measureCPUTime Assert.That (lazy(PEFS.Problem_0000.run input)) (Is.EqualTo(result)) (sprintf "1234 * %A = %A" input result)
        ////            Assert.That (PEFS.Problem_0000.run input, Is.EqualTo(result), sprintf "1234 * %A = %A" input result)
        private Z80 p;
        private VDP v;

        //private void SetAndExecute(int address, byte[] mempart)
        //{
        //    p.Memset(address, mempart);
        //    do
        //    {
        //        p.Execute();
        //    } while (p.instruction != 0xff);
        //}

        [Test, Description("Load8bitImmediate Test")]
        public void TestLoad8bitImmediate()
        {
            var program = new byte[256];
            UInt16 pc = 0x0000;
            p.Reset(pc);
            program[pc++] = 0x06; program[pc++] = 0x11;
            program[pc++] = 0x0e; program[pc++] = 0x22;
            program[pc++] = 0x16; program[pc++] = 0x33;
            program[pc++] = 0x1e; program[pc++] = 0x44;
            program[pc++] = 0x26; program[pc++] = 0x55;
            program[pc++] = 0x2e; program[pc++] = 0x66;
            program[pc++] = 0x36; program[pc++] = 0x77;
            program[pc++] = 0x3e; program[pc++] = 0x88;
            program[pc++] = 0xff;   // テスト実行を強制終了
            p.SetAndExecute(0, program);
            Assert.That(p.B, Is.EqualTo(0x11), "TEST: LD B, 0x11");
            Assert.That(p.C, Is.EqualTo(0x22), "TEST: LD C, 0x22");
            Assert.That(p.D, Is.EqualTo(0x33), "TEST: LD D, 0x33");
            Assert.That(p.E, Is.EqualTo(0x44), "TEST: LD E, 0x44");
            Assert.That(p.H, Is.EqualTo(0x55), "TEST: LD H, 0x55");
            Assert.That(p.L, Is.EqualTo(0x66), "TEST: LD L, 0x66");
            Assert.That(p.PeepedMEM[0x5566], Is.EqualTo(0x77), "TEST: LD (HL), 0x77");
            Assert.That(p.A, Is.EqualTo(0x88), "TEST: LD A, 0x88");
            //p.Execute2();
            //(p.PeepedMEM)[0] = 0;
            //Assert.That((p.PeepedMEM)[0], Is.EqualTo(0xcd), "Test Execute2");
            //Assert.That((p.PeepedMEM)[0], Is.EqualTo(0x00), "Test Execute2");
            //Assert.That(p.Execute2(), Is.EqualTo(1234), "Test Execute2");
            //Assertion.AssertEquals("NUnit Test on CSharp", 1234, p.Execute2());
            //Assert.That (/*PEFS.Problem_0000.run */a, NUnit.Framework.Is.EqualTo(b), "1234 * A = A");
        }

        [Test, Description("INC DEC 8bitREGS Test")]
        public void TestINCDEC8BitREGS()
        {
            var pg = new byte[256];
            UInt16 pc = 0x0000;
            p.Reset(pc);
            pg[pc++] = 0x04;    //INC   B
            pg[pc++] = 0x0C;    //INC   C
            pg[pc++] = 0x14;    //INC   D
            pg[pc++] = 0x1C;    //INC   E
            pg[pc++] = 0x24;    //INC   H
            pg[pc++] = 0x2C;    //INC   L
            pg[pc++] = 0x34;    //INC   (HL)
            pg[pc++] = 0x3e;    //LD    A, 0xff
            pg[pc++] = 0xff;    //LD    A, 0xff
            pg[pc++] = 0x3C;    //INC   A
            pg[pc++] = 0xff;    //RST   38H
            p.SetAndExecute(0, pg);
            Assert.That(p.B, Is.EqualTo(0x01), "TEST: INC B");
            Assert.That(p.C, Is.EqualTo(0x01), "TEST: INC C");
            Assert.That(p.D, Is.EqualTo(0x01), "TEST: INC D");
            Assert.That(p.E, Is.EqualTo(0x01), "TEST: INC E");
            Assert.That(p.H, Is.EqualTo(0x01), "TEST: INC H");
            Assert.That(p.L, Is.EqualTo(0x01), "TEST: INC L");
            Assert.That(p.PeepedMEM[0x0101], Is.EqualTo(0x01), "TEST: INC (HL)");
            Assert.That(p.A, Is.EqualTo(0x00), "TEST: INC A");
            //フラグの不定についてはマスクをかけてAssertすればいいのでは
            Assert.That(p.F & 0xd7, Is.EqualTo(0x54), "TEST: INC A(FLAG VALIDATION)");
            //01x1x100(SZxHxPNC)= F & 0xd7 = 0x54

            pc = 0x0000;
            p.Reset(pc);
            pg[pc++] = 0x05;    //DEC   B
            pg[pc++] = 0x0D;    //DEC   C
            pg[pc++] = 0x15;    //DEC   D
            pg[pc++] = 0x1D;    //DEC   E
            pg[pc++] = 0x25;    //DEC   H
            pg[pc++] = 0x2D;    //DEC   L
            pg[pc++] = 0x35;    //DEC   (HL)
            pg[pc++] = 0x3D;    //DEC   A
            pg[pc++] = 0xff;   // テスト実行を強制終了
            p.SetAndExecute(0, pg);
            Assert.That(p.B, Is.EqualTo(0xff), "TEST: DEC B");
            Assert.That(p.C, Is.EqualTo(0xff), "TEST: DEC C");
            Assert.That(p.D, Is.EqualTo(0xff), "TEST: DEC D");
            Assert.That(p.E, Is.EqualTo(0xff), "TEST: DEC E");
            Assert.That(p.H, Is.EqualTo(0xff), "TEST: DEC H");
            Assert.That(p.L, Is.EqualTo(0xff), "TEST: DEC L");
            Assert.That(p.PeepedMEM[0xffff], Is.EqualTo(0xff), "TEST: DEC (HL)");
            Assert.That(p.A, Is.EqualTo(0xff), "TEST: DEC A");

            Assert.That(p.F & 0xd7, Is.EqualTo(0x96), "TEST: INC A(FLAG VALIDATION)");
            //10x1x110(SZxHxPNC)= F & 0xd7 = 0x96
        }

        [Test, Description("PUSH POP Test")]
        public void TestPUSHPOP()
        {
            var program = new byte[256];

            //PUSH のテスト
            UInt16 pc = 0x0000;
            p.Reset(pc);    //Fもクリアされる
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
            
            program[pc++] = 0xff;   // テスト実行を強制終了
            p.SetAndExecute(0, program);
            Assert.That(p.PeepedMEM[0xffff], Is.EqualTo(0x11), "TEST: PUSH BC(B)");
            Assert.That(p.PeepedMEM[0xfffe], Is.EqualTo(0x22), "TEST: PUSH BC(C)");
            Assert.That(p.PeepedMEM[0xfffd], Is.EqualTo(0x33), "TEST: PUSH DE(D)");
            Assert.That(p.PeepedMEM[0xfffc], Is.EqualTo(0x44), "TEST: PUSH DE(E)");
            Assert.That(p.PeepedMEM[0xfffb], Is.EqualTo(0x55), "TEST: PUSH HL(H)");
            Assert.That(p.PeepedMEM[0xfffa], Is.EqualTo(0x66), "TEST: PUSH HL(L)");
            Assert.That(p.PeepedMEM[0xfff9], Is.EqualTo(0x88), "TEST: PUSH AF(A)");
            Assert.That(p.PeepedMEM[0xfff8], Is.EqualTo(0x00), "TEST: PUSH AF(F)");

            //POP のテスト
            pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる
            program[pc++] = 0x06; program[pc++] = 0x11; //LD B, 0x11
            program[pc++] = 0x0e; program[pc++] = 0xff; //LD C, 0xff
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
            Assert.That(p.L, Is.EqualTo(0x00), "TEST: POP HL(L)");
            Assert.That(p.H, Is.EqualTo(0x88), "TEST: POP HL(H)");
            Assert.That(p.E, Is.EqualTo(0x66), "TEST: POP DE(E)");
            Assert.That(p.D, Is.EqualTo(0x55), "TEST: POP DE(D)");
            Assert.That(p.C, Is.EqualTo(0x44), "TEST: POP BC(C)");
            Assert.That(p.B, Is.EqualTo(0x33), "TEST: POP BC(B)");
            Assert.That(p.F, Is.EqualTo(0xd7), "TEST: POP AF(F)");
            Assert.That(p.A, Is.EqualTo(0x11), "TEST: POP AF(A)");

        }

        [Test, Description("JP Test")]
        public void TestJp()
        {
            var program = new byte[256];

            //JP nn のテスト
            UInt16 pc = 0x0000;
            p.Reset(pc);    //Fもクリアされる
            program[pc++] = 0xc3;   //JP 0x0010
            program[pc++] = 0x10;
            program[pc++] = 0x00;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            pc = 0x0010;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)
            p.SetAndExecute(0, program);
            Assert.That(p.PC, Is.EqualTo(0x0011), "TEST: JP 0x0010");
        }

        [Test, Description("CALL RET Test")]
        public void TestCallRet()
        {
            //CALL RET のテスト
            var program = new byte[256];

            //CALL のテスト
            UInt16 pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる
            program[pc++] = 0xcd;   //CALL 0x0010
            program[pc++] = 0x10;
            program[pc++] = 0x00;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            pc = 0x0010;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)

            p.SetAndExecute(0, program);
            Assert.That(p.PeepedMEM[0xffff], Is.EqualTo(0x00), "TEST: CALL nn(ret PC high)");
            Assert.That(p.PeepedMEM[0xfffe], Is.EqualTo(0x03), "TEST: CALL nn(ret PC low)");
            Assert.That(p.SP, Is.EqualTo(0xfffe), "TEST: CALL 0x0010(SP)");
            Assert.That(p.PC, Is.EqualTo(0x0011), "TEST: CALL 0x0010(PC)");


            //RET のテスト
            pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる
            program[pc++] = 0xcd;   //CALL 0x0010
            program[pc++] = 0x10;
            program[pc++] = 0x00;
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            pc = 0x0010;
            program[pc++] = 0xc9;   //RET
            program[pc++] = 0xff;   // テスト実行を強制終了(ここを通らないことを確認する)

            p.SetAndExecute(0, program);
            //Assert.That(p.PeepedMEM[0xffff], Is.EqualTo(0x00), "TEST: CALL nn(ret PC high)");
            //Assert.That(p.PeepedMEM[0xfffe], Is.EqualTo(0x03), "TEST: CALL nn(ret PC low)");
            Assert.That(p.SP, Is.EqualTo(0x0000), "TEST: CALL 0x0010(SP)");
            Assert.That(p.PC, Is.EqualTo(0x0004), "TEST: CALL 0x0010(PC)");
        }

        [Test, Description("OUT Test 1(VDP Pixel Write)")]
        public void TestOutVDPPixelWrite()
        {
            var program = new byte[256];

            //OUT(VDP Pixel Write) のテスト
            UInt16 pc = 0x0000;
            p.Reset(pc);    //F, SPもクリアされる
            //program[pc++] = 0x3e;   //LD    A, 0x02;
            //program[pc++] = 0x00;   //(VDP Reset Command)
            //program[pc++] = 0xd3;   //OUT   n. A
            //program[pc++] = 0x00;   //PORT 0x03: VDP Command Port

            program[pc++] = 0x3e;   //LD    A, 0x00;
            program[pc++] = 0x00;   //(Destination Address(VRAM))
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x04;   //PORT 0x04: Destination Address 0(VRAM)
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x05;   //PORT 0x05: Destination Address 1(VRAM)
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x06;   //PORT 0x06: Destination Address 2(VRAM)
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x07;   //PORT 0x07: Destination Address 3(VRAM)

            program[pc++] = 0x3e;   //LD    A, 0xff;
            program[pc++] = 0xff;   //Pixel Data
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x01;   //PORT 0x01: Pixel Data
            program[pc++] = 0x3e;   //LD    A, 0x02;
            program[pc++] = 0x02;   //(Pixel Write Command)
            program[pc++] = 0xd3;   //OUT   n. A
            program[pc++] = 0x00;   //PORT 0x00: VDP Command Port
            program[pc++] = 0xff;   // テスト実行を強制終了(ここで終了することを確認する)
            
            p.SetAndExecute(0, program);
            Assert.That(v.PeepedVRAM[0], Is.EqualTo(0xff), "TEST: OUT Test 1(VDP Pixel Write)");
        }

        [SetUp]
        public void Init()
        {
            p = new Z80(0);
            v = new VDP(p, 256 * 1024);
            var m = new byte[0x10000];
            p.Memset(0, m);
            p.devNotify += v.Accept;

            //System.IO.StreamWriter writer = new System.IO.StreamWriter(@"d:\t\$$$.txt");
            //try
            //{
            //    writer.WriteLine("sample");
            //}
            //finally
            //{
            //    writer.Close();
            //}
        }

        [TearDown]
        public void Destroy()
        {
            //System.IO.File.Delete(@"d:\t\$$$.txt");
        }

        //[Test]
        //public void FileCheckSample()
        //{
        //    System.IO.StreamReader reader = new System.IO.StreamReader(@"c:\test$$$.txt");
        //    try
        //    {
        //        //Assertion.AssertEquals("テストファイルの確認", "sample", reader.ReadLine());
        //    }
        //    finally
        //    {
        //        reader.Close();
        //    }
        //}
    }
}