using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// 세이브 데이터 문자열 암호화 및 복호화 (AES)
public static class SaveEncryptor
{
    private const string KeySource = "NeonRoot_Save_Key"; // 변경 금지
    private const int IvLength = 16; // AES 블록 크기이자 IV 길이 (AES는 데이터를 16바이트씩 잘라서 처리해서 16)
    private static readonly byte[] Key = CreateKey(KeySource); // 실제로 암호화에 쓰이는 키

    private static byte[] CreateKey(string source)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(source));
        }
    }

    // JSON 문자열 -> 암호화된 Base64 문자열
    public static string Encrypt(string plainText)
    {
        if (plainText == null) return null;

        try
        {
            using (Aes aes = Aes.Create()) // 다 쓰고 바로 정리하려고 using문 사용
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // 저장할 때마다 새 랜덤 IV -> 같은 데이터도 매번 다른 결과

                byte[] iv = aes.IV;
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }

                // [IV][암호문] 순서로 붙이기 (IV는 딱히 비밀이 아니라서 같이 저장해도 됨)
                byte[] result = new byte[iv.Length + cipherBytes.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(result);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 암호화 실패\n{e}");
            return null;
        }
    }

    // 암호화된 Base64 문자열 -> JSON 문자열 (실패 시 null 리턴)
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return null;

        try
        {
            byte[] data = Convert.FromBase64String(encryptedText.Trim());
            if (data.Length <= IvLength) return null; // IV만 있거나 너무 짧으면 손상된 파일

            // 앞 16바이트를 IV로 떼어냄
            byte[] iv = new byte[IvLength];
            Buffer.BlockCopy(data, 0, iv, 0, IvLength);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(data, IvLength, data.Length - IvLength);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
        catch (Exception e)
        {
            // Base64 형식 오류, 패딩 오류 등 -> 손상되었거나 직접 수정된 파일
            Debug.LogWarning($"세이브 복호화 실패 (손상 또는 변조된 파일)\n{e.Message}");
            return null;
        }
    }
}