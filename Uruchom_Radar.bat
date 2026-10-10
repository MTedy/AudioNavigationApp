@echo off
echo Trwa kompilacja i uruchamianie aplikacji Nawigacja Audio...
dotnet clean
dotnet build
cd App.UI\bin\Debug\net10.0
start App.UI.exe
pause