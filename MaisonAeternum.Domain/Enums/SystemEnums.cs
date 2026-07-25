namespace MaisonAeternum.Domain.Enums;

public enum MediaType
{
    Image = 0,
    Video = 1,
    Audio = 2,
    Document = 3,
    Pdf = 4
}

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    SoftDeleted = 2,
    Restored = 3,
    Published = 4,
    Archived = 5,
    Revoked = 6,
    RoleAssigned = 7
}

public enum AiConversationContext
{
    General = 0,
    ModuleIntroduction = 1,
    QuizReview = 2,
    StudyRecommendation = 3
}

public enum MessageSender
{
    Learner = 0,
    Aurele = 1
}
