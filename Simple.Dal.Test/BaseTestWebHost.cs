using AutoMapper;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Core;
using Crpm.Infrastructure.Filters;
using Crpm.Infrastructure.Helpers;
using Crpm.Infrastructure.Interfaces;
using Crpm.Infrastructure.Models;
using Crpm.Infrastructure.Services;
using Crpm.Model.Data;
using Crpm.Model.Entities;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;

namespace Crpm.Dal.UnitTest
{
    /// <summary> define web test context </summary>
    public class TestWebHost : BaseService
    {
        protected IServiceScope serviceScope;
        protected bool disposedValue;
        protected string UserTestGuid = "00000000000000000000000000000000";
        public IWebHostBuilder WebHostBuilder;
        public TestServer testServer;

        /// <summary> define web test context </summary>
        public TestWebHost()
        {
            WebHostBuilder = new WebHostBuilder()
                .Configure(appBuilder =>
                {
                    GeneralContext.HttpContext.RequestServices = appBuilder.ApplicationServices;
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    env.EnvironmentName = "Test";

                    config.SetBasePath(env.ContentRootPath)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                          .AddEnvironmentVariables(prefix: "CRPM_")
                          .AddEnvironmentVariables()
                          .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    var loggerConfig = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration);

                    // appConfig
                    var configSection = context.Configuration.GetSection("AppConfig");
                    var appConfig = configSection?.Get<AppConfig>();
                    services.AddSingleton<IAppConfig>(appConfig);

                    // authOptions
                    var authConfigSection = context.Configuration.GetSection("AuthOptions");
                    var authOptions = authConfigSection?.Get<AuthOptions>();
                    services.AddSingleton<IAuthOptions>(authOptions);

                    // logger
                    Log.Logger = loggerConfig.CreateLogger();
                    Log.Information($"{Assembly.GetEntryAssembly().GetName().Name} API started");
                    services.AddSingleton(Log.Logger);

                    // contexts
                    var mockContext = new Mock<HttpContext>(MockBehavior.Strict);
                    mockContext.SetupGet(hc => hc.User.Identity.Name).Returns("4cast");
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

                    //var mockIActionContextAccessor = new Mock<IActionContextAccessor>();
                    //mockIActionContextAccessor.SetupGet(x => x.ActionContext)
                    //.Returns(new ActionContext());
                    //services.AddSingleton<IActionContextAccessor>(mockIActionContextAccessor.Object);
                    //services.AddSingleton<IActionContextAccessor>(mockIActionContextAccessor.Object);

                    // referenced assemblies
                    var referencedAssembliesNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                        .Select(x => x.Name)
                        .Where(x => x.ToUpper().Contains("CRPM"))
                        .ToList();

                    referencedAssembliesNames.Add(Assembly.GetExecutingAssembly().GetName().Name);
                    var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => referencedAssembliesNames.Contains(x.GetName().Name))
                        .ToList();
                    Util.CreateGuid(); //need for include reference to Crmp.Utilities

                    services
                        .AddMvc(options =>
                        {
                            options.Filters.Add(typeof(ApiExceptionFilter));
                            options.Filters.Add(typeof(ApiValidationFilter));
                        })
                        .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                        .AddFluentValidation(config =>
                        {
                            config.RegisterValidatorsFromAssemblies(referencedAssemblies);
                            config.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                        });

                    // AutoMapper
                    services.AddAutoMapper(referencedAssemblies);

                    services.AddScoped<OrganizationService>();
                    services.AddScoped<ActivityService>();
                    services.AddScoped<ReportService>();
                    services.AddScoped<ActivityTemplateService>();
                    services.AddScoped<FormService>();
                    services.AddScoped<FormTemplateService>();
                    services.AddScoped<GeneralService>();
                    services.AddScoped<ModelService>();
                    services.AddScoped<UserService>();

                    services.AddScoped(provider =>
                    {
                        var connectionString = context.Configuration["ConnectionStrings:CRPMDatabase"];
                        var builder = new DbContextOptionsBuilder<CRPMContext>();
                        var migrationsAssemblyName = typeof(TestWebHost).Assembly.GetName().Name;
                        builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName));
                        builder.UseLoggerFactory(CRPMContext.CRPMLoggerFactory);
                        var dbContext = new CRPMContext(builder.Options);
                        return dbContext;
                    });

                    var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
                    Expression<Func<IUrlHelper, string>> urlSetup = url =>
                        url.Action(It.Is<UrlActionContext>(uac => uac.Action == "Get"));
                    mockUrlHelper.Setup(urlSetup).Returns("mock/testing");

                    //GeneralContext.SetServiceProvider(services.BuildServiceProvider());
                    GeneralContext.HttpContext = new DefaultHttpContext() { RequestServices = services.BuildServiceProvider() };
                    var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                    mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(GeneralContext.HttpContext);

                    var requestMethod = GeneralContext.HttpContext?.Request?.Method;
                    requestMethod = string.IsNullOrEmpty(requestMethod) ? "GET" : requestMethod;
                    var mockBaseService = new Mock<IBaseService>();
                    Expression<Func<IBaseService, HttpMethod>> CurrentHttpMethodAction = x =>
                        It.Is<HttpMethod>(y => y.Method == "");
                    mockBaseService.Setup(x => x.CurrentHttpMethod).Returns(new HttpMethod("GET"));

                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(logging.Services
                        .BuildServiceProvider()
                        .GetRequiredService<ILogger>(),
                        dispose: true);
                })
                .UseSerilog()
                .UseStartup<Startup>();

            testServer = new TestServer(WebHostBuilder);
        }

        public UserDetails CurrentTestUser => GetTestUser();

        private UserDetails GetTestUser()
        {
            var userService = GeneralContext.GetService<UserService>();
            userService.DbContext = serviceScope.ServiceProvider.GetService<CRPMContext>();
            var testUser = new UserDetails()
            {
                UserGuid = null,
                UserId = int.MinValue.ToString(),
                UserName = "user_" + new Random().Next(int.MinValue, int.MaxValue) + "_test",
                Password = "123456",
                UserFirstName = "userFirstName_" + new Random().Next(int.MinValue, int.MaxValue),
                UserLastName = "userLastName_" + new Random().Next(int.MinValue, int.MaxValue),
                UserBusinessPhone = "123456789",
                UserMobilePhone = "123456789",
                UserNotes = "",
                UserCreateDate = Util.ConvertDateToString(DateTime.Now),
                UserStatus = "activate",
                OrgGuid = "aaaabbbbccccddddeeeeffffgggghhhh",
                UserAdminPermission = 8,
                RoleId = 8,
                UserType = 2,
                JobTitle = "",
                JobTitleGuid = "",
                Language = 2,
                UnitGuid = "aaaabbbbccccddddeeeeffffgggghhhh",
                OrgName = "",
                RoleName = "",
                UserMail = "test@test.com"
            };
            var code = userService.SaveUser(testUser, true).Result;
            var user = userService.DbContext.User.FirstOrDefault(x => x.UserId == int.MinValue.ToString());
            testUser.UserGuid = user.UserGuid;
            return testUser;
        }
    }
}
