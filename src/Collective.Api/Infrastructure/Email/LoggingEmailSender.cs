using Collective.Api.Modules.Contact;

namespace Collective.Api.Infrastructure.Email;

/// <summary>
/// Implementacion de desarrollo: deja constancia de que habria enviado el aviso.
/// Se sustituye por el proveedor real cuando se cierre DA-08, sin tocar nada mas.
/// No registra el contenido del mensaje ni el correo del remitente (regla S5).
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger = logger;

    public Task SendContactNotificationAsync(
        ContactRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Contact notification pending delivery. Request {ContactRequestId}, reason {Reason}.",
                request.Id,
                request.Reason);
        }

        return Task.CompletedTask;
    }
}
