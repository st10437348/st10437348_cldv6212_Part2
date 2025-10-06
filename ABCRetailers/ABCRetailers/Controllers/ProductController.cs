using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi api, ILogger<ProductController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var products = await _api.Products_ListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products
                    .Where(p => p.ProductName != null && p.ProductName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            ViewData["CurrentSearch"] = searchTerm;
            return View(products);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (product.Price <= 0 && !double.TryParse(product.PriceString, out var parsed))
                ModelState.AddModelError("Price", "Price must be a positive number.");

            if (!ModelState.IsValid) return View(product);

            try
            {
                var created = await _api.Products_CreateAsync(product, imageFile);
                TempData["Success"] = $"Product '{created.ProductName}' created successfully with price {created.PriceString}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", $"An error occurred while creating the product: {ex.Message}");
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _api.Products_GetAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (product.Price <= 0 && !double.TryParse(product.PriceString, out var parsed))
                ModelState.AddModelError("Price", "Price must be a positive number.");

            if (!ModelState.IsValid) return View(product);

            try
            {
                var updated = await _api.Products_UpdateAsync(product.RowKey, product, imageFile);
                TempData["Success"] = $"Product '{updated.ProductName}' has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError("", $"An error occurred while updating the product: {ex.Message}");
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _api.Products_DeleteAsync(id);
                TempData["Success"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting the product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



