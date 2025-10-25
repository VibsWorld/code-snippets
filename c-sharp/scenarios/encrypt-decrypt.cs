 public class EncryptDecryptUtils
    {
        private readonly string _aesKey;

        public EncryptDecryptUtils(string aesKey)
        {
            _aesKey = aesKey ?? throw new NullReferenceException("_aesKey cannot be null or empty");
        }
        public string encrypt(string encryptString)
        {
            //string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_aesKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public string Decrypt(string cipherText)
        {
            //string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_aesKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
    
    //Tests
    Console.Write("Enter Key to be used: ");
var key = Console.ReadLine() ?? "test";
Console.WriteLine($"Key entered is {key}");

var objEncryptDescrypt = new EncryptDecryptUtils(key);

Console.Write("Enter text to be encrypted: ");
var strTextTobeEncrypted = Console.ReadLine() ?? throw new ArgumentNullException("strTextTobeEncrypted");

Console.WriteLine($"Encrypted text is");
string encryptedText = objEncryptDescrypt.encrypt(strTextTobeEncrypted);
Console.WriteLine(encryptedText);

Console.Write("Enter Key again or press enter to pick from memory: ");
var key2 = Console.ReadLine() ?? key;
var descryptedText = objEncryptDescrypt.Decrypt(encryptedText);
Console.WriteLine("Descrypted Text is:");
Console.WriteLine(descryptedText);


public string DecryptString(string encrString)
{
    byte[] b;
    string decrypted;
    try
    {
        b = Convert.FromBase64String(encrString);
        decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b);
    }
    catch (FormatException fe)
    {
        decrypted = "";
    }
    return decrypted;
}

public string EnryptString(string strEncrypted)
{
    byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(strEncrypted);
    string encrypted = Convert.ToBase64String(b);
    return encrypted;
}
