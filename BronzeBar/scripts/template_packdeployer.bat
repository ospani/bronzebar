@echo off
echo "BronzeBar Package Deployer,
echo "===================================================="
for /r %%f in (solo_*.bat) do (
	call %%f
	if %ERRORLEVEL% NEQ 0 echo "problem" & pause)
echo "===================================================="
echo "Deployment finished."
pause