﻿using System;
using System.IO;
using System.Text;
using DocsGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

var factory = new DocsSwaggerFactory();
using var scope = factory.Services.CreateScope();


var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
var doc = swaggerProvider.GetSwagger("v1");

var sb = new StringBuilder();
var writer = new OpenApiYamlWriter(new StringWriter(sb));
doc.SerializeAsV3(writer);

var outputPath = Path.Combine(SwaggerOutputHelper.GetSolutionRoot(), "docs", "swagger.yaml");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, sb.ToString());

Console.WriteLine($"✅ Swagger YAML generated at {outputPath}");