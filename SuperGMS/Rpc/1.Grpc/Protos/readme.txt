先生成服务端的代码：GrpcServices="Server"
 <ItemGroup>
    <Protobuf Include="Protos\GrantGrpc.proto" GrpcServices="Server" />
  </ItemGroup>


  再生成客户端的代码： GrpcServices="Client"
 <ItemGroup>
    <Protobuf Include="Protos\GrantGrpc.proto" GrpcServices="Client" />
  </ItemGroup>