@echo off
set TAG=v1.0.0
set PUBLISH_DIR=bin\Published
gh release create %TAG% %PUBLISH_DIR%\*.* --title "%TAG%" --notes "Release %TAG%"
