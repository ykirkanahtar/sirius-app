FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ["src/Sirius.Web.Host/Sirius.Web.Host.csproj", "src/Sirius.Web.Host/"]
COPY ["src/Sirius.Web.Core/Sirius.Web.Core.csproj", "src/Sirius.Web.Core/"]
COPY ["src/Sirius.Application/Sirius.Application.csproj", "src/Sirius.Application/"]
COPY ["src/Sirius.Core/Sirius.Core.csproj", "src/Sirius.Core/"]
COPY ["src/Sirius.EntityFrameworkCore/Sirius.EntityFrameworkCore.csproj", "src/Sirius.EntityFrameworkCore/"]
WORKDIR "/src/src/Sirius.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Sirius.Web.Host", "src/Sirius.Web.Host"]
COPY ["src/Sirius.Web.Core", "src/Sirius.Web.Core"]
COPY ["src/Sirius.Application", "src/Sirius.Application"]
COPY ["src/Sirius.Core", "src/Sirius.Core"]
COPY ["src/Sirius.EntityFrameworkCore", "src/Sirius.EntityFrameworkCore"]
WORKDIR "/src/src/Sirius.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Sirius.Web.Host.dll"]
