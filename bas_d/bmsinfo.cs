using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bas_d
{
	class bmsinfo
	{
		// Bms file path
		public string Path = ""; 
		// Bms title
		public string Title = "";
		// Bms ShortTitle Get
		public string GetShortTitle()
		{
			string Short = "";
			// separate
			var Strings = Title.Split(new char[] { '(', '[', '{', '-', });
			Short = Strings[0];
			return Short.Trim();
		}
	}

	class bmsinfoget
	{
		public static bmsinfo GetBmsInfo(string path)
		{
			// create
			var inf = new bmsinfo();
			inf.Path = path;
			// open file
			var Sr = new StreamReader(path, Encoding.GetEncoding("Shift-JIS"));
			// read
			while(!Sr.EndOfStream){
				string TxLine = Sr.ReadLine();
				// Match check(Title)
				int TitleSch = TxLine.IndexOf("#TITLE ");
				if(TitleSch >= 0) {
					inf.Title = TxLine.Substring(TitleSch + "#TITLE ".Length);
					break;
				}
			}
			// close
			Sr.Close();
			// end
			return inf;
		}
	}
}
