using Collective.Api.Modules.Contact;

namespace Collective.Api.Infrastructure.Email;

/// <summary>
/// Puerto de salida hacia el proveedor de correo (regla A7). La implementacion
/// real se decide en DA-08; cambiarla no toca el caso de uso.
/// </summary>
public interface IEmailSender
{
    public Task SendContactNotificationAsync(
        ContactRequest request,
        CancellationToken cancellationToken);
}

/// <summary>Fallo al entregar un correo. Es esperado y recuperable.</summary>
public sealed class EmailDeliveryException : Exception
{
    public EmailDeliveryException()
    {
    }

    public EmailDeliveryException(string message) : base(message)
    {
    }

    public EmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
