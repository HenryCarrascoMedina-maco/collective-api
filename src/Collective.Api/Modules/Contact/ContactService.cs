using System.Diagnostics.CodeAnalysis;
using Collective.Api.Common.Results;
using Collective.Api.Common.Security;
using Collective.Api.Common.Validation;
using Collective.Api.Infrastructure.Email;
using Collective.Api.Persistence;

namespace Collective.Api.Modules.Contact;

/// <summary>
/// Caso de uso del formulario. No conoce HttpContext ni codigos HTTP (regla A4).
/// </summary>
public sealed class ContactService(
    AppDbContext dbContext,
    IEmailSender emailSender,
    IpHasher ipHasher,
    TimeProvider timeProvider,
    ILogger<ContactService> logger)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IpHasher _ipHasher = ipHasher;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ContactService> _logger = logger;

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase",
        Justification = "El correo se almacena en minusculas por convencion de formato, "
            + "no se usa para ninguna decision de seguridad.")]
    public async Task<Result<ContactSubmissionResult>> SubmitAsync(
        SubmitContactRequest request,
        ContactRequestContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        // Trampa para robots: se responde con exito para no ensenarle al bot
        // que le hemos visto.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogInformation("Contact submission discarded by honeypot.");
            return Result.Success(ContactSubmissionResult.Accepted);
        }

        var errors = DataAnnotationsValidator.Collect(request);
        if (errors.Count > 0)
        {
            return Result.Failure<ContactSubmissionResult>(
                Error.Validation("contact.invalid", "The request is not valid."));
        }

        var entity = new ContactRequest
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Company = Normalize(request.Company),
            Phone = Normalize(request.Phone),
            Reason = ContactReasonMap.Parse(request.Reason),
            Message = request.Message.Trim(),
            ProductSlug = Normalize(request.ProductSlug),
            SourcePage = Normalize(request.SourcePage),
            UserAgent = Truncate(context.UserAgent, 400),
            IpHash = _ipHasher.Hash(context.IpAddress),
            CreatedAt = _timeProvider.GetUtcNow(),
        };

        // Se guarda ANTES de intentar el correo: si el proveedor falla, el lead
        // no se pierde (PLAN_BACKEND.md 7.1).
        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyAsync(entity, cancellationToken).ConfigureAwait(false);

        return Result.Success(ContactSubmissionResult.Accepted);
    }

    private async Task NotifyAsync(ContactRequest entity, CancellationToken cancellationToken)
    {
        try
        {
            await _emailSender.SendContactNotificationAsync(entity, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (EmailDeliveryException exception)
        {
            // Degradacion elegante: el usuario ya envio bien y su mensaje esta
            // guardado. El fallo se registra para reintentarlo.
            _logger.LogError(
                exception,
                "Contact notification failed for request {ContactRequestId}.",
                entity.Id);
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= max ? value : value[..max];
    }
}

/// <summary>Datos de la peticion que el caso de uso necesita sin conocer HTTP.</summary>
public sealed record ContactRequestContext(string? IpAddress, string? UserAgent);
