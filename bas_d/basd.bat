@echo off

setlocal
IF NOT EXIST "data/dbpath" (
	goto lrset
)

:dlstart
echo 導入するファイルのURLを入力してください。
echo 汎用アップローダーの物に関してはup4615.zipのような表記でも可能です。
set /P url=:: 
echo;
call basd.exe -g "%url%"
echo;
goto END

:lrset
echo LR2本体の存在するパスを入力してください(例: D:\LR2beta3)。
set /P lrdir=:: 
echo;
IF NOT EXIST "%lrdir%"\ (
	echo 指定されたパスは存在しません。
	goto END
)
call basd.exe -s "%lrdir%"
echo;
goto dlstart

:END
endlocal
pause;
exit /B