using Microsoft.AspNetCore.Routing;

namespace LifeManagementTool.Controller.Interfaces;

public interface ICustonIdentityEndpoint
{
    public IEndpointRouteBuilder MapCustomIdentityEndpoints(IEndpointRouteBuilder route);
}