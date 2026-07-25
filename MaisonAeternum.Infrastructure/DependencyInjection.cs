using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Infrastructure.AiMentor;
using MaisonAeternum.Infrastructure.Identity;
using MaisonAeternum.Infrastructure.Persistence;
using MaisonAeternum.Infrastructure.Persistence.Interceptors;
using MaisonAeternum.Infrastructure.Persistence.Repositories;
using MaisonAeternum.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MaisonAeternum.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false; // no email verification step — see AccountController.Register
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IFormationRepository, FormationRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IAiConversationRepository, AiConversationRepository>();
        services.AddScoped<ILearnerProfileRepository, LearnerProfileRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IEmailSender, DevEmailSender>();

        services.AddAiMentor(configuration);

        return services;
    }
}
