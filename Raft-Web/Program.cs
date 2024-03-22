using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Raft_Web;
using Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

string gatewayURL = builder.Configuration.GetSection("GATEWAY_URL").Value ?? "";

if (gatewayURL == null || gatewayURL == "${GATEWAY_URL}") gatewayURL = "http://localhost:5161";

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<APIService>(provider =>
{
    return new APIService(gatewayURL);
});

await builder.Build().RunAsync();
