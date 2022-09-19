namespace IceSync.Domain.Exceptions.Custom;

public class NotFoundDomainException : ApplicationException
{
    public NotFoundDomainException(string msg) : base(msg) { }
}