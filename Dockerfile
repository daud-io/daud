FROM node:11
WORKDIR /app
COPY . ./
WORKDIR ./Game.Engine/wwwroot

RUN ["npm", "i"]
RUN ["npm", "run", "build"]

FROM microsoft/dotnet:2.1-sdk
WORKDIR /app
COPY --from=0 /app .
WORKDIR ./Game.Engine
RUN ["dotnet", "publish", "-c", "Release"]

WORKDIR ./bin/Release/netcoreapp2.1/publish
CMD ["dotnet", "Game.Engine.dll"]