using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using MaisonAeternum.Infrastructure.Identity;
using MaisonAeternum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainModule = MaisonAeternum.Domain.Entities.Module;

namespace MaisonAeternum.Web.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (await context.Categories.IgnoreQueryFilters().AnyAsync())
            return; // already seeded

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync(roleManager);
        var ranks = await SeedGuildRanksAsync(context);
        var categories = await SeedCategoriesAsync(context);
        await SeedBadgesAsync(context);
        await SeedAdminAsync(userManager);
        var trainers = await SeedTrainersAsync(context, userManager);
        var learners = await SeedLearnersAsync(context, userManager, ranks);
        var formations = await SeedFormationsAsync(context, categories, trainers);
        await SeedEnrollmentsAndActivityAsync(context, learners, formations, ranks);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Trainer", "Learner" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task<List<GuildRank>> SeedGuildRanksAsync(ApplicationDbContext context)
    {
        var ranks = new List<GuildRank>
        {
            new() { Name = "Apprentice", Level = 0, MinFormationsCompleted = 0, BadgeIconUrl = "/img/ranks/apprentice.svg", Description = "Newly admitted to the Maison, learning the fundamentals of the craft." },
            new() { Name = "Journeyman", Level = 1, MinFormationsCompleted = 3, BadgeIconUrl = "/img/ranks/journeyman.svg", Description = "A Compagnon of the guild, trusted with intermediate complications." },
            new() { Name = "Certified Horologer", Level = 2, MinFormationsCompleted = 6, BadgeIconUrl = "/img/ranks/certified-horologer.svg", Description = "Formally certified in advanced horological technique." },
            new() { Name = "Master of the Maison", Level = 3, MinFormationsCompleted = 10, BadgeIconUrl = "/img/ranks/master.svg", Description = "The highest rank of Maison Aeternum — a true master of eternal time." }
        };

        context.GuildRanks.AddRange(ranks);
        await context.SaveChangesAsync();
        return ranks;
    }

    private static async Task<List<Category>> SeedCategoriesAsync(ApplicationDbContext context)
    {
        var categories = new List<Category>
        {
            new() { Name = "Movements & Mechanisms", Slug = "movements-mechanisms", Description = "Mainsprings, gear trains, and escapements — the beating heart of every timepiece.", IconClass = "bi-gear-wide-connected", ColorHex = "#C9A24B", DisplayOrder = 1 },
            new() { Name = "Grand Complications", Slug = "grand-complications", Description = "Chronographs, tourbillons, perpetual calendars, and minute repeaters.", IconClass = "bi-stars", ColorHex = "#8B5CF6", DisplayOrder = 2 },
            new() { Name = "Case, Dial & Craftsmanship", Slug = "case-dial-craftsmanship", Description = "Guilloché, hand-finishing, and the art of the dial.", IconClass = "bi-brush", ColorHex = "#2DD4BF", DisplayOrder = 3 },
            new() { Name = "Restoration & Heritage", Slug = "restoration-heritage", Description = "Servicing and preserving vintage timepieces with provenance.", IconClass = "bi-clock-history", ColorHex = "#F59E0B", DisplayOrder = 4 },
            new() { Name = "The Business of Horology", Slug = "business-of-horology", Description = "Retail, authentication, and valuation for the modern Maison.", IconClass = "bi-briefcase", ColorHex = "#60A5FA", DisplayOrder = 5 }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        return categories;
    }

    private static async Task SeedBadgesAsync(ApplicationDbContext context)
    {
        var badges = new List<Badge>
        {
            new() { Name = "First Light", Description = "Completed your first module.", IconUrl = "/img/badges/first-light.svg", Category = BadgeCategory.Milestone, CriteriaDescription = "Complete 1 module" },
            new() { Name = "Steady Hands", Description = "Passed 5 quizzes without a single failed attempt.", IconUrl = "/img/badges/steady-hands.svg", Category = BadgeCategory.Mastery, CriteriaDescription = "Pass 5 quizzes on the first attempt" },
            new() { Name = "7-Day Bench Streak", Description = "Trained at the bench 7 days in a row.", IconUrl = "/img/badges/streak-7.svg", Category = BadgeCategory.Streak, CriteriaDescription = "7 consecutive active days" },
            new() { Name = "30-Day Bench Streak", Description = "Trained at the bench 30 days in a row.", IconUrl = "/img/badges/streak-30.svg", Category = BadgeCategory.Streak, CriteriaDescription = "30 consecutive active days" },
            new() { Name = "Night Owl Horologist", Description = "Completed a module after midnight.", IconUrl = "/img/badges/night-owl.svg", Category = BadgeCategory.Social, CriteriaDescription = "Complete a module between 00:00-04:00" },
            new() { Name = "Loupe Badge", Description = "Scored 100% on a Grand Complications exam.", IconUrl = "/img/badges/loupe.svg", Category = BadgeCategory.Mastery, CriteriaDescription = "Perfect score on a final exam" },
            new() { Name = "Guild Reviewer", Description = "Left 5 formation reviews.", IconUrl = "/img/badges/reviewer.svg", Category = BadgeCategory.Social, CriteriaDescription = "Submit 5 reviews" },
            new() { Name = "Master of the Maison", Description = "Reached the highest guild rank.", IconUrl = "/img/badges/master-rank.svg", Category = BadgeCategory.Milestone, CriteriaDescription = "Reach Master of the Maison" }
        };

        context.Badges.AddRange(badges);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@maisonaeternum.com";
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Isabelle",
            LastName = "Aeternum",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await userManager.CreateAsync(admin, "MaisonAdmin!2026");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task<List<TrainerProfile>> SeedTrainersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var trainerSeeds = new[]
        {
            new { First = "Jean-Marc", Last = "Verrier", Email = "jm.verrier@maisonaeternum.com", Bio = "Former head watchmaker at a Geneva grande maison, specializing in escapement theory and gear train precision.", Atelier = "Atelier Verrier, Geneva", Years = 22, Featured = true },
            new { First = "Elena", Last = "Rossetti", Email = "e.rossetti@maisonaeternum.com", Bio = "Complications specialist known for her tourbillon restorations and minute repeater tuning.", Atelier = "Rossetti Horlogerie, Milan", Years = 15, Featured = true },
            new { First = "Thibault", Last = "Moreau", Email = "t.moreau@maisonaeternum.com", Bio = "Guilloché and dial-craft master, trained in the traditional ateliers of the Vallée de Joux.", Atelier = "Moreau Cadrans, Vallée de Joux", Years = 18, Featured = false },
            new { First = "Margaux", Last = "Delacroix", Email = "m.delacroix@maisonaeternum.com", Bio = "Heritage restoration expert specializing in pre-war vintage movements and provenance research.", Atelier = "Delacroix Restauration, Paris", Years = 12, Featured = false },
            new { First = "Hiro", Last = "Tanaka", Email = "h.tanaka@maisonaeternum.com", Bio = "Retail and authentication authority advising major auction houses on valuation standards.", Atelier = "Tanaka Horology Advisory, Tokyo", Years = 10, Featured = false }
        };

        var profiles = new List<TrainerProfile>();

        foreach (var seed in trainerSeeds)
        {
            var user = new ApplicationUser
            {
                UserName = seed.Email,
                Email = seed.Email,
                FirstName = seed.First,
                LastName = seed.Last,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(user, "MaisonTrainer!2026");
            await userManager.AddToRoleAsync(user, "Trainer");

            var profile = new TrainerProfile
            {
                UserId = user.Id,
                Biography = seed.Bio,
                AtelierAffiliation = seed.Atelier,
                YearsOfExperience = seed.Years,
                AverageRating = 4.6m + (decimal)(seed.Years % 3) * 0.1m,
                IsFeatured = seed.Featured,
                SocialLinks = new List<TrainerSocialLink>
                {
                    new() { Platform = SocialPlatform.Instagram, Url = $"https://instagram.com/{seed.Last.ToLower()}atelier" },
                    new() { Platform = SocialPlatform.LinkedIn, Url = $"https://linkedin.com/in/{seed.First.ToLower()}-{seed.Last.ToLower()}" }
                }
            };

            context.TrainerProfiles.Add(profile);
            profiles.Add(profile);
        }

        await context.SaveChangesAsync();
        return profiles;
    }

    private static async Task<List<LearnerProfile>> SeedLearnersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, List<GuildRank> ranks)
    {
        var learnerSeeds = new[]
        {
            new { First = "Camille", Last = "Bertrand", Email = "camille.bertrand@example.com", RankIndex = 2, Streak = 34, LongestStreak = 41, Minutes = 3120 },
            new { First = "Noah", Last = "Whitfield", Email = "noah.whitfield@example.com", RankIndex = 1, Streak = 6, LongestStreak = 19, Minutes = 1580 },
            new { First = "Aiko", Last = "Sato", Email = "aiko.sato@example.com", RankIndex = 0, Streak = 2, LongestStreak = 2, Minutes = 240 },
            new { First = "Lucas", Last = "Ferreira", Email = "lucas.ferreira@example.com", RankIndex = 3, Streak = 0, LongestStreak = 67, Minutes = 5400 }
        };

        var profiles = new List<LearnerProfile>();
        var memberSeq = 1;

        foreach (var seed in learnerSeeds)
        {
            var user = new ApplicationUser
            {
                UserName = seed.Email,
                Email = seed.Email,
                FirstName = seed.First,
                LastName = seed.Last,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(user, "MaisonLearner!2026");
            await userManager.AddToRoleAsync(user, "Learner");

            var profile = new LearnerProfile
            {
                UserId = user.Id,
                GuildRankId = ranks[seed.RankIndex].Id,
                MemberNumber = $"MA-2026-{memberSeq++:D6}",
                MemberSince = DateTime.UtcNow.AddMonths(-Random.Shared.Next(2, 14)),
                CurrentStreakDays = seed.Streak,
                LongestStreakDays = seed.LongestStreak,
                TotalBenchMinutes = seed.Minutes
            };

            context.LearnerProfiles.Add(profile);
            profiles.Add(profile);
        }

        await context.SaveChangesAsync();
        return profiles;
    }

    private static async Task<List<Formation>> SeedFormationsAsync(ApplicationDbContext context, List<Category> categories, List<TrainerProfile> trainers)
    {
        var blueprints = new[]
        {
            new { Title = "Gear Train Fundamentals", Cat = 0, Trainer = 0, Diff = DifficultyLevel.Apprentice, Minutes = 180, Rating = 4.8m, Enroll = 214 },
            new { Title = "Escapement Theory & Adjustment", Cat = 0, Trainer = 0, Diff = DifficultyLevel.Journeyman, Minutes = 240, Rating = 4.7m, Enroll = 156 },
            new { Title = "Chronograph Construction", Cat = 1, Trainer = 1, Diff = DifficultyLevel.Journeyman, Minutes = 300, Rating = 4.9m, Enroll = 132 },
            new { Title = "Tourbillon Assembly Masterclass", Cat = 1, Trainer = 1, Diff = DifficultyLevel.MasterOfTheMaison, Minutes = 420, Rating = 5.0m, Enroll = 58 },
            new { Title = "Guilloché & Dial Finishing", Cat = 2, Trainer = 2, Diff = DifficultyLevel.CertifiedHorologer, Minutes = 260, Rating = 4.6m, Enroll = 89 },
            new { Title = "Hand-Finishing Fundamentals", Cat = 2, Trainer = 2, Diff = DifficultyLevel.Apprentice, Minutes = 150, Rating = 4.5m, Enroll = 178 },
            new { Title = "Vintage Movement Restoration", Cat = 3, Trainer = 3, Diff = DifficultyLevel.CertifiedHorologer, Minutes = 320, Rating = 4.8m, Enroll = 74 },
            new { Title = "Provenance & Authentication Basics", Cat = 3, Trainer = 3, Diff = DifficultyLevel.Journeyman, Minutes = 200, Rating = 4.4m, Enroll = 61 },
            new { Title = "Valuation for Auction Houses", Cat = 4, Trainer = 4, Diff = DifficultyLevel.CertifiedHorologer, Minutes = 220, Rating = 4.3m, Enroll = 47 },
            new { Title = "Retail & Client Relations", Cat = 4, Trainer = 4, Diff = DifficultyLevel.Apprentice, Minutes = 140, Rating = 4.2m, Enroll = 93 }
        };

        var formations = new List<Formation>();

        foreach (var bp in blueprints)
        {
            var formation = new Formation
            {
                Title = bp.Title,
                Slug = Slugify(bp.Title),
                CategoryId = categories[bp.Cat].Id,
                TrainerId = trainers[bp.Trainer].Id,
                Difficulty = bp.Diff,
                EstimatedMinutes = bp.Minutes,
                ShortDescription = $"Master {bp.Title.ToLower()} through guided theory, hands-on exercises, and a certifying Bench Trial.",
                FullDescription = $"This formation guides you step by step through {bp.Title}, combining recorded workshop sessions from the Maison's ateliers with practical exercises and a proctored final exam. On completion, you earn a Maison Aeternum certificate signed by your Master Watchmaker.",
                PrerequisitesText = "No prior formation required.",
                Status = FormationStatus.Published,
                HasCertificate = true,
                PublishedAt = DateTime.UtcNow.AddMonths(-Random.Shared.Next(1, 10)),
                EnrollmentCount = bp.Enroll,
                AverageRating = bp.Rating,
                Objectives = new List<FormationObjective>
                {
                    new() { Text = $"Understand the core principles behind {bp.Title.ToLower()}", DisplayOrder = 1 },
                    new() { Text = "Apply technique through guided practical exercises", DisplayOrder = 2 },
                    new() { Text = "Pass the certifying Bench Trial exam", DisplayOrder = 3 }
                },
                Modules = BuildModules(bp.Title)
            };

            // Quiz.FormationId is a required FK independent of the Module nav —
            // set the navigation so EF's graph fixup resolves it once Formation.Id is generated.
            foreach (var quiz in formation.Modules.SelectMany(m => m.Quizzes))
            {
                quiz.Formation = formation;
            }

            formations.Add(formation);
        }

        context.Formations.AddRange(formations);
        await context.SaveChangesAsync();
        return formations;
    }

    private static List<DomainModule> BuildModules(string formationTitle)
    {
        var moduleNames = new[] { "Foundations", "Guided Practice", "Certifying Bench Trial" };
        var modules = new List<DomainModule>();

        for (var i = 0; i < moduleNames.Length; i++)
        {
            var module = new DomainModule
            {
                Title = $"{moduleNames[i]}: {formationTitle}",
                Description = $"Part {i + 1} of {formationTitle}, covering {moduleNames[i].ToLower()}.",
                DisplayOrder = i + 1,
                EstimatedMinutes = 60 + i * 20,
                ContentItems = new List<ContentItem>
                {
                    new() { Type = ContentItemType.Video, Title = $"{moduleNames[i]} — Workshop Recording", ExternalUrl = "https://cdn.maisonaeternum.com/sample-video.mp4", DurationMinutes = 25, DisplayOrder = 1 },
                    new() { Type = ContentItemType.Pdf, Title = $"{moduleNames[i]} — Technical Manual", ExternalUrl = "https://cdn.maisonaeternum.com/sample-manual.pdf", DurationMinutes = 15, DisplayOrder = 2 }
                }
            };

            if (i == moduleNames.Length - 1)
            {
                module.Quizzes.Add(new Quiz
                {
                    Type = QuizType.FinalExam,
                    Title = $"Bench Trial — {formationTitle}",
                    Instructions = "You have limited time to answer every question. Randomized from the Maison's question bank.",
                    TimeLimitSeconds = 600,
                    PassingScore = 70m,
                    MaxAttempts = 3,
                    RandomizeQuestions = true,
                    QuestionsToServe = 5,
                    Questions = BuildQuestions()
                });
            }

            modules.Add(module);
        }

        return modules;
    }

    private static List<Question> BuildQuestions()
    {
        return new List<Question>
        {
            new()
            {
                Text = "What is the primary purpose of the escapement in a mechanical movement?",
                Type = QuestionType.SingleChoice,
                Explanation = "The escapement regulates the release of energy from the mainspring at a fixed rate, dividing time into equal impulses.",
                Points = 20,
                DisplayOrder = 1,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "To regulate the release of energy at a fixed rate", IsCorrect = true, DisplayOrder = 1 },
                    new() { Text = "To wind the mainspring automatically", IsCorrect = false, DisplayOrder = 2 },
                    new() { Text = "To display the current time on the dial", IsCorrect = false, DisplayOrder = 3 },
                    new() { Text = "To waterproof the case", IsCorrect = false, DisplayOrder = 4 }
                }
            },
            new()
            {
                Text = "A tourbillon is designed to counteract the effects of gravity on a watch's accuracy.",
                Type = QuestionType.TrueFalse,
                Explanation = "Correct — the rotating carriage averages out positional errors caused by gravity.",
                Points = 10,
                DisplayOrder = 2,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "True", IsCorrect = true, DisplayOrder = 1 },
                    new() { Text = "False", IsCorrect = false, DisplayOrder = 2 }
                }
            },
            new()
            {
                Text = "Which of the following are considered Grand Complications? (select all that apply)",
                Type = QuestionType.MultipleAnswer,
                Explanation = "Chronograph, perpetual calendar, and minute repeater are classic Grand Complications; a date window is a simple complication.",
                Points = 25,
                DisplayOrder = 3,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "Chronograph", IsCorrect = true, DisplayOrder = 1 },
                    new() { Text = "Perpetual calendar", IsCorrect = true, DisplayOrder = 2 },
                    new() { Text = "Minute repeater", IsCorrect = true, DisplayOrder = 3 },
                    new() { Text = "Date window", IsCorrect = false, DisplayOrder = 4 }
                }
            },
            new()
            {
                Text = "What tool is traditionally used to hand-finish beveled edges (anglage) on movement bridges?",
                Type = QuestionType.SingleChoice,
                Explanation = "A hand-held file paired with wood pegs and diamond paste is the traditional approach to anglage.",
                Points = 20,
                DisplayOrder = 4,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "A hand file with diamond paste", IsCorrect = true, DisplayOrder = 1 },
                    new() { Text = "A CNC milling machine", IsCorrect = false, DisplayOrder = 2 },
                    new() { Text = "An ultrasonic cleaner", IsCorrect = false, DisplayOrder = 3 },
                    new() { Text = "A demagnetizer", IsCorrect = false, DisplayOrder = 4 }
                }
            },
            new()
            {
                Text = "Provenance research is irrelevant when authenticating a vintage timepiece.",
                Type = QuestionType.TrueFalse,
                Explanation = "False — provenance (ownership history, service records, original documentation) is central to authentication and valuation.",
                Points = 15,
                DisplayOrder = 5,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "True", IsCorrect = false, DisplayOrder = 1 },
                    new() { Text = "False", IsCorrect = true, DisplayOrder = 2 }
                }
            }
        };
    }

    private static async Task SeedEnrollmentsAndActivityAsync(
        ApplicationDbContext context, List<LearnerProfile> learners, List<Formation> formations, List<GuildRank> ranks)
    {
        var random = Random.Shared;
        var certificateSeq = 1;

        foreach (var learner in learners)
        {
            var enrollCount = Math.Min(formations.Count, random.Next(3, 7));
            var chosen = formations.OrderBy(_ => random.Next()).Take(enrollCount).ToList();

            foreach (var formation in chosen)
            {
                var isCompleted = random.NextDouble() < 0.4;
                var progress = isCompleted ? 100m : random.Next(10, 95);
                var status = isCompleted ? EnrollmentStatus.Completed : EnrollmentStatus.InProgress;
                var completedAt = isCompleted ? DateTime.UtcNow.AddDays(-random.Next(1, 45)) : (DateTime?)null;

                context.Enrollments.Add(new Enrollment
                {
                    LearnerId = learner.Id,
                    FormationId = formation.Id,
                    EnrolledAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                    Status = status,
                    ProgressPercentage = progress,
                    CompletedAt = completedAt,
                    LastAccessedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
                });

                if (!isCompleted) continue;

                var finalExam = formation.Modules.SelectMany(m => m.Quizzes).FirstOrDefault(q => q.Type == QuizType.FinalExam);
                if (finalExam is null) continue;

                var score = random.Next(72, 100);
                var attempt = new QuizAttempt
                {
                    Quiz = finalExam,
                    LearnerId = learner.Id,
                    AttemptNumber = 1,
                    StartedAt = completedAt!.Value.AddMinutes(-15),
                    SubmittedAt = completedAt,
                    ScorePercentage = score,
                    Passed = true,
                    TimeTakenSeconds = random.Next(300, 590)
                };
                context.QuizAttempts.Add(attempt);

                context.Certificates.Add(new Certificate
                {
                    LearnerId = learner.Id,
                    Formation = formation,
                    QuizAttempt = attempt,
                    GuildRankId = learner.GuildRankId,
                    CertificateNumber = $"MA-{DateTime.UtcNow.Year}-{certificateSeq++:D6}",
                    VerificationToken = Guid.NewGuid(),
                    IssuedAt = completedAt.Value,
                    IsRevoked = false
                });
            }

            // Populate ~90 days of activity so the learning heatmap renders realistically.
            for (var dayOffset = 0; dayOffset < 90; dayOffset++)
            {
                if (random.NextDouble() > 0.55) continue; // sparse, realistic activity pattern

                var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-dayOffset));
                context.ActivityLogs.Add(new ActivityLog
                {
                    LearnerId = learner.Id,
                    ActivityDate = date,
                    MinutesSpent = random.Next(10, 90),
                    ModulesCompletedCount = random.Next(0, 3),
                    QuizAttemptsCount = random.Next(0, 2)
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static string Slugify(string title) =>
        title.ToLowerInvariant()
            .Replace(" & ", "-")
            .Replace(" ", "-")
            .Replace("é", "e")
            .Replace("'", "");
}
