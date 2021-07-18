FROM node:11
WORKDIR /app
COPY ./Game.Engine/wwwroot ./

RUN ["npm", "i", "--unsafe-perm"]

RUN ["npm", "run", "build"]

FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app
COPY . ./
COPY --from=0 /app/dist ./Game.Engine/wwwroot/dist

WORKDIR /app/Game.Engine
RUN dotnet publish -c Release -o out

WORKDIR /app/Game.Engine
RUN dotnet publish -c Release -o out

WORKDIR /app/Game.Util
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=1  /app/Game.Engine/out ./

EXPOSE 80
CMD ["dotnet", "Game.Engine.dll"]