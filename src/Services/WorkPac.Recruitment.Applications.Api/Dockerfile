FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Services/WorkPac.Recruitment.Applications.Api/", "src/Services/WorkPac.Recruitment.Applications.Api/"]
COPY ["src/Shared/WorkPac.Recruitment.Shared/", "src/Shared/WorkPac.Recruitment.Shared/"]
COPY ["src/Shared/WorkPac.Recruitment.Contracts/", "src/Shared/WorkPac.Recruitment.Contracts/"]
COPY ["src/Infrastructure/WorkPac.Recruitment.Infrastructure/", "src/Infrastructure/WorkPac.Recruitment.Infrastructure/"]
RUN dotnet restore "src/Services/WorkPac.Recruitment.Applications.Api/WorkPac.Recruitment.Applications.Api.csproj"
RUN dotnet publish "src/Services/WorkPac.Recruitment.Applications.Api/WorkPac.Recruitment.Applications.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkPac.Recruitment.Applications.Api.dll"]
