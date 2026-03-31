using LMS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Persistence;

/// <summary>
/// Password mặc định là Password@123
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Module> Modules => Set<Module>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<LessonAttachment> LessonAttachments => Set<LessonAttachment>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();

    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<CertificateTemplate> CertificateTemplates => Set<CertificateTemplate>();

    public DbSet<QAThread> QAThreads => Set<QAThread>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Badge> Badges => Set<Badge>();

    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserGroup> UserGroups => Set<UserGroup>();

    public DbSet<UserGroupMember> UserGroupMembers => Set<UserGroupMember>();

    public DbSet<CourseGroup> CourseGroups => Set<CourseGroup>();

    public DbSet<CourseGroupCourse> CourseGroupCourses => Set<CourseGroupCourse>();

    public DbSet<DepartmentCourseGroup> DepartmentCourseGroups { get; set; }

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<LessonChat> LessonChats => Set<LessonChat>();

    public DbSet<DocumentChat> DocumentChats => Set<DocumentChat>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeToUtcConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeToUtcConverter>();
    }

    private class DateTimeToUtcConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private class NullableDateTimeToUtcConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);
        var staticDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        m.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Module>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Lesson>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<LessonAttachment>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<LessonProgress>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Quiz>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<QuestionBank>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<QuizAttempt>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<QuizAnswer>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Certificate>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<CertificateTemplate>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<QAThread>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Note>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Badge>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<UserBadge>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Document>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<DocumentVersion>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<DocumentPermission>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<UserGroup>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<UserGroupMember>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<CourseGroup>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<CourseGroupCourse>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<LessonChat>().HasQueryFilter(e => !e.IsDeleted);
        m.Entity<DocumentChat>().HasQueryFilter(e => !e.IsDeleted);

        m.Entity<DepartmentCourseGroup>(
            e =>
            {
                e.HasKey(dcg => new { dcg.DepartmentId, dcg.CourseGroupId });
                e.HasQueryFilter(dcg => !dcg.CourseGroup.IsDeleted);
            });

        m.Entity<User>(
            e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256);
                e.Property(u => u.FullName).HasMaxLength(200);
                e.Property(u => u.Role).HasMaxLength(50);
            });

        m.Entity<Course>(
            e =>
            {
                e.HasOne(c => c.CreatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.Category)
                    .WithMany(cat => cat.Courses)
                    .HasForeignKey(c => c.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.Property(c => c.Title).HasMaxLength(500);
            });

        m.Entity<Category>(
            e =>
            {
                e.Property(c => c.Name).HasMaxLength(200).IsRequired();
                e.Property(c => c.Slug).HasMaxLength(200);
            });

        m.Entity<Module>(
            e =>
            {
                e.HasOne(mod => mod.Course)
                    .WithMany(c => c.Modules)
                    .HasForeignKey(mod => mod.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<Lesson>(
            e =>
            {
                e.HasOne(l => l.Module)
                    .WithMany(mod => mod.Lessons)
                    .HasForeignKey(l => l.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(l => l.Type).HasMaxLength(20);
                e.Property(l => l.VideoProvider).HasMaxLength(50);

                e.HasOne(l => l.Quiz).WithMany().HasForeignKey(l => l.QuizId).IsRequired(false);
            });

        m.Entity<LessonAttachment>(
            e =>
            {
                e.HasOne(la => la.Lesson)
                    .WithMany(l => l.Attachments)
                    .HasForeignKey(la => la.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<Enrollment>(
            e =>
            {
                e.HasOne(en => en.User)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(en => en.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(en => en.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(en => en.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(en => en.AssignedBy)
                    .WithMany()
                    .HasForeignKey(en => en.AssignedById)
                    .OnDelete(DeleteBehavior.SetNull);
                e.HasOne(en => en.GroupEnroll)
                    .WithMany(g => g.GroupEnrollments)
                    .HasForeignKey(en => en.GroupEnrollId)
                    .OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(en => new { en.UserId, en.CourseId }).IsUnique();
                e.Property(en => en.Status).HasMaxLength(20);
            });

        m.Entity<LessonProgress>(
            e =>
            {
                e.HasOne(lp => lp.User)
                    .WithMany(u => u.LessonProgresses)
                    .HasForeignKey(lp => lp.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(lp => lp.Lesson)
                    .WithMany(l => l.Progresses)
                    .HasForeignKey(lp => lp.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(lp => new { lp.UserId, lp.LessonId }).IsUnique();
            });

        m.Entity<Quiz>(
            e =>
            {
                e.HasOne(q => q.Course)
                    .WithMany(c => c.Quizzes)
                    .HasForeignKey(q => q.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<QuestionBank>(
            e =>
            {
                e.HasOne(qb => qb.Quiz)
                    .WithMany(q => q.Questions)
                    .HasForeignKey(qb => qb.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(qb => qb.CorrectAnswer).HasMaxLength(1);
                e.Property(qb => qb.Points).HasPrecision(5, 2);
            });

        m.Entity<QuizAttempt>(
            e =>
            {
                e.HasOne(qa => qa.User)
                    .WithMany(u => u.QuizAttempts)
                    .HasForeignKey(qa => qa.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(qa => qa.Quiz)
                    .WithMany(q => q.Attempts)
                    .HasForeignKey(qa => qa.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(qa => qa.Score).HasPrecision(5, 2);
                e.Property(qa => qa.Status).HasMaxLength(20);
            });

        m.Entity<QuizAnswer>(
            e =>
            {
                e.HasOne(qa => qa.Attempt)
                    .WithMany(a => a.Answers)
                    .HasForeignKey(qa => qa.AttemptId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qa => qa.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(qa => qa.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.Property(qa => qa.SelectedAnswer).HasMaxLength(1);
                e.HasIndex(qa => new { qa.AttemptId, qa.QuestionId }).IsUnique();
            });

        m.Entity<Certificate>(
            e =>
            {
                e.HasOne(c => c.User)
                    .WithMany(u => u.Certificates)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(c => c.Course)
                    .WithMany(c2 => c2.Certificates)
                    .HasForeignKey(c => c.CourseId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(c => c.Attempt).WithMany().HasForeignKey(c => c.AttemptId).OnDelete(DeleteBehavior.NoAction);
                e.HasIndex(c => c.CertificateCode).IsUnique();
                e.HasIndex(c => new { c.UserId, c.CourseId }).IsUnique();
            });

        m.Entity<CertificateTemplate>(
            e =>
            {
                e.HasOne(ct => ct.Course)
                    .WithOne(c => c.CertificateTemplate)
                    .HasForeignKey<CertificateTemplate>(ct => ct.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<QAThread>(
            e =>
            {
                e.HasOne(qa => qa.Lesson)
                    .WithMany(l => l.QAThreads)
                    .HasForeignKey(qa => qa.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qa => qa.User)
                    .WithMany(u => u.QAThreads)
                    .HasForeignKey(qa => qa.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(qa => qa.Parent)
                    .WithMany(qa2 => qa2.Replies)
                    .HasForeignKey(qa => qa.ParentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

        m.Entity<Note>(
            e =>
            {
                e.HasOne(n => n.User)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(n => n.Lesson)
                    .WithMany(l => l.Notes)
                    .HasForeignKey(n => n.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<LessonChat>(
            e =>
            {
                e.HasOne(lc => lc.Lesson).WithMany().HasForeignKey(lc => lc.LessonId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(lc => lc.User).WithMany().HasForeignKey(lc => lc.UserId).OnDelete(DeleteBehavior.NoAction);
            });

        m.Entity<DocumentChat>(
            e =>
            {
                e.HasOne(dc => dc.Document)
                    .WithMany()
                    .HasForeignKey(dc => dc.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(dc => dc.User).WithMany().HasForeignKey(dc => dc.UserId).OnDelete(DeleteBehavior.NoAction);
            });

        m.Entity<Badge>(e => e.Property(b => b.Name).HasMaxLength(100));
        m.Entity<UserBadge>(
            e =>
            {
                e.HasOne(ub => ub.User)
                    .WithMany(u => u.UserBadges)
                    .HasForeignKey(ub => ub.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(ub => ub.Badge)
                    .WithMany(b => b.UserBadges)
                    .HasForeignKey(ub => ub.BadgeId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();
            });

        m.Entity<Document>(
            e =>
            {
                e.HasOne(d => d.UploadedBy)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(d => d.CurrentVersion)
                    .WithMany()
                    .HasForeignKey(d => d.CurrentVersionId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.Property(d => d.Title).HasMaxLength(500);
            });

        m.Entity<DocumentVersion>(
            e =>
            {
                e.HasOne(dv => dv.Document)
                    .WithMany(d => d.Versions)
                    .HasForeignKey(dv => dv.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(dv => dv.UploadedBy)
                    .WithMany()
                    .HasForeignKey(dv => dv.UploadedById)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasIndex(dv => new { dv.DocumentId, dv.VersionNumber }).IsUnique();
            });

        m.Entity<DocumentPermission>(
            e =>
            {
                e.HasOne(dp => dp.Document)
                    .WithMany(d => d.Permissions)
                    .HasForeignKey(dp => dp.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(dp => dp.User).WithMany().HasForeignKey(dp => dp.UserId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(dp => dp.Role).WithMany().HasForeignKey(dp => dp.RoleId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(dp => dp.UserGroup)
                    .WithMany()
                    .HasForeignKey(dp => dp.UserGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(dp => dp.GrantedBy)
                    .WithMany()
                    .HasForeignKey(dp => dp.GrantedById)
                    .OnDelete(DeleteBehavior.NoAction);
            });

        m.Entity<Permission>(e => e.Property(p => p.Code).HasMaxLength(100));

        m.Entity<RolePermission>(
            e =>
            {
                e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                e.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<UserRole>(
            e =>
            {
                e.HasKey(ur => new { ur.UserId, ur.RoleId });
                e.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        m.Entity<UserGroup>(
            e =>
            {
                e.HasOne(g => g.CreatedBy).WithMany().HasForeignKey(g => g.CreatedById).OnDelete(DeleteBehavior.SetNull);
            });

        m.Entity<UserGroupMember>(
            e =>
            {
                e.HasOne(ugm => ugm.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(ugm => ugm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ugm => ugm.User).WithMany().HasForeignKey(ugm => ugm.UserId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(ugm => ugm.AddedBy)
                    .WithMany()
                    .HasForeignKey(ugm => ugm.AddedById)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasIndex(ugm => new { ugm.GroupId, ugm.UserId }).IsUnique();
            });

        m.Entity<CourseGroup>(
            e =>
            {
                e.HasOne(g => g.CreatedBy).WithMany().HasForeignKey(g => g.CreatedById).OnDelete(DeleteBehavior.SetNull);
            });

        m.Entity<CourseGroupCourse>(
            e =>
            {
                e.HasOne(cgc => cgc.Group)
                    .WithMany(g => g.Courses)
                    .HasForeignKey(cgc => cgc.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cgc => cgc.Course)
                    .WithMany()
                    .HasForeignKey(cgc => cgc.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(cgc => new { cgc.GroupId, cgc.CourseId }).IsUnique();
            });

        m.Entity<Category>()
            .HasData(
                new Category
                {
                    Id = 1,
                    Name = "Kỹ năng mềm",
                    Slug = "ky-nang-mem",
                    SortOrder = 1,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Category
                {
                    Id = 2,
                    Name = "Kỹ thuật",
                    Slug = "ky-thuat",
                    SortOrder = 2,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Category
                {
                    Id = 3,
                    Name = "Quy trình",
                    Slug = "quy-trinh",
                    SortOrder = 3,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Category
                {
                    Id = 4,
                    Name = "Lãnh đạo",
                    Slug = "lanh-dao",
                    SortOrder = 4,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Category
                {
                    Id = 5,
                    Name = "Số hóa",
                    Slug = "so-hoa",
                    SortOrder = 5,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                });

        m.Entity<User>()
            .HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin",
                    Email = "admin@lms.com",
                    PasswordHash = "$2a$12$3m6ntV3rzI0hNEhaIWUQ0ufB4w2P/CT6R.7ioRtSyeUrmceKBiZL2",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                });

        m.Entity<Badge>()
            .HasData(
                new Badge
                {
                    Id = 1,
                    Name = "Người mới bắt đầu",
                    Description = "Hoàn thành khóa học đầu tiên",
                    RequiredCourses = 1,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Badge
                {
                    Id = 2,
                    Name = "Học viên chăm chỉ",
                    Description = "Hoàn thành 3 khóa học",
                    RequiredCourses = 3,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Badge
                {
                    Id = 3,
                    Name = "Siêu nhân học tập",
                    Description = "Hoàn thành 5 khóa học",
                    RequiredCourses = 5,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Badge
                {
                    Id = 4,
                    Name = "Bậc thầy tri thức",
                    Description = "Hoàn thành 10 khóa học",
                    RequiredCourses = 10,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                });

        m.Entity<Role>()
            .HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Quản trị viên hệ thống",
                    IsSystem = true,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Role
                {
                    Id = 2,
                    Name = "Instructor",
                    Description = "Giảng viên",
                    IsSystem = true,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Role
                {
                    Id = 3,
                    Name = "Employee",
                    Description = "Nhân viên / Học viên",
                    IsSystem = true,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                });

        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = 1,
                Code = "course.view",
                Name = "Xem khóa học",
                Category = "Course",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 2,
                Code = "course.create",
                Name = "Tạo khóa học",
                Category = "Course",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 3,
                Code = "course.edit",
                Name = "Sửa khóa học",
                Category = "Course",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 4,
                Code = "course.delete",
                Name = "Xóa khóa học",
                Category = "Course",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 5,
                Code = "course.publish",
                Name = "Xuất bản khóa học",
                Category = "Course",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 6,
                Code = "enrollment.view",
                Name = "Xem ghi danh",
                Category = "Enrollment",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 7,
                Code = "enrollment.approve",
                Name = "Duyệt ghi danh",
                Category = "Enrollment",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 8,
                Code = "enrollment.assign",
                Name = "Gán học viên",
                Category = "Enrollment",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 9,
                Code = "doc.view",
                Name = "Xem tài liệu",
                Category = "Document",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 10,
                Code = "doc.upload",
                Name = "Tải tài liệu",
                Category = "Document",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 11,
                Code = "doc.delete",
                Name = "Xóa tài liệu",
                Category = "Document",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 12,
                Code = "quiz.create",
                Name = "Tạo bài tập",
                Category = "Quiz",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 13,
                Code = "quiz.manage",
                Name = "Quản lý bài tập",
                Category = "Quiz",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 14,
                Code = "report.view",
                Name = "Xem báo cáo",
                Category = "Report",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 15,
                Code = "user.manage",
                Name = "Quản lý người dùng",
                Category = "Admin",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 16,
                Code = "role.manage",
                Name = "Quản lý phân quyền",
                Category = "Admin",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 17,
                Code = "group.manage",
                Name = "Quản lý nhóm",
                Category = "Admin",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 18,
                Code = "certificate.view",
                Name = "Xem chứng chỉ",
                Category = "Certificate",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            },
            new Permission
            {
                Id = 19,
                Code = "certificate.manage",
                Name = "Quản lý chứng chỉ",
                Category = "Certificate",
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            }
        };
        m.Entity<Permission>().HasData(permissions);

        m.Entity<UserRole>()
            .HasData(
                new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedAt = staticDate,
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                });

        var adminPermissions = permissions.Select(
            p => new RolePermission
            {
                RoleId = 1,
                PermissionId = p.Id,
                GrantedAt = staticDate,
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            })
            .ToList();
        
        var instructorPermissionIds = new List<int> { 1, 2, 3, 6, 9, 10, 12, 13, 14, 18 };
        var instructorPermissions = instructorPermissionIds.Select(
            id => new RolePermission
            {
                RoleId = 2,
                PermissionId = id,
                GrantedAt = staticDate,
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            }).ToList();

        var employeePermissionIds = new List<int> { 1, 9, 18 };
        var employeePermissions = employeePermissionIds.Select(
            id => new RolePermission
            {
                RoleId = 3,
                PermissionId = id,
                GrantedAt = staticDate,
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            }).ToList();

        m.Entity<RolePermission>().HasData(adminPermissions);
        m.Entity<RolePermission>().HasData(instructorPermissions);
        m.Entity<RolePermission>().HasData(employeePermissions);
    }
}