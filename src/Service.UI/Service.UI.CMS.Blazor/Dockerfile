# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Cài đặt timezone Việt Nam (Asia/Ho_Chi_Minh)
RUN apt-get update && apt-get install -y tzdata \
    && ln -sf /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime \
    && echo "Asia/Ho_Chi_Minh" > /etc/timezone

WORKDIR /app

EXPOSE 3003
ENV PORT=3003
ENV ASPNETCORE_URLS=http://+:${PORT}

# Chạy bằng user app (có thể override bằng APP_UID khi build/run)
USER app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TanAn.UI.Blazor/TanAn.UI.Blazor.csproj", "TanAn.UI.Blazor/"]
COPY ["TanAn.Application/TanAn.Application.csproj", "TanAn.Application/"]
COPY ["TanAn.Domain/TanAn.Domain.csproj", "TanAn.Domain/"]
COPY ["TanAn.Infrastructure/TanAn.Infrastructure.csproj", "TanAn.Infrastructure/"]
COPY ["TanAn.Shared.Commons/TanAn.Shared.Commons.csproj", "TanAn.Shared.Commons/"]
COPY ["TanAn.Shared.Contracts/TanAn.Shared.Contracts.csproj", "TanAn.Shared.Contracts/"]

RUN dotnet restore "TanAn.UI.Blazor/TanAn.UI.Blazor.csproj"

COPY . .
WORKDIR "/src/TanAn.UI.Blazor"
RUN dotnet build "TanAn.UI.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TanAn.UI.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TanAn.UI.Blazor.dll"]
