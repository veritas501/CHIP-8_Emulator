﻿using System;
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
		Graphics gp;

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

		Dictionary<Keys, int> keyMap = new Dictionary<Keys, int>
			{
				{ Keys.D1, 0x1 },
				{ Keys.D2, 0x2 },
				{ Keys.D3, 0x3 },
				{ Keys.D4, 0xc },

				{ Keys.Q, 0x4 },
				{ Keys.W, 0x5 },
				{ Keys.E, 0x6 },
				{ Keys.R, 0xd },

				{ Keys.A, 0x7 },
				{ Keys.S, 0x8 },
				{ Keys.D, 0x9 },
				{ Keys.F, 0xe },

				{ Keys.Z, 0xa },
				{ Keys.X, 0x0 },
				{ Keys.C, 0xb },
				{ Keys.V, 0xf },
			};

		//绘图颜色
		readonly Color colorFalse = Color.FromArgb(143, 145, 133);
		readonly Color colorTrue = Color.FromArgb(17, 29, 43);

		SolidBrush sbFalse;
		SolidBrush sbTrue;

		//是否初始画板(第一次画图)
		bool firstDraw = true;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			gp = Graphics.FromHwnd(pictureBox1.Handle);
			sbFalse = new SolidBrush(colorFalse);
			sbTrue = new SolidBrush(colorTrue);

			thr = new Thread(new ThreadStart(RunGame));
			thr.IsBackground = true;
			thr.Start();
		}

		public void RunGame()
		{
			cpu = new CPU();
			cpu.DrawPic += new CPU.DrawHandler(DrawPic);

			//cpu.LoadRomToMem("MAZE");
			//cpu.LoadRomToMem("PONG");
			cpu.LoadRomToMem("TETRIS");


			cpu.Run();
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			//包含此键
			if (keyMap.ContainsKey(e.KeyCode))
			{
				//键按下
				cpu.keyboard[keyMap[e.KeyCode]] = 1;
#if DEBUG
				Console.WriteLine("{0} pressed -> {1:X}", e.KeyCode.ToString(), keyMap[e.KeyCode]);
#endif
			}
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			//包含此键
			if (keyMap.ContainsKey(e.KeyCode))
			{
				//键弹起
				cpu.keyboard[keyMap[e.KeyCode]] = 0;
#if DEBUG
				Console.WriteLine("{0} released -> {1:X}", e.KeyCode.ToString(), keyMap[e.KeyCode]);
#endif
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{

			thr.Abort();
		}

		//delegate update pic
		public void DrawPic(bool[,] frameBuf, bool[,] frameBuf_bak, int x, int y)
		{


			if (pictureBox1.InvokeRequired)
			{
				CPU.DrawHandler sch = new CPU.DrawHandler(DrawPic);
				this.Invoke(sch, new object[] { frameBuf, frameBuf_bak, x, y });
			}
			else
			{
				if (firstDraw)
				{
					firstDraw = false;
					gp.Clear(colorFalse);
				}

				//update picturebox1 image
				int pixelSize = pictureBox1.Size.Width / 64;

				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						if (frameBuf[i, j] != frameBuf_bak[i, j])
						{
							lock (gp)
							{
								try
								{
									//画矩形
									gp.FillRectangle(
										frameBuf[i, j] ? sbTrue : sbFalse,
										i * pixelSize,
										j * pixelSize,
										pixelSize,
										pixelSize);
								}
								catch (Exception ex)
								{
									MessageBox.Show("Paint Exception: " + ex.ToString());
#if DEBUG
									Console.WriteLine("Paint Exception: " + ex);
#endif
									throw;
								}
							}
						}
					}
				}
			}
		}
	}
}
