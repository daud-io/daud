pushd wwwroot
rmdir /s /q dist
call npm run build
popd
rmdir /s /q bin\Debug
dotnet publish
pushd bin\Debug\netcoreapp2.1\publish
docker build . -t andylippitt/iogame:%1
docker push andylippitt/iogame:%1
popd
