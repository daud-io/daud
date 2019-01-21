pushd wwwroot
call npm run build
popd
dotnet publish
pushd bin\Debug\netcoreapp2.1\publish
docker build . -t andylippitt/iogame:%1
docker push andylippitt/iogame:%1
popd
