FROM microsoft/dotnet:2.1-sdk AS build-env

ARG RID=linux-x64

WORKDIR /build

COPY Sds.MetadataStorage.Domain/Sds.MetadataStorage.Domain.csproj Sds.MetadataStorage.Domain/
COPY Sds.MetadataStorage.Tests/Sds.MetadataStorage.Tests.csproj Sds.MetadataStorage.Tests/

RUN dotnet restore Sds.MetadataStorage.Tests/Sds.MetadataStorage.Tests.csproj

COPY Sds.MetadataStorage.Domain Sds.MetadataStorage.Domain
COPY Sds.MetadataStorage.Tests Sds.MetadataStorage.Tests

RUN dotnet publish Sds.MetadataStorage.Tests/Sds.MetadataStorage.Tests.csproj -r $RID -c Release -o /dist

# Build runtime image
FROM microsoft/dotnet:2.1-sdk

LABEL maintainer="rick.zakharov@arqisoft.com"

WORKDIR /app

RUN apt-get update && apt-get install -y curl
RUN curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh > /app/wait-for-it.sh && chmod 777 /app/wait-for-it.sh

COPY --from=build-env /dist ./

ENTRYPOINT ["dotnet", "vstest", "./Sds.MetadataStorage.Tests.dll"]
