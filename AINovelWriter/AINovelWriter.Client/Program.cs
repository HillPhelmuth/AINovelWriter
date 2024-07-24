using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var services = builder.Services;
services.AddAuthorizationCore();
services.AddCascadingAuthenticationState();
services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

services.AddHttpClient("ServerAPI",
	  client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
	.CreateClient("ServerAPI"));

services.AddRadzenComponents();
await builder.Build().RunAsync();
