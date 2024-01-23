using System.Net;

namespace Web_Server_console_app
{
	class Program
	{
		private static HttpListener listener;
		private static string[] prefixes = { "http://localhost:8080/" };
		private static string contentPath = "C:\\Users\\andr515i\\Desktop\\skole\\H3\\web api asp net\\Web Server console app\\Web Server console app\\Webfolder\\";


		static void Main()
		{
			listener = new HttpListener();
			foreach (string prefix in prefixes)
			{
				listener.Prefixes.Add(prefix);
			}

			listener.Start();
			Console.WriteLine("Server is running. Press Ctrl+C to stop.");

			while (true)
			{
				HttpListenerContext context = listener.GetContext();
				ThreadPool.QueueUserWorkItem(HandleRequest, context);
			}
		}

		private static void HandleRequest(object state)
		{
			try
			{


			HttpListenerContext context = (HttpListenerContext)state;
			string url = context.Request.RawUrl.ToLower();
			string filePath = Path.Combine(contentPath, url.Trim('/'));

			byte[] responseBuffer;

			if (File.Exists(filePath) && filePath.StartsWith(contentPath))
			{
				// Read the content of the requested file
				string fileContent = File.ReadAllText(filePath);
				responseBuffer = GetBytes(fileContent);
			}
			else
			{
				responseBuffer = GetBytes(File.ReadAllText(contentPath + "notFound.html"));
			}
			context.Response.StatusCode = 404;
			context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
			context.Response.StatusDescription = "Not Found";
			context.Response.Close();
							}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private static byte[] GetBytes(string s)
		{
			return System.Text.Encoding.UTF8.GetBytes(s);
		}

		private void 
	}

}
