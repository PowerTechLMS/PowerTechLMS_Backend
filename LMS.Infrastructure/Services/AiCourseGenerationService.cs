using Hangfire;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class AiCourseGenerationService : IAiCourseGenerationService
{
    private readonly IPythonEnvService _pythonEnv;
    private readonly IHubContext<AiHub> _hubContext;
    private readonly ILogger<AiCourseGenerationService> _logger;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly ILlmService _llm;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailService _emailService;

    private static readonly ConcurrentDictionary<string, CourseGenerationProgress> _jobStore = new();

    public AiCourseGenerationService(
        IPythonEnvService pythonEnv,
        IHubContext<AiHub> hubContext,
        ILogger<AiCourseGenerationService> logger,
        IConfiguration config,
        AppDbContext db,
        ILlmService llm,
        IServiceScopeFactory scopeFactory,
        IEmailService emailService)
    {
        _pythonEnv = pythonEnv;
        _hubContext = hubContext;
        _logger = logger;
        _config = config;
        _db = db;
        _llm = llm;
        _scopeFactory = scopeFactory;
        _emailService = emailService;
    }

    public async Task<string> StartCourseGenerationAsync(
        int userId,
        string topic,
        string targetAudience,
        string additionalInfo)
    {
        var jobId = Guid.NewGuid().ToString("N");

        var aiTask = new AiTask
        {
            JobId = jobId,
            Topic = topic,
            Progress = 0,
            Status = "Đang chờ hàng đợi...",
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.AiTasks.Add(aiTask);
        await _db.SaveChangesAsync();

        var progress = new CourseGenerationProgress
        {
            JobId = jobId,
            Topic = topic,
            Progress = 0,
            Status = "Đang chờ hàng đợi...",
            CreatedAt = aiTask.CreatedAt
        };
        _jobStore[jobId] = progress;

        BackgroundJob.Enqueue<AiCourseGenerationService>(
            s => s.ExecuteGenerationJobAsync(jobId, topic, targetAudience, additionalInfo));

        return jobId;
    }

    public async Task<CourseGenerationProgress> GetProgressAsync(string jobId)
    {
        if(_jobStore.TryGetValue(jobId, out var progress))
        {
            return progress;
        }

        var task = await _db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
        if(task != null)
        {
            return new CourseGenerationProgress
            {
                JobId = task.JobId,
                Topic = task.Topic,
                Progress = task.Progress,
                Status = task.Status,
                ResultJson = task.ResultJson,
                IsCompleted = task.IsCompleted,
                IsFailed = task.IsFailed,
                ErrorMessage = task.ErrorMessage,
                CreatedAt = task.CreatedAt,
                SubTasks =
                    string.IsNullOrEmpty(task.SubTasksJson)
                        ? null
                        : JsonSerializer.Deserialize<List<AiSubTask>>(task.SubTasksJson)
            };
        }

        return new CourseGenerationProgress { JobId = jobId, IsFailed = true, ErrorMessage = "Không tìm thấy Job ID." };
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteGenerationJobAsync(
        string jobId,
        string topic,
        string targetAudience,
        string additionalInfo)
    {
        var state = _jobStore.GetOrAdd(jobId, _ => new CourseGenerationProgress { JobId = jobId, Topic = topic });
        state.Topic = topic;

        try
        {
            state.Status = "Bắt đầu khởi tạo quy trình AI...";
            state.Progress = 5;
            await UpdateTaskInDbAsync(jobId, 5, state.Status);
            await NotifyProgressAsync(state);

            var pythonPath = await _pythonEnv.GetPythonPathAsync();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "AI", "course_gen.py"),
                Path.Combine(currentDir, "AI", "course_gen.py"),
                Path.Combine(currentDir, "External", "AI", "course_gen.py"),
                Path.Combine(currentDir, "LMS.Infrastructure", "AI", "course_gen.py"),
                Path.Combine(currentDir, "..", "LMS.Infrastructure", "AI", "course_gen.py")
            };

            var scriptPath = possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];

            var input = new { topic, target_audience = targetAudience, additional_info = additionalInfo };

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{JsonSerializer.Serialize(input).Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var geminiKey = _config["LlmSettings:ApiKey"];
            if(!string.IsNullOrEmpty(geminiKey))
            {
                startInfo.EnvironmentVariables["GOOGLE_API_KEY"] = geminiKey;
                startInfo.EnvironmentVariables["OPENAI_API_KEY"] = geminiKey;
            }

            var llmBaseUrl = _config["LlmSettings:ApiUrl"]?.TrimEnd('/');
            if(!string.IsNullOrEmpty(llmBaseUrl))
            {
                startInfo.EnvironmentVariables["LLM_BASE_URL"] = llmBaseUrl + "/v1";
                startInfo.EnvironmentVariables["WEB_SEARCH_URL"] = $"{llmBaseUrl}/mcp/web_search_prime/mcp";
                startInfo.EnvironmentVariables["WEB_READER_URL"] = $"{llmBaseUrl}/mcp/web_reader/mcp";
            }

            startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            state.Status = "Đang chạy LangGraph suy luận và tra cứu internet...";
            state.Progress = 20;
            state.SubTasks = new List<AiSubTask>();
            await UpdateTaskInDbAsync(jobId, 20, state.Status);
            await NotifyProgressAsync(state);

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if(string.IsNullOrEmpty(e.Data))
                    return;

                if(e.Data.StartsWith("PROGRESS:"))
                {
                    try
                    {
                        var jsonProgress = e.Data.Substring(9);
                        var subTask = JsonSerializer.Deserialize<AiSubTask>(
                            jsonProgress,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if(subTask != null)
                        {
                            _ = UpdateSubTaskInDbAndNotifyAsync(jobId, subTask);
                        }
                    } catch
                    {
                    }
                } else
                {
                    outputBuilder.Append(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();

            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if(process.ExitCode != 0)
            {
                var error = await errorTask;
                throw new Exception($"Python script failed with exit code {process.ExitCode}: {error}");
            }

            var rawOutput = outputBuilder.ToString();
            var finalResultJson = ExtractJsonFromOutput(rawOutput);

            if(string.IsNullOrEmpty(finalResultJson))
            {
                throw new Exception(
                    "Python script didn't return a valid JSON result. Output: " +
                        (rawOutput.Length > 200 ? rawOutput.Substring(0, 200) + "..." : rawOutput));
            }

            state.ResultJson = finalResultJson;
            state.IsCompleted = true;
            state.Progress = 100;
            state.Status = "Hoàn tất tạo khung khóa học.";

            await UpdateTaskInDbAsync(jobId, 100, state.Status, finalResultJson, true);
            await SaveGeneratedCourseToDbAsync(jobId, finalResultJson);
            await NotifyProgressAsync(state);

            var aiTask = await _db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
            if(aiTask != null)
            {
                _db.Notifications
                    .Add(
                        new Notification
                        {
                            UserId = aiTask.CreatedById,
                            Title = "AI đã hoàn tất",
                            Message = $"Cấu trúc khóa học cho chủ đề '{aiTask.Topic}' đã sẵn sàng.",
                            Type = "AiGeneration",
                            Link = $"/admin/ai-tasks/{jobId}",
                            CreatedAt = DateTime.UtcNow
                        });
                await _db.SaveChangesAsync();
            }
        } catch(Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực hiện AI Course Generation Job {JobId}", jobId);
            state.IsFailed = true;
            state.ErrorMessage = ex.Message;
            state.Status = ("Lỗi: " + ex.Message).Length > 1990
                ? ("Lỗi: " + ex.Message).Substring(0, 1990)
                : ("Lỗi: " + ex.Message);

            await UpdateTaskInDbAsync(jobId, state.Progress, state.Status, null, false, true, ex.Message);
            await NotifyProgressAsync(state);

            var aiTask = await _db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
            if(aiTask != null)
            {
                _db.Notifications
                    .Add(
                        new Notification
                        {
                            UserId = aiTask.CreatedById,
                            Title = "AI thất bại",
                            Message = $"Có lỗi xảy ra khi tạo khóa học '{aiTask.Topic}': {ex.Message}",
                            Type = "AiGenerationError",
                            Link = $"/admin/ai-tasks/{jobId}",
                            CreatedAt = DateTime.UtcNow
                        });
                await _db.SaveChangesAsync();
            }
        }
    }

    private async Task<AiTask?> GetAiTaskFromDbAsync(string jobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
    }

    private async Task UpdateSubTaskInDbAndNotifyAsync(string jobId, AiSubTask subTask)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var task = await db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
        if(task == null)
            return;

        var subTasks = string.IsNullOrEmpty(task.SubTasksJson)
            ? new List<AiSubTask>()
            : JsonSerializer.Deserialize<List<AiSubTask>>(task.SubTasksJson) ?? new List<AiSubTask>();

        var existing = subTasks.FirstOrDefault(s => s.Id == subTask.Id);
        if(existing != null)
        {
            existing.Message = subTask.Message;
            existing.Status = subTask.Status;
        } else
        {
            subTasks.Add(subTask);
        }

        task.SubTasksJson = JsonSerializer.Serialize(subTasks);
        await db.SaveChangesAsync();

        if(_jobStore.TryGetValue(jobId, out var state))
        {
            state.SubTasks = subTasks;
            await NotifyProgressAsync(state);
        }
    }

    private async Task UpdateTaskInDbAsync(
        string jobId,
        int progress,
        string status,
        string? resultJson = null,
        bool isCompleted = false,
        bool isFailed = false,
        string? errorMessage = null)
    {
        var task = await _db.AiTasks.FirstOrDefaultAsync(t => t.JobId == jobId);
        if(task != null)
        {
            task.Progress = progress;
            task.Status = status;
            if(resultJson != null)
                task.ResultJson = resultJson;
            task.IsCompleted = isCompleted;
            task.IsFailed = isFailed;
            task.ErrorMessage = errorMessage;
            await _db.SaveChangesAsync();
        }
    }

    public async Task GenerateLessonVideoFrameAsync(int lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if(lesson is null)
            return;

        var systemPrompt = "Bạn là một chuyên gia biên kịch và quay dựng bài giảng video. Hãy gợi ý khung sườn video.";
        var userPrompt = $"Tên bài học: {lesson.Title}\nMô tả: {lesson.Content}";

        var suggestedFrame = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);
    }

    private async Task SaveGeneratedCourseToDbAsync(string jobId, string json)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var aiTask = await db.AiTasks.Include(t => t.CreatedBy).FirstOrDefaultAsync(t => t.JobId == jobId);
        if(aiTask == null)
            return;

        try
        {
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == "AI Generated");
            if(category == null)
            {
                category = new Category { Name = "AI Generated" };
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }

            var course = new Course
            {
                Title =
                    (result.TryGetProperty("title", out var ctCourse) ? ctCourse.GetString() : null) ?? "Khóa học AI",
                Description =
                    (result.TryGetProperty("description", out var cdCourse) ? cdCourse.GetString() : null) ??
                        string.Empty,
                Level = 3,
                IsPublished = false,
                CategoryId = category.Id,
                UserGroupId = null,
                CreatedById = aiTask.CreatedById,
                CreatedAt = DateTime.UtcNow
            };
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            if(result.TryGetProperty("modules", out var modulesElement) &&
                modulesElement.ValueKind == JsonValueKind.Array)
            {
                var orderIndex = 1;
                foreach(var modElement in modulesElement.EnumerateArray())
                {
                    var module = new Module
                    {
                        Title =
                            (modElement.TryGetProperty("title", out var mtMod) ? mtMod.GetString() : null) ??
                                $"Chương {orderIndex}",
                        CourseId = course.Id,
                        SortOrder = orderIndex++
                    };
                    db.Modules.Add(module);
                    await db.SaveChangesAsync();

                    if(modElement.TryGetProperty("lessons", out var lessonsElement) &&
                        lessonsElement.ValueKind == JsonValueKind.Array)
                    {
                        var resOrderIndex = 1;
                        foreach(var lessonElement in lessonsElement.EnumerateArray())
                        {
                            var type = (lessonElement.TryGetProperty("type", out var lt) ? lt.GetString() : null) ??
                                "Video";
                            var lesson = new Lesson
                            {
                                Title =
                                    (lessonElement.TryGetProperty("title", out var ltit) ? ltit.GetString() : null) ??
                                        "Bài học",
                                ModuleId = module.Id,
                                Type = type,
                                IsFreePreview = false,
                                SortOrder = resOrderIndex++,
                                VideoDurationSeconds =
                                    lessonElement.TryGetProperty("duration_seconds", out var ds) ? ds.GetInt32() : 600,
                                Content =
                                    lessonElement.TryGetProperty("content", out var ct)
                                        ? (ct.ValueKind == JsonValueKind.String ? ct.GetString() : ct.GetRawText())
                                        : null,
                                VideoDraftScript =
                                    lessonElement.TryGetProperty("video_script", out var vs)
                                        ? (vs.ValueKind == JsonValueKind.String ? vs.GetString() : vs.GetRawText())
                                        : null,
                            };

                            if(type == "RolePlay" &&
                                lessonElement.TryGetProperty("role_play", out var rpElement) &&
                                rpElement.ValueKind == JsonValueKind.Object)
                            {
                                var rpConfig = new RolePlayConfig
                                {
                                    Scenario =
                                        (rpElement.TryGetProperty("Scenario", out var sce)
                                                ? (sce.ValueKind == JsonValueKind.String
                                                    ? sce.GetString()
                                                    : sce.GetRawText())
                                                : null) ??
                                            string.Empty,
                                    PassScore =
                                        rpElement.TryGetProperty("PassScore", out var psrp) ? psrp.GetInt32() : 80,
                                    ScoringCriteria =
                                        (rpElement.TryGetProperty("ScoringCriteria", out var scr)
                                                ? (scr.ValueKind == JsonValueKind.String
                                                    ? scr.GetString()
                                                    : scr.GetRawText())
                                                : null) ??
                                            string.Empty,
                                    AdditionalRequirements =
                                        (rpElement.TryGetProperty("AdditionalRequirements", out var areq)
                                                ? (areq.ValueKind == JsonValueKind.String
                                                    ? areq.GetString()
                                                    : areq.GetRawText())
                                                : null) ??
                                            string.Empty,
                                };
                                lesson.RolePlayConfig = rpConfig;
                            }

                            if(type == "Essay" &&
                                lessonElement.TryGetProperty("essay", out var essayElement) &&
                                essayElement.ValueKind == JsonValueKind.Object)
                            {
                                var essayConfig = new EssayConfig { PassScore = 50 };
                                var qContent = essayElement.TryGetProperty("Question", out var eq)
                                    ? (eq.ValueKind == JsonValueKind.String ? eq.GetString() : eq.GetRawText())
                                    : "Hãy phân tích nội dung trên.";
                                var qCriteria = essayElement.TryGetProperty("ScoringCriteria", out var esc)
                                    ? (esc.ValueKind == JsonValueKind.String ? esc.GetString() : esc.GetRawText())
                                    : "Chấm điểm dựa trên tính logic và kiến thức học được.";

                                essayConfig.Questions
                                    .Add(
                                        new EssayQuestion
                                        {
                                            Content = qContent ?? string.Empty,
                                            ScoringCriteria = qCriteria,
                                            SortOrder = 1,
                                            Weight = 100
                                        });
                                lesson.EssayConfig = essayConfig;
                            }

                            db.Lessons.Add(lesson);
                            await db.SaveChangesAsync();

                            if(lessonElement.TryGetProperty("quiz", out var quizElement) &&
                                quizElement.ValueKind == JsonValueKind.Object)
                            {
                                var quiz = new Quiz
                                {
                                    Title =
                                        (quizElement.TryGetProperty("Title", out var qt) &&
                                                    qt.ValueKind == JsonValueKind.String
                                                ? qt.GetString()
                                                : null) ??
                                            "Mini Quiz",
                                    PassScore =
                                        quizElement.TryGetProperty("PassScore", out var psq) ? psq.GetInt32() : 80,
                                    CourseId = course.Id
                                };
                                db.Quizzes.Add(quiz);
                                await db.SaveChangesAsync();

                                if(quizElement.TryGetProperty("questions", out var qsE) &&
                                    qsE.ValueKind == JsonValueKind.Array)
                                {
                                    foreach(var qElement in qsE.EnumerateArray())
                                    {
                                        var qb = new QuestionBank
                                        {
                                            QuizId = quiz.Id,
                                            QuestionText =
                                                (qElement.TryGetProperty("QuestionText", out var qtex1)
                                                        ? qtex1.GetString()
                                                        : (qElement.TryGetProperty("questionText", out var qtex2)
                                                            ? qtex2.GetString()
                                                            : (qElement.TryGetProperty("question_text", out var qtex3)
                                                                ? qtex3.GetString()
                                                                : (qElement.TryGetProperty("Question", out var qtex4)
                                                                    ? qtex4.GetString()
                                                                    : (qElement.TryGetProperty(
                                                                            "question",
                                                                            out var qtex5)
                                                                        ? qtex5.GetString()
                                                                        : null))))) ??
                                                    "Câu hỏi",

                                            CorrectAnswer =
                                                (qElement.TryGetProperty("CorrectAnswer", out var ccax1)
                                                        ? ccax1.GetString()
                                                        : (qElement.TryGetProperty("correctAnswer", out var ccax2)
                                                            ? ccax2.GetString()
                                                            : (qElement.TryGetProperty("correct_answer", out var ccax3)
                                                                ? ccax3.GetString()
                                                                : (qElement.TryGetProperty("Answer", out var ccax4)
                                                                    ? ccax4.GetString()
                                                                    : (qElement.TryGetProperty("answer", out var ccax5)
                                                                        ? ccax5.GetString()
                                                                        : null))))) ??
                                                    "A",
                                            Points = 10
                                        };

                                        if(qElement.TryGetProperty("Options", out var options) ||
                                            qElement.TryGetProperty("options", out options))
                                        {
                                            qb.OptionA = (options.TryGetProperty("A", out var oa)
                                                    ? (oa.ValueKind == JsonValueKind.String
                                                        ? oa.GetString()
                                                        : oa.GetRawText())
                                                    : (options.TryGetProperty("a", out var oal)
                                                        ? (oal.ValueKind == JsonValueKind.String
                                                            ? oal.GetString()
                                                            : oal.GetRawText())
                                                        : null)) ??
                                                string.Empty;
                                            qb.OptionB = (options.TryGetProperty("B", out var ob)
                                                    ? (ob.ValueKind == JsonValueKind.String
                                                        ? ob.GetString()
                                                        : ob.GetRawText())
                                                    : (options.TryGetProperty("b", out var obl)
                                                        ? (obl.ValueKind == JsonValueKind.String
                                                            ? obl.GetString()
                                                            : obl.GetRawText())
                                                        : null)) ??
                                                string.Empty;
                                            qb.OptionC = (options.TryGetProperty("C", out var oc)
                                                    ? (oc.ValueKind == JsonValueKind.String
                                                        ? oc.GetString()
                                                        : oc.GetRawText())
                                                    : (options.TryGetProperty("c", out var ocl)
                                                        ? (ocl.ValueKind == JsonValueKind.String
                                                            ? ocl.GetString()
                                                            : ocl.GetRawText())
                                                        : null)) ??
                                                string.Empty;
                                            qb.OptionD = (options.TryGetProperty("D", out var od)
                                                    ? (od.ValueKind == JsonValueKind.String
                                                        ? od.GetString()
                                                        : od.GetRawText())
                                                    : (options.TryGetProperty("d", out var odl)
                                                        ? (odl.ValueKind == JsonValueKind.String
                                                            ? odl.GetString()
                                                            : odl.GetRawText())
                                                        : null)) ??
                                                string.Empty;
                                        }
                                        db.QuestionBanks.Add(qb);
                                    }
                                    await db.SaveChangesAsync();

                                    lesson.QuizId = quiz.Id;
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }

            if(result.TryGetProperty("final_exam", out var feElement) && feElement.ValueKind == JsonValueKind.Object)
            {
                var finalQuiz = new Quiz
                {
                    Title =
                        (feElement.TryGetProperty("Title", out var fet) && fet.ValueKind == JsonValueKind.String
                                ? fet.GetString()
                                : (feElement.TryGetProperty("title", out var fetl) &&
                                        fetl.ValueKind == JsonValueKind.String
                                    ? fetl.GetString()
                                    : null)) ??
                            "Bài kiểm tra cuối khóa",
                    PassScore =
                        (feElement.TryGetProperty("PassScore", out var feps1)
                            ? feps1.GetInt32()
                            : (feElement.TryGetProperty("pass_score", out var feps2) ? feps2.GetInt32() : 80)),
                    CourseId = course.Id
                };
                db.Quizzes.Add(finalQuiz);
                await db.SaveChangesAsync();

                if(feElement.TryGetProperty("questions", out var feQs) && feQs.ValueKind == JsonValueKind.Array)
                {
                    foreach(var qEl in feQs.EnumerateArray())
                    {
                        var qbFinal = new QuestionBank
                        {
                            QuizId = finalQuiz.Id,
                            QuestionText =
                                (qEl.TryGetProperty("QuestionText", out var qtexF1)
                                        ? qtexF1.GetString()
                                        : (qEl.TryGetProperty("questionText", out var qtexF2)
                                            ? qtexF2.GetString()
                                            : (qEl.TryGetProperty("question_text", out var qtexF3)
                                                ? qtexF3.GetString()
                                                : (qEl.TryGetProperty("Question", out var qtexF4)
                                                    ? qtexF4.GetString()
                                                    : (qEl.TryGetProperty("question", out var qtexF5)
                                                        ? qtexF5.GetString()
                                                        : null))))) ??
                                    "Câu hỏi",

                            CorrectAnswer =
                                (qEl.TryGetProperty("CorrectAnswer", out var ccaxF1)
                                        ? ccaxF1.GetString()
                                        : (qEl.TryGetProperty("correctAnswer", out var ccaxF2)
                                            ? ccaxF2.GetString()
                                            : (qEl.TryGetProperty("correct_answer", out var ccaxF3)
                                                ? ccaxF3.GetString()
                                                : (qEl.TryGetProperty("Answer", out var ccaxF4)
                                                    ? ccaxF4.GetString()
                                                    : (qEl.TryGetProperty("answer", out var ccaxF5)
                                                        ? ccaxF5.GetString()
                                                        : null))))) ??
                                    "A",
                            Points = 10
                        };

                        if(qEl.TryGetProperty("Options", out var optionsF) ||
                            qEl.TryGetProperty("options", out optionsF))
                        {
                            qbFinal.OptionA = (optionsF.TryGetProperty("A", out var oaF)
                                    ? (oaF.ValueKind == JsonValueKind.String ? oaF.GetString() : oaF.GetRawText())
                                    : (optionsF.TryGetProperty("a", out var oaFl)
                                        ? (oaFl.ValueKind == JsonValueKind.String ? oaFl.GetString() : oaFl.GetRawText())
                                        : null)) ??
                                string.Empty;
                            qbFinal.OptionB = (optionsF.TryGetProperty("B", out var obF)
                                    ? (obF.ValueKind == JsonValueKind.String ? obF.GetString() : obF.GetRawText())
                                    : (optionsF.TryGetProperty("b", out var obFl)
                                        ? (obFl.ValueKind == JsonValueKind.String ? obFl.GetString() : obFl.GetRawText())
                                        : null)) ??
                                string.Empty;
                            qbFinal.OptionC = (optionsF.TryGetProperty("C", out var ocF)
                                    ? (ocF.ValueKind == JsonValueKind.String ? ocF.GetString() : ocF.GetRawText())
                                    : (optionsF.TryGetProperty("c", out var ocFl)
                                        ? (ocFl.ValueKind == JsonValueKind.String ? ocFl.GetString() : ocFl.GetRawText())
                                        : null)) ??
                                string.Empty;
                            qbFinal.OptionD = (optionsF.TryGetProperty("D", out var odF)
                                    ? (odF.ValueKind == JsonValueKind.String ? odF.GetString() : odF.GetRawText())
                                    : (optionsF.TryGetProperty("d", out var odFl)
                                        ? (odFl.ValueKind == JsonValueKind.String ? odFl.GetString() : odFl.GetRawText())
                                        : null)) ??
                                string.Empty;
                        }
                        db.QuestionBanks.Add(qbFinal);
                    }
                    await db.SaveChangesAsync();
                }
            }

            if(aiTask.CreatedBy != null && !string.IsNullOrEmpty(aiTask.CreatedBy.Email))
            {
                var emailBody = $@"
                    <p>Chào <b>{aiTask.CreatedBy.FullName}</b>,</p>
                    <p>Khóa học AI cho chuyên đề <b>'{aiTask.Topic}'</b> đã được tạo thành công!</p>
                    <p>Thông tin khóa học:</p>
                    <ul>
                        <li><b>Tiêu đề:</b> {course.Title}</li>
                        <li><b>Cấp độ:</b> 3</li>
                        <li><b>Phạm vi:</b> Khóa chung toàn hệ thống</li>
                        <li><b>Trạng thái:</b> Chưa xuất bản (Đang chờ kiểm duyệt)</li>
                    </ul>
                    <p>Vui lòng vào hệ thống quản trị để kiểm tra nội dung.</p>
                ";
                _emailService.QueueEmail(aiTask.CreatedBy.Email, "Thông báo: Khóa học AI đã sẵn sàng", emailBody);
            }
        } catch(Exception e)
        {
            _logger.LogError(e, "Lỗi khi mapping JSON vào Entity.");
        }
    }

    private async Task NotifyProgressAsync(CourseGenerationProgress progress)
    { await _hubContext.Clients.Group($"job_{progress.JobId}").SendAsync("JobProgressUpdated", progress); }

    private string? ExtractJsonFromOutput(string output)
    {
        if(string.IsNullOrEmpty(output))
            return null;

        const string startMarker = "[RESULT_JSON_START]";
        const string endMarker = "[RESULT_JSON_END]";

        int startIndex = output.IndexOf(startMarker);
        int endIndex = output.LastIndexOf(endMarker);

        if(startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            startIndex += startMarker.Length;
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        int firstOpen = output.IndexOf('{');
        int lastClose = output.LastIndexOf('}');
        if(firstOpen != -1 && lastClose != -1 && lastClose > firstOpen)
        {
            return output.Substring(firstOpen, lastClose - firstOpen + 1);
        }

        return null;
    }
}
