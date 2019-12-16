using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Azure.Devices.Common;
using NLog;
using NorthwindConsole.Models;

namespace NorthwindConsole
{
    class MainClass
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            logger.Info("Program started");
            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");//Complete
                    Console.WriteLine("2) Add Category");//Complete
                    Console.WriteLine("3) Display Specific Category and related products");//Complete
                    Console.WriteLine("4) Display all Categories and their related products");//Complete
                    Console.WriteLine("5) Display Products");//Complete
                    Console.WriteLine("6) Add Product");//Complete
                    Console.WriteLine("7) Edit Product");//Check
                    Console.WriteLine("8) Edit Category");//Check
                    Console.WriteLine("9) Delete a specific record from Products");//Complete
                    Console.WriteLine("10) Delete a specific record from Categories");//Complete
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        //Display Categories
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.WriteLine($"{query.Count()} records returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                    }
                    else if (choice == "2")
                    {
                        //Add Category 
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var db = new NorthwindContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db = new NorthwindContext();
                                db.Categories.Add(category);
                                db.SaveChanges();
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        //Display Specific Category and related products
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Product p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        //Display all Categories and their related products
                        var db = new NorthwindContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");

                            foreach (Product p in item.Products)
                            {
                                Console.WriteLine($"{p.ProductName}");
                            }

                        } 
                        
                    }
                    else if (choice == "5")
                    {
                        //Display Products
                        Console.WriteLine("Choose one of the following options to display products");
                        Console.WriteLine("(A)ctive");
                        Console.WriteLine("(D)iscontinued");
                        Console.WriteLine("(B)oth");
                        var productChoice = Console.ReadLine().ToUpper();
                        var db = new NorthwindContext();
                        var query = db.Products.OrderBy(p => p.ProductName);

                        if (productChoice == "A")
                        {
                            //display only active products
                            foreach (var item in query)
                            {
                                if (item.Discontinued == false)
                                {
                                    Console.WriteLine($"{item.ProductID}) {item.ProductName}");
                                }
                                else
                                { logger.Info("No Active Products"); }
                            }
                        }
                        else if (productChoice == "D")
                        {
                            //display only discontinued products
                            foreach (var item in query)
                            {
                                if(item.Discontinued == true)
                                {
                                    Console.WriteLine($"{item.ProductID} - {item.ProductName}");
                                }
                                else
                                { logger.Info("No Discontinued Products"); }
                            }
                        }
                        else if (productChoice == "B")
                        {
                            //display all products 
                            Console.WriteLine($"{query.Count()} records returned");
                            foreach (var item in query)
                            {
                                Console.Write(item.ProductName + " - ");
                                Console.WriteLine(item.Discontinued ? "Discontinued" : "Active");
                            }
                        }
                        else
                        { logger.Error("Please Enter Valid Option"); }
                    }
                    else if (choice == "6")
                    {
                        //Add Product
                        Product product = new Product();
                        Console.WriteLine("Enter Product Name:");
                        product.ProductName = Console.ReadLine();
                       

                        ValidationContext context = new ValidationContext(product, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        
                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            var db = new NorthwindContext();
                            // check for unique name
                            if (db.Products.Any(c => c.ProductName == product.ProductName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                            }
                            else
                            {
                                Console.WriteLine("Would you like to add this product to a Category? Y/N");
                                var response = Console.ReadLine().ToUpper();

                                if (response == "Y")
                                {
                                    var query = db.Categories.OrderBy(p => p.CategoryName);

                                    Console.WriteLine($"{query.Count()} records returned");
                                    foreach (var item in query)
                                    {
                                        Console.WriteLine($"{item.CategoryId} - {item.CategoryName} - {item.Description}");
                                    }

                                    Console.WriteLine("Please enter the appropriate categoryID");
                                    product.CategoryId = int.Parse(Console.ReadLine());
                                }
                                else
                                {
                                    product.CategoryId = null;
                                }
                                product.ProductID = db.Products.Count() + 1;
                                logger.Info("Validation passed");
                                db = new NorthwindContext();
                                db.Products.Add(product);
                                db.SaveChanges();
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "7")
                    {
                        //Edit Product
                        Console.WriteLine("Choose the product to edit");

                        var db = new NorthwindContext();
                        var product = GetProduct(db);
                
                        if (product != null)
                        {
                            Product UpdatedProduct = InputProduct(db);
                            if (UpdatedProduct != null)
                            {
                                UpdatedProduct.ProductID = product.ProductID;
                                db.EditProduct(UpdatedProduct);
                            }
                            else
                            {
                                logger.Error("Invalid Product Update");
                            }
                        }
                        else
                        {
                            logger.Error("Invalid Product");
                        }
                    }
                    else if (choice == "8")
                    {
                        //Edit Category
                        Console.WriteLine("Choose the category to edit");
                        var db = new NorthwindContext();
                        var category = GetCategory(db);
                        if (category != null)
                        {
                            Category UpdatedCategory = InputCategory(db);
                            if (UpdatedCategory != null)
                            {
                                UpdatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(UpdatedCategory);
                            }
                            else
                            {
                                logger.Error("Invalid Category Update");
                            }
                        }
                        else
                        {
                            logger.Error("Invalid Category");
                        }
                    }
                    else if (choice == "9")
                    {
                        // delete product
                        Console.WriteLine("Choose the product to delete");
                        var db = new NorthwindContext();
                        var product = GetProduct(db);
                        if (product != null)
                        {
                            db.DeleteProduct(product);
                        }
                        else
                        {
                            logger.Error("Invalid Product");
                        }
                    }
                    else if (choice == "10")
                    {
                        // delete category
                        Console.WriteLine("Choose the category to delete");
                        var db = new NorthwindContext();
                        var category = GetCategory(db);
                        if (category != null)
                        {
                            db.DeleteCategory(category);
                        }
                        else
                        {
                            logger.Error("Invalid Category");
                        }
                    }
                    Console.WriteLine();

                    } while (choice.ToLower() != "q") ;
                }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            logger.Info("Program ended");
        }
        public static Product InputProduct(NorthwindContext db)
        {
            Product product = new Product();
            Console.WriteLine("Enter the Product Name");
            product.ProductName = Console.ReadLine();
            
            if (product != null)
            {
                return product;
            }
            else
            {
                logger.Error("Invalid product edit");
            }
            return null;
        }

        public static Product GetProduct(NorthwindContext db)
        {
            var categories = db.Categories.Include("Products").OrderBy(b => b.CategoryName);
            foreach (Category b in categories)
            {
                Console.WriteLine(b.CategoryName);
                if (b.Products.Count() == 0)
                {
                    Console.WriteLine($"No Products");
                }
                else
                {
                    foreach (Product p in b.Products)
                    {
                        Console.WriteLine($"  {p.ProductID}) {p.ProductName}");
                    }
                }
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Product product = db.Products.FirstOrDefault(p => p.ProductID == ProductId);
                if (product != null)
                {
                    return product;
                }
                else
                {
                    logger.Error("Please enter valid product");
                }
            }
            return null;
        }
        public static Category InputCategory(NorthwindContext db)
        {
            Category category = new Category();
            Console.WriteLine("Enter the CategoryName");
            category.CategoryName = Console.ReadLine();

            if (category != null)
            {
                return category;
            }
            else
            {
                logger.Error("Invalid category edit");
            }
            return null;
        }

        public static Category GetCategory(NorthwindContext db)
        {
            var categories = db.Categories.Include("Products").OrderBy(b => b.CategoryName);
            foreach (Category b in categories)
            {
                Console.WriteLine($"  {b.CategoryId}) {b.CategoryName}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Category category = db.Categories.FirstOrDefault(p => p.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
                else
                {
                    logger.Error("Please enter vaid category");
                }
            }
            return null;
        }
    }
}
