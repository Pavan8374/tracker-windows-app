using System.Diagnostics;

/// <summary>
/// Entropy manager
/// </summary>
public class EntropyManager
{
    private const string ENTROPY_FILE_NAME = "entropy.key";
    private const int ENTROPY_SIZE = 256; // 256 bits for strong security                    

    /// <summary>
    /// Get production entropy
    /// </summary>
    /// <returns></returns>
    public static byte[] GetProductionEntropy()
    {
        string entropyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NatWinTracker",
            ENTROPY_FILE_NAME);

        if (File.Exists(entropyPath))
        {
            try
            {
                return File.ReadAllBytes(entropyPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read entropy file: {ex.Message}");
            }
        }

        return GenerateAndSaveNewEntropy(entropyPath);
    }

    /// <summary>
    /// Generate and save new entrophy.
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns>Byte[]</returns>
    private static byte[] GenerateAndSaveNewEntropy(string path)
    {
        byte[] entropy = new byte[ENTROPY_SIZE];
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(entropy);
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(entropy, 0, entropy.Length);
            }

            var fileInfo = new FileInfo(path);
            var security = fileInfo.GetAccessControl();
            security.SetAccessRuleProtection(true, false); 

            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().User;
            security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                currentUser,
                System.Security.AccessControl.FileSystemRights.FullControl,
                System.Security.AccessControl.AccessControlType.Allow));

            security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                new System.Security.Principal.SecurityIdentifier("S-1-5-18"), 
                System.Security.AccessControl.FileSystemRights.FullControl,
                System.Security.AccessControl.AccessControlType.Allow));

            fileInfo.SetAccessControl(security);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save entropy file: {ex.Message}");
        }

        return entropy;
    }
}