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

public static string AdvancedSecurityAlgorithm()
{
    // Store Directory Paths
    string[] dirlist = Directory.GetDirectories(Global.applicationfolderpath, "*", SearchOption.AllDirectories);
    
    // Initalize a List for the passwords
    List<string> passwordlist = new();

    // Store the most used passwords
    string[] worstpasswords = { "123456", "123456789", "password", "qwerty", "12345678", "12345", "123123", "111111", "1234", "1234567890", "1234567", "abc123", "1q2w3e4r5t", "q1w2e3r4t5y6", "iloveyou", "123", "000000", "123321", "1q2w3e4r", "qwertyuiop", "654321", "qwerty123", "1qaz2wsx3edc", "password1", "1qaz2wsx", "666666", "dragon", "ashley", "princess", "987654321", "123qwe", "159753", "monkey", "q1w2e3r4", "zxcvbnm", "123123123", "asdfghjkl", "pokemon", "football", "killer", "112233", "michael", "shadow", "121212", "daniel", "asdasd", "qazwsx", "1234qwer", "superman", "123456a", "azerty", "qwe123", "master", "7777777", "sunshine", "N0=Acc3ss", "1q2w3e", "abcd1234", "1234561", "computer", "fuckyou ", "aaaaaa", "555555", "asdfgh", "asd123", "baseball", "0123456789", "charlie", "123654", "qwer1234", "naruto", "a123456", "jessica", "soccer", "jordan", "liverpool", "thomas", "lol123", "michelle", "123abc", "nicole", "11111111", "starwars", "samsung", "1111", "secret", "joshua", "123456789a", "andrew", "222222", "q1w2e3r4t5", "147258369", "hunter", "Password", "qazwsxedc", "lovely", "999999", "jennifer", "letmein", "tigger" };
    
    // Create Counters for the SecurityPage
    int riskypasswordscount = 0;
    int weakpasswordscount = 0;
    int reusedpasswordscount = 0;
    int safepasswordscount = 0;


    // Skip the Folders for the list in Config
    string json = File.ReadAllText(Global.configfilename);
    ConfigJson config = JsonConvert.DeserializeObject<ConfigJson>(json);
    List<string> list0 = new(dirlist);
    if (config.SkipFolders != null)
    {
        foreach (string folder in config.SkipFolders)
        {
            list0.Remove(System.IO.Path.Combine(Global.applicationfolderpath, folder));
        }
    }

    // Update the list of Directory Paths
    string[] updateddirlist = list0.ToArray();


    // Iterate each Directory
    foreach (string dir in updateddirlist)
    {
        string listpath = System.IO.Path.Combine(Global.applicationfolderpath, dir);
        
        // Store the Directory's files
        string[] filelist = Directory.GetFiles(listpath, "*");

        // Iterate each file 
        foreach (string file in filelist)
        {
            // Check if the Format is JSON
            string[] lines = File.ReadAllLines(file);
            if (lines.Length < 4)
            {
                UpdateClass.FormatFileIdToJson(file);
            }

            // Decrypt password
            string Key = Application.Current.Properties["Key"].ToString();
            string json1 = File.ReadAllText(file).ToString();
            UpdateClass.IdProperties idJson = JsonConvert.DeserializeObject<UpdateClass.IdProperties>(json1);
            string passwordDecrypted = CryptFunction.Decrypt(idJson.Data[2], Key);
            
            FileInfo fileinfo = new(file);
            string filename = fileinfo.Name.Replace(".aes", "");
            
            // Check password's Security Level
            if (worstpasswords.Contains(passwordDecrypted))
            {
                riskypasswordscount += 1;
            }
            else if (passwordDecrypted.Length < 8 || passwordDecrypted.All(char.IsLetter) || passwordDecrypted.All(char.IsNumber))
            {
                weakpasswordscount += 1;
            }
            else if (passwordlist.Count > 0 && passwordlist.Contains(passwordDecrypted))
            {
                reusedpasswordscount += 1;
            }

            // Add the password to the password's List
            passwordlist.Add(passwordDecrypted);
        }
    }

    // Calculate Safe password count
    safepasswordscount = passwordlist.Count - riskypasswordscount - weakpasswordscount - reusedpasswordscount;

    // Calculate Security Rate
    float securityRate;
    float safepasswordscountf = (float)safepasswordscount;
    float passwordlistcountf = (float)passwordlist.Count;
    if (passwordlist.Count > 0)
    {
        securityRate = (safepasswordscountf / passwordlistcountf) * 100f;
        securityRate = (float)Math.Round(securityRate * 100f) / 100f;
    }
    else
    {
        securityRate = 100;
    }

    // Return the string with Security Rate
    return String.Format("{0:0}", securityRate) + "%";
}
