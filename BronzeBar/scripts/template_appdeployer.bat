@echo off
if not exist "{0}" (echo "Destination dir does not exist." & exit /b 3)
if not exist %0\..\{1} (echo "Source dir does not exist." & exit /b 3)
xcopy %0\..\{1} {2} /s /y /i /f
pause