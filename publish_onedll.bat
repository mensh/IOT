dotnet publish -r linux-arm64 /p:ShowLinkerSizeComparison=true /p:self-contained=false
pushd .\bin\Debug\netcoreapp3.1\linux-arm64\publish
"C:\Program Files\PuTTY\pscp.exe" -P 22 -pw menshikov  -v -r .\IOT.dll .\IOT.pdb root@192.168.1.102:/root/IOT
popd