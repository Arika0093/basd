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
		// Bms subtitle
		public string Subtitle = "";
		// Bms ShortTitle Get
		public string GetShortTitle()
		{
			// If Sub title exist, return raw title
			if(Subtitle != "") {
				return Title;
			}
			// separate
			var Strings = Title.Split(new string[] { "(", "[", "{", "-", "\""},
				StringSplitOptions.RemoveEmptyEntries);
			return Strings[0].Trim();
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
				}
				// Match check(Sub Title)
				int SubtitleSch = TxLine.IndexOf("#SUBTITLE ");
				if(SubtitleSch >= 0) {
					inf.Subtitle = TxLine.Substring(SubtitleSch + "#SUBTITLE ".Length);
				}
			}
			// close
			Sr.Close();
			// end
			return inf;
		}
	}
}
