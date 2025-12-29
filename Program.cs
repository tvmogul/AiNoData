using AiNoData.Services.Budget;
using AiNoData.Services.Drone;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Cryptography;

namespace AiNoData
{
    public class Program
    {
        public static IConfiguration? Configuration { get; private set; }

        public static void Main(string[] args)
        {
            // deletes the entire AiNetProfit folder on first run (per-machine)
            //FirstRunCleanup.Run();

            //// ---- OCR bootstrap BEFORE building/running the host ----
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            //    && ElectronNET.API.HybridSupport.IsElectronActive)
            //{
            //    // Windows only: copy native DLLs to all Electron.NET runtime folders
            //    TesseractDeployer.CopyFromNuGetToElectronTargets();
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            //         && ElectronNET.API.HybridSupport.IsElectronActive)
            //{
            //    // macOS only: check for dylibs (bundled or Homebrew) and set loader path
            //    MacOcrDeps.Ensure();
            //}

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    Configuration = config.Build();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowCorsPolicy", builder =>
                        {
                            builder.SetIsOriginAllowed(_ => true)
                                   .AllowAnyHeader()
                                   .AllowAnyMethod()
                                   .AllowCredentials();
                        });
                    });

                    services.AddHttpContextAccessor();

                    services.AddControllersWithViews()
                        .AddJsonOptions(options =>
                        {
                            options.JsonSerializerOptions.PropertyNamingPolicy = null;
                            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                            options.JsonSerializerOptions.Converters.Add(new ByteArrayConverter()); // ? Added for proper byte[] model binding
                        });

                    services.AddScoped<IZ3DAllocatorService, Z3DAllocatorService>();

                    services.AddScoped<IDroneZ3DService, DroneZ3DService>();

                })
                .Configure(app =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                    var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

                    //NativeOcrBootstrap.Init(app.ApplicationServices);
                    //OcrNativeDeployer.Ensure();

                    var _Env = configuration["Environment"];
                    var _AppName = configuration["AppName"];

                    var defaultFileProvider = new FileExtensionContentTypeProvider();
                    defaultFileProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
                    defaultFileProvider.Mappings[".ipa"] = "application/octet-stream";
                    defaultFileProvider.Mappings[".plist"] = "text/xml";
                    defaultFileProvider.Mappings[".mp4"] = "video/mp4";
                    defaultFileProvider.Mappings[".avi"] = "video/x-msvideo";
                    defaultFileProvider.Mappings[".mov"] = "video/quicktime";
                    defaultFileProvider.Mappings[".wmv"] = "video/x-ms-wmv";
                    defaultFileProvider.Mappings[".flv"] = "video/x-flv";
                    defaultFileProvider.Mappings[".mkv"] = "video/x-matroska";
                    defaultFileProvider.Mappings[".webm"] = "video/webm";
                    defaultFileProvider.Mappings[".mp3"] = "audio/mpeg";
                    defaultFileProvider.Mappings[".wav"] = "audio/wav";
                    defaultFileProvider.Mappings[".ogg"] = "audio/ogg";
                    defaultFileProvider.Mappings[".flac"] = "audio/flac";
                    defaultFileProvider.Mappings[".pdf"] = "application/pdf";
                    defaultFileProvider.Mappings[".doc"] = "application/msword";
                    defaultFileProvider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    defaultFileProvider.Mappings[".xls"] = "application/vnd.ms-excel";
                    defaultFileProvider.Mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    defaultFileProvider.Mappings[".ppt"] = "application/vnd.ms-powerpoint";
                    defaultFileProvider.Mappings[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    defaultFileProvider.Mappings[".jpg"] = "image/jpeg";
                    defaultFileProvider.Mappings[".jpeg"] = "image/jpeg";
                    defaultFileProvider.Mappings[".png"] = "image/png";
                    defaultFileProvider.Mappings[".gif"] = "image/gif";
                    defaultFileProvider.Mappings[".bmp"] = "image/bmp";
                    defaultFileProvider.Mappings[".svg"] = "image/svg+xml";
                    defaultFileProvider.Mappings[".zip"] = "application/zip";
                    defaultFileProvider.Mappings[".rar"] = "application/x-rar-compressed";
                    defaultFileProvider.Mappings[".7z"] = "application/x-7z-compressed";
                    defaultFileProvider.Mappings[".tar"] = "application/x-tar";
                    defaultFileProvider.Mappings[".gz"] = "application/gzip";
                    defaultFileProvider.Mappings[".json"] = "application/json";
                    defaultFileProvider.Mappings[".xml"] = "application/xml";
                    defaultFileProvider.Mappings[".csv"] = "text/csv";
                    defaultFileProvider.Mappings[".txt"] = "text/plain";

                    // ?? For testing in Development: show detailed exception pages (helps surface inner exceptions in Electron.NET)
                    if (env.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        ContentTypeProvider = defaultFileProvider
                    });

                    // Raw JSON logger middleware
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.Path == "/Transactions/Import")
                        {
                            context.Request.EnableBuffering();
                            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                            var body = await reader.ReadToEndAsync();
                            Console.WriteLine("RAW JSON: " + body);
                            context.Request.Body.Position = 0;
                        }

                        await next();
                    });

                    app.UseRouting();

                    app.Use(async (context, next) =>
                    {
                        string scriptNonce;
                        using (var rng = RandomNumberGenerator.Create())
                        {
                            var nonceBytes = new byte[32];
                            rng.GetBytes(nonceBytes);
                            scriptNonce = Convert.ToBase64String(nonceBytes);
                        }

                        context.Items["ScriptNonce"] = scriptNonce;

                        string[] domains = new string[]
                        {
                            "https://www.googletagmanager.com",
                            "https://www.google-analytics.com",
                            "https://fonts.googleapis.com",
                            "https://fonts.gstatic.com",
                            "https://cdn.datatables.net",
                            "https://cdnjs.cloudflare.com",
                            "https://cdn.jsdelivr.net",
                            "https://stackpath.bootstrapcdn.com",
                            "https://code.jquery.com",
                            "https://ajax.aspnetcdn.com",
                            "https://www.adobe.com",
                            "https://adobe.com",
                            "https://localhost:*",
                            "wss://localhost:*"
                        };

                        string scriptSrcDomains = string.Join(" ", domains);

                        string csp = $"default-src 'self'; " +
                                     $"script-src 'self' 'nonce-{scriptSrcDomains}' 'strict-dynamic' {scriptSrcDomains}; " +
                                     $"font-src 'self' https://fonts.gstatic.com {scriptSrcDomains}; " +
                                     $"img-src 'self' data: blob: {scriptSrcDomains}; " +
                                     $"object-src 'none'; " +
                                     $"media-src 'self' blob: {scriptSrcDomains}; " +
                                     $"connect-src {scriptSrcDomains} ws://localhost:* https://localhost:* http://localhost:*; " +
                                     $"style-src 'self' 'unsafe-inline' {scriptSrcDomains}; " +
                                     $"frame-ancestors 'self'; " +
                                     $"form-action 'self' {scriptSrcDomains} https://localhost:5001;";

                        context.Response.Headers["Content-Security-Policy"] = "";

                        await next();
                    });

                    app.UseCors("AllowCorsPolicy");

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    });

                });
        }
    }
}
