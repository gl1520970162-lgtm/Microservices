using System;
using System.Net;
using System.Net.Mail;
using Email.API.Domain.AggregatesModel.EmailAggregate;
using Email.API.Extensions;
using Email.API.Infrastructure;
using Email.API.Services;
using Google.Protobuf.Reflection;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Services.Common;
using Services.Core.BaseRepository;
using Services.Core.Consts;
using Services.Core.StaticVariables;
using Services.Core.Tools.Cache;

namespace Email.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = MyMain(args).Result;
            app.Run();
        }

        public static async Task<WebApplication>  MyMain(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddHealthChecks(builder.Configuration);
            builder.Services.AddDbContexts(builder.Configuration); //配置迁移目录、DbContext；
            builder.Services.AddApplicationOptions(builder.Configuration);
            //builder.Services.AddIntegrationServices();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            #region Cors 跨域
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CoreConsts.RequestPolicyName, policy =>
                {
                    var hasOrigins = builder.Configuration["CorUrls"]?.Length > 0;
                    if (hasOrigins)
                    {
                        //policy.WithOrigins(appConfig.CorUrls);
                        policy.WithOrigins(builder.Configuration["CorUrls"]);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }
                    policy
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                    if (hasOrigins)
                    {
                        policy.AllowCredentials();
                    }
                });

                //允许任何源访问Api策略，使用时在控制器或者接口上增加特性[EnableCors(AdminConsts.AllowAnyPolicyName)]
                options.AddPolicy(CoreConsts.AllowAnyPolicyName, policy =>
                {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            #endregion Cors 跨域

            #region  配置Kestrel服务器
            //builder.WebHost.ConfigureKestrel((context, options) =>
            //{
            //    //设置应用服务器Kestrel请求体最大为100MB
            //    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;

            //});
            //builder.WebHost.ConfigureKestrel((context, options) =>
            //{
            //    options.Listen(IPAddress.Loopback, 8001);
            //    //options.Listen(IPAddress.Loopback, 5001,
            //    // listenOptions =>
            //    // {
            //    //     listenOptions.UseHttps("certificate.pfx", "12345678");
            //    // });
            //    //options.ListenLocalhost(5001,
            //    // listenOptions =>
            //    // {
            //    //     listenOptions.UseHttps("certificate.pfx", "12345678");
            //    // });

            //    //options.ListenAnyIP(8000);
            //    //options.ListenAnyIP(5001,
            //    //listenOptions =>
            //    //{
            //    //    listenOptions.UseHttps("certificate.pfx", "G0AlhAgK");
            //    //});
            //    //options.ListenAnyIP(5003);
            //    //options.ListenAnyIP(5004, opts => opts.UseHttps());
            //    //options.ListenLocalhost(5005, opts => opts.UseHttps());
            //});

            #endregion

            var services = builder.Services;

            #region 公共注册
            services.AddScoped<IMapper, Mapper>();
            //services.AddSingleton<ICacheTool, MemoryCacheTool>();
            #endregion

            #region 私有注册；
            services.AddScoped<IBaseRepository<EmailDBContext, EmailEntity>, BaseRepository<EmailDBContext, EmailEntity, long>>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton((new EmailContextDesignFactory().CreateDbContext(null)));

            services
            .AddFluentEmail("fromemail@test.test")
            .AddRazorRenderer()
            .AddSmtpSender("smtp.qq.com", "1520970162@qq.com", "kzueteghnrvnjgjg");
            //SmtpClient smtp = new SmtpClient
            //{
            //    //smtp服务器地址
            //    Host = "smtp.qq.com",
            //    UseDefaultCredentials = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    //这里输入你在发送smtp服务器的用户名和密码
            //    Credentials = new NetworkCredential("*****@qq.com", "*******")
            //};
            #endregion
            //services.AddSingleton(services.BuildServiceProvider());
            StaticVariable._ServiceProvider = services.BuildServiceProvider();


            var app = builder.Build();

            app.UseCors("RequestPolicy");

            app.UseServiceDefaults();

            //app.MapPicApi();
            app.MapControllers();
            //app.MapGrpcService<CatalogService>();

            // REVIEW: This is done fore development east but shouldn't be here in production
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EmailDBContext>();
                var settings = app.Services.GetService<IOptions<EmailSettings>>();
                //var logger = app.Services.GetService<ILogger<CatalogContextSeed>>();
                await context.Database.MigrateAsync();

                //await new CatalogContextSeed().SeedAsync(context, app.Environment, settings, logger);
                //var integEventContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
                //await integEventContext.Database.MigrateAsync();
            }
            return app;
        }

    }
}
