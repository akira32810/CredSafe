using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using CredSafeDotnetCore.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CredSafeDotnetCore.BusinessClass
{
    public class GetSecretStrings
    {
        public static Keys getkeysHid(string FileLocation, string KeyName) {
            var keyModel = new Keys();
            string data = string.Empty;
            using(StreamReader reader = new StreamReader(FileLocation)) {
                data= reader.ReadToEnd();

            }
            if (!string.IsNullOrEmpty(data)) {

                var jsonresult = JsonConvert.DeserializeObject<List<Keys>>(data).Where(x=>x.Name == KeyName).FirstOrDefault();;

               // var dataFound = jsonresult.Where(x=>x.Name == KeyName).FirstOrDefault();

            
                keyModel = jsonresult;
            }

            return keyModel;
        

        }

        public static async Task<bool> ValidateUser(string FileLocation, string uname, string pw)
        {
            string data = string.Empty;

            using (StreamReader sr = new StreamReader(FileLocation))
            {
                data = await sr.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(data))
            {
                var jsonresultAccounts = JsonConvert.DeserializeObject<List<Keys>>(data).Where(x=>x.Name=="Accounts").ToList();

                if (jsonresultAccounts.Count > 0)
                {
                   var userMatch = jsonresultAccounts.Find(x => x.KeyEnc == uname);

                    if (userMatch != null)
                    {
                        if (pw == userMatch.Salt)
                        {
                            return true;
                        }
                    }

                    else
                        return false;
                }

                

            }

            return false;

        }


        


    }
}