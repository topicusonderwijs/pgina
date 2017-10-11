@echo off

del /F /Q %~dps0*.log 2>NUL
if "%VCpath%"=="" call %~dps0..\getVCpath.cmd || exit /b 1

for /f "tokens=*" %%e in ('dir %~dps0*.sln /B /S /A') do (
	@echo.
	@echo compiling %%~ne
    
    IF EXIST "%%~dpe\packages.config" (
        echo "getting nuget packages for %%~dpe"
        %~dps0..\.nuget\NuGet.exe install "%%~dpe\packages.config" -OutputDirectory "%%~dpe\packages"
        @echo.
    )
	%VCpath% %%~se /Rebuild "Release|Any CPU" /Out %~dps0%%~ne.log || (
		@echo failed to compile
		pause
		exit /b 1
	)
	find /I "===" %~dps0%%~ne.log
)

if Not "%~1" == "batch" (
	@echo finished
	pause
)
exit /b 0
