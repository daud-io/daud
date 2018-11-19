#! /bin/bash
if ! heroku whoami; then
    heroku login
fi
heroku container:login

dotnet publish -c Release
pushd bin/Release/netcoreapp2.1/publish
heroku container:push web 
heroku container:release web
popd