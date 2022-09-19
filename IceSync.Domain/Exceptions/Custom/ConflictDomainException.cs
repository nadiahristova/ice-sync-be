namespace IceSync.Domain.Exceptions.Custom;

public class ConflictDomainException : ApplicationException
{
    public ConflictDomainException(string msg) : base(msg) { }
}