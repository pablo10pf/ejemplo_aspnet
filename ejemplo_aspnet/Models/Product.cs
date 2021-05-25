using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ejemplo_aspnet.Models
{
    public enum SuperMarkets
    {
        Carrefour = 0,
        Mercadona = 1,
        CorteIngles = 2
    }
    public class Product
    {
        public int ID { get; set; }
        public string Name{ get; set; }
        public double Price { get; set; }
        public string Link { get; set; }
        public int ID_Supermarket { get; set; }
    }

    public class ProductDBContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
    }
}