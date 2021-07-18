FROM microsoft/dotnet:2.1-sdk
COPY . ./
RUN ["dotnet", "publish", "-c", "Release"]
WORKDIR ./bin/Release/netcoreapp5.0/publish

CMD ["dotnet", "Game.Util.dll"]