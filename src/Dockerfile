FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Authorization.Samples.csproj", "src/"]
RUN dotnet restore "src/Authorization.Samples.csproj"
COPY . src
WORKDIR "/src/src"
RUN dotnet build "Authorization.Samples.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authorization.Samples.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authorization.Samples.dll"]
