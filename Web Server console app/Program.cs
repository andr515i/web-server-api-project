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
			// Initialize the HttpListener and add prefixes
			listener = new HttpListener();
			foreach (string prefix in prefixes)
			{
				listener.Prefixes.Add(prefix);
			}

			listener.Start();
			Console.WriteLine("Server is running. Press Ctrl+C to stop.");

			while (true)
			{
				// Listen for incoming requests and handle them asynchronously
				HttpListenerContext context = listener.GetContext();
				ThreadPool.QueueUserWorkItem(HandleRequest, context);
			}
		}

		private static async void HandleRequest(object state)
		{
			try
			{
				HttpListenerContext context = (HttpListenerContext)state;
				string url = context.Request.RawUrl.ToLower();
				string filePath = Path.Combine(contentPath, url.Trim('/'));

				byte[] responseBuffer;

				if (File.Exists(filePath) && filePath.StartsWith(contentPath))
				{
					// Serve static files from the Webfolder
					string fileContent = File.ReadAllText(filePath);
					responseBuffer = GetBytes(fileContent);
					context.Response.StatusCode = 200;
					context.Response.StatusDescription = "Ok";
				}
				else if (url.Contains("bt.dk/nyheder"))
				{
					// Fetch and serve HTML content from "bt.dk/nyheder"
					string responseBody = await MethodName("nyheder");

					// Convert relative URLs to absolute URLs
					responseBody = responseBody.Replace("href=\"/", "href=\"https://bt.dk/");

					responseBuffer = GetBytes(responseBody);

					context.Response.StatusCode = 200;
					context.Response.StatusDescription = "Ok";
				}
				else if (url.Contains("bt.dk"))
				{
					// Fetch and serve HTML content from "bt.dk"
					responseBuffer = GetBytes(await MethodName(url));
					context.Response.StatusCode = 200;
					context.Response.StatusDescription = "Ok";
				}
				else
				{
					// Serve a custom "notFound.html" for other cases
					responseBuffer = GetBytes(File.ReadAllText(contentPath + "notFound.html"));

					context.Response.StatusCode = 404;
					context.Response.StatusDescription = "Not Found";
				}

				// Write the response to the output stream and close the response
				context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
				context.Response.Close();
			}
			catch (Exception e)
			{
				// Log any exceptions that occur during request handling
				await Console.Out.WriteLineAsync(e.Message);
			}
		}

		private static async Task<string> MethodName(string request)
		{
			HttpClient client = new HttpClient();

			try
			{
				// Set the base address for the HttpClient to "https://bt.dk/"
				client.BaseAddress = new Uri("https://bt.dk/");

				// Fetch HTML content from the specified URL
				using HttpResponseMessage response = await client.GetAsync(request);
				response.EnsureSuccessStatusCode();
				string responseBody = await response.Content.ReadAsStringAsync();

				// Return the HTML content
				return responseBody;
			}
			catch (HttpRequestException e)
			{
				// Handle the case where the address is not found
				return "Could not find the address you were searching for.\n" + e.Message;
			}
		}

		private static byte[] GetBytes(string s)
		{
			// Convert a string to UTF-8 encoded bytes
			return System.Text.Encoding.UTF8.GetBytes(s);
		}
	}
}
