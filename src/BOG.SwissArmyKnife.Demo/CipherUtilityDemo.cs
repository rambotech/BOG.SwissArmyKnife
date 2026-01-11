using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BOG.SwissArmyKnife.Demo
{
    public class CipherUtilityDemo
    {
        private enum CryptographyAlgorithm : int
        {
            Aes = 0,
            DES = 1,
            TripleDES = 2
        }

        public CipherUtilityDemo()
        {
        }

        private SymmetricAlgorithm HydrateSymmetricAlgorithmClass(CryptographyAlgorithm alg)
        {
            switch (alg)
            {
                case CryptographyAlgorithm.Aes:
                    return Aes.Create();
                case CryptographyAlgorithm.DES:
                    return DES.Create();
                case CryptographyAlgorithm.TripleDES:
                    return TripleDES.Create();
                default:
                    throw new NotSupportedException($"Symmetric algorithm {alg} is not supported.");
            }
        }

        public void Demos()
        {
            var raw = "The quick brown fox jumped over the lazy dog's back";
            var password = "YeneM1lka$";
            var salt = string.Empty;

            foreach (var alg in Enum.GetValues<CryptographyAlgorithm>())
            {
                var cryptoObj = HydrateSymmetricAlgorithmClass(alg);
                var cipher = new CipherUtility(cryptoObj);
                int pass = 1;
                while (pass < 3)
                {
                    Console.WriteLine("=======================================");
                    salt = DateTime.Now.ToString("fffssmm");
                    Console.WriteLine($"Pass: {pass} of {Enum.GetName<CryptographyAlgorithm>(alg)}...");
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
