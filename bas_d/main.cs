// --------------------------------
// basd (bms append score downloader)
//  -?: show this help
//  -s: set LR2 path
//      (ex: -s "D:/LR2beta3/")
//  -g: get file to url
//      (ex: -g "http://hoge.com/bms/append.zip")
//	-m: move .bmx file from select directory
//	    (ex: -m "D:/bmsfiles/unflied/")
// --------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace bas_d
{
	class Program
	{
		const string HelpMsg =
			"+------------------------------------------------------------------------+" + "\n" +
			" basd (bms append score downloader)" + "\n" +
			"  BMS差分ファイルをダウンロードし、解凍して導入する作業を自動で行います。" + "\n" +
			"+------------------------------------------------------------------------+" + "\n" +
			" -?: このヘルプを表示します。" + "\n" +
			" -s: LR2のパスを指定します。差分自動導入に必須です。" + "\n" +
			"    (ex: -s \"D:/LR2beta3/\")" + "\n" +
			" -g: URLから譜面をダウンロードし、解凍後自動で導入を行います。-gは省略可。" + "\n" +
			"    (ex: -g \"http://hoge.com/bms/append.zip\")" + "\n" +
			" -m: 指定されたフォルダに存在する差分を自動で導入します。" + "\n" +
			"    (ex: -m \"D:/bmsfiles/unflied/\")" + "\n" +
			"+------------------------------------------------------------------------+" + "\n" +
			" 注意点: 差分自動導入は完全ではないので、失敗することがあります。" + "\n" +
			" 　　　  (特に曲名が違う場合は無理です。)" + "\n" +
			" 　　　  その場合は、お手数ですが手動での導入をお願いします。" + "\n" +
			" 　　　  また、LR2側で更新をしないと導入が完了しない場合があります。" + "\n" +
			"+------------------------------------------------------------------------+";

		static void Main(string[] args)
		{
			// Option Get
			if(args.Length <= 0) {
				// Error
				Console.WriteLine(HelpMsg);
				return;
			}
			for(int i=0; i<args.Length; i++) {
				// switch
				switch(args[i]) {
					// Help
					case "-?":
					case "/?":
					case "-h":
					case "/h":
					case "-help":
						Console.WriteLine(HelpMsg);
						break;
					// Create Database
					case "-s":
					case "/s":
					case "-set":
						// Directory check
						if(Directory.Exists(NextParamCheck(args, i))) {
							// create
							database.SaveDbPath(args[i + 1]);
						}
						else {
							// Error
							Console.WriteLine("Error: Unexist Directory.");
							break;
						}
						i++;
						break;
					// Get File
					case "-g":
					case "/g":
					case "-get":
						// Get
						GetBmsFile(NextParamCheck(args, i));
						// End
						i++;
						break;
					// Move .bmx File
					case "-m":
					case "/m":
					case "-move":
						// Move
						MoveBmsFile(NextParamCheck(args, i));
						// End
						i++;
						break;
					default:
						// Path Check
						if(download.IsExistPath(args[i])) {
							GetBmsFile(args[i]);
							break;
						}
						// Error
						Console.WriteLine("Error: Unknown param. {0}", args[i]);
						break;
				}
			}
		}

		// get-decomp
		static void GetBmsFile(string path)
		{
			// Download
			var DlPath = download.GetResource(path);
			// Path Check
			if(DlPath == null) {
				// Error
				Console.WriteLine("Error: Download error.");
				return;
			}

			// Decompress
			var DcmpDir = decompress.Decompress(DlPath);
			// DirCheck
			if(DcmpDir == null) {
				// Error
				Console.WriteLine("Error: Decompress error.");
				return;
			}
			// Move
			MoveBmsFile(DcmpDir);
			return;
		}

		// move
		static void MoveBmsFile(string dir)
		{
			// LR2Database Exist Check
			if(!File.Exists("data/dbpath")) {
				// Error
				Console.WriteLine("Error: Unset LR2 Path. use -s option.");
				return;
			}

			// BMX File Found
			var BmxFileNames = Directory.GetFiles(dir, "*.bm*", SearchOption.AllDirectories);
			// Title Get
			foreach(var BMxFile in BmxFileNames) {
				// Ignore .bmp file
				if(Path.GetExtension(BMxFile) == ".bmp") {
					continue;
				}
				// Info get
				var BmxInfo = bmsinfoget.GetBmsInfo(BMxFile);
				// check
				if(BmxInfo != null && BmxInfo.Title != "") {
					// Get Path from Database
					var MoveDir = database.GetBmsDirectory(BmxInfo.GetShortTitle());
					// Directory Check
					if(MoveDir == null) {
						// Error
						Console.WriteLine("Error: \"{0}\" Song Folder Not Found.", BmxInfo.GetShortTitle());
						continue;
					}
					// Move File
					if(!File.Exists(MoveDir + "/" + Path.GetFileName(BmxInfo.Path))) {
						File.Move(BmxInfo.Path, MoveDir + "/" + Path.GetFileName(BmxInfo.Path));
						Console.WriteLine("Move: \"{0}\" -> \"{1}\"",
							Path.GetFileName(BmxInfo.Path), Path.GetFileName(MoveDir));
					}
					else {
						Console.WriteLine("Warn: \"{0}\" is already exist.", Path.GetFileName(BmxInfo.Path));
					}
				}
				else {
					// Error
					Console.WriteLine("Error: Title Get Error. {0} isn't BMX File.", BMxFile);
				}
			}
		}

		// commons
		static string NextParamCheck(string[] args, int i)
		{
			return (args.Length > i+1 ? args[i+1] : null);
		}
	}
}
