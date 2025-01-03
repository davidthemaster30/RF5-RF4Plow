rm -rf obj
rm -rf bin

dotnet build RF5-RF4Plow.csproj -f net6.0 -c Release

zip -j 'RF5-RF4Plow_v1.1.0.zip' './bin/Release/net6.0/RF5RF4Plow.dll'