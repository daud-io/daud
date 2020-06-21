FROM node:14
WORKDIR /app
COPY ./Game.Engine/wwwroot ./

RUN ["npm", "ci"]
RUN ["npm", "run", "build"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
COPY . ./
COPY --from=0 /app/dist ./Game.Engine/wwwroot/dist

WORKDIR /app/Game.Engine
RUN ["dotnet", "publish", "-c", "Release"]

WORKDIR /app/Game.Util
RUN ["dotnet", "publish", "-c", "Release"]

WORKDIR /app/Game.Registry
RUN ["dotnet", "publish", "-c", "Release"]

WORKDIR /app/Game.Engine/bin/Release/netcoreapp3.1/publish
EXPOSE 5000
CMD ["dotnet", "Game.Engine.dll"]
