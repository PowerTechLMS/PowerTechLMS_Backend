using Hangfire;
using Hangfire.SqlServer;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Services;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5100");

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

builder.Services
    .AddDbContext<AppDbContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddSingleton<TextExtractionService>();
builder.Services.AddSingleton<VectorDbService>(sp => new VectorDbService("localhost", 6334));
builder.Services
    .AddSingleton<ITranscriptionService>(
        sp => new WhisperService(Path.Combine(builder.Environment.ContentRootPath, "models", "ggml-small-vi.bin")));
builder.Services.AddScoped<IAiProcessingService, AiProcessingService>();

builder.Services
    .AddHangfire(
        configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

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
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

builder.Services
    .AddControllers()
    .AddJsonOptions(
        options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
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

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

var wwwroot = builder.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if(!Directory.Exists(wwwroot))
    Directory.CreateDirectory(wwwroot);
var uploadsPath = Path.Combine(wwwroot, "uploads");
if(!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".ts"] = "video/mp2t";
provider.Mappings[".vtt"] = "text/vtt";
provider.Mappings[".srt"] = "text/plain";

app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard(
    "/admin/hangfire",
    new DashboardOptions { Authorization = new[] { new HangfireDashboardAuthorizationFilter() } });

app.MapControllers();
app.MapHub<VideoHub>("/hubs/video");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
