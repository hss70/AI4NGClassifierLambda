using System.IO;
using AI4NGClassifierLambda;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DocsGenerator;

public class DocsSwaggerFactory : WebApplicationFactory<LocalEntryPoint>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var contentRoot = SwaggerOutputHelper.GetContentRoot();
        builder.UseContentRoot(contentRoot);

        builder.ConfigureServices(services =>
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Classifier API", Version = "v1" });

                var xmlPath = Path.Combine(contentRoot, "bin", "Release", "net8.0", "AI4NGClassifierLambda.xml");
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });
        });

        return base.CreateHost(builder);
    }
}