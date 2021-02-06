FROM microsoft/dotnet/sdk:5.0
COPY . ./
RUN ["dotnet", "publish", "-c", "Release"]
WORKDIR ./bin/Release/netcoreapp5.0/publish

CMD ["dotnet", "Game.Registry.dll"]
