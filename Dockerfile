FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /app
COPY SPHL.fsproj Program.fs ./
RUN dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS runtime
RUN mkdir /app
COPY --from=build /app/bin/Release/net5.0 /app
WORKDIR /app
ENV ASPNETCORE_URLS="http://0.0.0.0:8080"
ENTRYPOINT [ "dotnet", "SPHL.dll" ]
