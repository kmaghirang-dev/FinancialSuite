namespace FinancialSuite.Domain.Interfaces.Utilities
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
