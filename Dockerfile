FROM node:11
WORKDIR /app
COPY ./Game.Engine/wwwroot ./

RUN ["npm", "i"]
RUN ["npm", "run", "build"]

FROM microsoft/dotnet:2.1-sdk
WORKDIR /app
COPY . ./
COPY --from=0 /app/dist ./Game.Engine/wwwroot/dist
WORKDIR ./Game.Engine
RUN ["dotnet", "publish", "-c", "Release"]

WORKDIR ./bin/Release/netcoreapp2.1/publish
EXPOSE 5000
CMD ["dotnet", "Game.Engine.dll"]