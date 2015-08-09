using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Win32DllManage
{
	//アンマネージドなDLLをLoadLibrary(Win32API)で動的にリンクし、確実に解放するため、ハンドルを管理するクラス
	//参考	http://msdn.microsoft.com/ja-jp/library/microsoft.win32.safehandles.safehandlezeroorminusoneisinvalid(v=vs.90).aspx
	
	class SafeModuleHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		//コンストラクタ
		public SafeModuleHandle(string dllPath)
			: base(true)
		{
			try
			{
				IntPtr hModule = LoadLibrary(dllPath);
				this.SetHandle(hModule);	//this.handleがセットされる
			}
			catch { this.SetHandle(IntPtr.Zero); }	//この場合は(this.IsInvalid==false)となる
		}

		//解放
		protected override bool ReleaseHandle()
		{
			if (!this.IsInvalid)
			{
				try { FreeLibrary(this.handle); }
				finally { this.SetHandle(IntPtr.Zero); }
			}
			return true;
		}

		//ロードしたモジュール内の関数ポインタを取得する
		public IntPtr GetProcAddress(string procName)
		{
			if (this.IsInvalid) return IntPtr.Zero;

			IntPtr hProc = IntPtr.Zero;
			try { hProc = GetProcAddress(this.handle, procName); }
			catch { }
			return hProc;
		}

		#region 動的なDLLロード／解放のWin32API

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);

		/*
		HMODULE LoadLibrary(
		  LPCTSTR lpFileName   // モジュールのファイル名
		);
		http://msdn.microsoft.com/ja-jp/library/cc429241.aspx

		FARPROC GetProcAddress(
		  HMODULE hModule,    // DLL モジュールのハンドル
		  LPCSTR lpProcName   // 関数名
		);
		http://msdn.microsoft.com/ja-jp/library/cc429133.aspx

		BOOL FreeLibrary(
		  HMODULE hModule   // DLL モジュールのハンドル
		);
		http://msdn.microsoft.com/ja-jp/library/cc429103.aspx
		*/

		#endregion

	}	//class
}	//namespace
