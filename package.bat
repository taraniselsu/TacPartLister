@echo off

set DIR=TacPartLister_v%1

mkdir Release\%DIR%

xcopy /s /f /y Plugins Release\%DIR%\Plugins\
xcopy /s /f /y PluginData Release\%DIR%\PluginData\
copy /y LICENSE.txt Release\%DIR%\
copy /y Readme.txt Release\%DIR%\

cd Release
7z a -tzip %DIR%.zip %DIR%
cd ..