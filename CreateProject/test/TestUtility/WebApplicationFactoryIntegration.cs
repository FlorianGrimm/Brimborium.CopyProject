using Brimborium.Tracerit.Logger;

namespace Brimborium.CreateProject.Test.TestUtility;

public class WebApplicationFactoryIntegration : IAsyncInitializer {
    private WebApplication? _Application;
    public WebApplication GetApplication() => this._Application ?? throw new InvalidOperationException("Application yet is not set");
    public IServiceProvider GetServices() => this.GetApplication().Services;

    private ITracor? _Tracor;
    public ITracor GetTracor() => this._Tracor
        ??= this.GetServices().GetRequiredService<ITracor>();

    private ITracorValidator? _TracorValidator;
    public ITracorValidator GetTracorValidator() => this._TracorValidator
        ??= this.GetServices().GetRequiredService<ITracorValidator>();


    private string? _AddressHttpQ = null;
    private string? _AddressHttpsQ = null;
    public string GetBaseAddress() {
        if (this._AddressHttpsQ is null || this._AddressHttpQ is null) {
            var server = this.GetApplication().Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
            var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
            if (serverAddressesFeature is { Addresses: { } addresses }) {
                foreach (var addressQ in addresses) {
                    if (addressQ is { Length: > 0 } address) {
                        if (address.StartsWith("http:")) {
                            this._AddressHttpQ = address;
                        }
                        if (address.StartsWith("https:")) {
                            this._AddressHttpsQ = address;
                            break;
                        }
                    }
                }
            }
        }

        return this._AddressHttpsQ ?? this._AddressHttpQ ?? throw new Exception("Cannot find BaseAddress");
    }

    private Flurl.Url? _BaseUrl;
    public Flurl.Url GetBaseUrl() {
        if (this._BaseUrl is Flurl.Url baseUrl) {
            return baseUrl.Clone();
        } else {
            baseUrl = new(this.GetBaseAddress());
            if ("https" == baseUrl.Scheme) {
                if (443 == baseUrl.Port) {
                    baseUrl.Port = null;
                }
            } else if ("http" == baseUrl.Scheme) {
                if (80 == baseUrl.Port) {
                    baseUrl.Port = null;
                }
            }
            if (string.IsNullOrEmpty(baseUrl.Path)) {
                baseUrl.Path = "/";
            }
            this._BaseUrl = baseUrl;
            return baseUrl.Clone();
        }
    }

    public HttpClient CreateClient() {
        var server = this.GetApplication().Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var socketsHandler = new SocketsHttpHandler();
        socketsHandler.Credentials = CredentialCache.DefaultCredentials;
        var result = new HttpClient(socketsHandler, true);
        if (this.GetBaseAddress() is { Length: > 0 } baseAddress) {
            result.BaseAddress = new Uri(baseAddress);
        }
        return result;
    }

    public async Task InitializeAsync() {
        string pathStaticAssets = GetPathStaticAssets();
        var contentRoot = Program.GetContentRoot();
        var tsc = new TaskCompletionSource<WebApplication>();
        var taskServer = Program.RunAsync(
            args: new string[] {
                @"--environment=Development",
                $"--contentRoot={contentRoot}",
                @"--applicationName=Sample.Test",
                $"--StaticAssets={pathStaticAssets}"
            },
            new StartupActions() {
                Testtime = true,
                ConfigureWebApplicationBuilder = (builder) => {
                    builder.Configuration.AddJsonFile(GetAppsettingsJson(), optional: false);
                    builder.Services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, null);
                    builder.Services.AddTracor(true);
                    builder.Logging.AddTracorLogger((options) => { options.LogLevel = LogLevel.Trace; });
                    builder.Services.AddTracorActivityListener(true, null, (options) => {
                        options.AllowAllActivitySource = true;
                    });
                    builder.Services.AddReplacements();
                },
                ConfigureWebApplication = (app) => {
                    app.Services.TracorActivityListenerStart();
                },
                RunningWebApplication = (app, task) => {
                    tsc.SetResult(app);
                }
            });
        await Task.Delay(100);
        this._Application = await tsc.Task;
    }

    private static string GetPathStaticAssets() {
        var assemblyLocation = Path.GetDirectoryName(
                typeof(WebApplicationFactoryIntegration).Assembly.Location ?? throw new Exception("")
            ) ?? throw new Exception("");
        var result = Path.Combine(
            assemblyLocation,
            "Sample.staticwebassets.endpoints.json");
        return result;
    }
    private static string GetAppsettingsJson([CallerFilePath] string callerFilePath = "") {
        var testUtilityDir = System.IO.Path.GetDirectoryName(callerFilePath) ?? throw new Exception();
        var projectDir = System.IO.Path.GetDirectoryName(testUtilityDir) ?? throw new Exception();
        var result = System.IO.Path.Combine(projectDir, "appsettings.json");
        return result;
    }
}