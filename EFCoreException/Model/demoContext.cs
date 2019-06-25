using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using TestModel.Models;
using TestModel.Models.Mapping;

namespace UnitTest.EFModel.Models
{
    public partial class demoContext : DbContext
    {
       
       
        private static DbContextOptions<demoContext> dbContextOptions
        {
            get
            {
                DbContextOptionsBuilder<demoContext> options = new DbContextOptionsBuilder<demoContext>();
                options.UseMySql("Data Source=192.168.100.211;port=3306;Initial Catalog=demo;user id=FrameworkTest;password=FrameworkTest;Character Set=utf8;Allow Zero Datetime=true;persistsecurityinfo=True;Convert Zero Datetime=true;pooling=true;MaximumPoolsize=3000;", a=>a.DisableBackslashEscaping().MaxBatchSize(1));
                options.UseLazyLoadingProxies();
                return options.Options;
            }
        }
        public demoContext() : base(dbContextOptions)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         
        }


        public demoContext(DbContextOptions<demoContext> options) : base(options)
        {

        }

        public virtual DbSet<item> item { get; set; }
        public virtual DbSet<itemDetail> item_detail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new itemMap());
            modelBuilder.ApplyConfiguration(new itemDetailMap());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
