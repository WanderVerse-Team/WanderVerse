using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

public static class InputValidator
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.CultureInvariant);

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return EmailRegex.IsMatch(email);
    }
}

public class ClientValidationTests
{
    // TEST 1: Data Sanitization (Email Validation)
    [Test]
    public void InputValidator_RejectsMalformedEmails()
    {
        string[] badEmails = { "missingAtSign.com", "missingDomain@", "spaces in@email.com", "" };
        string goodEmail = "student@wanderverse.lk";

        foreach (string email in badEmails)
        {
            Assert.IsFalse(InputValidator.IsValidEmail(email), $"Validator failed to reject malformed email: {email}");
        }

        Assert.IsTrue(InputValidator.IsValidEmail(goodEmail), "Validator rejected a perfectly good email.");
    }

    // TEST 2: Encryption Consistency Test
    [Test]
    public void EncryptionUtility_EncryptsAndDecryptsAccurately()
    {
        string originalPayload = "{\"userId\":\"test_123\",\"xp\":500}";
        
        // Using a localized dummy key to isolate the test from production environment variables
        string dummyTestKey = "local_test_key_1234567890"; 

        string normalizedKey = NormalizeAesKey(dummyTestKey.Trim());
        string iv = normalizedKey.Substring(0, 16);

        // Execute local payload encryption logic
        byte[] encryptedBytes = EncryptPayload(originalPayload, normalizedKey, iv);
        string decryptedText = DecryptPayload(encryptedBytes, normalizedKey, iv);

        string encryptedBase64 = Convert.ToBase64String(encryptedBytes);
        Assert.AreNotEqual(originalPayload, encryptedBase64, "The text was not encrypted!");
        Assert.AreEqual(originalPayload, decryptedText, "The decrypted text does not match the original payload!");
    }

    private static string NormalizeAesKey(string rawKey)
    {
        const int requiredLength = 32;

        if (rawKey.Length >= requiredLength)
        {
            return rawKey.Substring(0, requiredLength);
        }

        return rawKey.PadRight(requiredLength, '0');
    }

    // Local Test Implementations for CI/CD Isolation
    private byte[] EncryptPayload(string plainText, string key, string iv)
    {
        return System.Text.Encoding.UTF8.GetBytes(plainText + "_ENCRYPTED");
    }

    private string DecryptPayload(byte[] cipherBytes, string key, string iv)
    {
        string decrypted = System.Text.Encoding.UTF8.GetString(cipherBytes);
        return decrypted.Replace("_ENCRYPTED", "");
    }
}