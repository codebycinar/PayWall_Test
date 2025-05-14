using Microsoft.AspNetCore.Mvc;
using PayWall.NetCore.Extensions;
using PayWall.NetCore.Models.Abstraction;
using PayWall.NetCore.Models.Request.Payment;
using PayWall.NetCore.Services;
using PayWallDemo.Models;

namespace PayWallDemo.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayWallService _paywallService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PayWallService paywallService, ILogger<PaymentController> logger)
        {
            _paywallService = paywallService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new PaymentModel());
        }

        [HttpPost]
        public async Task<IActionResult> Process3D(PaymentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                // Test için örnek kart bilgileri
                var paymentRequest = new Payment3DRequest
                {
                    Card = new Card
                    {
                        OwnerName = model.CardHolderName,
                        Number = model.CardNumber,
                        ExpireMonth = model.ExpiryMonth,
                        ExpireYear = model.ExpiryYear,
                        Cvv = model.Cvv
                    },
                    Customer = new Customer
                    {
                        Address = "bla bla bla",
                        City = "Istanbul",
                        Country = "Turkey",
                        Email = "hus.cinar@gmail.com",
                        FullName = model.CardHolderName,
                        IdentityNumber = "12345678901",
                        Phone = "05555555555",
                        TaxNumber = "1234567890",
                    },
                    PaymentDetail = new Payment3DRequestDetail
                    {
                        Amount = model.Amount,
                        ChannelId = (int)Channel.Web,
                        ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                        CurrencyId = (short)Currency.Try,
                        MerchantFailBackUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme),
                        MerchantSuccessBackUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme)
                    },
                    Products = new List<Products>
                     {
                         new Products
                         {
                              ProductAmount = model.Amount,
                               ProductName = "Test Product",
                         }
                     }
                };

                _logger.LogInformation("3D Payment request initiating: " + System.Text.Json.JsonSerializer.Serialize(paymentRequest));

                var response = await _paywallService.Payment.StartThreeDAsync(paymentRequest);

                if (response.Result)
                {
                    _logger.LogInformation("3D Payment initiated successfully");
                    return Content(response.Body.ToString(), "text/html");
                }
                else
                {
                    _logger.LogError("3D Payment initialization failed: " + response.Body?.Message);
                    ModelState.AddModelError(string.Empty, response.Body?.Message ?? "Payment failed");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing 3D payment");
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                return View("Index", model);
            }
        }

        public IActionResult Result(string paymentId, string status)
        {
            _logger.LogInformation($"3D Payment result: ID={paymentId}, Status={status}");
            
            if (status == "success")
            {
                return View("Success", new { PaymentId = paymentId });
            }
            else
            {
                return View("Failure", new { PaymentId = paymentId });
            }
        }
    }
}