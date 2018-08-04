dotnet publish
pushd bin\Debug\netcoreapp2.0\publish
docker build . -t andylippitt/iogame:%1
docker push andylippitt/iogame:%1
popd
