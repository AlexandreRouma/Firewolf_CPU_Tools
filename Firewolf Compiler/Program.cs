using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firewolf_Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = File.ReadAllText("program.asm");
            input = concatIncludes(input);
            input = cleanup(input);
            compile(input, "");
            File.WriteAllText("output.asm", input);
            Console.ReadLine();
        }

        static string concatIncludes(string file)
        {
            file += "\n";
            foreach (string line in file.Split('\n'))
            {
                if (line.StartsWith(".include"))
                {
                    string filename = line.Replace("\"", "").Replace("\r", "").Split(' ')[1];
                    try
                    {
                        file += concatIncludes(File.ReadAllText(filename)) + "\n";
                        Console.WriteLine($"Included \"{filename}\"");
                    }
                    catch
                    {
                        panic($"Can't find {filename}");
                    }
                    file = file.Replace(line, "");
                }
                
            }
            return file;
        }

        static string cleanup(string file)
        {
            string cleanedFile = "";
            foreach (string line in file.Split('\n'))
            {
                string cleanedLine = line.TrimStart(' ').TrimStart('\t').Replace("\r","");
                if (cleanedLine != "")
                    if (cleanedLine.Contains(";"))
                        cleanedLine = cleanedLine.Substring(0, cleanedLine.IndexOf(';'));
                if (cleanedLine != "")
                    cleanedLine = cleanedLine.TrimEnd(' ').TrimEnd('\t');
                if (cleanedLine != "")
                    cleanedFile += cleanedLine + "\n";
            }
            cleanedFile = cleanedFile.TrimEnd('\n').TrimEnd('\r');
            return cleanedFile;
        }

        static void compile(string input, string path)
        {
            short counter = 0;
            List<Label> labels = new List<Label>();
            List<Label> toAddLater = new List<Label>();
            byte[] output = new byte[0x10000];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(output));
            foreach (string line in input.Split('\n'))
            {
                if (line.EndsWith(":"))
                {
                    Label label;
                    label.name = line.TrimEnd(':');
                    label.address = counter;
                    labels.Add(label);
                }
                else
                {
                    Instruction inst = decodeLine(line);
                    switch (inst.opcode)
                    {
                        case "hlt":
                            writer.Write((byte)0x00);
                            counter++;
                            break;
                        case "mov":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg2)
                                {
                                    case "AR":
                                        switch (inst.arg1)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x01);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x02);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x03);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x04);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg1)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x05);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x06);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x07);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x08);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg1)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x09);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x0A);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x0B);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x0C);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg1)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x0D);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x0E);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x0F);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x10);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg1)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x11);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x12);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x13);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x14);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (inst.arg1 == "AR" && inst.arg2 == "$[BR]")
                            {
                                writer.Write((byte)0xFC);
                                counter++;
                                break;
                            }
                            else if (inst.arg1 == "$[BR]" && inst.arg2 == "AR")
                            {
                                writer.Write((byte)0xFD);
                                counter++;
                                break;
                            }
                            else if (isRegister(inst.arg1) && isValueAtAddress(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x15);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x16);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x17);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x18);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x19);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write(switchHL(getValueFromString(inst.arg2.Substring(2, inst.arg2.Length - 3))));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isValueAtPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x15);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x16);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x17);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x18);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x19);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(2, inst.arg2.Length - 3);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }



                            else if (isValueAtAddress(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg2)
                                {
                                    case "AR":
                                        writer.Write((byte)0x1A);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x1B);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x1C);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x1D);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x1E);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write(switchHL(getValueFromString(inst.arg1.Substring(2, inst.arg1.Length - 3))));
                                counter += 3;
                            }
                            else if (isValueAtPointerToLabel(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg2)
                                {
                                    case "AR":
                                        writer.Write((byte)0x1A);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x1B);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x1C);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x1D);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x1E);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(2, inst.arg1.Length - 3);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }

                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x1F);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x20);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x21);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x22);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x23);
                                        break;
                                    case "SP":
                                        writer.Write((byte)0x24);
                                        break;
                                    case "SS":
                                        writer.Write((byte)0x25);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write(switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x1F);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x20);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x21);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x22);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x23);
                                        break;
                                    case "SP":
                                        writer.Write((byte)0x24);
                                        break;
                                    case "SS":
                                        writer.Write((byte)0x25);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else if (isValueAtAddress(inst.arg1) && isValue(inst.arg2))
                            {
                                writer.Write((byte)0x26);
                                writer.Write(switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isValueAtPointerToLabel(inst.arg1) && isValue(inst.arg2))
                            {
                                writer.Write((byte)0x26);
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(2, inst.arg1.Length - 3);
                                toAddLater.Add(lbl);
                                writer.Write(switchHL(getValueFromString(inst.arg2)));
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "add":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x27);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x28);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x29);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x2A);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x2B);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x2C);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x2D);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x2E);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x2F);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x30);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x31);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x32);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x33);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x34);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x35);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x36);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x37);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x38);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x39);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x3A);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x3B);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x3C);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x3D);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x3E);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x3F);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x3B);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x3C);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x3D);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x3E);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x3F);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "sub":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x40);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x41);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x42);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x43);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x44);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x45);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x46);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x47);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x48);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x49);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x4A);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x4B);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x4C);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x4D);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x4E);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x4F);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x50);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x51);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x52);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x53);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x54);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x55);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x56);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x57);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x58);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x54);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x55);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x56);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x57);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x58);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "mul":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x59);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x5A);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x5B);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x5C);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x5D);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x5E);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x5F);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x60);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x61);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x62);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x63);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x64);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x65);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x66);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x67);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x68);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x69);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x6A);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x6B);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x6C);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x6D);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x6E);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x6F);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x70);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x71);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x6D);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x6E);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x6F);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x70);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x71);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "div":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x72);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x73);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x74);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x75);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x76);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x77);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x78);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x79);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x7A);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x7B);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x7C);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x7D);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x7E);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x7F);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x80);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x81);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x82);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x83);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x84);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x85);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x86);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x87);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x88);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x89);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x8A);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x86);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0x87);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0x88);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0x89);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0x8A);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "and":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0x8B);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x8C);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x8D);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x8E);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x8F);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x90);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x91);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x92);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x93);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x94);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x95);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x96);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x97);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x98);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x99);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0x9A);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0x9B);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0x9C);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0x9D);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0x9E);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x9F);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xA0);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xA1);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xA2);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xA3);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0x9F);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xA0);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xA1);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xA2);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xA3);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "or":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0xA4);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xA5);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xA6);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xA7);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xA8);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xA9);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xAA);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xAB);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xAC);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xAD);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xAE);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xAF);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xB0);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xB1);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xB2);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xB3);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xB4);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xB5);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xB6);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xB7);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0xB8);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xB9);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xBA);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xBB);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xBC);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0xB8);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xB9);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xBA);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xBB);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xBC);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        case "xor":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0xBD);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xBE);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xBF);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xC0);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xC1);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xC2);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xC3);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xC4);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xC5);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xC6);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xC7);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xC8);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xC9);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xCA);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xCB);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xCC);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xCD);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xCE);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xDF);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xD0);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else if (isRegister(inst.arg1) && isValue(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0xD1);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xD2);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xD3);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xD4);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xD5);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isRegister(inst.arg1) && isPointerToLabel(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        writer.Write((byte)0xD1);
                                        break;
                                    case "BR":
                                        writer.Write((byte)0xD2);
                                        break;
                                    case "CR":
                                        writer.Write((byte)0xD3);
                                        break;
                                    case "DR":
                                        writer.Write((byte)0xD4);
                                        break;
                                    case "ER":
                                        writer.Write((byte)0xD5);
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg2.Substring(1, inst.arg2.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "cmp":
                            if (isRegister(inst.arg1) && isRegister(inst.arg2))
                            {
                                switch (inst.arg1)
                                {
                                    case "AR":
                                        switch (inst.arg2)
                                        {
                                            case "BR":
                                                writer.Write((byte)0xD6);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xD7);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xD8);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xD9);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "BR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xDA);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xDB);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xDC);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xDD);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "CR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xDE);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xDF);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xE0);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xE1);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "DR":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xE2);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xE3);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xE4);
                                                break;
                                            case "ER":
                                                writer.Write((byte)0xE5);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    case "ER":
                                        switch (inst.arg2)
                                        {
                                            case "AR":
                                                writer.Write((byte)0xE6);
                                                break;
                                            case "BR":
                                                writer.Write((byte)0xE7);
                                                break;
                                            case "CR":
                                                writer.Write((byte)0xE8);
                                                break;
                                            case "DR":
                                                writer.Write((byte)0xE9);
                                                break;
                                            default:
                                                panic($"ERROR, Invalid line: \"{line}\"");
                                                break;
                                        }
                                        break;
                                    default:
                                        panic($"ERROR, Invalid line: \"{line}\"");
                                        break;
                                }
                                counter++;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "jmp":
                            if (inst.arg1 == "[AR]")
                            {
                                writer.Write((byte)0xFE);
                                counter++;
                                break;
                            }
                            else if(isValue(inst.arg1))
                            {
                                writer.Write((byte)0xEA);
                                writer.Write((short)switchHL(getValueFromString(inst.arg2)));
                                counter += 3;
                            }
                            else if (isPointerToLabel(inst.arg1))
                            {
                                writer.Write((byte)0xEA);
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(1, inst.arg1.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "jne":
                            if (isValue(inst.arg1))
                            {
                                writer.Write((byte)0xEB);
                                writer.Write((short)switchHL(getValueFromString(inst.arg1)));
                                counter += 3;
                            }
                            else if (isPointerToLabel(inst.arg1))
                            {
                                writer.Write((byte)0xEB);
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(1, inst.arg1.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "jgr":
                            if (isValue(inst.arg1))
                            {
                                writer.Write((byte)0xEC);
                                writer.Write((short)switchHL(getValueFromString(inst.arg1)));
                                counter += 3;
                            }
                            else if (isPointerToLabel(inst.arg1))
                            {
                                writer.Write((byte)0xEC);
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(1, inst.arg1.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "jsm":
                            if (isValue(inst.arg1))
                            {
                                writer.Write((byte)0xED);
                                writer.Write((short)switchHL(getValueFromString(inst.arg1)));
                                counter += 3;
                            }
                            else if (isPointerToLabel(inst.arg1))
                            {
                                writer.Write((byte)0xED);
                                counter++;
                                Label lbl;
                                lbl.address = counter;
                                lbl.name = inst.arg1.Substring(1, inst.arg1.Length - 2);
                                toAddLater.Add(lbl);
                                writer.Write((short)0x00);
                                counter += 2;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;

                        case "out":
                            writer.Write((byte)0xEE);
                            counter++;
                            break;

                        case "in":
                            writer.Write((byte)0xEF);
                            counter++;
                            break;

                        case "push":
                            switch (inst.arg1)
                            {
                                case "AR":
                                    writer.Write((byte)0xF0);
                                    break;
                                case "BR":
                                    writer.Write((byte)0xF1);
                                    break;
                                case "CR":
                                    writer.Write((byte)0xF2);
                                    break;
                                case "DR":
                                    writer.Write((byte)0xF3);
                                    break;
                                case "ER":
                                    writer.Write((byte)0xF4);
                                    break;
                                default:
                                    panic($"ERROR, Invalid line: \"{line}\"");
                                    break;
                            }
                            counter++;
                            break;

                        case "pop":
                            switch (inst.arg1)
                            {
                                case "AR":
                                    writer.Write((byte)0xF5);
                                    break;
                                case "BR":
                                    writer.Write((byte)0xF6);
                                    break;
                                case "CR":
                                    writer.Write((byte)0xF7);
                                    break;
                                case "DR":
                                    writer.Write((byte)0xF8);
                                    break;
                                case "ER":
                                    writer.Write((byte)0xF9);
                                    break;
                                default:
                                    panic($"ERROR, Invalid line: \"{line}\"");
                                    break;
                            }
                            counter++;
                            break;

                        case "sftl":
                            writer.Write((byte)0xFA);
                            if (isValue(inst.arg1))
                            {
                                writer.Write((short)((switchHL(getValueFromString(inst.arg1)) & 0x000F) | 0x0100));
                                counter += 3;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            counter += 2;
                            break;

                        case "sftr":
                            writer.Write((byte)0xFA);
                            if (isValue(inst.arg1))
                            {
                                writer.Write((short)(switchHL(getValueFromString(inst.arg1)) & 0x000F));
                                counter += 3;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            counter += 2;
                            break;

                        case "nop":
                            writer.Write((byte)0xFF);
                            counter++;
                            break;

                        case "str":
                            if (isString(inst.arg1))
                            {
                                string str = formatStr(inst.arg1.Substring(1, inst.arg1.Length - 2));
                                foreach (char c in str)
                                {
                                    writer.Write((byte)c);
                                }
                                counter += (short)str.Length;
                            }
                            else
                            {
                                panic($"ERROR, Invalid line: \"{line}\"");
                            }
                            break;
                        default:
                            panic($"ERROR, Unknown opcode: {inst.opcode}");
                            break;
                    }
                }
            }

            int size = counter;

            foreach (Label toFill in toAddLater)
            {
                if (labels.Where(l => l.name == toFill.name).Count() > 0)
                {
                    writer.BaseStream.Position = toFill.address;
                    writer.Write(switchHL(labels.Where(l => l.name == toFill.name).FirstOrDefault().address));
                }
                else
                {
                    panic($"ERROR, Can't find label: \"{toFill.name}\"");
                }
            }

            File.WriteAllBytes("out.bin", output.Take(size).ToArray());

            Console.WriteLine("Done.");
        }

        static string formatStr(string str)
        {
            string rtn = str;
            rtn = rtn.Replace("\\n","\n");
            return rtn;
        }

        static bool isString(string arg)
        {
            if (arg.StartsWith("\"") && arg.EndsWith("\""))
                return true;
            else
            {
                return false;
            }
        }

        static short switchHL(short input)
        {
            return (short)(((input & 0x00FF) << 8) | ((input & 0xFF00) >> 8));
        }

        static short getValueFromString(string val)
        {
            int i = 0;
            if (val.StartsWith("0x"))
            {
                return Int16.Parse(val.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            else
            {
                return Int16.Parse(val);
            }
        }

        static bool isAddress(string arg)
        {
            if (arg.StartsWith("[") && arg.EndsWith("]"))
            {
                string inside = arg.Substring(1, arg.Length - 1);
                if (inside.Contains("0x"))
                    return true;
                else { return false; }
            }
            else { return false; }
        }

        static bool isValueAtAddress(string arg)
        {
            if (arg.StartsWith("$[") && arg.EndsWith("]"))
            {
                string inside = arg.Substring(1, arg.Length - 1);
                if (inside.Contains("0x"))
                    return true;
                else { return false; }
            }
            else { return false; }
        }

        static string[] registers = { "AR","BR","CR","DR","ER","SP","SS" };

        static bool isRegister(string arg)
        {
            if (registers.Contains(arg))
                return true;
            else { return false; }
        }

        static bool isPointerToLabel(string arg)
        {
            if (arg.StartsWith("[") && arg.EndsWith("]"))
                return true;
            else { return false; }
        }

        static bool isValueAtPointerToLabel(string arg)
        {
            if (arg.StartsWith("$[") && arg.EndsWith("]"))
                return true;
            else { return false; }
        }



        static bool isValue(string str)
        {
            if (str.StartsWith("0x"))
            {
                return true;
            }
            else
            {
                foreach (char c in str)
                {
                    if (c < '0' || c > '9')
                        return false;
                }
                return true;
            }
        }

        static Instruction decodeLine(string line)
        {
            Instruction inst;
            inst.opcode = line.Split(' ')[0];
            string args = line.TrimStart((inst.opcode + " ").ToArray());
            switch (inst.opcode)
            {
                case "str":
                    break;
                default:
                    args = args.Replace(" ", "");
                    break;
            }
            inst.arg1 = "";
            inst.arg2 = "";
            if (args != "")
                inst.arg1 = args.Split(',')[0];
            if (args.Split(',').Count() > 1)
                inst.arg2 = args.Split(',')[1];
            return inst;
        }

        struct Instruction
        {
            public string opcode;
            public string arg1;
            public string arg2;
        };

        struct Label
        {
            public string name;
            public short address;
        };

        static void panic(string error)
        {
            Console.WriteLine(error);
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
