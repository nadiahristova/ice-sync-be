namespace IceSync.Domain.Exceptions.Custom;
public class InternalDomainException : ApplicationException
{
    public InternalDomainException(string msg) : base(msg) { }
}