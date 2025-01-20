﻿using System.Security.Cryptography;
using System.Text;

namespace Domain.Authontication;

public static class PasswordHasher
{
    public static string HashPassword(string input)
    {
        using MD5 md5Hash = MD5.Create();
        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        md5Hash.Dispose();
        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    // Verify a hash against a string.
    public static bool VerifyPassword(string input, string hash)
    {
        // Hash the input.

        string hashOfInput = HashPassword(input);

        // Create a StringComparer an compare the hashes.
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        if (0 == comparer.Compare(hashOfInput, hash))
            return true;
        return false;
    }
}
