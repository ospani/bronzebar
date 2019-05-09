@echo off

if not exist "{0}" (echo ("Destination dir does not exist.") exit 0) else (echo "Destination directory found!")
if not exist %0\..\{1} (echo ("Source dir does not exist.") exit 0) else (echo "Source directory found!")

xcopy %0\..\{1} {2} /s /y /i /f

pause