using System.Text;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Use port 5100 to avoid macOS AirPlay conflict on port 5000
builder.WebHost.UseUrls("http://localhost:5100");

// Increase upload limits to 100MB
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
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
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

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
var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(); // Phục vụ toàn bộ wwwroot bao gồm cả wwwroot/uploads

app.UseAuthentication();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

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
    ");
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}

app.Run();
