﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

class JavaAES
{
    static JavaAES ins;
    public static JavaAES Instance { get { if (ins == null) ins = new JavaAES(); return ins; } }

    Aes rDel, rDel2;

    private JavaAES()
    {
        var key = Encoding.UTF8.GetBytes("994AA5847C555EB8");//
        rDel = Aes.Create();
        rDel.Key = key;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.None;

        rDel2 = Aes.Create();
        rDel2.Key = key;
        rDel2.Mode = CipherMode.ECB;
        rDel2.Padding = PaddingMode.None;
    }
    public string EncryptS(string toEncrypt)
    {
        var r = Encrypt(Encoding.UTF8.GetBytes(toEncrypt));
        return Encoding.UTF8.GetString(r, 0, r.Length);
    }
    public string EncryptS(byte[] toEncrypt)
    {
        var r = Encrypt(toEncrypt);
        return Encoding.UTF8.GetString(r, 0, r.Length);
    }
    public byte[] Encrypt(string toEncryptArray)
    {
        return Encrypt(Encoding.UTF8.GetBytes(toEncryptArray));
    }
    public byte[] Encrypt(byte[] toEncryptArray)
    {
        int len = toEncryptArray.Length;
        int r = len % 16;
        if (r > 0)
        {
            int l = toEncryptArray.Length + 16 - r;
            byte[] tmp = new byte[l];
            for (int i = 0; i < len; i++)
                tmp[i] = toEncryptArray[i];
            toEncryptArray = tmp;
        }
        var enTransform = rDel.CreateEncryptor();
        return enTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
    public string DecryptS(byte[] toDecrypt)
    {
        return Encoding.UTF8.GetString(Decrypt(toDecrypt));
    }
    public byte[] Decrypt(byte[] toEncryptArray)
    {
        var deTransform = rDel2.CreateDecryptor();
        return deTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
    public byte[] Decrypt(byte[] toEncryptArray, int start, int len)
    {
        var deTransform = rDel2.CreateDecryptor();
        return deTransform.TransformFinalBlock(toEncryptArray, start, len);
    }
}
class AES
{
    static AES ins;
    public static AES Instance { get { if (ins == null) ins = new AES(); return ins; } }

    Aes rDel, rDel2;
    private AES()
    {
        var key = Encoding.UTF8.GetBytes("BF5CA09D209B463180EFFA34CCE39C1A");//e888ad1f733c4f2e //
        var iv = Encoding.UTF8.GetBytes("E64F13009A40487F");
        rDel = Aes.Create();
        rDel.Key = key;
        rDel.IV = iv;
        rDel.Mode = CipherMode.CBC;
        rDel.Padding = PaddingMode.Zeros;

        rDel2 = Aes.Create();
        rDel2.Key = key;
        rDel2.IV = iv;
        rDel2.Mode = CipherMode.CBC;
        rDel2.Padding = PaddingMode.None;
    }
    public string EncryptS(string toEncrypt)
    {
        var r = Encrypt(Encoding.UTF8.GetBytes(toEncrypt));
        return Encoding.UTF8.GetString(r, 0, r.Length);
    }
    public string EncryptS(byte[] toEncrypt)
    {
        var r = Encrypt(toEncrypt);
        return Encoding.UTF8.GetString(r, 0, r.Length);
    }
    public byte[] Encrypt(string toEncryptArray)
    {
        return Encrypt(Encoding.UTF8.GetBytes(toEncryptArray));
    }
    public byte[] Encrypt(byte[] toEncryptArray)
    {
        var enTransform = rDel.CreateEncryptor();
        return enTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
    public string DecryptS(byte[] toDecrypt)
    {
        return Encoding.UTF8.GetString(Decrypt(toDecrypt));
    }
    public byte[] Decrypt(byte[] toEncryptArray)
    {
        var deTransform = rDel2.CreateDecryptor();
        return deTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
    public byte[] Decrypt(byte[] toEncryptArray,int start,int len)
    {
        var deTransform = rDel2.CreateDecryptor();
        return deTransform.TransformFinalBlock(toEncryptArray, start, len);
    }
    public static byte[] Decrypt(byte[] data,string key,string iv)
    {
        var  de = Aes.Create();
        de.Key = Encoding.UTF8.GetBytes(key);
        de.IV = Encoding.UTF8.GetBytes(iv);
        de.Mode = CipherMode.CBC;
        de.Padding = PaddingMode.None;
        var deTransform = de.CreateDecryptor();
        return deTransform.TransformFinalBlock(data, 0, data.Length);
    }
}