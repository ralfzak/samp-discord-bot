FROM microsoft/dotnet:2.1-sdk AS builder

WORKDIR /sln

COPY ./ .
RUN dotnet publish -c Release -o "/sln/release"

FROM microsoft/dotnet:2.1-aspnetcore-runtime

COPY --from=builder /sln/release .
ENTRYPOINT ["dotnet", "main.dll"]