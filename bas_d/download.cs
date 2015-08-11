// --------------------------------
// file download class
// --------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace bas_d
{
	class download
	{
		public const string DlDirectory = "score/";

		public static string GetResource(string Url)
		{
			string DlPath = DlDirectory + Path.GetFileName(Url);
			if(File.Exists(DlPath)){
				return DlPath;
			}
			if(!IsExistPath(Url)) {
				return null;
			}
			var Wc = new WebClient();
			Wc.DownloadFile(Url, DlPath);
			Console.WriteLine("File Download: Success.");
			return DlPath;
		}

		public static bool IsExistPath(string Url)
		{
			HttpWebResponse response = null;
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(Url);
				request.Method = "HEAD";
				response = (HttpWebResponse)request.GetResponse();
			}
			catch
			{
				Console.WriteLine("Error: Path({0}) is not exist.", Url);
				return false;
			}
			finally
			{
				if (response != null)
				{
					response.Close();
				}
			}
			return true;		
		}
	}
}
