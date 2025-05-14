using Microsoft.AspNetCore.Mvc;
using PayWall.NetCore.Models.Abstraction;
using PayWall.NetCore.Models.Request.Payment;
using PayWall.NetCore.Services;
using PayWallDemo.Models;

namespace PayWallDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly PayWallService _paywallService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(PayWallService paywallService, ILogger<HomeController> logger, IConfiguration config)
        {
            _paywallService = paywallService;
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View(new PaymentModel());
        }

        public IActionResult PaymentResult(string paymentId, string status)
        {
            _logger.LogInformation($"PaymentResult: PaymentId={paymentId}, Status={status}");

            // Handle 3D secure callback here
            if (status == "success")
            {
                return View("Success", new { PaymentId = paymentId });
            }
            else
            {
                return View("Failure", new { PaymentId = paymentId });
            }
        }

        // 3D Secure Payment Example
        [HttpPost]
        public async Task<IActionResult> Process3DPayment(PaymentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Payment", model);
                }

                var paymentRequest = new Payment3DRequest
                {
                    Card = new Card
                    {
                        OwnerName = model.CardHolderName,
                        Number = model.CardNumber,
                        ExpireMonth = model.ExpiryMonth,
                        ExpireYear = model.ExpiryYear,
                        Cvv = model.Cvv,
                        CardSave = new CardSave
                        {
                            Save = false,
                        },
                        UniqueCode = ""
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
                        MerchantSuccessBackUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme),
                        MerchantUniqueCode = Guid.NewGuid().ToString(),
                        Installment = 1
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

                //PayWall.NetCore.Services.PayWallService payWallService = new PayWallService()
                var response = await _paywallService.Payment.StartThreeDAsync(paymentRequest);

                if (response.Result)
                {
                    // Return the 3D Secure HTML content to redirect the user to the bank's verification page
                    return Redirect(response.Body.RedirectUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Body?.Message ?? "3D payment initialization failed");
                    return View("Payment", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing 3D payment");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your 3D secure payment: " + ex.Message);
                return View("Payment", model);
            }
        }
    }
}