using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

// Global paths
public class Global
{
    public static string appdatapath = Environment.GetEnvironmentVariable("LocalAppData");
    public static string applicationfolder = "iManagerPLUS";
    public static string applicationfolderpath = Path.Combine(appdatapath, applicationfolder);
    public static string accountfilename = Path.Combine(applicationfolderpath, "account.aes");
    public static string configfilename = Path.Combine(applicationfolderpath, "config");
}

// Private User's Info
public static string AccountKey, AccountUsername, AccountPassword;

// User's Configuration Setting
public class ConfigJson
{
    public string Theme { get; set; }
    public string[] SkipFolders { get; set; }
    public bool JsonFileFormat { get; set; }
}

// Encrypt & Decrypt Class
public class CryptClass {}

public static class SecurityChecker
{
    public static string GetSecurityRate()
    {
        var passwordList = new List<string>();

        // Store the most used passwords
        var worstPasswords = new[] { "123456", "123456789", "password", "qwerty", "12345678", "12345", "123123", "111111", "1234", "1234567890", "1234567", "abc123", "1q2w3e4r5t", "q1w2e3r4t5y6", "iloveyou", "123", "000000", "123321", "1q2w3e4r", "qwertyuiop", "654321", "qwerty123", "1qaz2wsx3edc", "password1", "1qaz2wsx", "666666", "dragon", "ashley", "princess", "987654321", "123qwe", "159753", "monkey", "q1w2e3r4", "zxcvbnm", "123123123", "asdfghjkl", "pokemon", "football", "killer", "112233", "michael", "shadow", "121212", "daniel", "asdasd", "qazwsx", "1234qwer", "superman", "123456a", "azerty", "qwe123", "master", "7777777", "sunshine", "N0=Acc3ss", "1q2w3e", "abcd1234", "1234561", "computer", "fuckyou ", "aaaaaa", "555555", "asdfgh", "asd123", "baseball", "0123456789", "charlie", "123654", "qwer1234", "naruto", "a123456", "jessica", "soccer", "jordan", "liverpool", "thomas", "lol123", "michelle", "123abc", "nicole", "11111111", "starwars", "samsung", "1111", "secret", "joshua", "123456789a", "andrew", "222222", "q1w2e3r4t5", "147258369", "hunter", "Password", "qazwsxedc", "lovely", "999999", "jennifer", "letmein", "tigger", "sunshine1", "hello123", "welcome1", "admin", "princess1", "password123", "football1", "letmein123", "iloveyou1", "monkey1", "12345678910", "1q2w3e4r5t6y", "123abc123", "superman1", "jackson1", "abcdefg", "12345a", "qwertyuiop123", "qwerty123456", "access123", "1111111", "dragon123", "michael1", "baseball1", "starwars1", "123qwe123", "jessica1", "babygirl1", "shadow1", "123456789abc", "trustno1", "iloveyou2", "harley1", "charlie1", "letmein1", "12345q", "mercedes1", "qwertyuio", "jordan23", "qwerty1234", "maggie1", "1234abcd", "123456b", "jennifer1", "justin1", "george1", "sunshine123", "ferrari1", "corvette1", "jackass1", "1qazxsw23edc", "qwerty12345", "ninja123", "snoopy1", "jason1", "dallas1", "michelle1", "marshall1", "p@ssw0rd" };

        // Store Directory Paths
        var directoryList = Directory.GetDirectories(GlobalClass.Global.applicationfolderpath, "*", SearchOption.AllDirectories);

        // Skip the Folders for the list in Config
        var configJson = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText(GlobalClass.Global.configfilename));
        var skipFolders = configJson.SkipFolders ?? Array.Empty<string>();
        var updatedDirectoryList = directoryList.Except(skipFolders.Select(folder => Path.Combine(GlobalClass.Global.applicationfolderpath, folder))).ToArray();

        // Create Counters for the SecurityPage
        int riskyPasswordsCount = 0;
        int weakPasswordsCount = 0;
        int reusedPasswordsCount = 0;

        // Iterate each Directory
        foreach (var directory in updatedDirectoryList)
        {
            var directoryPath = Path.Combine(GlobalClass.Global.applicationfolderpath, directory);

            // Store the Directory's files
            var fileList = Directory.GetFiles(directoryPath, "*");

            // Iterate each file 
            foreach (var filePath in fileList)
            {
                // Check if the Format is JSON
                if (File.ReadLines(filePath).Count() < 4)
                {
                    UpdateClass.FormatFileIdToJson(filePath);
                }

                // Decrypt password
                var key = Application.Current.Properties["Key"].ToString();
                var idJson = JsonConvert.DeserializeObject<UpdateClass.IdProperties>(File.ReadAllText(filePath));
                var passwordDecrypted = CryptingClass.Aes256CbcEncrypter.Decrypt(idJson.Data[2], key);

                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name.Replace(".aes", "");

                // Check password's Security Level
                if (worstPasswords.Contains(passwordDecrypted) || passwordDecrypted.Length < 5)
                {
                    riskyPasswordsCount++;
                }
                else if (passwordDecrypted.Length < 11 && (passwordDecrypted.All(char.IsLetter) || passwordDecrypted.All(char.IsNumber) || passwordDecrypted.All(char.IsLetterOrDigit)))
                {
                    weakPasswordsCount++;
                }
                else if (passwordList.Contains(passwordDecrypted))
                {
                    reusedPasswordsCount++;
                }

                // Add the password to the password's List
                passwordList.Add(passwordDecrypted);
            }
        }

        // Calculate Safe password count
        var safePasswordsCount = passwordList.Count - riskyPasswordsCount - weakPasswordsCount - reusedPasswordsCount;

        RiskNum = riskyPasswordsCount.ToString();
        WeakNum = weakPasswordsCount.ToString();
        ReusedNum = reusedPasswordsCount.ToString();
        SafeNum = safePasswordsCount.ToString();
        SkippedNum = (ItemsTotal - passwordList.Count).ToString();
        CheckedNum = (passwordList.Count).ToString();

        // Calculate Security Rate
        float securityRate;
        if (passwordList.Count > 0)
        {
            securityRate = ((float)safePasswordsCount / passwordList.Count) * 100f;
            securityRate = (float)Math.Round(securityRate * 100f) / 100f;
        }
        else
        {
            securityRate = 100;
        }

        SecurityRateNum = securityRate;

        // Return the string with Security Rate
        return $"{securityRate:0}%";
    }
}
