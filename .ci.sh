#!/bin/bash
set -e

xbuild /t:CalculateVersion /p:Configuration=Prerelease /p:SolutionDir=$(pwd) *.csproj
xbuild /t:CIBuild /p:Configuration=Prerelease /p:SolutionDir=$(pwd) *.csproj
