using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CredSafeDotnetCore.Context;
using CredSafeDotnetCore.Model;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CredSafeDotnetCore.BusinessClass;
using System.Security.Cryptography;
using System.Net;

//use get/post to get cookies by getting/posting correct password- set expire data on cookies
//if cookies doesn't exist all function does not work



namespace CredSafeDotnetCore.Controllers
{
    //[Route("api/[controller]")]
    [Route("")]
    [ApiController]
    //public class CredInfoController : ControllerBase

    public class CredInfoController : Controller
    {


        private readonly CRContext _context;

        private readonly Keys _keys = GetSecretStrings.getkeysHid(Startup.FileLocation, "encryptPass");

        private IHttpContextAccessor _accessor;
        public CredInfoController(CRContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> TestingIP()
        {

             // Cryptography.Decrypt<AesManaged>(cRModel.CRPass, _keys.KeyEnc, _keys.Salt);
            //   return Json(new { result = UserInfo.GetRemoteIPAddress(blob,true) });

            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var ip2 = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            // var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            //  return new string[] { ip, "value2" };
            //IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
            //var ip = heserver.AddressList[0].ToString();
            //var ip2 = heserver.AddressList[1].ToString();
            //var ip3 = heserver.AddressList[2].ToString();

            return Content("ip1: " + ip + ", ip2:" + ip2);



        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Testing()
        {


            //  return Json(new { result = ip });
            string html = string.Empty;
            string url = @"http://gd.geobytes.com/GetCityDetails";


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();

                var jsondata = JsonConvert.DeserializeObject<GeoByteData>(html);

                return Json(new { result = jsondata.geobytesremoteip });
            }




        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateTokenForService()
        {
            string data = string.Empty;

            string Token = string.Empty;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();

            }

            if (!string.IsNullOrEmpty(data))
            {
                var jsonCredential = JsonConvert.DeserializeObject<Credential>(data);

                Token = await CreateToken.MakeToken(Startup.FileLocation, jsonCredential.uname, jsonCredential.pw, GetIPAddr(), GetUserAgent());

                if (!string.IsNullOrEmpty(Token))
                {
                    Token myToken = new Token();
                    myToken.Tokenstr = Token;

                    _context.Token.Add(myToken);
                    await _context.SaveChangesAsync();
                    //create and save token here.

                    return Json(new { result = Token, Expire = "30 days" });
                }

            }


            return Json(new { result = "Invalid username or password, cannot create token" });
        }



        // PUT: api/CredInfo/5
        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> UpdateService(int id, CRModel cRModel)
        {
            var findID = await _context.CRModel.FindAsync(id);
            if (findID == null)
            {
                return BadRequest();
            }


            //_context.Entry(cRModel).State = EntityState.Modified;
            //  var TokenStr = cRModel.TokenStr;
            var CompareToken = await _context.Token.Where(x => x.Tokenstr == cRModel.TokenStr).ToListAsync();

            var tokenOwner = CreateToken.ReturnOwnerStr(cRModel.TokenStr);
            if (tokenOwner == findID.Owner)
            {
                if (!string.IsNullOrEmpty(cRModel.TokenStr) && CompareToken.Count == 1 && CreateToken.ValidateToken(cRModel.TokenStr, GetIPAddr(), GetUserAgent()))
                {


                    findID.CRUser = Cryptography.Encrypt<AesManaged>(cRModel.CRUser, _keys.KeyEnc, _keys.Salt);
                    findID.CRPass = Cryptography.Encrypt<AesManaged>(cRModel.CRPass, _keys.KeyEnc, _keys.Salt); ;
                    findID.CRService = cRModel.CRService;
                    try
                    {
                        await _context.SaveChangesAsync();

                        return Json(new { result = "Modify Success!" });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CRModelExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }


                    }
                }

                else

                    return Json(new { result = "Invalid token or request" });

            }

            else
                return Json(new { result = "This service does not belong to you or invalid service" });

        }


        [HttpPost] // api/CredInfo/id/#idNum
        [Route("[action]")]
        public async Task<IActionResult> GetServiceByCRID()
        {
            //    CRModel model = new CRModel();
            string data = string.Empty;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();

            }
            if (!string.IsNullOrEmpty(data))
            {
                var jsonresult = JsonConvert.DeserializeObject<CRModel>(data);

                var CompareToken = await _context.Token.Where(x => x.Tokenstr == jsonresult.TokenStr).ToListAsync();

                var tokenOwner = CreateToken.ReturnOwnerStr(jsonresult.TokenStr);

                if (jsonresult.CRID != 0 && CompareToken.Count == 1 && CreateToken.ValidateToken(jsonresult.TokenStr, GetIPAddr(), GetUserAgent()))
                {
                    var cRModel = await _context.CRModel.Where(x => x.CRID == jsonresult.CRID && x.Owner == tokenOwner).FirstOrDefaultAsync();
                    cRModel.CRUser = Cryptography.Decrypt<AesManaged>(cRModel.CRUser, _keys.KeyEnc, _keys.Salt);
                    cRModel.CRPass = Cryptography.Decrypt<AesManaged>(cRModel.CRPass, _keys.KeyEnc, _keys.Salt);
                    return Json(cRModel);
                }
            }


            return Json(new { result = "ID doesn't exist or invalid/expire token" });

        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetAllServices()
        {
            string data = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(data))
            {
                var jsonresult = JsonConvert.DeserializeObject<CRModel>(data);

                var CompareToken = await _context.Token.Where(x => x.Tokenstr == jsonresult.TokenStr).ToListAsync();

                var tokenOwner = CreateToken.ReturnOwnerStr(jsonresult.TokenStr);

                if (CompareToken.Count == 1 && CreateToken.ValidateToken(jsonresult.TokenStr, GetIPAddr(), GetUserAgent()))
                {
                    //  var resultSet = await _context.CRModel.ToListAsync();
                    var resultSet = await _context.CRModel.Where(x => x.Owner == tokenOwner).ToListAsync();



                    return Json(resultSet);
                }

            }


            return Json(new { result = "Invalid or expire token" });
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetDataByService()
        {
            string data = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(data))
            {
                var jsonresult = JsonConvert.DeserializeObject<CRModel>(data);

                var CompareToken = await _context.Token.Where(x => x.Tokenstr == jsonresult.TokenStr).ToListAsync();

                var tokenOwner = CreateToken.ReturnOwnerStr(jsonresult.TokenStr);

                if (!string.IsNullOrEmpty(jsonresult.CRService) && CompareToken.Count == 1 && CreateToken.ValidateToken(jsonresult.TokenStr, GetIPAddr(), GetUserAgent()))
                {
                    var resultSet = await _context.CRModel.Where(x => x.CRService.Contains(jsonresult.CRService) && x.Owner == tokenOwner).ToListAsync();
                    if (resultSet.Count > 0)
                    {
                        foreach (var item in resultSet)
                        {
                            item.CRUser = Cryptography.Decrypt<AesManaged>(item.CRUser, _keys.KeyEnc, _keys.Salt);
                            item.CRPass = Cryptography.Decrypt<AesManaged>(item.CRPass, _keys.KeyEnc, _keys.Salt);
                        }

                        return Json(resultSet);
                    }
                    else

                        return Json(new { result = "No results found" });
                }

            }


            return Json(new { result = "Invalid or expire token" });
        }

        [HttpPost]
        public async Task<ActionResult<CRModel>> PostCRModel()
        {


            CRModel mymodel = new CRModel();
            string data = string.Empty;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }


            if (!string.IsNullOrEmpty(data))
            {
                var jsonresult = JsonConvert.DeserializeObject<CRModel>(data);
                var CompareToken = await _context.Token.Where(x => x.Tokenstr == jsonresult.TokenStr).ToListAsync();
                var tokenOwner = CreateToken.ReturnOwnerStr(jsonresult.TokenStr);
                if (!string.IsNullOrEmpty(jsonresult.CRUser) && CompareToken.Count == 1 && CreateToken.ValidateToken(jsonresult.TokenStr, GetIPAddr(), GetUserAgent()))
                {
                    mymodel.CRService = jsonresult.CRService;
                    mymodel.Owner = tokenOwner;
                    mymodel.CRUser = Cryptography.Encrypt<AesManaged>(jsonresult.CRUser, _keys.KeyEnc, _keys.Salt);
                    mymodel.CRPass = Cryptography.Encrypt<AesManaged>(jsonresult.CRPass, _keys.KeyEnc, _keys.Salt);
                }
            }

            _context.CRModel.Add(mymodel);

            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetCRModel", new { id = mymodel.CRID }, mymodel);
            return await _context.CRModel.FindAsync(mymodel.CRID);
        }



        // DELETE: api/CredInfo/5
        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {

            var cRModelResult = await _context.CRModel.FindAsync(id);

            string data = string.Empty;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }

            if (cRModelResult == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(data) && cRModelResult != null)
            {
                var jsonresult = JsonConvert.DeserializeObject<CRModel>(data);
                var CompareToken = await _context.Token.Where(x => x.Tokenstr == jsonresult.TokenStr).ToListAsync();
                var tokenOwner = CreateToken.ReturnOwnerStr(jsonresult.TokenStr);

                if (tokenOwner == cRModelResult.Owner)
                {
                    //if owner of delete id pass in is equal to tokenowner, then you can delete

                    if (!string.IsNullOrEmpty(jsonresult.TokenStr) && CompareToken.Count == 1 && CreateToken.ValidateToken(jsonresult.TokenStr, GetIPAddr(), GetUserAgent()))
                    {
                        _context.CRModel.Remove(cRModelResult);
                        await _context.SaveChangesAsync();

                        return Json(new { result = "Delete Success!" });
                    }

                    else

                        return Json(new { result = "Invalid or expire token" });

                }

                else
                {
                    return Json(new { result = "This service does not belong to you" });
                }
            }

            else
            {


                return Json(new { result = "Empty data." });
            }
        }

        private bool CRModelExists(int id)
        {

            return _context.CRModel.Any(e => e.CRID == id);


        }

        private string GetIPAddr()
        {


            return _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        private string GetUserAgent()
        {
            return Request.Headers["User-Agent"].ToString();
        }




    }
}
