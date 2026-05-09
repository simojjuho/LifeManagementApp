using Microsoft.OpenApi;

namespace LifeManagementTool.Extensions;

public static class OpenAPISwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LifeManagementTool API",
                Version = "v0.1",
                Description = """
                              # LifeManagementTool API
                              
                              ## Description
                              The werld is filled with project management tools and this will be another one of those. The difference is that this tool is going to serve only my own purposes. I need a small and easy-to-use tool that I can use to track my tasks at my day job. This project is an answer to that need and it at the same time lets me study programming.
                              
                              This project comes from the need to have a project simple enough to keep my motivation high and challengin enough to keep on learning.
                              
                              ## Key features (not final list, features not implemented yet)
                              - Keep list of tasks that need to be done and tasks that are done.
                              - Every task has an urgency classification and final due date if there is one.
                              - User role management and share tasks 
                              
                              ## Used packages
                              - Swashbuckle.AspNetCore
                              
                              ## Plans for the future
                              My aim is to publish this on a private web site and use is by myself. If it works there might be a public version of the app. I'm not trying to compete with commercial apps but just myself.
                              """,
                                Contact = new OpenApiContact
                                {
                                    Name = "Juho Simojoki",
                                    Email = "juho [dot] [the only river in northern Finland without dams is my last name [at] outlook [dot] com",
                                }
            });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid JWT token."
            });
            
            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement()
            {
                [new OpenApiSecuritySchemeReference("bearer", doc)] = []
            });

            string[] hiddenEndpoints =
            [
                "api/auth/register",
                "api/auth/refresh",
                "api/auth/confirmemail",
                "api/auth/resendconfirmationemail",
                "api/auth/forgotpassword",
                "api/auth/resetpassword",
                "api/auth/manage",
                "api/auth/manage/info",
                "api/auth/manage/2fa"
            ];
            
            c.DocInclusionPredicate((docName, description) =>
            {
                var path = description.RelativePath.ToLowerInvariant();

                if (path is null)
                    return false;
                
                if (hiddenEndpoints.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }

                return true;

            });
        });
        return services;
    }
}