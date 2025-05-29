using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AI4NGClassifierLambda;

public class DocsSwaggerFactory : WebApplicationFactory<LocalEntryPoint>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Register Swagger services manually
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Classifier API", Version = "v1" });

                // Try to locate the XML doc file from the build output of the Lambda project
                var possiblePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "AI4NGClassifierLambda.xml"),
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "src", "AI4NGClassifierLambda", "bin", "Release", "net8.0", "AI4NGClassifierLambda.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "AI4NGClassifierLambda", "bin", "Release", "net8.0", "AI4NGClassifierLambda.xml")
                };

                foreach (var xmlPath in possiblePaths)
                {
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                        Console.WriteLine($"✅ Loaded XML comments from: {xmlPath}");
                        break;
                    }
                }
            });
        });

        return base.CreateHost(builder);
    }
}