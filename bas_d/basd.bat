@echo off

setlocal
IF NOT EXIST "data/dbpath" (
	goto lrset
)

:dlstart
echo ��������t�@�C����URL����͂��Ă��������B
echo �ėp�A�b�v���[�_�[�̕��Ɋւ��Ă�up4615.zip�̂悤�ȕ\�L�ł��\�ł��B
set /P url=:: 
echo;
call basd.exe -g "%url%"
echo;
goto END

:lrset
echo LR2�{�̂̑��݂���p�X����͂��Ă�������(��: D:\LR2beta3)�B
set /P lrdir=:: 
echo;
IF NOT EXIST "%lrdir%"\ (
	echo �w�肳�ꂽ�p�X�͑��݂��܂���B
	goto END
)
call basd.exe -s "%lrdir%"
echo;
goto dlstart

:END
endlocal
pause;
exit /B