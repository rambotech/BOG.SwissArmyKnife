using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BOG.SwissArmyKnife.Demo
{
    public class CipherUtilityDemo
    {
        List<string> AlgorithmClassList = new List<string>();

        public CipherUtilityDemo()
        {
            HydrateSymmetricAlgorithmClasses();
            AlgorithmClassList.Add("System.Security.Cryptography.AesManaged");
            AlgorithmClassList.Add("System.Security.Cryptography.TripleDESCryptoServiceProvider");
            AlgorithmClassList.Add("System.Security.Cryptography.RijndaelManaged");
        }

        private void HydrateSymmetricAlgorithmClasses()
        {
            var type = typeof(SymmetricAlgorithm);
            foreach (Type thisType in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p)).Where(t => type.IsClass))
            {
                if (!thisType.IsAbstract && thisType.IsAnsiClass)
                {
                    if (thisType.Name.Contains("Managed") || thisType.Name.Contains("Provider"))
                    {
                        AlgorithmClassList.Add(thisType.ToString());
                    }
                }
            }
        }

        public void Demos()
        {
            var raw = "The quick brown fox jumped over the lazy dog's back";
            var password = "YeneM1lka$";
            var salt = string.Empty;

            foreach (var algorithm in AlgorithmClassList)
            {
#if FALSE    // as of this time, reflection on algorithm classes is not working in netstandard20 dotnetcore
                var cipherType = Type.GetType(algorithm);
                var cipher = new CipherUtility((System.Security.Cryptography.SymmetricAlgorithm)Activator.CreateInstance(cipherType));
#else
                CipherUtility cipher = null;
                if (algorithm == "System.Security.Cryptography.AesManaged")
                {
                    cipher = new CipherUtility(new AesManaged());
                }
                else if (algorithm == "System.Security.Cryptography.TripleDESCryptoServiceProvider")
                {
                    cipher = new CipherUtility(new TripleDESCryptoServiceProvider());
                }
                else if (algorithm == "System.Security.Cryptography.RijndaelManaged")
                {
                    cipher = new CipherUtility(new RijndaelManaged());
                }
#endif
                int pass = 1;
                while (pass < 3)
                {
                    Console.WriteLine("=======================================");
                    salt = DateTime.Now.ToString("fffssmm");
                    Console.WriteLine($"Pass: {pass} of {algorithm}...");
                    Console.WriteLine($"  raw: {raw}");
                    var encrypted = cipher.Encrypt(raw, password, salt, Base64FormattingOptions.InsertLineBreaks);
                    Console.WriteLine($"  encrypted: {encrypted}");
                    var decrypted = cipher.Decrypt(encrypted, password, salt);
                    Console.WriteLine($"  decrypted: {decrypted}");
                    Console.WriteLine("Press ENTER for next");
                    Console.ReadLine();
                    pass++;
                }
            }
        }
    }
}
