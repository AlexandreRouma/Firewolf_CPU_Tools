using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Firewolf_Emulator
{
    class Program
    {
        public static Int16 register_AR = 0;
        public static Int16 register_BR = 0;
        public static Int16 register_CR = 0;
        public static Int16 register_DR = 0;
        public static Int16 register_ER = 0;
        public static Int16 register_SP = 0;
        public static Int16 register_SS = 0;
        public static Int16 register_SL = 0;
        public static Int16 register_IR = 0;
        public static Int16 register_PC = 0;
        public static Int16 register_CM = 0;

        public static Int16 addr = 0;
        public static byte shift = 0;

        public static bool isHalted = false;

        public static bool tick;

        public static byte[] memory = new byte[0x10000];

        static void Main(string[] args)
        {
            new Thread(() => run_simulation("testfile.bin")).Start();
            Console.ReadLine();
        }

        public static void dump()
        {
            File.WriteAllBytes("ramdump.bin", Program.memory);
            debug_wnd.Log($"<== RAM DUMPED ==>\n");
        }


        static debug_window debug_wnd = new debug_window();

        static BinaryReader2 reader = new BinaryReader2(new MemoryStream(memory));

        static string filename = "";

        static void run_simulation(string _filename)
        {

            filename = _filename;


            new Thread(() => debug_wnd.ShowDialog()).Start();

            while (!debug_wnd.ready) ;

            debug_wnd.Log($"Firewolf Emulator v1.0\n----------------------\n\n");

            debug_wnd.Log("Initializing RAM...    ");
            for (int i = 0; i < 0x10000; i++)
            {
                memory[i] = 0x00;
            }
            debug_wnd.Log("Done.\n");

            debug_wnd.Log($"Loading {filename}... ");
            BinaryReader2 binary_file = new BinaryReader2(File.Open(filename, FileMode.Open));
            for (int i = 0; i < binary_file.BaseStream.Length; i++)
            {
                memory[i] = binary_file.ReadByte();
            }
            binary_file.Close();
            debug_wnd.Log("Done.\n");
            Thread.Sleep(2000);

            Timer t = new Timer(TimerTick, null, 0, 1);

            while (true)
            {
                while (!tick) Thread.Sleep(1);
                tick = false;

                switch (reader.ReadByte())
                {
                    // ------------------------- HLT -------------------------
                    case 0x00:
                        debug_wnd.checkBox1.Checked = true;
                        break;
                    // ------------------------- MOV -------------------------
                    case 0x01:
                        register_BR = register_AR;
                        register_PC++;
                        break;
                    case 0x02:
                        register_CR = register_AR;
                        register_PC++;
                        break;
                    case 0x03:
                        register_DR = register_AR;
                        register_PC++;
                        break;
                    case 0x04:
                        register_ER = register_AR;
                        register_PC++;
                        break;
                    case 0x05:
                        register_AR = register_BR;
                        register_PC++;
                        break;
                    case 0x06:
                        register_CR = register_BR;
                        register_PC++;
                        break;
                    case 0x07:
                        register_DR = register_BR;
                        register_PC++;
                        break;
                    case 0x08:
                        register_ER = register_BR;
                        register_PC++;
                        break;
                    case 0x09:
                        register_AR = register_CR;
                        register_PC++;
                        break;
                    case 0x0A:
                        register_BR = register_CR;
                        register_PC++;
                        break;
                    case 0x0B:
                        register_DR = register_CR;
                        register_PC++;
                        break;
                    case 0x0C:
                        register_ER = register_CR;
                        register_PC++;
                        break;
                    case 0x0D:
                        register_AR = register_DR;
                        register_PC++;
                        break;
                    case 0x0E:
                        register_BR = register_DR;
                        register_PC++;
                        break;
                    case 0x0F:
                        register_CR = register_DR;
                        register_PC++;
                        break;
                    case 0x10:
                        register_ER = register_DR;
                        register_PC++;
                        break;
                    case 0x11:
                        register_AR = register_ER;
                        register_PC++;
                        break;
                    case 0x12:
                        register_BR = register_ER;
                        register_PC++;
                        break;
                    case 0x13:
                        register_CR = register_ER;
                        register_PC++;
                        break;
                    case 0x14:
                        register_DR = register_ER;
                        register_PC++;
                        break;
                    case 0x15:
                        addr = reader.ReadInt16();
                        register_AR = (short)(memory[addr] << 8 | memory[addr + 1]);
                        register_PC += 3;
                        break;
                    case 0x16:
                        addr = reader.ReadInt16();
                        register_BR = (short)(memory[addr] << 8 | memory[addr + 1]);
                        register_PC += 3;
                        break;
                    case 0x17:
                        addr = reader.ReadInt16();
                        register_CR = (short)(memory[addr] << 8 | memory[addr + 1]);
                        register_PC += 3;
                        break;
                    case 0x18:
                        addr = reader.ReadInt16();
                        register_DR = (short)(memory[addr] << 8 | memory[addr + 1]);
                        register_PC += 3;
                        break;
                    case 0x19:
                        addr = reader.ReadInt16();
                        register_ER = (short)(memory[addr] << 8 | memory[addr + 1]);
                        register_PC += 3;
                        break;
                    case 0x1A:
                        addr = reader.ReadInt16();
                        memory[addr] = (byte)(register_AR >> 8);
                        memory[addr + 1] = (byte)register_AR;
                        register_PC += 3;
                        break;
                    case 0x1B:
                        addr = reader.ReadInt16();
                        memory[addr] = (byte)(register_BR >> 8);
                        memory[addr + 1] = (byte)register_BR;
                        register_PC += 3;
                        break;
                    case 0x1C:
                        addr = reader.ReadInt16();
                        memory[addr] = (byte)(register_CR >> 8);
                        memory[addr + 1] = (byte)register_CR;
                        register_PC += 3;
                        break;
                    case 0x1D:
                        addr = reader.ReadInt16();
                        memory[addr] = (byte)(register_DR >> 8);
                        memory[addr + 1] = (byte)register_DR;
                        register_PC += 3;
                        break;
                    case 0x1E:
                        addr = reader.ReadInt16();
                        memory[addr] = (byte)(register_ER >> 8);
                        memory[addr + 1] = (byte)register_ER;
                        register_PC += 3;
                        break;
                    case 0x1F:
                        register_AR = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x20:
                        register_BR = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x21:
                        register_CR = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x22:
                        register_DR = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x23:
                        register_ER = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x24:
                        register_SP = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x25:
                        register_SS = reader.ReadInt16();
                        register_PC += 3;
                        break;
                    case 0x26:
                        addr = reader.ReadInt16();
                        memory[addr] = reader.ReadByte();
                        register_PC += 4;
                        break;
                    // ------------------------- ADD -------------------------
                    case 0x27:
                        register_AR = (Int16)(register_AR + register_BR);
                        register_PC++;
                        break;
                    case 0x28:
                        register_AR = (Int16)(register_AR + register_CR);
                        register_PC++;
                        break;
                    case 0x29:
                        register_AR = (Int16)(register_AR + register_DR);
                        register_PC++;
                        break;
                    case 0x2A:
                        register_AR = (Int16)(register_AR + register_ER);
                        register_PC++;
                        break;
                    case 0x2B:
                        register_BR = (Int16)(register_BR + register_AR);
                        register_PC++;
                        break;
                    case 0x2C:
                        register_BR = (Int16)(register_BR + register_CR);
                        register_PC++;
                        break;
                    case 0x2D:
                        register_BR = (Int16)(register_BR + register_DR);
                        register_PC++;
                        break;
                    case 0x2E:
                        register_BR = (Int16)(register_BR + register_ER);
                        register_PC++;
                        break;
                    case 0x2F:
                        register_CR = (Int16)(register_CR + register_AR);
                        register_PC++;
                        break;
                    case 0x30:
                        register_CR = (Int16)(register_CR + register_BR);
                        register_PC++;
                        break;
                    case 0x31:
                        register_CR = (Int16)(register_CR + register_DR);
                        register_PC++;
                        break;
                    case 0x32:
                        register_CR = (Int16)(register_CR + register_ER);
                        register_PC++;
                        break;
                    case 0x33:
                        register_DR = (Int16)(register_DR + register_AR);
                        register_PC++;
                        break;
                    case 0x34:
                        register_DR = (Int16)(register_DR + register_BR);
                        register_PC++;
                        break;
                    case 0x35:
                        register_DR = (Int16)(register_DR + register_CR);
                        register_PC++;
                        break;
                    case 0x36:
                        register_DR = (Int16)(register_DR + register_ER);
                        register_PC++;
                        break;
                    case 0x37:
                        register_ER = (Int16)(register_ER + register_AR);
                        register_PC++;
                        break;
                    case 0x38:
                        register_ER = (Int16)(register_ER + register_BR);
                        register_PC++;
                        break;
                    case 0x39:
                        register_ER = (Int16)(register_ER + register_CR);
                        register_PC++;
                        break;
                    case 0x3A:
                        register_ER = (Int16)(register_ER + register_DR);
                        register_PC++;
                        break;
                    case 0x3B:
                        register_AR = (Int16)(register_AR + reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x3C:
                        register_BR = (Int16)(register_BR + reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x3D:
                        register_CR = (Int16)(register_CR + reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x3E:
                        register_DR = (Int16)(register_DR + reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x3F:
                        register_ER = (Int16)(register_ER + reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- SUB -------------------------
                    case 0x40:
                        register_AR = (Int16)(register_AR - register_BR);
                        register_PC++;
                        break;
                    case 0x41:
                        register_AR = (Int16)(register_AR - register_CR);
                        register_PC++;
                        break;
                    case 0x42:
                        register_AR = (Int16)(register_AR - register_DR);
                        register_PC++;
                        break;
                    case 0x43:
                        register_AR = (Int16)(register_AR - register_ER);
                        register_PC++;
                        break;
                    case 0x44:
                        register_BR = (Int16)(register_BR - register_AR);
                        register_PC++;
                        break;
                    case 0x45:
                        register_BR = (Int16)(register_BR - register_CR);
                        register_PC++;
                        break;
                    case 0x46:
                        register_BR = (Int16)(register_BR - register_DR);
                        register_PC++;
                        break;
                    case 0x47:
                        register_BR = (Int16)(register_BR - register_ER);
                        register_PC++;
                        break;
                    case 0x48:
                        register_CR = (Int16)(register_CR - register_AR);
                        register_PC++;
                        break;
                    case 0x49:
                        register_CR = (Int16)(register_CR - register_BR);
                        register_PC++;
                        break;
                    case 0x4A:
                        register_CR = (Int16)(register_CR - register_DR);
                        register_PC++;
                        break;
                    case 0x4B:
                        register_CR = (Int16)(register_CR - register_ER);
                        register_PC++;
                        break;
                    case 0x4C:
                        register_DR = (Int16)(register_DR - register_AR);
                        register_PC++;
                        break;
                    case 0x4D:
                        register_DR = (Int16)(register_DR - register_BR);
                        register_PC++;
                        break;
                    case 0x4E:
                        register_DR = (Int16)(register_DR - register_CR);
                        register_PC++;
                        break;
                    case 0x4F:
                        register_DR = (Int16)(register_DR - register_ER);
                        register_PC++;
                        break;
                    case 0x50:
                        register_ER = (Int16)(register_ER - register_AR);
                        register_PC++;
                        break;
                    case 0x51:
                        register_ER = (Int16)(register_ER - register_BR);
                        register_PC++;
                        break;
                    case 0x52:
                        register_ER = (Int16)(register_ER - register_CR);
                        register_PC++;
                        break;
                    case 0x53:
                        register_ER = (Int16)(register_ER - register_DR);
                        register_PC++;
                        break;
                    case 0x54:
                        register_AR = (Int16)(register_AR - reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x55:
                        register_BR = (Int16)(register_BR - reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x56:
                        register_CR = (Int16)(register_CR - reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x57:
                        register_DR = (Int16)(register_DR - reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x58:
                        register_ER = (Int16)(register_ER - reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- MUL -------------------------
                    case 0x59:
                        register_AR = (Int16)(register_AR * register_BR);
                        register_PC++;
                        break;
                    case 0x5A:
                        register_AR = (Int16)(register_AR * register_CR);
                        register_PC++;
                        break;
                    case 0x5B:
                        register_AR = (Int16)(register_AR * register_DR);
                        register_PC++;
                        break;
                    case 0x5C:
                        register_AR = (Int16)(register_AR * register_ER);
                        register_PC++;
                        break;
                    case 0x5D:
                        register_BR = (Int16)(register_BR * register_AR);
                        register_PC++;
                        break;
                    case 0x5E:
                        register_BR = (Int16)(register_BR * register_CR);
                        register_PC++;
                        break;
                    case 0x5F:
                        register_BR = (Int16)(register_BR * register_DR);
                        register_PC++;
                        break;
                    case 0x60:
                        register_BR = (Int16)(register_BR * register_ER);
                        register_PC++;
                        break;
                    case 0x61:
                        register_CR = (Int16)(register_CR * register_AR);
                        register_PC++;
                        break;
                    case 0x62:
                        register_CR = (Int16)(register_CR * register_BR);
                        register_PC++;
                        break;
                    case 0x63:
                        register_CR = (Int16)(register_CR * register_DR);
                        register_PC++;
                        break;
                    case 0x64:
                        register_CR = (Int16)(register_CR * register_ER);
                        register_PC++;
                        break;
                    case 0x65:
                        register_DR = (Int16)(register_DR * register_AR);
                        register_PC++;
                        break;
                    case 0x66:
                        register_DR = (Int16)(register_DR * register_BR);
                        register_PC++;
                        break;
                    case 0x67:
                        register_DR = (Int16)(register_DR * register_CR);
                        register_PC++;
                        break;
                    case 0x68:
                        register_DR = (Int16)(register_DR * register_ER);
                        register_PC++;
                        break;
                    case 0x69:
                        register_ER = (Int16)(register_ER * register_AR);
                        register_PC++;
                        break;
                    case 0x6A:
                        register_ER = (Int16)(register_ER * register_BR);
                        register_PC++;
                        break;
                    case 0x6B:
                        register_ER = (Int16)(register_ER * register_CR);
                        register_PC++;
                        break;
                    case 0x6C:
                        register_ER = (Int16)(register_ER * register_DR);
                        register_PC++;
                        break;
                    case 0x6D:
                        register_AR = (Int16)(register_AR * reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x6E:
                        register_BR = (Int16)(register_BR * reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x6F:
                        register_CR = (Int16)(register_CR * reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x70:
                        register_DR = (Int16)(register_DR * reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x71:
                        register_ER = (Int16)(register_ER * reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- DIV -------------------------
                    case 0x72:
                        register_AR = (Int16)(register_AR / register_BR);
                        register_BR = (Int16)(register_AR % register_BR);
                        register_PC++;
                        break;
                    case 0x73:
                        register_AR = (Int16)(register_AR / register_CR);
                        register_BR = (Int16)(register_AR % register_CR);
                        register_PC++;
                        break;
                    case 0x74:
                        register_AR = (Int16)(register_AR / register_DR);
                        register_BR = (Int16)(register_AR % register_DR);
                        register_PC++;
                        break;
                    case 0x75:
                        register_AR = (Int16)(register_AR / register_ER);
                        register_BR = (Int16)(register_AR % register_ER);
                        register_PC++;
                        break;
                    case 0x76:
                        register_AR = (Int16)(register_BR / register_AR);
                        register_BR = (Int16)(register_BR % register_AR);
                        register_PC++;
                        break;
                    case 0x77:
                        register_AR = (Int16)(register_BR / register_CR);
                        register_BR = (Int16)(register_BR % register_CR);
                        register_PC++;
                        break;
                    case 0x78:
                        register_AR = (Int16)(register_BR / register_DR);
                        register_BR = (Int16)(register_BR % register_DR);
                        register_PC++;
                        break;
                    case 0x79:
                        register_AR = (Int16)(register_BR / register_ER);
                        register_BR = (Int16)(register_BR % register_ER);
                        register_PC++;
                        break;
                    case 0x7A:
                        register_AR = (Int16)(register_CR / register_AR);
                        register_BR = (Int16)(register_CR % register_AR);
                        register_PC++;
                        break;
                    case 0x7B:
                        register_AR = (Int16)(register_CR / register_BR);
                        register_BR = (Int16)(register_CR % register_BR);
                        register_PC++;
                        break;
                    case 0x7C:
                        register_AR = (Int16)(register_CR / register_DR);
                        register_BR = (Int16)(register_CR % register_DR);
                        register_PC++;
                        break;
                    case 0x7D:
                        register_AR = (Int16)(register_CR / register_ER);
                        register_BR = (Int16)(register_CR % register_ER);
                        register_PC++;
                        break;
                    case 0x7E:
                        register_AR = (Int16)(register_DR / register_AR);
                        register_BR = (Int16)(register_DR % register_AR);
                        register_PC++;
                        break;
                    case 0x7F:
                        register_AR = (Int16)(register_DR / register_BR);
                        register_BR = (Int16)(register_DR % register_BR);
                        register_PC++;
                        break;
                    case 0x80:
                        register_AR = (Int16)(register_DR / register_CR);
                        register_BR = (Int16)(register_DR % register_CR);
                        register_PC++;
                        break;
                    case 0x81:
                        register_AR = (Int16)(register_DR / register_ER);
                        register_BR = (Int16)(register_DR % register_ER);
                        register_PC++;
                        break;
                    case 0x82:
                        register_AR = (Int16)(register_ER / register_AR);
                        register_BR = (Int16)(register_ER % register_AR);
                        register_PC++;
                        break;
                    case 0x83:
                        register_AR = (Int16)(register_ER / register_BR);
                        register_BR = (Int16)(register_ER % register_BR);
                        register_PC++;
                        break;
                    case 0x84:
                        register_AR = (Int16)(register_ER / register_CR);
                        register_BR = (Int16)(register_ER % register_CR);
                        register_PC++;
                        break;
                    case 0x85:
                        register_AR = (Int16)(register_ER / register_DR);
                        register_BR = (Int16)(register_ER % register_DR);
                        register_PC++;
                        break;
                    case 0x86:
                        register_AR = (Int16)(register_AR / reader.ReadInt16());
                        register_BR = (Int16)(register_AR % reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x87:
                        register_AR = (Int16)(register_BR / reader.ReadInt16());
                        register_BR = (Int16)(register_BR % reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x88:
                        register_AR = (Int16)(register_CR / reader.ReadInt16());
                        register_BR = (Int16)(register_CR % reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x89:
                        register_AR = (Int16)(register_DR / reader.ReadInt16());
                        register_BR = (Int16)(register_DR % reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0x8A:
                        register_AR = (Int16)(register_ER / reader.ReadInt16());
                        register_BR = (Int16)(register_ER % reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- AND -------------------------
                    case 0x8B:
                        register_AR = (Int16)(register_AR * register_BR);
                        register_PC++;
                        break;
                    case 0x8C:
                        register_AR = (Int16)(register_AR & register_CR);
                        register_PC++;
                        break;
                    case 0x8D:
                        register_AR = (Int16)(register_AR & register_DR);
                        register_PC++;
                        break;
                    case 0x8E:
                        register_AR = (Int16)(register_AR & register_ER);
                        register_PC++;
                        break;
                    case 0x8F:
                        register_BR = (Int16)(register_BR & register_AR);
                        register_PC++;
                        break;
                    case 0x90:
                        register_BR = (Int16)(register_BR & register_CR);
                        register_PC++;
                        break;
                    case 0x91:
                        register_BR = (Int16)(register_BR & register_DR);
                        register_PC++;
                        break;
                    case 0x92:
                        register_BR = (Int16)(register_BR & register_ER);
                        register_PC++;
                        break;
                    case 0x93:
                        register_CR = (Int16)(register_CR & register_AR);
                        register_PC++;
                        break;
                    case 0x94:
                        register_CR = (Int16)(register_CR & register_BR);
                        register_PC++;
                        break;
                    case 0x95:
                        register_CR = (Int16)(register_CR & register_DR);
                        register_PC++;
                        break;
                    case 0x96:
                        register_CR = (Int16)(register_CR & register_ER);
                        register_PC++;
                        break;
                    case 0x97:
                        register_DR = (Int16)(register_DR & register_AR);
                        register_PC++;
                        break;
                    case 0x98:
                        register_DR = (Int16)(register_DR & register_BR);
                        register_PC++;
                        break;
                    case 0x99:
                        register_DR = (Int16)(register_DR & register_CR);
                        register_PC++;
                        break;
                    case 0x9A:
                        register_DR = (Int16)(register_DR & register_ER);
                        register_PC++;
                        break;
                    case 0x9B:
                        register_ER = (Int16)(register_ER & register_AR);
                        register_PC++;
                        break;
                    case 0x9C:
                        register_ER = (Int16)(register_ER & register_BR);
                        register_PC++;
                        break;
                    case 0x9D:
                        register_ER = (Int16)(register_ER & register_CR);
                        register_PC++;
                        break;
                    case 0x9E:
                        register_ER = (Int16)(register_ER & register_DR);
                        register_PC++;
                        break;
                    case 0x9F:
                        register_AR = (Int16)(register_AR & reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xA0:
                        register_BR = (Int16)(register_BR & reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xA1:
                        register_CR = (Int16)(register_CR & reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xA2:
                        register_DR = (Int16)(register_DR & reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xA3:
                        register_ER = (Int16)(register_ER & reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- OR -------------------------
                    case 0xA4:
                        register_AR = (Int16)(register_AR | register_BR);
                        register_PC++;
                        break;
                    case 0xA5:
                        register_AR = (Int16)(register_AR | register_CR);
                        register_PC++;
                        break;
                    case 0xA6:
                        register_AR = (Int16)(register_AR | register_DR);
                        register_PC++;
                        break;
                    case 0xA7:
                        register_AR = (Int16)(register_AR | register_ER);
                        register_PC++;
                        break;
                    case 0xA8:
                        register_BR = (Int16)(register_BR | register_AR);
                        register_PC++;
                        break;
                    case 0xA9:
                        register_BR = (Int16)(register_BR | register_CR);
                        register_PC++;
                        break;
                    case 0xAA:
                        register_BR = (Int16)(register_BR | register_DR);
                        register_PC++;
                        break;
                    case 0xAB:
                        register_BR = (Int16)(register_BR | register_ER);
                        register_PC++;
                        break;
                    case 0xAC:
                        register_CR = (Int16)(register_CR | register_AR);
                        register_PC++;
                        break;
                    case 0xAD:
                        register_CR = (Int16)(register_CR | register_BR);
                        register_PC++;
                        break;
                    case 0xAE:
                        register_CR = (Int16)(register_CR | register_DR);
                        register_PC++;
                        break;
                    case 0xAF:
                        register_CR = (Int16)(register_CR | register_ER);
                        register_PC++;
                        break;
                    case 0xB0:
                        register_DR = (Int16)(register_DR | register_AR);
                        register_PC++;
                        break;
                    case 0xB1:
                        register_DR = (Int16)(register_DR | register_BR);
                        register_PC++;
                        break;
                    case 0xB2:
                        register_DR = (Int16)(register_DR | register_CR);
                        register_PC++;
                        break;
                    case 0xB3:
                        register_DR = (Int16)(register_DR | register_ER);
                        register_PC++;
                        break;
                    case 0xB4:
                        register_ER = (Int16)(register_ER | register_AR);
                        register_PC++;
                        break;
                    case 0xB5:
                        register_ER = (Int16)(register_ER | register_BR);
                        register_PC++;
                        break;
                    case 0xB6:
                        register_ER = (Int16)(register_ER | register_CR);
                        register_PC++;
                        break;
                    case 0xB7:
                        register_ER = (Int16)(register_ER | register_DR);
                        register_PC++;
                        break;
                    case 0xB8:
                        register_AR = (Int16)(register_AR | reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xB9:
                        register_BR = (Int16)(register_BR | reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xBA:
                        register_CR = (Int16)(register_CR | reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xBB:
                        register_DR = (Int16)(register_DR | reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xBC:
                        register_ER = (Int16)(register_ER | reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- OR -------------------------
                    case 0xBD:
                        register_AR = (Int16)(register_AR ^ register_BR);
                        register_PC++;
                        break;
                    case 0xBE:
                        register_AR = (Int16)(register_AR ^ register_CR);
                        register_PC++;
                        break;
                    case 0xBF:
                        register_AR = (Int16)(register_AR ^ register_DR);
                        register_PC++;
                        break;
                    case 0xC0:
                        register_AR = (Int16)(register_AR ^ register_ER);
                        register_PC++;
                        break;
                    case 0xC1:
                        register_BR = (Int16)(register_BR ^ register_AR);
                        register_PC++;
                        break;
                    case 0xC2:
                        register_BR = (Int16)(register_BR ^ register_CR);
                        register_PC++;
                        break;
                    case 0xC3:
                        register_BR = (Int16)(register_BR ^ register_DR);
                        register_PC++;
                        break;
                    case 0xC4:
                        register_BR = (Int16)(register_BR ^ register_ER);
                        register_PC++;
                        break;
                    case 0xC5:
                        register_CR = (Int16)(register_CR ^ register_AR);
                        register_PC++;
                        break;
                    case 0xC6:
                        register_CR = (Int16)(register_CR ^ register_BR);
                        register_PC++;
                        break;
                    case 0xC7:
                        register_CR = (Int16)(register_CR ^ register_DR);
                        register_PC++;
                        break;
                    case 0xC8:
                        register_CR = (Int16)(register_CR ^ register_ER);
                        register_PC++;
                        break;
                    case 0xC9:
                        register_DR = (Int16)(register_DR ^ register_AR);
                        register_PC++;
                        break;
                    case 0xCA:
                        register_DR = (Int16)(register_DR ^ register_BR);
                        register_PC++;
                        break;
                    case 0xCB:
                        register_DR = (Int16)(register_DR ^ register_CR);
                        register_PC++;
                        break;
                    case 0xCC:
                        register_DR = (Int16)(register_DR ^ register_ER);
                        register_PC++;
                        break;
                    case 0xCD:
                        register_ER = (Int16)(register_ER ^ register_AR);
                        register_PC++;
                        break;
                    case 0xCE:
                        register_ER = (Int16)(register_ER ^ register_BR);
                        register_PC++;
                        break;
                    case 0xCF:
                        register_ER = (Int16)(register_ER ^ register_CR);
                        register_PC++;
                        break;
                    case 0xD0:
                        register_ER = (Int16)(register_ER ^ register_DR);
                        register_PC++;
                        break;
                    case 0xD1:
                        register_AR = (Int16)(register_AR ^ reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xD2:
                        register_BR = (Int16)(register_BR ^ reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xD3:
                        register_CR = (Int16)(register_CR ^ reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xD4:
                        register_DR = (Int16)(register_DR ^ reader.ReadInt16());
                        register_PC += 3;
                        break;
                    case 0xD5:
                        register_ER = (Int16)(register_ER ^ reader.ReadInt16());
                        register_PC += 3;
                        break;
                    // ------------------------- CMP -------------------------
                    case 0xD6:
                        if (register_AR == register_BR)
                            register_CM = 0x00;
                        else if (register_AR > register_BR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xD7:
                        if (register_AR == register_CR)
                            register_CM = 0x00;
                        else if (register_AR > register_CR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xD8:
                        if (register_AR == register_DR)
                            register_CM = 0x00;
                        else if (register_AR > register_DR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xD9:
                        if (register_AR == register_ER)
                            register_CM = 0x00;
                        else if (register_AR > register_ER)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDA:
                        if (register_BR == register_AR)
                            register_CM = 0x00;
                        else if (register_BR > register_AR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDB:
                        if (register_BR == register_CR)
                            register_CM = 0x00;
                        else if (register_BR > register_CR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDC:
                        if (register_BR == register_DR)
                            register_CM = 0x00;
                        else if (register_BR > register_DR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDD:
                        if (register_BR == register_ER)
                            register_CM = 0x00;
                        else if (register_BR > register_ER)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDE:
                        if (register_CR == register_AR)
                            register_CM = 0x00;
                        else if (register_CR > register_AR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xDF:
                        if (register_CR == register_BR)
                            register_CM = 0x00;
                        else if (register_CR > register_BR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE0:
                        if (register_CR == register_DR)
                            register_CM = 0x00;
                        else if (register_CR > register_DR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE1:
                        if (register_CR == register_ER)
                            register_CM = 0x00;
                        else if (register_CR > register_ER)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE2:
                        if (register_DR == register_AR)
                            register_CM = 0x00;
                        else if (register_DR > register_AR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE3:
                        if (register_DR == register_BR)
                            register_CM = 0x00;
                        else if (register_DR > register_BR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE4:
                        if (register_DR == register_CR)
                            register_CM = 0x00;
                        else if (register_DR > register_CR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE5:
                        if (register_DR == register_ER)
                            register_CM = 0x00;
                        else if (register_DR > register_ER)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE6:
                        if (register_ER == register_AR)
                            register_CM = 0x00;
                        else if (register_ER > register_AR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE7:
                        if (register_ER == register_BR)
                            register_CM = 0x00;
                        else if (register_ER > register_BR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE8:
                        if (register_ER == register_CR)
                            register_CM = 0x00;
                        else if (register_ER > register_CR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xE9:
                        if (register_ER == register_DR)
                            register_CM = 0x00;
                        else if (register_ER > register_DR)
                            register_CM = 0x01;
                        else
                            register_CM = 0x02;
                        register_PC++;
                        break;
                    case 0xEA:
                        register_PC = reader.ReadInt16();
                        reader.BaseStream.Position = register_PC;
                        break;
                    case 0xEB:
                        if (register_CM != 0x00)
                        {
                            register_PC = reader.ReadInt16();
                            reader.BaseStream.Position = register_PC;
                        }
                        else
                        {
                            reader.ReadInt16();
                            register_PC += 3;
                        }
                        break;
                    case 0xEC:
                        if (register_CM == 0x01)
                        {
                            register_PC = reader.ReadInt16();
                            reader.BaseStream.Position = register_PC;
                        }
                        else
                        {
                            reader.ReadInt16();
                            register_PC += 3;
                        }
                        break;
                    case 0xED:
                        if (register_CM == 0x02)
                        {
                            register_PC = reader.ReadInt16();
                            reader.BaseStream.Position = register_PC;
                        }
                        else
                        {
                            reader.ReadInt16();
                            register_PC += 3;
                        }
                        break;
                    case 0xEE:
                        WriteIO((byte)(register_AR & 0xFF00), (byte)(register_AR & 0x00FF));
                        register_PC ++;
                        break;
                    case 0xEF:
                        register_AR = (Int16)Console.ReadKey().KeyChar;
                        register_PC++;
                        break;
                    case 0xF0:
                        if (register_SS != 0x00)
                        {
                            memory[register_SP + register_SL + 1] = (byte)register_AR;
                            memory[register_SP + register_SL] = (byte)(register_AR >> 8);
                            register_SL += 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF1:
                        if (register_SS != 0x00)
                        {
                            memory[register_SP + register_SL + 1] = (byte)register_BR;
                            memory[register_SP + register_SL] = (byte)(register_BR >> 8);
                            register_SL += 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF2:
                        if (register_SS != 0x00)
                        {
                            memory[register_SP + register_SL + 1] = (byte)register_CR;
                            memory[register_SP + register_SL] = (byte)(register_CR >> 8);
                            register_SL += 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF3:
                        if (register_SS != 0x00)
                        {
                            memory[register_SP + register_SL + 1] = (byte)register_DR;
                            memory[register_SP + register_SL] = (byte)(register_DR >> 8);
                            register_SL += 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF4:
                        if (register_SS != 0x00)
                        {
                            memory[register_SP + register_SL + 1] = (byte)register_ER;
                            memory[register_SP + register_SL] = (byte)(register_ER >> 8);
                            register_SL += 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF5:
                        if (register_SS != 0x00)
                        {
                            register_AR = (short)(memory[register_SP + register_SL - 2] << 8 | memory[register_SP + register_SL - 1]);
                            register_SL -= 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF6:
                        if (register_SS != 0x00)
                        {
                            register_BR = (short)(memory[register_SP + register_SL - 2] << 8 | memory[register_SP + register_SL - 1]);
                            register_SL -= 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF7:
                        if (register_SS != 0x00)
                        {
                            register_CR = (short)(memory[register_SP + register_SL - 2] << 8 | memory[register_SP + register_SL - 1]);
                            register_SL -= 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xF8:
                        if (register_SS != 0x00)
                        {
                            register_DR = (short)(memory[register_SP + register_SL - 2] << 8 | memory[register_SP + register_SL - 1]);
                            register_SL -= 2;
                        }
                        else { panic("NO STACK DEFINED !"); }
                        register_PC++;
                        break;
                    case 0xF9:
                        if (register_SS != 0x00)
                        {
                            register_ER = (short)(memory[register_SP + register_SL - 2] << 8 | memory[register_SP + register_SL - 1]);
                            register_SL -= 2;
                        }
                        else { panic("NO STACK DEFINED !\n"); }
                        register_PC++;
                        break;
                    case 0xFA:
                        shift = reader.ReadByte();
                        if ((shift & (1 << 5 - 1)) != 0)
                            register_AR = (short)(register_AR >> (shift - 32));
                        else { register_AR = (short)(register_AR << shift); }
                        register_PC += 2;
                        break;
                    case 0xFB:
                        register_PC = register_IR;
                        reader.BaseStream.Position = register_PC;
                        break;
                    case 0xFC:
                        register_AR = (short)(memory[register_BR] << 8 | memory[register_BR + 1]);
                        register_PC++;
                        break;
                    case 0xFD:
                        memory[register_BR] = (byte)(register_AR >> 8);
                        memory[register_BR + 1] = (byte)register_AR;
                        register_PC += 3;
                        break;
                    case 0xFE:
                        register_PC = register_AR;
                        reader.BaseStream.Position = register_PC;
                        break;
                    case 0xFF:
                        break;
                    default:
                        panic($"INVALID OPCODE: 0x{$"{memory[register_PC]:X}".PadLeft(2, '0')}\n");
                        break;
                }

                debug_wnd.update_registers();
            }
        }

        static void TimerTick(Object o)
        {
            if (!isHalted)
                tick = true;
        }

        static void panic(string error)
        {
            debug_wnd.Log($"ERROR: {error}");
            debug_wnd.checkBox1.Checked = true;
        }

        public static void reset()
        {
            register_AR = 0x0000;
            register_BR = 0x0000;
            register_CR = 0x0000;
            register_DR = 0x0000;
            register_ER = 0x0000;
            register_SP = 0x0000;
            register_SS = 0x0000;
            register_SL = 0x0000;
            register_IR = 0x0000;
            register_PC = 0x0000;
            register_CM = 0x0000;
            reader.BaseStream.Position = 0x0000;
            Console.Clear();
            debug_wnd.update_registers();
            debug_wnd.Log($"<== CPU RESET ==>\n");
            debug_wnd.Log("Initializing RAM...    ");
            for (int i = 0; i < 0x10000; i++)
            {
                memory[i] = 0x00;
            }
            debug_wnd.Log("Done.\n");

            debug_wnd.Log($"Loading {filename}... ");
            BinaryReader2 binary_file = new BinaryReader2(File.Open(filename, FileMode.Open));
            for (int i = 0; i < binary_file.BaseStream.Length; i++)
            {
                memory[i] = binary_file.ReadByte();
            }
            binary_file.Close();
            debug_wnd.Log("Done.\n");
            debug_wnd.checkBox1.Checked = false;
        }
            
        static void WriteIO(byte port, byte data)
        {
            switch (port){
                case 0x00:
                    Console.Write((char)data);
                    break;
                default:
                    break;
            }
        }
    }

    

    class BinaryReader2 : BinaryReader
    {
        public BinaryReader2(System.IO.Stream stream) : base(stream) { }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public Int16 ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public Int64 ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public UInt32 ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
    }
}
