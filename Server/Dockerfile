FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app
COPY . ./

WORKDIR /app/Game.Engine
RUN dotnet publish -c Release -o /app/out

WORKDIR /app/Game.Engine
RUN dotnet publish -c Release -o /app/out

WORKDIR /app/Game.Util
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=0  /app/out ./

COPY ./Samples ./Samples
COPY ./Game.Util/scripts/game /usr/bin/game
RUN chmod +x /usr/bin/game

EXPOSE 80
CMD ["dotnet", "Game.Engine.dll"]
