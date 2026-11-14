# fancyauth, without plugins: they come from images/fancyauth-plugins in arnold-infra, mounted at /app/plugins.
# Built and imported locally, never pushed: see docs/runbooks/migrate-mumble.md there.
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:fe0fd4e93bd2fbb47c883b7a6369e5c8d3db8bf01cdc5981f6ee35fbdf3ff4fb AS build
WORKDIR /src
COPY . .
RUN dotnet publish Fancyauth/Fancyauth.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0@sha256:80a51954ac70925e5783cb1ed35129a4243dcb1cddef20066b67346e17db284d
WORKDIR /app
COPY --from=build /app .
# The image's non-root user (UID 1654).
USER app
ENTRYPOINT ["dotnet", "Fancyauth.dll"]
