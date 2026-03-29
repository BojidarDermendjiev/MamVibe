namespace MomVibe.Infrastructure.Persistence.Converters;

using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// EF Core value converter that transparently AES-256-CBC-encrypts nullable string columns.
/// The 16-byte IV is prepended to the ciphertext and stored as Base64.
/// Key must be exactly 32 bytes, provided from configuration ("Security:IbanEncryptionKey" as Base64).
/// </summary>
public class AesEncryptionConverter : ValueConverter<string?, string?>
{
    public AesEncryptionConverter(byte[] key)
        : base(
            value => value == null ? null : Encrypt(value, key),
            stored => stored == null ? null : Decrypt(stored, key))
    { }

    private static string Encrypt(string plaintext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        // Prefix IV so Decrypt can reconstruct it without storing it separately
        var result = new byte[aes.IV.Length + cipher.Length];
        aes.IV.CopyTo(result, 0);
        cipher.CopyTo(result, aes.IV.Length);
        return Convert.ToBase64String(result);
    }

    private static string Decrypt(string stored, byte[] key)
    {
        var data = Convert.FromBase64String(stored);
        const int ivLength = 16;
        if (data.Length <= ivLength)
            throw new CryptographicException("Ciphertext too short.");
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = data[..ivLength];
        using var decryptor = aes.CreateDecryptor();
        var cipher = data[ivLength..];
        var plaintext = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plaintext);
    }
}
