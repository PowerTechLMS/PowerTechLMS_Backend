using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.Services;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .ConfigureKestrel(
        options =>
        {
            options.Limits.MaxRequestBodySize = 2147483648;
        });

builder.Services
    .Configure<FormOptions>(
        options =>
        {
            options.MultipartBodyLengthLimit = 2147483648;
            options.ValueLengthLimit = int.MaxValue;
            options.MemoryBufferThreshold = int.MaxValue;
        });

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services
    .AddDbContext<PostgreSqlDbContext>(
        options => options.UseNpgsql(
            connectionString,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

if(databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<PostgreSqlDbContext>());
} else
{
    builder.Services
        .AddDbContext<AppDbContext>(
            options => options.UseSqlServer(
                connectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

builder.Services
    .AddAuthorization(
        options =>
        {
            options.AddPolicy("CourseView", p => p.RequireClaim("permission", "course.view"));
            options.AddPolicy("CourseCreate", p => p.RequireClaim("permission", "course.create"));
            options.AddPolicy("CourseEdit", p => p.RequireClaim("permission", "course.edit"));
            options.AddPolicy("CourseDelete", p => p.RequireClaim("permission", "course.delete"));
            options.AddPolicy("CoursePublish", p => p.RequireClaim("permission", "course.publish"));

            options.AddPolicy("EnrollmentView", p => p.RequireClaim("permission", "enrollment.view"));
            options.AddPolicy("EnrollmentApprove", p => p.RequireClaim("permission", "enrollment.approve"));
            options.AddPolicy("EnrollmentAssign", p => p.RequireClaim("permission", "enrollment.assign"));

            options.AddPolicy("DocView", p => p.RequireClaim("permission", "doc.view"));
            options.AddPolicy("DocUpload", p => p.RequireClaim("permission", "doc.upload"));
            options.AddPolicy("DocDelete", p => p.RequireClaim("permission", "doc.delete"));

            options.AddPolicy("QuizCreate", p => p.RequireClaim("permission", "quiz.create"));
            options.AddPolicy("QuizManage", p => p.RequireClaim("permission", "quiz.manage"));

            options.AddPolicy("ReportView", p => p.RequireClaim("permission", "report.view"));

            options.AddPolicy("UserManage", p => p.RequireClaim("permission", "user.manage"));
            options.AddPolicy("RoleManage", p => p.RequireClaim("permission", "role.manage"));
            options.AddPolicy("GroupManage", p => p.RequireClaim("permission", "group.manage"));

            options.AddPolicy(
                "GroupView",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.IsInRole("Instructor") ||
                            context.User.IsInRole("Giảng viên") ||
                            context.User.HasClaim("permission", "group.manage")));

            options.AddPolicy(
                "RoleView",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.IsInRole("Instructor") ||
                            context.User.IsInRole("Giảng viên") ||
                            context.User.HasClaim("permission", "role.manage")));

            options.AddPolicy(
                "UserList",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.IsInRole("Instructor") ||
                            context.User.IsInRole("Giảng viên") ||
                            context.User.HasClaim("permission", "user.manage") ||
                            context.User.HasClaim("permission", "enrollment.assign") ||
                            context.User.HasClaim("permission", "enrollment.view") ||
                            context.User.HasClaim("permission", "enrollment.approve") ||
                            context.User.HasClaim("permission", "course.view")));

            options.AddPolicy(
                "CertificateView",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.IsInRole("Instructor") ||
                            context.User.IsInRole("Giảng viên") ||
                            context.User.HasClaim("permission", "certificate.view") ||
                            context.User.HasClaim("permission", "certificate.manage")));
            options.AddPolicy("CertificateManage", p => p.RequireClaim("permission", "certificate.manage"));
            options.AddPolicy(
                "RolePlayManage",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.HasClaim("permission", "roleplay.manage")));

            options.AddPolicy(
                "EssayManage",
                p => p.RequireAssertion(
                        context => context.User.IsInRole("Admin") ||
                            context.User.IsInRole("Quản trị viên") ||
                            context.User.HasClaim("permission", "essay.manage")));
        });


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IRolePlayService, RolePlayService>();
builder.Services.AddScoped<IEssayService, EssayService>();
builder.Services.AddScoped<RolePlayScoringJob>();
builder.Services.AddHttpClient<ILlmService, LlmService>();
builder.Services.AddSingleton<AiSidecarManager>();
builder.Services.AddSingleton<IAiSidecarManager>(sp => sp.GetRequiredService<AiSidecarManager>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<AiSidecarManager>());
builder.Services.AddScoped<IAiToolService, AiToolService>();
builder.Services.AddHttpClient<IAiAgentClient, AiAgentClient>();

builder.Services.AddSingleton<TextExtractionService>();
builder.Services
    .AddSingleton<VectorDbService>(
        sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<VectorDbService>>();
            var embeddingService = sp.GetRequiredService<IPythonEmbeddingService>();
            var url = config["Qdrant:Url"] ?? "http://localhost:6334";
            return new VectorDbService(url, logger, embeddingService);
        });
builder.Services.AddSingleton<IPythonEnvService, PythonEnvService>();
builder.Services.AddSingleton<IPythonEmbeddingService, PythonEmbeddingService>();
builder.Services.AddSingleton<ITranscriptionService, FasterWhisperService>();
builder.Services.AddScoped<IAiProcessingService, AiProcessingService>();
builder.Services.AddScoped<IAiCourseGenerationService, AiCourseGenerationService>();
builder.Services.AddScoped<IOutdatedDocumentScannerService, OutdatedDocumentScannerService>();
builder.Services.AddScoped<DocumentOutdatedJob>();

builder.Services
    .AddHangfire(
        configuration =>
        {
            if(databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
            } else
            {
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(
                        connectionString,
                        new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true
                        });
            }
        });

builder.Services.AddHangfireServer();

builder.Services.AddSingleton<IFFmpegDownloader, FFmpegDownloader>();
builder.Services.AddSingleton<IVideoProcessingQueue, VideoProcessingQueue>();
builder.Services.AddHostedService<VideoProcessingWorker>();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IMailQueue, MailQueue>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<MailBackgroundService>();

builder.Services
    .AddCors(
        options =>
        {
            var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
            var origins = new List<string> { "http://localhost:3000", "http://localhost:5173" };
            if(!origins.Contains(frontendUrl))
                origins.Add(frontendUrl);

            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    policy.WithOrigins(origins.ToArray()).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
        });

builder.Services
    .AddControllers()
    .AddJsonOptions(
        options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddSwaggerGen(
        options =>
        {
            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                    });
            options.AddSecurityRequirement(
                document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("bearer", document)] = []
                    });
        });

var app = builder.Build();

if(args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    if(databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();
        context.Database.Migrate();
    } else
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }

    return;
}

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

var storageRoot = builder.Configuration["Storage:RootPath"];
if(string.IsNullOrEmpty(storageRoot))
{
    storageRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
}
if(!Directory.Exists(storageRoot))
    Directory.CreateDirectory(storageRoot);

var uploadsPath = Path.Combine(storageRoot, "uploads");
if(!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".ts"] = "video/mp2t";
provider.Mappings[".vtt"] = "text/vtt";
provider.Mappings[".srt"] = "text/plain";

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads",
        ContentTypeProvider = provider,
        OnPrepareResponse =
            ctx =>
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                ctx.Context.Response.Headers
                    .Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Range");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
            }
    });

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard(
    "/admin/hangfire",
    new DashboardOptions { Authorization = new[] { new HangfireDashboardAuthorizationFilter() } });

app.MapControllers();
app.MapHub<VideoHub>("/hubs/video");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<AiHub>("/hubs/ai");

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<DocumentOutdatedJob>(
        "OutdatedDocumentScanner",
        job => job.RunAsync(),
        "0 1 * * *");
}

app.Run();
