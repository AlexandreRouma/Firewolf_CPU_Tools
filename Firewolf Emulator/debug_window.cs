using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Firewolf_Emulator
{
    public partial class debug_window : Form
    {
        public debug_window()
        {
            InitializeComponent();
        }

        private void debug_window_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        public void update_registers()
        {
            textBox1.Text = $"{Program.register_AR:X}".PadLeft(4, '0');
            textBox2.Text = $"{Program.register_BR:X}".PadLeft(4, '0');
            textBox3.Text = $"{Program.register_CR:X}".PadLeft(4, '0');
            textBox4.Text = $"{Program.register_DR:X}".PadLeft(4, '0');
            textBox5.Text = $"{Program.register_ER:X}".PadLeft(4, '0');
            textBox6.Text = $"{Program.register_SP:X}".PadLeft(4, '0');
            textBox7.Text = $"{Program.register_SS:X}".PadLeft(4, '0');
            textBox8.Text = $"{Program.register_SL:X}".PadLeft(4, '0');
            textBox9.Text = $"{Program.register_IR:X}".PadLeft(4, '0');
            textBox10.Text = $"{Program.register_PC:X}".PadLeft(4, '0');
            textBox11.Text = $"{Program.register_CM:X}".PadLeft(4, '0');
        }

        public void Log(string logtext)
        {
            richTextBox1.Text += logtext;
        }

        public bool ready = false;

        private void debug_window_Shown(object sender, EventArgs e)
        {
            ready = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Program.reset();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.tick = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.isHalted = checkBox1.Checked;
            if (checkBox1.Checked)
                Log($"<==  CPU HALTED ==>\n");
            else
                Log($"<== CPU UN-HALTED ==>\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(Program.dump).Start();
        }

        
    }
}
