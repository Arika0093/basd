// --------------------------------
// archive decompress class
// --------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace bas_d
{
	class decompress
	{
		// decompress
		public static string Decompress(string ArcPath)
		{
			// check
			switch(Path.GetExtension(ArcPath)){
				case ".bms":
				case ".bme":
				case ".bml":
					// Nothing
					Console.WriteLine("Archive Decompress: No need.");
					return "arcive/";
				case ".zip":
					// Zip archive
					var ZipArc = new ZipArchive(new FileStream(ArcPath, FileMode.Open));
					ZipArc.ExtractToDirectory("arcive/dcmp/");
					Console.WriteLine("Archive Decompress: Success(Type: Zip).");
					return "arcive/dcmp/";
				default:
					// Error Message
					Console.WriteLine("Error: this extension({0}) is not supported.",
						Path.GetExtension(ArcPath));
					return null;
			}
		}


	}
}
