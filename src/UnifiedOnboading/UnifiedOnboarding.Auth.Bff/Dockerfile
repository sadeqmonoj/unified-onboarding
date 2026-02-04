FROM mcr.microsoft.com/dotnet/runtime-deps:10.0
WORKDIR /app
COPY . .
RUN chmod +x ./UnifiedOnboarding.Auth.Bff || true
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["./UnifiedOnboarding.Auth.Bff"]