using AutoMapper;
using FluentValidation;
using MaisonAeternum.Application.AiMentor;
using MaisonAeternum.Application.AiMentor.Abstractions;
using MaisonAeternum.Application.Assessment;
using MaisonAeternum.Application.Assessment.Abstractions;
using MaisonAeternum.Application.Catalog;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Learning;
using MaisonAeternum.Application.Learning.Abstractions;
using MaisonAeternum.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MaisonAeternum.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAccountProvisioningService, AccountProvisioningService>();

        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IAiMentorService, AiMentorService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITrainerProfileService, TrainerProfileService>();
        services.AddScoped<IFormationService, FormationService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IQuizAttemptService, QuizAttemptService>();

        return services;
    }
}
