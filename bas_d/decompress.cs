// --------------------------------
// archive decompress class
// --------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using UnRarDllWraper;

namespace bas_d
{
	class decompress
	{
		// decompress path
		public const string ExtractPath = "score/";

		// decompress
		// @return value: true(decomp), false(no need), null(error)
		public static bool? Decompress(string ArcPath)
		{
			// check
			switch(Path.GetExtension(ArcPath)){
				case ".bms":
				case ".bme":
				case ".bml":
					// Nothing
					Console.WriteLine("Archive Extract: No need.");
					// Move File
					File.Move(ArcPath, ExtractPath + Path.GetFileName(ArcPath));
					return false;
				case ".zip":
					// Zip archive
					var ZipArc = new ZipArchive(new FileStream(ArcPath, FileMode.Open));
					ZipArc.ExtractToDirectory(ExtractPath);
					Console.WriteLine("Archive Extract: Success(Type: Zip).");
					ZipArc.Dispose();
					return true;
				case ".rar":
					// Rar archive
					// Load Unrar.dll
					var rarMgr = new UnRarDllMgr();
					var UnrarPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\bin\\unrar64.dll";
					if(!rarMgr.LoadModule(UnrarPath)) {
						// Error Message
						Console.WriteLine("Error: Can't Load \"Unrar64.dll\"");
						return null;
					}
					// Filelist get
					var FileLists = rarMgr.GetFileList(ArcPath);
					// All Extract
					foreach(var FileData in FileLists) {
						rarMgr.FileExtractToFolder(ArcPath, FileData.FileNameW, ExtractPath);
					}
					Console.WriteLine("Archive Extract: Success(Type: Rar).");
					// Release Unrar.dll
					rarMgr.UnloadModule();
					rarMgr.CloseArchive();
					return true;
				default:
					// Error Message
					Console.WriteLine("Error: this extension({0}) is not supported.",
						Path.GetExtension(ArcPath));
					return null;
			}
		}


	}
}
