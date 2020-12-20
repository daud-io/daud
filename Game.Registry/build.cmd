dotnet publish -c release
pushd bin\Release\netcoreapp5.0\publish
docker build . -t andylippitt/iogame-registry:%1
docker push andylippitt/iogame-registry:%1
popd
