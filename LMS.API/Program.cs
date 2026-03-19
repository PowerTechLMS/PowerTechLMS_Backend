using System.Text;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Services;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Use port 5100 to avoid macOS AirPlay conflict on port 5000
builder.WebHost.UseUrls("http://localhost:5100");

// Increase upload limits to 2GB
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2147483648; // 2GB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2147483648; // 2GB
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Course Policies
    options.AddPolicy("CourseView", p => p.RequireClaim("permission", "course.view"));
    options.AddPolicy("CourseCreate", p => p.RequireClaim("permission", "course.create"));
    options.AddPolicy("CourseEdit", p => p.RequireClaim("permission", "course.edit"));
    options.AddPolicy("CourseDelete", p => p.RequireClaim("permission", "course.delete"));
    options.AddPolicy("CoursePublish", p => p.RequireClaim("permission", "course.publish"));

    // Enrollment Policies
    options.AddPolicy("EnrollmentView", p => p.RequireClaim("permission", "enrollment.view"));
    options.AddPolicy("EnrollmentApprove", p => p.RequireClaim("permission", "enrollment.approve"));
    options.AddPolicy("EnrollmentAssign", p => p.RequireClaim("permission", "enrollment.assign"));

    // Document Policies
    options.AddPolicy("DocView", p => p.RequireClaim("permission", "doc.view"));
    options.AddPolicy("DocUpload", p => p.RequireClaim("permission", "doc.upload"));
    options.AddPolicy("DocDelete", p => p.RequireClaim("permission", "doc.delete"));

    // Quiz Policies
    options.AddPolicy("QuizCreate", p => p.RequireClaim("permission", "quiz.create"));
    options.AddPolicy("QuizManage", p => p.RequireClaim("permission", "quiz.manage"));

    // Report Policies
    options.AddPolicy("ReportView", p => p.RequireClaim("permission", "report.view"));

    // Admin Policies
    options.AddPolicy("UserManage", p => p.RequireClaim("permission", "user.manage"));
    options.AddPolicy("RoleManage", p => p.RequireClaim("permission", "role.manage"));
    options.AddPolicy("GroupManage", p => p.RequireClaim("permission", "group.manage"));

    options.AddPolicy("GroupView", p => p.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.IsInRole("Quản trị viên") ||
        context.User.IsInRole("Instructor") || context.User.IsInRole("Giảng viên") ||
        context.User.HasClaim("permission", "group.manage")
    ));

    options.AddPolicy("RoleView", p => p.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.IsInRole("Quản trị viên") ||
        context.User.IsInRole("Instructor") || context.User.IsInRole("Giảng viên") ||
        context.User.HasClaim("permission", "role.manage")
    ));

    options.AddPolicy("UserList", p => p.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.IsInRole("Quản trị viên") ||
        context.User.IsInRole("Instructor") || context.User.IsInRole("Giảng viên") ||
        context.User.HasClaim("permission", "user.manage") || 
        context.User.HasClaim("permission", "enrollment.assign") ||
        context.User.HasClaim("permission", "enrollment.view") ||
        context.User.HasClaim("permission", "enrollment.approve") ||
        context.User.HasClaim("permission", "course.view")
    ));

    // Certificate Policies
    options.AddPolicy("CertificateView", p => p.RequireAssertion(context =>
        context.User.IsInRole("Admin") || 
        context.User.IsInRole("Quản trị viên") ||
        context.User.IsInRole("Instructor") || 
        context.User.IsInRole("Giảng viên") ||
        context.User.HasClaim("permission", "certificate.view") || 
        context.User.HasClaim("permission", "certificate.manage")
    ));
    options.AddPolicy("CertificateManage", p => p.RequireClaim("permission", "certificate.manage"));
});


// Services DI
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

// Video Processing Services
builder.Services.AddSingleton<IFFmpegDownloader, FFmpegDownloader>();
builder.Services.AddSingleton<IVideoProcessingQueue, VideoProcessingQueue>();
builder.Services.AddHostedService<VideoProcessingWorker>();
builder.Services.AddSignalR();

// Email Services
builder.Services.AddSingleton<IMailQueue, MailQueue>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<MailBackgroundService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LMS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Setup uploads directory in wwwroot
var wwwroot = builder.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwroot)) Directory.CreateDirectory(wwwroot);
var uploadsPath = Path.Combine(wwwroot, "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/x-mpegURL";
provider.Mappings[".ts"] = "video/MP2T";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VideoHub>("/hubs/video");
app.MapHub<NotificationHub>("/hubs/notifications");

// Auto-migrate database (simple version for dev)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.EnsureDeleted(); // Tránh xóa database liên tục gây mất dữ liệu và lỗi session
    db.Database.EnsureCreated();
    
    // Auto-patch missing DB columns for those who don't run EF Migrations
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Courses' AND COLUMN_NAME = 'RequiresApproval')
        BEGIN
            ALTER TABLE Courses ADD RequiresApproval BIT NOT NULL DEFAULT 1;
        END

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Lessons' AND COLUMN_NAME = 'ReadingDurationSeconds')
        BEGIN
            ALTER TABLE Lessons ADD ReadingDurationSeconds INT NOT NULL DEFAULT 0;
        END

        -- BÁO CÁO: TỰ ĐỘNG CHÈN QUYỀN XEM CHỨNG CHỈ NẾU CHƯA CÓ
        IF NOT EXISTS (SELECT * FROM Permissions WHERE Code = 'certificate.view')
        BEGIN
            INSERT INTO Permissions (Code, Name, Category, IsDeleted) 
            VALUES ('certificate.view', N'Xem chứng chỉ', 'Certificate', 0);
            
            DECLARE @permId INT = SCOPE_IDENTITY();
            -- Gán cho Admin (Role 1)
            IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 1 AND PermissionId = @permId)
                INSERT INTO RolePermissions (RoleId, PermissionId, IsDeleted) VALUES (1, @permId, 0);
            -- Gán cho Instructor (Role 2)
            IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 2 AND PermissionId = @permId)
                INSERT INTO RolePermissions (RoleId, PermissionId, IsDeleted) VALUES (2, @permId, 0);
        END

        -- TỰ ĐỘNG CHÈN QUYỀN QUẢN LÝ CHỨNG CHỈ (certificate.manage)
        IF NOT EXISTS (SELECT * FROM Permissions WHERE Code = 'certificate.manage')
        BEGIN
            INSERT INTO Permissions (Code, Name, Category, IsDeleted, CreatedAt, UpdatedAt) 
            VALUES ('certificate.manage', N'Quản lý chứng chỉ', 'Certificate', 0, GETUTCDATE(), GETUTCDATE());
            
            DECLARE @managePermId INT = (SELECT Id FROM Permissions WHERE Code = 'certificate.manage');
            -- Gán cho Admin (Role 1)
            IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 1 AND PermissionId = @managePermId)
                INSERT INTO RolePermissions (RoleId, PermissionId, IsDeleted, GrantedAt) VALUES (1, @managePermId, 0, GETUTCDATE());
            -- Gán cho Instructor (Role 2)
            IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 2 AND PermissionId = @managePermId)
                INSERT INTO RolePermissions (RoleId, PermissionId, IsDeleted, GrantedAt) VALUES (2, @managePermId, 0, GETUTCDATE());
        END
    ");
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}

app.Run();
