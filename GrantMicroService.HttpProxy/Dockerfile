FROM registry.cn-hangzhou.aliyuncs.com/grant-oms/dotnet:v3.1-dotnet-runtime

WORKDIR /publish
COPY  ./Publish/ /publish/
ENTRYPOINT ["dotnet", "Grant.HttpProxy.dll"]