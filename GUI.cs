using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using static nel.NoelAnimator;

namespace BobOnGradle
{
	public class GUI
	{
		Plugin plugin;
		Process process;
		Socket socket;
		ConcurrentQueue<string> queue = new();
		public GUI(Plugin plugin,Process process)
		{
			this.plugin = plugin;
			this.process = process;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23456));
			Thread t = new Thread(() => receive(socket, queue));
			t.Start();
		}
		public static void receive(Socket socket,ConcurrentQueue<string> queue)
		{
			while(true)
			{
				byte[] b = new byte[1024];
				int len=socket.Receive(b);
				string s = Encoding.UTF8.GetString(b,0,len);
				queue.Enqueue(s);
			}
		}
		public void update()
		{
			if(process.HasExited)
			{
				Application.Quit(1);
				return;
			}
			//Console.WriteLine("updating");
			while(!queue.IsEmpty)
			{
				string s;
				queue.TryDequeue(out s);
				parse(s);
			}
			//Console.WriteLine("updated");
		}
		void parse(string s)
		{
			Console.WriteLine("parsing "+s);
			if (s.Equals("keepalive"))
				return;
			string[] p = s.Split(new char[] { ' ' });
			string arg0 = p[0], arg1 = p[1];
			if (p[0]=="enable")
			{
				plugin.modules.enable(arg1);
			}
			else
			{
				plugin.modules.disable(arg1);
			}
		}
	}
}
