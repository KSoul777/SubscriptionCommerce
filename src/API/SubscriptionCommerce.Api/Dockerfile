# The build context is the repository root (see docker-compose.yml), so the shared
# Directory.Build.props, Directory.Packages.props, nuget.config and .editorconfig take part in the build.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Restore first, from the project files only, so the restore layer is cached until a project file changes.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo
COPY ["Directory.Build.props", "Directory.Packages.props", "nuget.config", "./"]
COPY ["src/API/SubscriptionCommerce.Api/SubscriptionCommerce.Api.csproj", "src/API/SubscriptionCommerce.Api/"]
COPY ["src/Common/SubscriptionCommerce.Common.Domain/SubscriptionCommerce.Common.Domain.csproj", "src/Common/SubscriptionCommerce.Common.Domain/"]
COPY ["src/Common/SubscriptionCommerce.Common.Application/SubscriptionCommerce.Common.Application.csproj", "src/Common/SubscriptionCommerce.Common.Application/"]
COPY ["src/Common/SubscriptionCommerce.Common.Infrastructure/SubscriptionCommerce.Common.Infrastructure.csproj", "src/Common/SubscriptionCommerce.Common.Infrastructure/"]
COPY ["src/Common/SubscriptionCommerce.Common.Presentation/SubscriptionCommerce.Common.Presentation.csproj", "src/Common/SubscriptionCommerce.Common.Presentation/"]
COPY ["src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Domain/SubscriptionCommerce.Modules.Pricing.Domain.csproj", "src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Domain/"]
COPY ["src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Application/SubscriptionCommerce.Modules.Pricing.Application.csproj", "src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Application/"]
COPY ["src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Infrastructure/SubscriptionCommerce.Modules.Pricing.Infrastructure.csproj", "src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Infrastructure/"]
COPY ["src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Presentation/SubscriptionCommerce.Modules.Pricing.Presentation.csproj", "src/Modules/Pricing/SubscriptionCommerce.Modules.Pricing.Presentation/"]
COPY ["src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Domain/SubscriptionCommerce.Modules.Customers.Domain.csproj", "src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Domain/"]
COPY ["src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Application/SubscriptionCommerce.Modules.Customers.Application.csproj", "src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Application/"]
COPY ["src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Infrastructure/SubscriptionCommerce.Modules.Customers.Infrastructure.csproj", "src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Infrastructure/"]
COPY ["src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Presentation/SubscriptionCommerce.Modules.Customers.Presentation.csproj", "src/Modules/Customers/SubscriptionCommerce.Modules.Customers.Presentation/"]
RUN dotnet restore "./src/API/SubscriptionCommerce.Api/SubscriptionCommerce.Api.csproj"
COPY . .
WORKDIR "/repo/src/API/SubscriptionCommerce.Api"
RUN dotnet build "./SubscriptionCommerce.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SubscriptionCommerce.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubscriptionCommerce.Api.dll"]
