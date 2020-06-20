FROM microsoft/dotnet:3.1-sdk
COPY . ./
RUN ["dotnet", "publish", "-c", "Release"]
WORKDIR ./bin/Release/netcoreapp3.1/publish

CMD ["dotnet", "Game.Util.dll"]