namespace MaisonAeternum.Domain.Entities;

public class SentEmail
{
    public int Id { get; set; }
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public DateTime SentAt { get; set; }
}
