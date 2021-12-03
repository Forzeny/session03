using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatCoreTest
{
	public class ChatServer
	{
		private int m_port;
		private TcpListener m_listener;
		private Thread m_handleThread;
		private readonly Dictionary<string, TcpClient> m_clients = new Dictionary<string, TcpClient>();
		private readonly Dictionary<string, string> m_userNames = new Dictionary<string, string>();

		public ChatServer()
		{
		}

		public void Bind(int port)
		{
			m_port = port;

			m_listener = new TcpListener(IPAddress.Any, port);
			Console.WriteLine("Server start at port {0}", port);
			m_listener.Start();
		}

		public void Start()
		{
			m_handleThread = new Thread(ClientsHandler);
			m_handleThread.Start();

			while (true)
			{
				Console.WriteLine("Waiting for a connection... ");
				var client = m_listener.AcceptTcpClient();

				var clientId = client.Client.RemoteEndPoint.ToString();
				Console.WriteLine("Client has connected from {0}", clientId);

				lock (m_clients)
				{
					m_clients.Add(clientId, client);
					m_userNames.Add(clientId, "Unknown");
				}
			}
		}

		private void ClientsHandler()
		{
			while (true)
			{
				var disconnectedClients = new List<string>();

				lock (m_clients)
				{
					foreach (var clientId in m_clients.Keys)
					{
						var client = m_clients[clientId];

						try
						{
							if (!client.Connected)
							{
								disconnectedClients.Add(clientId);
							}
							if (client.Available > 0)
							{
								ReceiveMessage(clientId);
							}
						}
						catch (Exception e)
						{
							Console.WriteLine("Client {0} Receive Error: {1}", clientId, e.Message);
						}
					}

					foreach (var clientId in disconnectedClients)
					{
						RemoveClient(clientId);
					}
				}
			}
		}

		private void RemoveClient(string clientId)
		{
			Console.WriteLine("Client {0} has disconnected...", clientId);
			var client = m_clients[clientId];
			m_clients.Remove(clientId);
			m_userNames.Remove(clientId);
			client.Close();
		}

		private int pos = 0;
		private void ReceiveMessage(string clientId)
		{
			pos = 0;

			var client = m_clients[clientId];
			var stream = client.GetStream();

			var numBytes = client.Available;
			var buffer = new byte[numBytes];

			var bytes = new byte[numBytes];
			stream.Read(buffer, 0, numBytes);


			//read length
			var bytesRead = ReadInt(buffer);
			Console.WriteLine("Message length : " + bytesRead);

			//read name
			m_userNames[clientId] = ReadString(buffer);
			Console.WriteLine("User : " + m_userNames[clientId]);

			//read message
			Console.WriteLine(ReadInt(buffer));
			Console.WriteLine(ReadFloat(buffer));
			Console.WriteLine(ReadString(buffer));
		}

		private int ReadInt(byte[] buffer)
		{
			var bytes = new byte[4];
			Array.Copy(buffer, pos, bytes, 0, 4);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			var request = BitConverter.ToInt32(bytes, 0);
			pos += bytes.Length;
			return request;
		}
		private float ReadFloat(byte[] buffer)
		{
			var bytes = new byte[4];
			Array.Copy(buffer, pos, bytes, 0, 4);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			var request = BitConverter.ToSingle(bytes, 0);
			pos += bytes.Length;
			return request;
		}
		private string ReadString(byte[] buffer)
		{
			//read string length
			var stringLength = ReadInt(buffer);

			//read string
			var bytes = new byte[stringLength];
			Array.Copy(buffer, pos, bytes, 0, stringLength);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			var message = System.Text.Encoding.Unicode.GetString(bytes);
			pos += bytes.Length;
			return message;
		}

		private void Broadcast(string senderId, string message)
		{
			var data = $"MESSAGE:{m_userNames[senderId]}:{message}";
			var buffer = System.Text.Encoding.ASCII.GetBytes(data);

			foreach (var clientId in m_clients.Keys)
			{
				if (clientId != senderId)
				{
					try
					{
						m_clients[clientId].GetStream().Write(buffer, 0, buffer.Length);
					}
					catch (Exception e)
					{
						Console.WriteLine("Client {0} Send Failed: {1}", clientId, e.Message);
					}
				}
			}
		}
	}
}
