using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Win32DllManage;	//SafeModuleHandle

namespace UnRarDllWraper
{
	partial class UnRarDllMgr
	{
		private SafeModuleHandle shModule = null;
		private SafeArchiveHandle shArchive = new SafeArchiveHandle();

		//dllをロードする
		public bool LoadModule(string unrarDllPath)
		{
			UnloadModule();
			shModule = new SafeModuleHandle(unrarDllPath);
			return !shModule.IsInvalid;
		}

		//dllを解放する
		public void UnloadModule()
		{
			if (shModule != null && !shModule.IsInvalid) shModule.Dispose();
			shModule = null;
		}

		//書庫ファイルをオープンする
		public bool OpenArchive(string arcFilePath, OpenMode openMode)
		{
			if (string.IsNullOrEmpty(arcFilePath)) return false;

			RAROpenArchiveDataEx tArchiveData = new RAROpenArchiveDataEx();
			tArchiveData.ArcName = tArchiveData.ArcNameW = arcFilePath;
			tArchiveData.OpenMode = openMode;
			tArchiveData.SetCommentBuffer(false);	//必要なら適宜trueとする
			tArchiveData.Callback = IntPtr.Zero;	//必要なら適宜セットする
			tArchiveData.UserData = IntPtr.Zero;	//必要なら適宜セットする

			shArchive.CloseArchive();
			return shArchive.OpenArchive(ref tArchiveData);
		}

		//書庫ファイルをクローズする
		public bool CloseArchive()
		{
			ReturnCode retCode = (ReturnCode)shArchive.CloseArchive();
			return (retCode == ReturnCode.Success);
		}

		//書庫内現在位置のファイルの情報を取得する
		public ReturnCode GetFileHeader(ref RARHeaderDataEx tRARHeaderDataEx)
		{
			if (shArchive.IsInvalid) return ReturnCode.Error;
			tRARHeaderDataEx.SetCommentBuffer();

			ReturnCode retCode = ReturnCode.Error;
			try { retCode = (ReturnCode)RARReadHeaderEx(shArchive.DangerousGetHandle(), ref tRARHeaderDataEx); }
			catch { }
			if (retCode == ReturnCode.Success) tRARHeaderDataEx.DosDateTimeToFileTime();
			return retCode;
		}

		//次の書庫内ファイルの位置に移動する
		public bool FileSkip()
		{
			return (FileProcess(Operation.RAR_SKIP, null) == ReturnCode.Success);
		}

		//書庫内現在位置のファイルを解凍する
		public bool FileExtract(string distFilePath)
		{
			return (FileProcess(Operation.RAR_EXTRACT, distFilePath) == ReturnCode.Success);
		}

		private ReturnCode FileProcess(Operation operation, string extractFolderPath)
		{
			ReturnCode retCode = ReturnCode.Error;
			try { retCode = (ReturnCode)RARProcessFileW(shArchive.DangerousGetHandle(), (int)operation, extractFolderPath, null); }
			catch { }
			return retCode;
		}

		//------------------------------------------------------------------
		//	利便性のため用意したメソッド
		//------------------------------------------------------------------

		//書庫内のファイル一覧を取得する
		//戻り値:	長さ0以上の配列（エラー時もnullは返さない）
		public RARHeaderDataEx[] GetFileList(string arcFilePath)
		{
			//書庫ファイルを参照モードでオープンする
			if (!OpenArchive(arcFilePath, OpenMode.RAR_OM_LIST)) return new RARHeaderDataEx[0];

			//書庫内のファイル情報を取得する
			List<RARHeaderDataEx> headerList = new List<RARHeaderDataEx>();
			try
			{
				RARHeaderDataEx headerData = new RARHeaderDataEx();
				int limitCount = 0;	//無限ループ対策（保険）
				while (++limitCount < int.MaxValue)
				{
					ReturnCode retCode = GetFileHeader(ref headerData);
					if (retCode == ReturnCode.ERAR_END_ARCHIVE) break;
					if (retCode == ReturnCode.Success)
					{
						headerList.Add(headerData);
						headerData = new RARHeaderDataEx();
					}
					FileSkip();
				}
			}
			finally { CloseArchive(); }	//書庫ファイルをクローズする
			
			RARHeaderDataEx[] headerDatas = new RARHeaderDataEx[headerList.Count];
			if (0 < headerList.Count) headerList.CopyTo(headerDatas);
			headerList.Clear();
			return headerDatas;
		}

		//書庫内のファイルを取り出してフォルダに保存する
		public bool FileExtractToFolder(string arcFilePath, string entryPath, string folderPath)
		{
			//書庫ファイルを解凍モードでオープンする
			if (!OpenArchive(arcFilePath, OpenMode.RAR_OM_EXTRACT)) return false;

			//目的のファイルを探して解凍する
			RARHeaderDataEx headerData = new RARHeaderDataEx();
			bool isFind = false, isOK = false;
			try
			{
				for (int i = 0; i < int.MaxValue && !isFind; i++)
				{
					ReturnCode retCode = GetFileHeader(ref headerData);
					if (retCode == ReturnCode.ERAR_END_ARCHIVE) break;
					if (retCode == ReturnCode.Success)
					{
						if (isFind = (headerData.FileNameW == entryPath))	//代入と判定
						{
							isOK = FileExtract(folderPath);	//ファイルを保存する
							break;
						}
					}
					FileSkip();
				}
			}
			finally { CloseArchive(); }	//書庫ファイルをクローズする

			return (isFind && isOK);
		}

		#region unrar.dll読み込みの定義

		/*	SafeArchiveHandleクラスで定義
		[DllImport("unrar.dll")]
		private static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx archiveData);
		//HANDLE PASCAL RAROpenArchiveEx(struct RAROpenArchiveDataEx *ArchiveData)

		[DllImport("unrar.dll")]
		private static extern int RARCloseArchive(IntPtr hArcData);
		//int PASCAL RARCloseArchive(HANDLE hArcData)
		*/

		[DllImport("unrar64.dll")]
		private static extern int RARReadHeaderEx(IntPtr hArcData, ref RARHeaderDataEx headerData);
		//int PASCAL RARReadHeaderEx(HANDLE hArcData,struct RARHeaderDataEx *HeaderData)

		[DllImport("unrar64.dll", CharSet = CharSet.Unicode)]
		private static extern int RARProcessFileW(IntPtr hArcData, int operation,
			[MarshalAs(UnmanagedType.LPWStr)]
			string destPath,
			[MarshalAs(UnmanagedType.LPWStr)]
			string destName
			);
		//int PASCAL RARProcessFileW(HANDLE hArcData,int Operation,wchar_t *DestPath,wchar_t *DestName)

		private delegate int UNRARCallback(uint msg, IntPtr userData, IntPtr p1, IntPtr p2);
		//int CALLBACK CallbackProc(UINT msg,LPARAM UserData,LPARAM P1,LPARAM P2); 

		#endregion
	}

	//書庫ファイルのオープン／クローズのラッパークラス
	partial class UnRarDllMgr
	{
		//RAROpenArchiveEx()のリファレンスの戻り値の説明で、'Archive handle'がファイルハンドルなのか何なのか分からない。
		//よって、SafeFileHandlesではなく派生クラスで管理することにする。
		
		private class SafeArchiveHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public SafeArchiveHandle()
				: base(true)
			{ }

			public bool OpenArchive(ref RAROpenArchiveDataEx archiveData)
			{
				try
				{
					IntPtr hArchive = RAROpenArchiveEx(ref archiveData);
					this.SetHandle(hArchive);
				}
				catch { this.SetHandle(IntPtr.Zero); }
				return !this.IsInvalid;
			}

			public int CloseArchive()
			{
				int retCode = (int)ReturnCode.Error;
				try { retCode = RARCloseArchive(this.handle); }
				finally { this.SetHandle(IntPtr.Zero); }
				return retCode;
			}

			//解放
			protected override bool ReleaseHandle()
			{
				if (!this.IsInvalid) CloseArchive();
				return true;
			}

			#region unrar.dll読み込みの定義

			[DllImport("unrar64.dll")]
			private static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx archiveData);
			//HANDLE PASCAL RAROpenArchiveEx(struct RAROpenArchiveDataEx *ArchiveData)

			[DllImport("unrar64.dll")]
			private static extern int RARCloseArchive(IntPtr hArcData);
			//int PASCAL RARCloseArchive(HANDLE hArcData)

			#endregion
		}
	}

	#region 構造体の定義

	[StructLayout(LayoutKind.Sequential)]
	struct RAROpenArchiveDataEx
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public string ArcName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string ArcNameW;
		public OpenMode OpenMode;
		public ReturnCode OpenResult;
		[MarshalAs(UnmanagedType.LPStr)]
		public string CmtBuf;
		private int CmtBufSize;
		private int CmtSize;
		public CmtState CmtState;
		public ArchiveFlag Flags;
		public IntPtr Callback;
		public IntPtr UserData;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		private uint[] Reserved;

		public void SetCommentBuffer(bool reqComment)
		{
			if (reqComment)
			{
				int bufSize = 64 * 1024;
				CmtBuf = new string('\0', bufSize);
				CmtBufSize = bufSize;
			}
			else
			{
				CmtBuf = null;
				CmtBufSize = 0;
			}
		}
	}
	/*
	struct RAROpenArchiveDataEx
	{
	  char         *ArcName;
	  wchar_t      *ArcNameW;
	  unsigned int  OpenMode;
	  unsigned int  OpenResult;
	  char         *CmtBuf;
	  unsigned int  CmtBufSize;
	  unsigned int  CmtSize;
	  unsigned int  CmtState;
	  unsigned int  Flags;
	  UNRARCALLBACK Callback;
	  LPARAM        UserData;
	  unsigned int  Reserved[28];
	};
	*/

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct RARHeaderDataEx
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024 / 2)]
		private string ArcName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public readonly string ArcNameW;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024 / 2)]
		private string FileName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public readonly string FileNameW;
		private uint FlagBits;
		private uint PackSizeLow;
		private uint PackSizeHigh;
		private uint UnpSizeLow;
		private uint UnpSizeHigh;
		public readonly HostOS HostOS;
		public readonly uint FileCRC;
		private uint MsDosFileTime;
		public readonly uint UnpackVer;
		public readonly Method Method;
		public readonly FileAttributes FileAttr;
		[MarshalAs(UnmanagedType.LPStr)]
		private string CmtBuf;
		private int CmtBufSize;
		private int CmtSize;
		private CmtState CmtState;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		private uint[] Reserved;

		public FileFlag Flag { get { return (FileFlag)(FlagBits & 0x1F); } }	//uint dicSizeBits = (FlagBits & 0xE0);
		public long PackSize { get { return (PackSizeHigh * (uint.MaxValue + 1L) + PackSizeLow); } }
		public long UnpackSize { get { return (UnpSizeHigh * (uint.MaxValue + 1L) + UnpSizeLow); } }
		public DateTime FileTimeLocal { get; private set; }

		public void SetCommentBuffer()
		{
			CmtBuf = null;
			CmtBufSize = 0;
		}

		public void DosDateTimeToFileTime()
		{
			//参考	http://msdn.microsoft.com/ja-jp/library/cc429703.aspx

			uint fatDate = ((MsDosFileTime >> 16) & 0xFFFF);
			uint fatTime = (MsDosFileTime & 0xFFFF);

			//bit15-9:year(+1980), bit8-5:month, bit4-0:day
			int day = (int)(fatDate & 0x1F); fatDate >>= 5;
			int month = (int)(fatDate & 0x0F); fatDate >>= 4;
			int year = (int)(fatDate & 0x7F) + 1980;

			//bit15-11:hour{00-23}, bit10-5:minute{00-59}, bit4-0:second{00-29}(*2)
			int second = (int)(fatTime & 0x1F) * 2; fatTime >>= 5;
			int minute = (int)(fatTime & 0x3F); fatTime >>= 6;
			int hour = (int)(fatTime & 0x1F);

			//MsDosFileTimeにはローカル時刻が格納されている
			//MS-DOS形式のタイムスタンプの仕様上、2秒の誤差が出る場合がある
			FileTimeLocal = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
		}
	}
	/*
	struct RARHeaderDataEx
	{
	  char         ArcName[1024];
	  wchar_t      ArcNameW[1024];
	  char         FileName[1024];	//↓
	  wchar_t      FileNameW[1024];	//書庫内ファイルパス（純粋なファイル名のみではない）
	  unsigned int Flags;
	  unsigned int PackSize;
	  unsigned int PackSizeHigh;
	  unsigned int UnpSize;
	  unsigned int UnpSizeHigh;
	  unsigned int HostOS;
	  unsigned int FileCRC;
	  unsigned int FileTime;
	  unsigned int UnpVer;
	  unsigned int Method;
	  unsigned int FileAttr;
	  char         *CmtBuf;
	  unsigned int CmtBufSize;
	  unsigned int CmtSize;
	  unsigned int CmtState;
	  unsigned int Reserved[1024];
	};
	*/

	#endregion

	#region 定数の定義（列挙型）

	enum OpenMode : int
	{
		RAR_OM_LIST = 0,
		RAR_OM_EXTRACT = 1,
		RAR_OM_LIST_INCSPLIT = 2
	}

	enum CmtState : int
	{
		NotPresent = 0,
		ReadCompletely = 1,
		ERAR_NO_MEMORY = ReturnCode.ERAR_NO_MEMORY,
		ERAR_BAD_DATA = ReturnCode.ERAR_BAD_DATA,
		ERAR_SMALL_BUF = ReturnCode.ERAR_SMALL_BUF
	}

	enum ReturnCode : int
	{
		Error = int.MaxValue,
		Success = 0,
		CancelExtraction = -1,

		ERAR_END_ARCHIVE = 10,
		ERAR_NO_MEMORY = 11,
		ERAR_BAD_DATA = 12,
		ERAR_BAD_ARCHIVE = 13,
		ERAR_UNKNOWN_FORMAT = 14,
		ERAR_EOPEN = 15,
		ERAR_ECREATE = 16,
		ERAR_ECLOSE = 17,
		ERAR_EREAD = 18,
		ERAR_EWRITE = 19,
		ERAR_SMALL_BUF = 20,
		ERAR_UNKNOWN = 21,
		ERAR_MISSING_PASSWORD = 22
	}

	[Flags]
	enum ArchiveFlag : uint
	{
		VolumeAttribute = 0x0001,	//archive volume
		ArchiveCommentPresent = 0x0002,
		ArchiveLockAttribute = 0x0004,
		SolidAttribute = 0x0008,	//solid archive
		NewVolumeNamingScheme = 0x0010,	//'volname.partN.rar'
		AuthenticityInformationPresent = 0x0020,
		RecoveryRecordPresent = 0x0040,
		BlockHeadersAreEncrypted = 0x0080,
		FirstVolume = 0x0100	//set only by RAR 3.0 and later
	}

	[Flags]
	enum FileFlag : uint
	{
		FileContinuedFromPreviousVolume = 0x01,
		FileContinuedOnNextVolume = 0x02,
		FileEncryptedWithPassword = 0x04,
		FileCommentPresent = 0x08,
		PreviousFilesDataIsUsed = 0x10 //solid flag
	}

	enum HostOS : int
	{
		MSDOS = 0,
		OS2 = 1,
		Windows = 2,
		Unix = 3
	}

	enum Method : int
	{
		None = 0,

		Storing = 0x30,
		Fastest = 0x31,
		Fast = 0x32,
		Normal = 0x33,
		Good = 0x34,
		Best = 0x35
	}

	enum Operation : int
	{
		RAR_SKIP = 0,
		RAR_TEST = 1,
		RAR_EXTRACT = 2
	}

	#endregion

}	//namespace
