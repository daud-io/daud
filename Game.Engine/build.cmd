rmdir /s /q _IGNORE
pushd wwwroot
rmdir /s /q dist
call npm run build
popd
rmdir /s /q bin\Release
dotnet publish -c release
pushd bin\Release\netcoreapp5.0\publish
docker build . -t andylippitt/iogame:%1
docker push andylippitt/iogame:%1
popd
