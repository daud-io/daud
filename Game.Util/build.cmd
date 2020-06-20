rmdir /s /q bin\Release
dotnet publish -c release
pushd bin\Release\netcoreapp3.1\publish
docker build . -t andylippitt/iogame-util:%1
docker push andylippitt/iogame-util:%1
popd
