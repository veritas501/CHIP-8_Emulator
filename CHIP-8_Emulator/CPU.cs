using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CHIP_8_Emulator
{
	/*
		memory layout:
		 0x000~0x080: Reserved for Font Set
		 0x080~0x200: UNK
		 0x200~0xFFF: Chip-8 Program/Data Space
		
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
	class CPU
	{
		public byte[] FontMem = { // 4x5 low-res mode font sprites (0-F)
	0xF0, 0x90, 0x90, 0x90, 0xF0, 0x20, 0x60, 0x20,
	0x20, 0x70, 0xF0, 0x10, 0xF0, 0x80, 0xF0, 0xF0,
	0x10, 0xF0, 0x10, 0xF0, 0xA0, 0xA0, 0xF0, 0x20,
	0x20, 0xF0, 0x80, 0xF0, 0x10, 0xF0, 0xF0, 0x80,
	0xF0, 0x90, 0xF0, 0xF0, 0x10, 0x20, 0x40, 0x40,
	0xF0, 0x90, 0xF0, 0x90, 0xF0, 0xF0, 0x90, 0xF0,
	0x10, 0xF0, 0xF0, 0x90, 0xF0, 0x90, 0x90, 0xE0,
	0x90, 0xE0, 0x90, 0xE0, 0xF0, 0x80, 0x80, 0x80,
	0xF0, 0xE0, 0x90, 0x90, 0x90, 0xE0, 0xF0, 0x80,
	0xF0, 0x80, 0xF0, 0xF0, 0x80, 0xF0, 0x80, 0x80,

	// 8x10 high-res mode font sprites (0-F)
	0x3C, 0x7E, 0xE7, 0xC3, 0xC3, 0xC3, 0xC3, 0xE7,
	0x7E, 0x3C, 0x18, 0x38, 0x58, 0x18, 0x18, 0x18,
	0x18, 0x18, 0x18, 0x3C, 0x3E, 0x7F, 0xC3, 0x06,
	0x0C, 0x18, 0x30, 0x60, 0xFF, 0xFF, 0x3C, 0x7E,
	0xC3, 0x03, 0x0E, 0x0E, 0x03, 0xC3, 0x7E, 0x3C,
	0x06, 0x0E, 0x1E, 0x36, 0x66, 0xC6, 0xFF, 0xFF,
	0x06, 0x06, 0xFF, 0xFF, 0xC0, 0xC0, 0xFC, 0xFE,
	0x03, 0xC3, 0x7E, 0x3C, 0x3E, 0x7C, 0xC0, 0xC0,
	0xFC, 0xFE, 0xC3, 0xC3, 0x7E, 0x3C, 0xFF, 0xFF,
	0x03, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x60, 0x60,
	0x3C, 0x7E, 0xC3, 0xC3, 0x7E, 0x7E, 0xC3, 0xC3,
	0x7E, 0x3C, 0x3C, 0x7E, 0xC3, 0xC3, 0x7F, 0x3F,
	0x03, 0x03, 0x3E, 0x7C, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

	// 6-bit ASCII character patterns
	0x00,  // |        |
	0x10,  // |   *    |
	0x20,  // |  *     |
	0x88,  // |*   *   |
	0xA8,  // |* * *   |
	0x50,  // | * *    |
	0xF8,  // |*****   |
	0x70,  // | ***    |
	0x80,  // |*       |
	0x90,  // |*  *    |
	0xA0,  // |* *     |
	0xB0,  // |* **    |
	0xC0,  // |**      |
	0xD0,  // |** *    |
	0xE0,  // |***     |
	0xF0,  // |****    |

	// 6-bit ASCII characters from 0x100-
	0x46, 0x3E, 0x56,  // @
	0x99, 0x9F, 0x4F,  // A
	0x5F, 0x57, 0x4F,  // B
	0x8F, 0x88, 0x4F,  // C
	0x5F, 0x55, 0x4F,  // D
	0x8F, 0x8F, 0x4F,  // E
	0x88, 0x8F, 0x4F,  // F
	0x9F, 0x8B, 0x4F,  // G
	0x99, 0x9F, 0x49,  // H
	0x27, 0x22, 0x47,  // I
	0xAE, 0x22, 0x47,  // J
	0xA9, 0xAC, 0x49,  // K
	0x8F, 0x88, 0x48,  // L
	0x43, 0x64, 0x53,  // M
	0x99, 0xDB, 0x49,  // N
	0x9F, 0x99, 0x4F,  // O
	0x88, 0x9F, 0x4F,  // P
	0x9F, 0x9B, 0x4F,  // Q
	0xA9, 0x9F, 0x4F,  // R
	0x1F, 0x8F, 0x4F,  // S
	0x22, 0x22, 0x56,  // T
	0x9F, 0x99, 0x49,  // U
	0x22, 0x55, 0x53,  // V
	0x55, 0x44, 0x54,  // W
	0x53, 0x52, 0x53,  // X
	0x22, 0x52, 0x53,  // Y
	0xCF, 0x12, 0x4F,  // Z
	0x8C, 0x88, 0x3C,  // [
	0x10, 0xC2, 0x40,  // \
	0x2E, 0x22, 0x3E,  // ]
	0x30, 0x25, 0x50,  // ^
	0x06, 0x00, 0x50,  // _
	0x00, 0x00, 0x40,  // space
	0x0C, 0xCC, 0x2C,  // !
	0x00, 0x50, 0x45,  // "
	0x65, 0x65, 0x55,  // #
	0x46, 0x46, 0x56,  // $
	0xDF, 0xBF, 0x4F,  // %
	0x5F, 0xAF, 0x4E,  // &
	0x00, 0x80, 0x18,  // '
	0x21, 0x22, 0x41,  // (
	0x12, 0x11, 0x42,  // )
	0x53, 0x56, 0x53,  // *
	0x22, 0x26, 0x52,  // +
	0x2E, 0x00, 0x30,  // ,
	0x00, 0x06, 0x50,  // -
	0xCC, 0x00, 0x20,  // .
	0xC0, 0x12, 0x40,  // /
	0x9F, 0x99, 0x4F,  // 0
	0x22, 0x22, 0x32,  // 1
	0x8F, 0x1F, 0x4F,  // 2
	0x1F, 0x1F, 0x4F,  // 3
	0x22, 0xAF, 0x4A,  // 4
	0x1F, 0x8F, 0x4F,  // 5
	0x9F, 0x8F, 0x4F,  // 6
	0x11, 0x11, 0x4F,  // 7
	0x9F, 0x9F, 0x4F,  // 8
	0x1F, 0x9F, 0x4F,  // 9
	0x80, 0x80, 0x10,  // :
	0x2E, 0x20, 0x30,  // ;
	0x21, 0x2C, 0x41,  // <
	0xE0, 0xE0, 0x30,  // =
	0x2C, 0x21, 0x4C,  // >
	0x88, 0x1F, 0x4F,  // ?

	// Extra scratch memory in the ROM, used for ASCII sprites.
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,};

		public Random rd;

		public short PC;//pointer counter, 2bytes
		public byte SP;//stack pointer, 1bytes
		public short[] Stack;//size: 64 bytes
		public byte[] Mem;//size: 4k bytes

		public byte[] Regs;//V1~VF,1*16bytes
		public short I;//reg Index, 2bytes
		public byte ST;//reg sound timer, 1byte
		public byte DT;//reg delay timer, 1byte

		public bool[,] Framebuf_drawing;//64x32 bit memory
		public bool[,] Framebuf_pending;//64x32 bit memory

		public bool needDraw = false;
		public bool drawFinish = false;

		public static readonly int screenWeight = 64;
		public static readonly int screenHeight = 32;


		public byte[] keyboard;//16个按键



		//delegate
		public delegate void DrawHandler(bool[,] frameBuf, int x, int y);
		public event DrawHandler DrawPic;
		public delegate void SoundHandler(int ms);
		public event SoundHandler MakeSound;

		public CPU()
		{

			//分配内存
			Stack = new short[32];
			Mem = new byte[0x1000];
			Regs = new byte[0x10];
			Framebuf_drawing = new bool[screenWeight, screenHeight];
			Framebuf_pending = new bool[screenWeight, screenHeight];
			keyboard = new byte[0x10];

			PC = 0x200;
			SP = 0;
			I = 0x200;
			ST = 0;
			DT = 0;

			rd = new Random();
		}

		public void Tick()
		{
			if (ST != 0)
			{
				MakeSound((int)(ST * (1000f / 60)));
				ST = 0;
#if DEBUG
						Console.WriteLine("ST--");
#endif
			}

			if (DT != 0)
			{
				DT -= 1;
#if DEBUG
						Console.WriteLine("DT--");
#endif
			}

			if (needDraw)
			{
				DrawPic(Framebuf_pending, screenWeight, screenHeight);
				needDraw = false;
			}
		}

		public bool LoadRomToMem(string filepath)
		{
			FileStream fs;
			try
			{
				fs = File.Open(filepath, FileMode.Open);//open file
				fs.Read(Mem, 0x200, 0xfff - 0x200);//load rom
				Array.Copy(FontMem, Mem, 0x200);//复制前面的font到mem
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				Console.WriteLine(ex);
				return false;
			}

			return true;
		}

		public void Disasm()
		{
			short code = (short)((Mem[PC] << 8) + Mem[PC + 1]);
#if DEBUG
			Console.Write(string.Format("PC:0x{0:X} => ", PC));
#endif
			PC += 2;

			short nnn = (short)(code & 0xfff);
			byte X = (byte)(code >> 8 & 0xf);
			byte KK = (byte)(code & 0xff);
			byte Y = (byte)(code >> 4 & 0xf);

			switch ((code & 0xf000) >> 12)
			{
				case 0:
					{
						switch (code & 0xfff)
						{
							case 0x0e0://00E0 CLS
								{
									DisasmCLS();
#if DEBUG
									Console.WriteLine("CLS");
#endif
									break;
								}
							case 0x0ee://00EE RET
								{
									DisasmRET();
#if DEBUG
									Console.WriteLine("RET");
#endif
									break;
								}
							default://0NNN SYS addr
								{
#if DEBUG
									Console.WriteLine("SYS addr " + string.Format("0x{0:X}", code & 0xfff) + "(unused)");
#endif
									break;
								}
						}
						break;
					}
				case 1:
					{
						//1NNN JP addr
						DisasmJP(nnn);
#if DEBUG
						Console.WriteLine("JP addr " + string.Format("0x{0:X}", nnn));
#endif
						break;
					}
				case 2:
					{
						//2NNN CALL addr
						DisasmCALL(nnn);
#if DEBUG
						Console.WriteLine("CALL addr " + string.Format("0x{0:X}", nnn));
#endif
						break;
					}
				case 3:
					{
						//3XKK SE VX, KK
						DisasmSEVXKK(X, KK);
#if DEBUG
						Console.WriteLine(string.Format("SE V{0:X} == 0x{1:X}", X, KK));
#endif
						break;
					}
				case 4:
					{
						//4XKK SNE VX, KK
						DisasmSNEVXKK(X, KK);
#if DEBUG
						Console.WriteLine(string.Format("SNE V{0:X} == 0x{1:X}", X, KK));
#endif
						break;
					}
				case 5:
					{
						if ((code & 0xf) != 0)
						{
							MessageBox.Show(string.Format("unknown instruction : 0x{0:X}", code));
#if DEBUG
							Console.WriteLine(string.Format("unknown instruction : 0x{0:X}", code));
#endif
							break;
						}
						//5XY0 SE VX, VY
						DisasmSEVXVY(X, Y);
#if DEBUG
						Console.WriteLine(string.Format("SE V{0:X} == V{1:X}", X, Y));
#endif
						break;
					}
				case 6:
					{
						//6XKK LD VX, KK
						DisasmLDVXKK(X, KK);
#if DEBUG
						Console.WriteLine(string.Format("LD V{0:X}, 0x{1:X}", X, KK));
#endif
						break;
					}
				case 7:
					{
						//7XKK ADD VX, KK
						DisasmADDVXKK(X, KK);
#if DEBUG
						Console.WriteLine(string.Format("ADD V{0:X}, 0x{1:X}", X, KK));
#endif
						break;
					}
				case 8:
					{

						switch (code & 0xf)
						{
							case 0://8XY0 LD Vx,Vy
								{
									DisasmLDVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("LD V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 1://8XY1 OR Vx,Vy
								{
									DisasmORVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("OR V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 2://8XY2 AND Vx,Vy
								{
									DisasmANDVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("AND V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 3://8XY3 XOR Vx,Vy
								{
									DisasmXORVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("XOR V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 4://8XY4 ADD Vx,Vy
								{
									DisasmADDVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("ADD V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 5://8XY5 SUB Vx,Vy
								{
									DisasmSUBVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("SUB V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 6://8XY6 SHR Vx,1
								{
									DisasmSHRVX(X);
#if DEBUG
									Console.WriteLine(string.Format("SHR V{0:X}, 1", X));
#endif
									break;
								}
							case 7://8XY7 SUBN Vx,Vy
								{
									DisasmSUBNVXVY(X, Y);
#if DEBUG
									Console.WriteLine(string.Format("SUBN V{0:X}, V{1:X}", X, Y));
#endif
									break;
								}
							case 0xe://8XYE SHL Vx,1
								{
									DisasmSHLVX(X);
#if DEBUG
									Console.WriteLine(string.Format("SHL V{0:X}, 1", X));
#endif
									break;
								}
							default:
								{
									MessageBox.Show(string.Format("unknown instruction : 0x{0:X}", code));
#if DEBUG
									Console.WriteLine(string.Format("unknown instruction : 0x{0:X}", code));
#endif
									break;
								}
						}
						break;
					}
				case 9:
					{
						if ((code & 0xf) != 0)
						{
							MessageBox.Show(string.Format("unknown instruction : 0x{0:X}", code));
#if DEBUG
							Console.WriteLine(string.Format("unknown instruction : 0x{0:X}", code));
#endif
							break;
						}
						//9XY0 SNE VX, VY
						DisasmSNEVXVY(X, Y);
#if DEBUG
						Console.WriteLine(string.Format("SNE V{0:X} == V{1:X}", X, Y));
#endif
						break;
					}
				case 0xa:
					{
						//ANNN LD I, addr
						DisasmLDI(nnn);
#if DEBUG
						Console.WriteLine(string.Format("LD I, 0x{0:X}", nnn));
#endif
						break;
					}
				case 0xb:
					{
						//BNNN JP V0, addr
						DisasmJPV0(nnn);
#if DEBUG
						Console.WriteLine(string.Format("JP V0, 0x{0:X}", nnn));
#endif
						break;
					}
				case 0xc:
					{
						//CXKK RND VX, byte
						DisasmRND(X, KK);
#if DEBUG
						Console.WriteLine(string.Format("RND V{0:X}, 0x{1:X}", X, KK));
#endif
						break;
					}
				case 0xd:
					{
						//DXYN DRW VX,VY,nibble

						DisasmDRW(X, Y, (byte)(code & 0xf));
#if DEBUG
						Console.WriteLine(string.Format("DRW V{0:X}, V{1:X}, 0x{2:X}", X, Y, code & 0xf));
#endif
						break;
					}
				case 0xe:
					{
						switch (KK)
						{
							case 0x9e:
								{
									//EX9E SKP VX
									DisasmSKP(X);
#if DEBUG
									Console.WriteLine(string.Format("SKP V{0:X}", X));
#endif
									break;
								}
							case 0xa1:
								{
									//EXA1 SKNP VX
									DisasmSKNP(X);
#if DEBUG
									Console.WriteLine(string.Format("SKNP V{0:X}", X));
#endif
									break;
								}
							default:
								{
									MessageBox.Show(string.Format("unknown instruction : 0x{0:X}", code));
#if DEBUG
									Console.WriteLine(string.Format("unknown instruction : 0x{0:X}", code));
#endif
									break;
								}
						}
						break;
					}
				case 0xf:
					{
						switch (KK)
						{
							case 0x07:
								{
									//FX07 LD VX, DT
									DisasmLDVXDT(X);
#if DEBUG
									Console.WriteLine(string.Format("LD V{0:X}, DT", X));
#endif
									break;
								}
							case 0x0a:
								{
									//FX0A LD VX, K
									DisasmLDVXK(X);
#if DEBUG
									Console.WriteLine(string.Format("LD V{0:X}, K", X));
#endif
									break;
								}
							case 0x15:
								{
									//FX15 LD DT, VX
									DisasmLDDTVX(X);
#if DEBUG
									Console.WriteLine(string.Format("LD DT, V{0:X}", X));
#endif
									break;
								}
							case 0x18:
								{
									//FX18 LD ST, VX
									DisasmLDSTVX(X);
#if DEBUG
									Console.WriteLine(string.Format("LD ST, V{0:X}", X));
#endif
									break;
								}
							case 0x1e:
								{
									//FX1E ADD I, VX
									DisasmADDIVX(X);
#if DEBUG
									Console.WriteLine(string.Format("ADD I, V{0:X}", X));
#endif
									break;

								}
							case 0x29:
								{
									//FX29 LD F, VX
									DisasmLDFVX(X);
#if DEBUG
									Console.WriteLine(string.Format("LD F, V{0:X}", X));
#endif
									break;
								}
							case 0x33:
								{
									//FX33 LD B, VX
									DisasmLDBVX(X);
#if DEBUG
									Console.WriteLine(string.Format("LD B, V{0:X}", X));
#endif
									break;
								}
							case 0x55:
								{
									//FX55 LD [I], VX
									DisasmLDIVX(X);
#if DEBUG
									Console.WriteLine(string.Format("LD [I], V{0:X}", X));
#endif
									break;
								}
							case 0x65:
								{
									//FX65 LD VX, [I]
									DisasmLDVXI(X);
#if DEBUG
									Console.WriteLine(string.Format("LD V{0:X}, [I]", X));
#endif
									break;
								}
							default:
								{
									MessageBox.Show(string.Format("unknown instruction : 0x{0:X}", code));
#if DEBUG
									Console.WriteLine(string.Format("unknown instruction : 0x{0:X}", code));
#endif
									break;
								}
						}
						break;
					}

			}

		}
		//==================================================================

		public void DisasmCLS()//00E0
		{
			Array.Clear(Framebuf_drawing, 0, Framebuf_drawing.Length);
			Array.Clear(Framebuf_pending, 0, Framebuf_drawing.Length);
		}

		public void DisasmRET()//00EE
		{
			PC = Stack[SP];
			if (SP != 0)
			{
				SP--;
			}
			else
			{
				MessageBox.Show(string.Format("stack is empty while RET"));
#if DEBUG
				Console.WriteLine(string.Format("stack is empty while RET"));
#endif
			}

		}

		public void DisasmJP(short addr)//1NNN
		{
			PC = addr;
		}

		public void DisasmCALL(short addr)//2NNN
		{
			SP++;
			Stack[SP] = PC;

			PC = addr;
		}

		public void DisasmSEVXKK(byte X, byte KK)//3XKK
		{
			if (Regs[X] == KK)
			{
				PC += 2;
			}
		}

		public void DisasmSNEVXKK(byte X, byte KK)//4XKK
		{
			if (Regs[X] != KK)
			{
				PC += 2;
			}
		}

		public void DisasmSEVXVY(byte X, byte Y)//5XY0
		{
			if (Regs[X] == Regs[Y])
			{
				PC += 2;
			}
		}

		public void DisasmLDVXKK(byte X, byte KK)//6XKK
		{
			Regs[X] = KK;
		}

		public void DisasmADDVXKK(byte X, byte KK)//7XKK
		{
			Regs[X] += KK;
		}

		public void DisasmLDVXVY(byte X, byte Y)//8XY0
		{
			Regs[X] = Regs[Y];
		}

		public void DisasmORVXVY(byte X, byte Y)//8XY1
		{
			Regs[X] |= Regs[Y];
		}

		public void DisasmANDVXVY(byte X, byte Y)//8XY2
		{
			Regs[X] &= Regs[Y];
		}

		public void DisasmXORVXVY(byte X, byte Y)//8XY3
		{
			Regs[X] ^= Regs[Y];
		}

		public void DisasmADDVXVY(byte X, byte Y)//8XY4
		{
			Regs[0xf] = (byte)(Regs[X] + Regs[Y] > 0xff ? 1 : 0);
			Regs[X] += Regs[Y];
		}

		public void DisasmSUBVXVY(byte X, byte Y)//8XY5
		{
			Regs[0xf] = (byte)(Regs[X] > Regs[Y] ? 1 : 0);
			Regs[X] -= Regs[Y];
		}

		public void DisasmSHRVX(byte X)//8XY6
		{
			Regs[0xf] = (byte)((Regs[X] & 1) != 0 ? 1 : 0);
			Regs[X] >>= 1;
		}

		public void DisasmSUBNVXVY(byte X, byte Y)//8XY7
		{
			Regs[0xf] = (byte)(Regs[Y] > Regs[X] ? 1 : 0);
			Regs[X] = (byte)(Regs[Y] - Regs[X]);
		}

		public void DisasmSHLVX(byte X)//8XYE
		{
			Regs[0xf] = (byte)(Regs[X] >> 7);
			Regs[X] <<= 1;
		}

		public void DisasmSNEVXVY(byte X, byte Y)//9XY0
		{
			if (Regs[X] != Regs[Y])
			{
				PC += 2;
			}
		}

		public void DisasmLDI(short addr)//ANNN
		{
			I = addr;
		}

		public void DisasmJPV0(short addr)//BNNN
		{
			PC = (short)(Regs[0] + addr);
		}

		public void DisasmRND(byte X, byte KK)//CXKK
		{
			Regs[X] = (byte)(rd.Next(0xff) & KK);
		}

		/// <summary>
		/// 画图函数
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="nibble"></param>
		public void DisasmDRW(byte X, byte Y, byte nibble)//DXYN
		{
			//Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change after the execution of this instruction. As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen

			//执行到这条指令时设置needDraw,否则会不刷新.
			needDraw = true;

			Regs[0xf] = 0;
			byte startX = Regs[X];
			byte startY = Regs[Y];
			int pixel = 0;

			for (int i = 0; i < nibble; i++)
			{
				byte pixelLine = Mem[I + i];
				for (int j = 0; j < 8; j++)
				{
					if ((startX + j) < screenWeight && (startY + i) < screenHeight)//越界
					{
						pixel = Framebuf_drawing[startX + j, startY + i] ? 1 : 0;
						Framebuf_drawing[startX + j, startY + i] = (pixel ^ ((pixelLine >> (7 - j)) & 1)) == 1 ? true : false;
						if (!Framebuf_drawing[startX + j, startY + i] && Framebuf_pending[startX + j, startY + i])//之前为1,之后为0,设置VF=1
						{
							Regs[0xf] = 1;
						}
					}
				}
			}

			//drawing完的显示图形放到pending
			Array.Copy(Framebuf_drawing, Framebuf_pending, screenWeight * screenHeight);
		}

		public void DisasmSKP(byte X)//EX9E
		{
			if (keyboard[Regs[X]] == 1)
			{
				PC += 2;
			}
		}

		public void DisasmSKNP(byte X)//EXA1
		{
			if (keyboard[Regs[X]] == 0)
			{
				PC += 2;
			}
		}

		public void DisasmLDVXDT(byte X)//FX07
		{
			Regs[X] = DT;
		}

		public void DisasmLDVXK(byte X)//FX0A
		{
			for (byte i = 0; i < 0x10; i++)
			{
				if (keyboard[i] == 1)
				{
					Regs[X] = i;
					return;
				}
			}

			PC -= 2;//如果所有键都没有按下,延时后重新执行这条指令.
		}

		public void DisasmLDDTVX(byte X)//FX15
		{
			DT = Regs[X];
		}

		public void DisasmLDSTVX(byte X)//FX18
		{
			ST = Regs[X];
		}

		public void DisasmADDIVX(byte X)//FX1E
		{
			I += Regs[X];
		}

		public void DisasmLDFVX(byte X)//FX29
		{
			I = (short)(Regs[X] * 5);
		}

		public void DisasmLDBVX(byte X)//FX33
		{
			Mem[I + 0] = (byte)((Regs[X] / 100) % 10);
			Mem[I + 1] = (byte)((Regs[X] / 10) % 10);
			Mem[I + 2] = (byte)(Regs[X] % 10);
		}

		public void DisasmLDIVX(byte X)//FX55
		{
			for (byte i = 0; i <= X; i++)
			{
				Mem[I + i] = Regs[i];
			}
		}

		public void DisasmLDVXI(byte X)//FX65
		{
			for (byte i = 0; i <= X; i++)
			{
				Regs[i] = Mem[I + i];
			}
		}
	}
}
