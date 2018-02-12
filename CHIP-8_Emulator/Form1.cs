using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHIP_8_Emulator
{
	public partial class Form1 : Form
	{
		CPU cpu;
		Thread thr;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			thr = new Thread(new ThreadStart(RunGame));
			thr.Start();
		}

		public void RunGame()
		{
			cpu = new CPU();
			cpu.LoadRomToMem("MAZE");

			cpu.Run();
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			/*
			 keyboard input:
			  1 2 3 C
			  4 5 6 D
			  7 8 9 E
			  A 0 B C
			 ->
			  1 2 3 4
			  Q W E R
			  A S D F
			  Z X C V
			 */
			switch (e.KeyCode)
			{
				case Keys.D1:
					{
						cpu.keyboard[0x1] = 1;
#if DEBUG
						Console.WriteLine("D1 pressed -> 1");
#endif
						break;
					}
				case Keys.D2:
					{
						cpu.keyboard[0x2] = 1;
#if DEBUG
						Console.WriteLine("D2 pressed -> 2");
#endif
						break;
					}
				case Keys.D3:
					{
						cpu.keyboard[0x3] = 1;
#if DEBUG
						Console.WriteLine("D3 pressed -> 3");
#endif
						break;
					}
				case Keys.D4:
					{
						cpu.keyboard[0xc] = 1;
#if DEBUG
						Console.WriteLine("D4 pressed -> C");
#endif
						break;
					}
				case Keys.Q:
					{
						cpu.keyboard[0x4] = 1;
#if DEBUG
						Console.WriteLine("Q pressed -> 4");
#endif
						break;
					}
				case Keys.W:
					{
						cpu.keyboard[0x5] = 1;
#if DEBUG
						Console.WriteLine("W pressed -> 5");
#endif
						break;
					}
				case Keys.E:
					{
						cpu.keyboard[0x6] = 1;
#if DEBUG
						Console.WriteLine("E pressed -> 6");
#endif
						break;
					}
				case Keys.R:
					{
						cpu.keyboard[0xd] = 1;
#if DEBUG
						Console.WriteLine("R pressed -> D");
#endif
						break;
					}
				case Keys.A:
					{
						cpu.keyboard[0x7] = 1;
#if DEBUG
						Console.WriteLine("A pressed -> 7");
#endif
						break;
					}
				case Keys.S:
					{
						cpu.keyboard[0x8] = 1;
#if DEBUG
						Console.WriteLine("S pressed -> 8");
#endif
						break;
					}
				case Keys.D:
					{
						cpu.keyboard[0x9] = 1;
#if DEBUG
						Console.WriteLine("D pressed -> 9");
#endif
						break;
					}
				case Keys.F:
					{
						cpu.keyboard[0xe] = 1;
#if DEBUG
						Console.WriteLine("F pressed -> E");
#endif
						break;
					}
				case Keys.Z:
					{
						cpu.keyboard[0xa] = 1;
#if DEBUG
						Console.WriteLine("Z pressed -> A");
#endif
						break;
					}
				case Keys.X:
					{
						cpu.keyboard[0x0] = 1;
#if DEBUG
						Console.WriteLine("X pressed -> 0");
#endif
						break;
					}
				case Keys.C:
					{
						cpu.keyboard[0xb] = 1;
#if DEBUG
						Console.WriteLine("C pressed -> B");
#endif
						break;
					}
				case Keys.V:
					{
						cpu.keyboard[0xc] = 1;
#if DEBUG
						Console.WriteLine("V pressed -> C");
#endif
						break;
					}
			}
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			/*
			 keyboard input:
			  1 2 3 C
			  4 5 6 D
			  7 8 9 E
			  A 0 B C
			 ->
			  1 2 3 4
			  Q W E R
			  A S D F
			  Z X C V
			 */
			switch (e.KeyCode)
			{
				case Keys.D1:
					{
						cpu.keyboard[0x1] = 0;
#if DEBUG
						Console.WriteLine("D1 released -> 1");
#endif
						break;
					}
				case Keys.D2:
					{
						cpu.keyboard[0x2] = 0;
#if DEBUG
						Console.WriteLine("D2 released -> 2");
#endif
						break;
					}
				case Keys.D3:
					{
						cpu.keyboard[0x3] = 0;
#if DEBUG
						Console.WriteLine("D3 released -> 3");
#endif
						break;
					}
				case Keys.D4:
					{
						cpu.keyboard[0xc] = 0;
#if DEBUG
						Console.WriteLine("D4 released -> C");
#endif
						break;
					}
				case Keys.Q:
					{
						cpu.keyboard[0x4] = 0;
#if DEBUG
						Console.WriteLine("Q released -> 4");
#endif
						break;
					}
				case Keys.W:
					{
						cpu.keyboard[0x5] = 0;
#if DEBUG
						Console.WriteLine("W released -> 5");
#endif
						break;
					}
				case Keys.E:
					{
						cpu.keyboard[0x6] = 0;
#if DEBUG
						Console.WriteLine("E released -> 6");
#endif
						break;
					}
				case Keys.R:
					{
						cpu.keyboard[0xd] = 0;
#if DEBUG
						Console.WriteLine("R released -> D");
#endif
						break;
					}
				case Keys.A:
					{
						cpu.keyboard[0x7] = 0;
#if DEBUG
						Console.WriteLine("A released -> 7");
#endif
						break;
					}
				case Keys.S:
					{
						cpu.keyboard[0x8] = 0;
#if DEBUG
						Console.WriteLine("S released -> 8");
#endif
						break;
					}
				case Keys.D:
					{
						cpu.keyboard[0x9] = 0;
#if DEBUG
						Console.WriteLine("D released -> 9");
#endif
						break;
					}
				case Keys.F:
					{
						cpu.keyboard[0xe] = 0;
#if DEBUG
						Console.WriteLine("F released -> E");
#endif
						break;
					}
				case Keys.Z:
					{
						cpu.keyboard[0xa] = 0;
#if DEBUG
						Console.WriteLine("Z released -> A");
#endif
						break;
					}
				case Keys.X:
					{
						cpu.keyboard[0x0] = 0;
#if DEBUG
						Console.WriteLine("X released -> 0");
#endif
						break;
					}
				case Keys.C:
					{
						cpu.keyboard[0xb] = 0;
#if DEBUG
						Console.WriteLine("C released -> B");
#endif
						break;
					}
				case Keys.V:
					{
						cpu.keyboard[0xc] = 0;
#if DEBUG
						Console.WriteLine("V released -> C");
#endif
						break;
					}
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			thr.Abort();
		}
	}
}
