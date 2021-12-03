using System;
using System.Net;
using System.Text;

namespace ChatCoreTest
{
	internal class Program
	{
		private static byte[] m_MessageData;
		private static byte[] m_PacketData;
		private static uint m_Pos;

		public static void Main(string[] args)
		{
			ChatClient m_chatClient = new ChatClient();

			m_chatClient.Connect("192.168.24.57", 4099);

			m_PacketData = new byte[1024];
			m_MessageData = new byte[1024];
			m_Pos = 0;

			Console.WriteLine("Please enter your name:");
			Write(Console.ReadLine());

			Write(109);
			Write(109.99f);
			Write("Hello!");


			Array.Resize(ref m_MessageData, (int)m_Pos);
			CreatePacket((int)m_Pos);

			m_chatClient.SendData(m_PacketData);

			m_chatClient.Disconnect();
			//Console.ReadLine();

		}

		private static bool CreatePacket(int i)
		{

			var bytes = BitConverter.GetBytes(i);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			bytes.CopyTo(m_PacketData, 0);
			m_MessageData.CopyTo(m_PacketData, bytes.Length);
			return true;
		}

		// write an integer into a byte array
		private static bool Write(int i)
		{
			// convert int to byte array
			var bytes = BitConverter.GetBytes(i);
			_Write(bytes);
			return true;
		}

		// write a float into a byte array
		private static bool Write(float f)
		{
			// convert int to byte array
			var bytes = BitConverter.GetBytes(f);
			_Write(bytes);
			return true;
		}

		// write a string into a byte array
		private static bool Write(string s)
		{
			// convert string to byte array
			var bytes = Encoding.Unicode.GetBytes(s);

			// write byte array length to packet's byte array
			if (Write(bytes.Length) == false)
			{
				return false;
			}

			_Write(bytes);
			return true;
		}

		// write a byte array into packet's byte array
		private static void _Write(byte[] byteData)
		{
			// converter little-endian to network's big-endian
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(byteData);
			}

			byteData.CopyTo(m_MessageData, m_Pos);
			m_Pos += (uint)byteData.Length;
		}
	}
}
