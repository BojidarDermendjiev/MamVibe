namespace MomVibe.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Delegating handler that signs every outbound n8n webhook request with an
/// HMAC-SHA256 signature. The signature is computed over the raw request body
/// and attached as the <c>X-MamVibe-Signature</c> header so n8n workflows can
/// verify the payload originated from this API.
/// </summary>
public class N8nHmacHandler : DelegatingHandler
{
    private readonly byte[] _keyBytes;

    /// <summary>Initializes a new instance of <see cref="N8nHmacHandler"/> with the provided shared secret.</summary>
    /// <param name="secret">The HMAC-SHA256 shared secret (UTF-8 encoded).</param>
    public N8nHmacHandler(string secret)
    {
        _keyBytes = Encoding.UTF8.GetBytes(secret);
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null && _keyBytes.Length > 0)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            var signature = Convert.ToHexString(
                HMACSHA256.HashData(_keyBytes, Encoding.UTF8.GetBytes(body)));
            request.Headers.TryAddWithoutValidation("X-MamVibe-Signature", signature);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
