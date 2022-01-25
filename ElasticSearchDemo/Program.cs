using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// elastic search config
builder.Services.AddSingleton<IElasticClient>(sp =>
{
    ConnectionSettings settings = new(
        "test:YXAtc291dGgtMS5hd3MuZWxhc3RpYy1jbG91ZC5jb206OTI0MyQzNTZjZDE3NjY0YWI0ODMyOTQyN2I3NWMwYTkwNzFhMyQ4M2ZiNmE5N2UxZGM0NDEzODc3YjY5ZTY0ZmZlMDAzNA==",
        new BasicAuthenticationCredentials(
            "elastic",
            "YvVvU3zVP7GjjrUCB6NwxNpJ"
        )
    );

    //new Uri("https://n3o-test.es.ap-south-1.aws.elastic-cloud.com:9243"
    return new ElasticClient(settings);
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
