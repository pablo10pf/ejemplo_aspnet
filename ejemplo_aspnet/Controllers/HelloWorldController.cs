using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System;
using ejemplo_aspnet.Models;
using System.Data.Entity;
using OpenQA.Selenium.Interactions;
using System.Drawing;

namespace ejemplo_aspnet.Controllers
{
    public class HelloWorldController : Controller
    {
        private ProductDBContext db = new ProductDBContext();


        // GET: HelloWorld
        public ActionResult Index()
        {
            return View();
        }

        // GET: HelloWorld/Welcome
        public ActionResult Welcome()
        {
            return View();
        }

        //GET: HelloWorld/Comparador
        public ActionResult Comparador(string productString)
        {
            ViewBag.Message = productString;
            //return View("Comparador",ConvertProductStringToProductList(productString));
            ConvertProductStringToProductList(productString);
            return View("Comparador", db.Products.ToList());
        }

        private List<string> ConvertProductStringToProductList(string productString)
        {
            List<string> productList = productString.Split('\n').ToList();
            db.Products.RemoveRange(db.Products);
            
            //ScrapCarrefour(productList);
            ScrapMercadona(productList);
            //ScrapCorteIngles(productList);
            //ScrapAlcampo(productList);
            return productList;
        }


        private void ScrapCarrefour(List<string> prods)
        {
            string URL = "https://www.carrefour.es/"+"?q=";
            IWebDriver driver = new ChromeDriver();
            foreach (var prod in prods)
            {
                driver.Navigate().GoToUrl(URL+prod);
                Thread.Sleep(1000);
                //var inputSearchCarrefour = driver.FindElement(By.XPath("//div[@class='search-bar']/input"));
                //inputSearchCarrefour.SendKeys("atun");


                if(driver.FindElements(By.ClassName("ebx-no-results")).Count > 0)
                {
                    saveProduct("ERROR", "", 0, (int)SuperMarkets.Carrefour);
                }
                else
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
            Thread.Sleep(1000);
            //var inputPostalCode = driver.FindElement(By.Name("postalCode"));
            //inputPostalCode.SendKeys("46001");
            //inputPostalCode.Submit();


            foreach (var prod in prods)
            {

                var inputSearch = driver.FindElement(By.Id("search"));
                inputSearch.SendKeys(prod);
                Thread.Sleep(1000);

                if (driver.FindElements(By.XPath("//div[@class='search-no-results']")).Count > 0 ) {
                    saveProduct("ERROR", "", 0, (int)SuperMarkets.Mercadona);
                }
                else{
                    var product = driver.FindElement(By.XPath("//div[@class='product-container']/div/button/div[@class='product-cell__info']"));
                    string[] v = product.FindElement(By.XPath("//div[@class='product-price']")).Text.Split(' ');

                    saveProduct(product.FindElement(By.XPath("//h4")).Text,
                        URL+"search-results?query="+prod, double.Parse(v[0]), (int)SuperMarkets.Mercadona);
                }
                
                //driver.FindElement(By.ClassName("product-cell__content-link")).Click();

                
                /*IWebElement element = driver.FindElement(By.ClassName("product-cell__content-link"));
                Point point = element.Location;
                Actions actions = new Actions(driver);
                actions.MoveByOffset(point.X, point.Y).Click().Perform();
                var aaa = driver.FindElement(By.XPath("//meta[@property='og:url']"));*/

                inputSearch.Clear();
            }
            driver.Quit();
        }

        private void ScrapCorteIngles(List<string> prods)
        {
            string URL = "https://www.elcorteingles.es/supermercado/"+"buscar/?term=";
            //string URL = "https://www.elcorteingles.es/supermercado/";
            
            foreach (var prod in prods)
            {
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl(URL+prod);
                
                /*var inputSearch = driver.FindElement(By.Name("term"));
                inputSearch.SendKeys("atun");
                inputSearch.Submit();*/
                Thread.Sleep(1000);

                if (driver.FindElements(By.ClassName("inplace_notification")).Count > 0)
                {
                    saveProduct("ERROR", "", 0, (int)SuperMarkets.CorteIngles);
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

        private void ScrapAlcampo(List<string> prods)
        {
            string URL = "https://www.alcampo.es/compra-online/search/?text=";

            foreach (string prod in prods)
            {
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl(URL + prod);
                Thread.Sleep(1000);
                /*var inputSearch = driver.FindElement(By.Id("search"));
                inputSearch.SendKeys("atun");
                inputSearch.Submit();*/
                driver.FindElement(By.ClassName("cookie-button")).Click();
                driver.FindElement(By.XPath("//body")).Click();
                //driver.FindElement(By.CssSelector(".secondary-button.black-secondary")).Click();

                if (driver.FindElements(By.ClassName("titleNoResult")).Count > 0)
                {
                    saveProduct("ERROR", "", 0, (int)SuperMarkets.Alcampo);
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
        
        private void saveProduct(string name, string link, double price, int idSuperMarket)
        {
            Product p = new Product();
            p.Name = name;
            p.Price = price;
            p.Link = link ;
            p.ID_Supermarket = idSuperMarket;
            db.Products.Add(p);
            db.SaveChanges();
        }
    }
}   