// Khai báo rõ để build được cả khi host restore không sinh global using của Umbraco (vd. Railpack)
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

// Nạp biến môi trường từ file .env (nếu có) – dùng cho host chỉ hỗ trợ cấu hình qua .env.
// Mỗi dòng: KEY=VALUE, dùng "__" thay cho ":" (vd. Umbraco__CMS__Unattended__InstallUnattended=true).
foreach (var dir in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() }.Distinct())
{
    var envFile = Path.Combine(dir, ".env");
    if (!File.Exists(envFile)) continue;
    foreach (var raw in File.ReadAllLines(envFile))
    {
        var line = raw.Trim();
        if (line.Length == 0 || line.StartsWith('#') || !line.Contains('=')) continue;
        var key = line[..line.IndexOf('=')].Trim();
        var value = line[(line.IndexOf('=') + 1)..].Trim().Trim('"');
        if (Environment.GetEnvironmentVariable(key) == null) Environment.SetEnvironmentVariable(key, value);
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
