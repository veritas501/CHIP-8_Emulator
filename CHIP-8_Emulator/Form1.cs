using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
		Graphics gp;

		readonly TimeSpan Tick60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
		readonly TimeSpan CPUTick = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);
		readonly Stopwatch stopWatch = Stopwatch.StartNew();

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

		public bool[,] Framebuf_screen;

		Thread thrGameloop;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//初始化画图
			gp = Graphics.FromHwnd(screen.Handle);
			sbFalse = new SolidBrush(colorFalse);
			sbTrue = new SolidBrush(colorTrue);

			Framebuf_screen = new bool[CPU.screenWeight, CPU.screenHeight];
			Array.Clear(Framebuf_screen, 0, Framebuf_screen.Length);

			//RunGame("MAZE");
			//RunGame("PONG");
			//RunGame("TETRIS");
		}

		void RunGame(string romPath)
		{
			//初始化cpu
			cpu = new CPU();
			//初始化delegate
			cpu.DrawPic += new CPU.DrawHandler(DrawPic);
			cpu.MakeSound += new CPU.SoundHandler(MakeSound);

			//cpu加载ROM
			if (cpu.LoadRomToMem(romPath))
			{
				//加载成功
				thrGameloop = new Thread(GameLoop) { IsBackground = true };
				thrGameloop.Start();
			}
		}

		//主要的循环,模拟时钟,cpu执行指令,60hz的屏幕刷新以及ST和DT寄存器
		void GameLoop()
		{
			TimeSpan lastTime = stopWatch.Elapsed;
			TimeSpan currentTime;
			TimeSpan elapsedTime;

			while (true)
			{
				currentTime = stopWatch.Elapsed;
				elapsedTime = currentTime - lastTime;

				while (elapsedTime >= Tick60Hz)
				{
					Invoke((Action)(() => { cpu.Tick(); }));

					elapsedTime -= Tick60Hz;
					lastTime += Tick60Hz;
				}
				Invoke((Action)(() => { cpu.Disasm(); }));

				Thread.Sleep(CPUTick);
			}
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
			//终止线程
			try
			{
				thrGameloop.Abort();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		//delegate 画图
		public void DrawPic(bool[,] frameBuf, int x, int y)
		{
			
			if (screen.InvokeRequired)
			{
				CPU.DrawHandler sch = new CPU.DrawHandler(DrawPic);
				this.Invoke(sch, new object[] { frameBuf, x, y });
			}
			else
			{
				if (firstDraw)
				{
					firstDraw = false;
					gp.Clear(colorFalse);
				}

				//update picturebox1 image
				int pixelSize = screen.Size.Width / 64;

				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						if (frameBuf[i, j] != Framebuf_screen[i, j])
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

				Array.Copy(frameBuf, Framebuf_screen, CPU.screenWeight * CPU.screenHeight);
			}
		}

		//delegate 发声
		public void MakeSound(int ms)
		{
			Console.Beep(1000, ms);
		}

		//打开游戏ROM文件
		private void openRomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openfd = new OpenFileDialog();

			openfd.Title = "选择游戏ROM:";
			openfd.Filter = "ROM文件(*.*)|*.*";
			if (openfd.ShowDialog() == DialogResult.OK)
			{
				string wholePath = openfd.FileName;

				try
				{
					thrGameloop.Abort();
				}catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
				
				firstDraw = true;
				Array.Clear(Framebuf_screen, 0, Framebuf_screen.Length);
				RunGame(wholePath);
			}
		}
	}
}
