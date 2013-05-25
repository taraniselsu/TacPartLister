set DIR=%1..\TacPartLister\Plugins\
if not exist %DIR% mkdir %DIR%
copy Tac*.dll %DIR%

cd %1..
call test.bat