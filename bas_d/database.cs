// --------------------------------
// database access class
// --------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace bas_d
{
	class database
	{
		public static bool SaveDbPath(string lr2dir)
		{
			if(File.Exists(lr2dir + "/lr2files/database/song.db")) {
				var dbpath = lr2dir + "\\lr2files\\database\\song.db";
				var fs = File.Create("data/dbpath");
				var sw = new StreamWriter(fs);
				sw.Write(dbpath);
				sw.Close();
				fs.Close();
				Console.WriteLine("Database Path Save: Success.");
				return true;
			}
			return false;
		}

		public static string GetBmsDirectory(string SongTitle)
		{
			// Open Database
			var Cn = OpenLR2Database();
			// Command Create
			var Cmd = Cn.CreateCommand();
			Cmd.CommandText = "SELECT path FROM song WHERE title LIKE \"" + SongTitle + "\"";
			// Read
			var reader = Cmd.ExecuteReader();
			if(reader == null) {
				return null;
			}
			return reader.Read() ? Path.GetDirectoryName(reader["path"].ToString()) : null;
		}

		public static SQLiteConnection OpenLR2Database()
		{
			// Path Get
			if(LR2Handle == null){
				var sr = new StreamReader("data/dbpath");
				var path = sr.ReadToEnd();
				sr.Close();
				LR2Handle = new SQLiteConnection("Data Source=" + path);
				LR2Handle.Open();
			}
			return LR2Handle;
		}

		public static void CloseLR2Database()
		{
			LR2Handle.Close();
			LR2Handle = null;
		}

		private static SQLiteConnection LR2Handle = null;
	}
}
