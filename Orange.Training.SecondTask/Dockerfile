FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Orange.Training.SecondTask/Orange.Training.SecondTask.csproj", "Orange.Training.SecondTask/"]
RUN dotnet restore "Orange.Training.SecondTask/Orange.Training.SecondTask.csproj"

COPY . .
WORKDIR "/src/Orange.Training.SecondTask"
RUN dotnet build "Orange.Training.SecondTask.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orange.Training.SecondTask.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orange.Training.SecondTask.dll"]