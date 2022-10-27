FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /publish_dir
COPY  . /publish_dir/
ENTRYPOINT ["dotnet", "SuperGMS.HttpProxy.dll"]