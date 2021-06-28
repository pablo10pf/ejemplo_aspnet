using ejemplo_aspnet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace ejemplo_aspnet.Controllers
{
    public class ComparatorController : Controller
    {
        private ProductDBContext db = new ProductDBContext();


        // GET: Comparator
        public ActionResult Index()
        {
            return View();
        }

        // GET: Comparator/Welcome
        public ActionResult Welcome()
        {
            return View();
        }

        //GET: Comparator/Comparator
        public ActionResult Comparator(string productString)
        {
            if (!productString.Equals(""))
            {
                List<string> productList = productString.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                ViewBag.Message = CreateStringWithProducts(productList);
                Scrap(productList);
                return View("Comparator", db.Products.ToList());
            }
            else
            {
                return View("Welcome");
            }
            
            
        }

        private string CreateStringWithProducts(List<string> productList)
        {
            string productString="";
            foreach(string prod in productList)
            {
                if (!prod.Equals(""))
                {
                    productString = productString.Insert(productString.Length,prod+",");
                }
            }
            return productString;
        }

        private void Scrap(List<string> productList)
        {
            db.Products.RemoveRange(db.Products);

            ScrapCarrefour(productList);
            ScrapMercadona(productList);
            ScrapCorteIngles(productList);
            ScrapAlcampo(productList);
        }


        private void ScrapCarrefour(List<string> prods)
        {
            string URL = "https://www.carrefour.es/" + "?q=";
            IWebDriver driver = new ChromeDriver();
            foreach (var prod in prods)
            {
                driver.Navigate().GoToUrl(URL + prod);
                Thread.Sleep(500);
                if (driver.FindElements(By.ClassName("ebx-no-results")).Count > 0)
                {
                    saveProduct(prod, "ERROR", 0, (int)SuperMarkets.Carrefour);
                }
                else if (!prod.Equals(""))
                { 
                    var product = driver.FindElement(By.ClassName("ebx-result__wrapper"));
                    string[] v = product.FindElement(By.TagName("p")).FindElement(By.ClassName("ebx-result-price__value")).GetAttribute("innerText").Split(' ');
                    saveProduct(product.FindElement(By.ClassName("ebx-result-title")).GetAttribute("innerText"),
                        product.FindElement(By.TagName("a")).GetAttribute("href"), double.Parse(v[0]), (int)SuperMarkets.Carrefour);
                }
            }
            driver.Quit();
        }

        private void ScrapMercadona(List<string> prods)
        {
            string URL = "https://tienda.mercadona.es/";
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(URL);
            Thread.Sleep(300);
            foreach (var prod in prods)
            {
                var inputSearch = driver.FindElement(By.Id("search"));
                inputSearch.SendKeys(prod);
                Thread.Sleep(500);
                if (driver.FindElements(By.XPath("//div[@class='search-no-results']")).Count > 0)
                {
                    saveProduct(prod, "ERROR", 0, (int)SuperMarkets.Mercadona);
                }
                else if (!prod.Equals(""))
                {
                    var product = driver.FindElement(By.XPath("//div[@class='product-container']/div/button/div[@class='product-cell__info']"));
                    string[] v = product.FindElement(By.XPath("//div[@class='product-price']")).Text.Split(' ');
                    saveProduct(product.FindElement(By.XPath("//h4")).Text,
                        URL + "search-results?query=" + prod, double.Parse(v[0]), (int)SuperMarkets.Mercadona);
                }
                inputSearch.Clear();
            }
            driver.Quit();
        }

        private void ScrapCorteIngles(List<string> prods)
        {
            string URL = "https://www.elcorteingles.es/supermercado/" + "buscar/?term=";
            foreach (var prod in prods)
            {
                if (!prod.Equals("")) { 
                    IWebDriver driver = new ChromeDriver();
                    driver.Navigate().GoToUrl(URL + prod);
                    Thread.Sleep(300);
                    if (driver.FindElements(By.ClassName("inplace_notification")).Count > 0)
                    {
                        saveProduct(prod, "ERROR", 0, (int)SuperMarkets.CorteIngles);
                    }
                    else 
                    {
                        var product = driver.FindElement(By.ClassName("grid-item"));
                        string[] v = product.FindElement(By.XPath("//div[@class='product_tile-right_container']/div/div/div/div")).Text.Split(' ');
                        saveProduct(product.FindElement(By.XPath("//div[@class='product_tile-right_container']/div/h4")).Text,
                            product.FindElement(By.XPath("//div[@class='product_tile-right_container']/div/h4/a")).GetAttribute("href"),
                            double.Parse(v[0]), (int)SuperMarkets.CorteIngles);
                    }
                    driver.Quit();
                }
            }
        }

        private void ScrapAlcampo(List<string> prods)
        {
            string URL = "https://www.alcampo.es/compra-online/search/?text=";
            foreach (string prod in prods)
            {
                if (!prod.Equals(""))
                {
                    IWebDriver driver = new ChromeDriver();
                    driver.Navigate().GoToUrl(URL + prod);
                    Thread.Sleep(300);
                    driver.FindElement(By.ClassName("cookie-button")).Click();
                    driver.FindElement(By.XPath("//body")).Click();
                    if (driver.FindElements(By.ClassName("titleNoResult")).Count > 0)
                    {
                        saveProduct(prod, "ERROR", 0, (int)SuperMarkets.Alcampo);
                    }
                    else
                    {
                        var product = driver.FindElement(By.ClassName("productGridItem"));
                        string[] v = product.FindElement(By.ClassName("price")).Text.Split(' ');
                        saveProduct(product.FindElement(By.XPath("//h2/a")).GetAttribute("title"),
                            product.FindElement(By.XPath("//h2/a")).GetAttribute("href"), double.Parse(v[0]), (int)SuperMarkets.Alcampo);
                    }
                    driver.Quit();
                }
            }
        }

        private void saveProduct(string name, string link, double price, int idSuperMarket)
        {
            Product p = new Product();
            p.Name = name;
            p.Price = price;
            p.Link = link;
            p.ID_Supermarket = idSuperMarket;
            db.Products.Add(p);
            db.SaveChanges();
        }
    }
}