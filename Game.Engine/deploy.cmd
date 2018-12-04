call heroku login
call heroku container:login

dotnet publish -c Release
pushd bin\Release\netcoreapp2.1\publish
call heroku container:push web --app %1
call heroku container:release web --app %1
popd