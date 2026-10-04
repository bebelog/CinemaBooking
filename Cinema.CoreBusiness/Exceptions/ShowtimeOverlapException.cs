namespace Cinema.CoreBusiness.Exceptions;

public class ShowtimeOverlapException : Exception
{
    public ShowtimeOverlapException(string message) : base(message) { }
}
