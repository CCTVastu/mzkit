@echo off

SET drive=%~d0
SET R_HOME=%drive%\GCModeller\src\R-sharp\App\net6.0
SET Rscript="%R_HOME%/Rscript.exe"
SET REnv="%R_HOME%/R#.exe"

%Rscript% --build /src ../ /save ../../mzkit_1.1.0-beta.zip --skip-src-build
%REnv% --install.packages ../../mzkit_1.1.0-beta.zip

pause