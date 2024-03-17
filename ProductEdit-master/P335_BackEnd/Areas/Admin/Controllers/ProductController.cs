using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using P335_BackEnd.Areas.Admin.Models;
using P335_BackEnd.Data;
using P335_BackEnd.Entities;
using P335_BackEnd.Services;

namespace P335_BackEnd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly FileService _fileService;

        public ProductController(AppDbContext dbContext, FileService fileService)
        {
            _dbContext = dbContext;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            var products = _dbContext.Products.AsNoTracking().ToList();

            var model = new ProductIndexVM
            {
                Products = products
            };

            return View(model);
        }

        public IActionResult Add()
        {
            var categories = _dbContext.Categories.AsNoTracking().ToList();
            var productTypes = _dbContext.ProductTypes.AsNoTracking().ToList();

            var model = new ProductAddVM
            {
                Categories = categories,
                ProductTypes = productTypes
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Add(ProductAddVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newProduct = new Product();

            newProduct.Name = model.Name;
            newProduct.Price = (decimal)model.Price;

            var foundCategory = _dbContext.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
            if (foundCategory is null) return View(model);

            newProduct.Category = foundCategory;

            if (model.ProductTypeId != null)
            {
                var foundProductType = _dbContext.ProductTypes.FirstOrDefault(x => x.Id == model.ProductTypeId);
                if (foundProductType is null) return View(model);

                newProduct.ProductTypeProducts = new()
                {
                    new ProductTypeProduct
                    {
                        ProductType = foundProductType
                    }
                };
            }

            newProduct.ImageUrl = _fileService.AddFile(model.Image, Path.Combine("img", "featured"));

            _dbContext.Add(newProduct);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int? id)
        {
            if (id is null) return BadRequest();

            Product? product = _dbContext.Products.Include(p => p.ProductTypeProducts).FirstOrDefault(x => x.Id == id);

            List<Category> category = _dbContext.Categories.AsNoTracking().ToList();
            List<ProductType> productTypes = _dbContext.ProductTypes.AsNoTracking().ToList();

            if (product is null) return NotFound();
            ProductEditVM editedModel = new ProductEditVM()
            {
                Name = product.Name,
                ProductTypes = productTypes,
                Categories = category,
                CategoryId = product.CategoryId,
                Price = product.Price,
                ProductTypeId = product.ProductTypeProducts.FirstOrDefault()?.ProductTypeId,
                ImageUrl = product.ImageUrl,
            };
            return View(editedModel);

        }

        [HttpPost]
        public IActionResult Edit(ProductEditVM editedProduct)
        {
            Product? product = _dbContext.Products.FirstOrDefault(p => p.Id == editedProduct.Id);
            if (product is null) return NotFound();
            if (editedProduct.ImageUrl != product.ImageUrl && editedProduct.ImageUrl is null)
            {
                _fileService.DeleteFile(product.ImageUrl, Path.Combine("img", "featured"));
                product.ImageUrl = null;
            }
            else if (editedProduct.Image != null)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    _fileService.DeleteFile(product.ImageUrl, Path.Combine("img", "featured"));

                }
                product.ImageUrl = _fileService.AddFile(editedProduct.Image, Path.Combine("img", "featured"));
            }

            product.Name = editedProduct.Name;
            product.Price = (decimal)editedProduct.Price;
            product.CategoryId = editedProduct.CategoryId;

            product.ProductTypeProducts = new List<ProductTypeProduct>()
            {
                new()
                {
                    ProductTypeId = (int)editedProduct.ProductTypeId,
                }
            };
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products.FirstOrDefault(x => x.Id == id);

            if (product is null) return NotFound();

            _fileService.DeleteFile(product.ImageUrl, Path.Combine("img", "featured"));

            _dbContext.Remove(product);

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}