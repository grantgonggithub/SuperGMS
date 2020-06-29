FROM registry.cn-hangzhou.aliyuncs.com/img/dotnet:v3.1-dotnet-runtime

WORKDIR /publish
COPY  ./Publish/ /publish/
ENTRYPOINT ["dotnet", "SuperGMS.HttpProxy.dll"]