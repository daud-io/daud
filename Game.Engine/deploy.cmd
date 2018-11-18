heroku login
heroku container:login

dotnet publish -c Release
pushd bin\Release\netcoreapp2.1\publish
heroku container:push web --app %1
heroku container:release web --app %1
popd