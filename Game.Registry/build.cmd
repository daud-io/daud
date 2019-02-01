dotnet publish -c release
pushd bin\Release\netcoreapp2.1\publish
docker build . -t andylippitt/iogame-registry:%1
docker push andylippitt/iogame-registry:%1
popd
