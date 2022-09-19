namespace IceSync.Domain.Exceptions;
public class Rfc7807Object
{
    public string Title { get; set; } = null!;

    public int Status { get; set; }

    public string Type { get; set; } = null!;

    public string Instance { get; set; } = null!;

    public ICollection<string> Errors { get; set; } = new List<string>();
}
