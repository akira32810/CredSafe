using CredSafeDotnetCore.BusinessClass;
using CredSafeDotnetCore.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CredSafeDotnetCore.Context
{
    public class CRContext : DbContext
    {


        //public CRContext(DbContextOptions<CRContext> options) : base(options)
        //{
        //}

        public DbSet<CRModel> CRModel { get; set; }

        public DbSet<Token> Token { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Keys data = GetSecretStrings.getkeysHid(Startup.FileLocation, "connString");

            string encString = Cryptography.Encrypt<AesManaged>(data.StringToEnc, data.KeyEnc, data.Salt);

            optionsBuilder.UseMySql(Cryptography.Decrypt<AesManaged>(encString, data.KeyEnc, data.Salt));


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
     
           base.OnModelCreating(modelBuilder);

        }
    }
}
