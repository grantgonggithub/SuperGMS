namespace csharp GrantMicroService.GrantRpc.Thrift.IGrantRpc

service GrantService{
    string Send(1:string my_args)
}