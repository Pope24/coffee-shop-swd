using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessObjects.Services;
using BussinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Const;

namespace CoffeeShop.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;

        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public decimal Revenue { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<TopProductViewModel> TopProducts { get; set; } = new();
        public int[] MonthlyOrderData { get; set; } = new int[12];

        public IndexModel(
            IOrderService orderService,
            IUserService userService,
            IProductService productService)
        {
            _orderService = orderService;
            _userService = userService;
            _productService = productService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var orders = await _orderService.GetAllOrder();
                var users = await _userService.GetUsers();
                var products = await _productService.GetAllProduct();

                // Calculate dashboard stats
                TotalOrders = orders.Count();
                TotalUsers = users.Count();
                TotalProducts = products.Count();
                Revenue = orders.Sum(o => o.TotalAmount); // Changed from TotalPrice to TotalAmount

                // Get recent orders
                RecentOrders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new RecentOrderViewModel
                    {
                        OrderId = o.OrderId.ToString(), // Convert Guid to string
                        OrderDate = o.OrderDate,
                        Status = GetPaymentStatus(o.PaymentMethod) // Use PaymentMethod instead of Status
                    })
                    .ToList();

                // Get top products
                TopProducts = products
                    .Take(5) // In a real app, you would calculate the top products by order count
                    .Select(p => new TopProductViewModel
                    {
                        Name = p.ProductName,
                        Category = GetCategoryName(p.CategoryID),
                        // Get price from the first product size if available
                        Price = p.ProductSizes?.FirstOrDefault()?.Price ?? 0,
                        OrderCount = new Random().Next(5, 50) // Simulated data
                    })
                    .ToList();

                // Generate monthly order data (simulated)
                MonthlyOrderData = GenerateMonthlyOrderData();

                return Page();
            }
            catch (Exception)
            {
                // Initialize with default values in case of error
                TotalOrders = 0;
                TotalUsers = 0;
                TotalProducts = 0;
                Revenue = 0;
                
                return Page();
            }
        }

        private string GetPaymentStatus(string paymentMethod)
        {
            return paymentMethod switch
            {
                "Cash" => "Completed",
                "Credit Card" => "Completed",
                "PayPal" => "Completed",
                _ => "Processing"
            };
        }

        private string GetCategoryName(int categoryId)
        {
            return categoryId switch
            {
                1 => "Coffee",
                2 => "Tea",
                3 => "Pastry",
                _ => "Other"
            };
        }

        private int[] GenerateMonthlyOrderData()
        {
            // Simulated data for the chart
            var random = new Random();
            var data = new int[12];
            for (int i = 0; i < 12; i++)
            {
                data[i] = random.Next(5, 30);
            }
            return data;
        }
    }

    public class RecentOrderViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopProductViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int OrderCount { get; set; }
    }
}
