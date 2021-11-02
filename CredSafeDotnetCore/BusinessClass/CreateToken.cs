using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CredSafeDotnetCore.Model;
using System.Security.Cryptography;

namespace CredSafeDotnetCore.BusinessClass
{
    public class CreateToken
    {

        public static async Task<string> MakeToken(string FileName, string user, string pw, string ip, string userAgent)
        {
           
            bool ValidUser = await GetSecretStrings.ValidateUser(FileName, user, pw);
            string token = string.Empty;
            if (ValidUser)
            {
                Keys tokenKey = GetSecretStrings.getkeysHid(Startup.FileLocation, "tokenEnc");
                string userEnc = Cryptography.Encrypt<AesManaged>(user, tokenKey.KeyEnc, tokenKey.Salt);
                string ipEnc = Cryptography.Encrypt<AesManaged>(ip, tokenKey.KeyEnc, tokenKey.Salt);
                string UserAgentEnc = userAgent;
                string DatetimeEnc = Cryptography.Encrypt<AesManaged>(DateTime.UtcNow.ToString(), tokenKey.KeyEnc, tokenKey.Salt);
                //create token
                token = userEnc + ":" + ipEnc + ":" + userAgent + ":" + DatetimeEnc;

            }

            return token;


        }

        public static bool ValidateToken(string TokenStr, string ip, string userAgent)
        {
            Keys tokenKey = GetSecretStrings.getkeysHid(Startup.FileLocation, "tokenEnc");
            string TokenCreatestr = Cryptography.Decrypt<AesManaged>(TokenStr.Split(':')[3].ToString(), tokenKey.KeyEnc, tokenKey.Salt);
            string TokenUserAgent = TokenStr.Split(':')[2].ToString();
            string TokenIP = Cryptography.Decrypt<AesManaged>(TokenStr.Split(':')[1].ToString(), tokenKey.KeyEnc, tokenKey.Salt);

            DateTime TokenCreatedTime = DateTime.Parse(TokenCreatestr);
            DateTime now = DateTime.UtcNow;



           // if (TokenCreatedTime.AddMinutes(30) <= now)
           //expire in 30 minutes
           if (now <= TokenCreatedTime.AddDays(30))
            {
                if (ip == TokenIP && userAgent == TokenUserAgent)
                {
                    return true;
                }
                else
                    return false;
            }

            else
                return false;

            //if more than 30 , return false on token

        }


        public static string ReturnOwnerStr(string TokenStr)
        {
            Keys tokenKey = GetSecretStrings.getkeysHid(Startup.FileLocation, "tokenEnc");
            string TokenOwner = Cryptography.Decrypt<AesManaged>(TokenStr.Split(':')[0].ToString(), tokenKey.KeyEnc, tokenKey.Salt);

            return TokenOwner;

        }


    }
}
