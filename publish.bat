dotnet publish -r linux-arm64 /p:ShowLinkerSizeComparison=true /p:self-contained=false
pushd .\bin\Debug\netcoreapp3.1\linux-arm64\publish
"C:\Program Files\PuTTY\pscp.exe" -P 22 -pw menshikov  -v -r .\* root@orangepi4:/root/IOT
popd


