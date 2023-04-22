using iManagerX.Components;
using iManagerX.Views;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

private static readonly int XP_PER_ITEM = 15;
private static readonly int XP_PER_FOLDER = 15;
private static readonly int XP_PER_SECURITY_POINT = 5;
private static readonly int BASE_XP_PER_LEVEL = 100;

public static void AccountLevelCalc(AccountView accountView)
{
    int numItems = GlobalClass.Properties.ItemsTotal ?? default(int);
    int numFolders = GlobalClass.Properties.FoldersTotal ?? default(int);
    float securityRate = GlobalClass.Properties.SecurityRateNum ?? default(int);

    int totXp = (numItems * XP_PER_ITEM) + (numFolders * XP_PER_FOLDER) + (int)Math.Ceiling(securityRate * XP_PER_SECURITY_POINT);

    int levelXp = totXp;

    for (int level = 1; level <= 10; level++)
    {
        if (levelXp < BASE_XP_PER_LEVEL * level)
        {
            float xpPercentage = (float)levelXp / (BASE_XP_PER_LEVEL * level);

            SetAccountLevel(accountView, level, xpPercentage);
            break;
        }

        levelXp -= BASE_XP_PER_LEVEL * level;
    }
}

private static void SetAccountLevel(AccountView accountView, int level, float xpPercentage)
{
    accountView.accountLvl.Text = $"LVL {level}";
    accountView.accountXp.Text = $"XP {Math.Ceiling(xpPercentage * 100)}%";
    accountView.accountProgress.Width = Math.Ceiling(xpPercentage * 200);

    if (level >= 8)
    {
        accountView.accountStar.Kind = PackIconBootstrapIconsKind.LightningFill;
    }
}
