using Microsoft.AspNetCore.Mvc;
using PayWall.NetCore.Models.Request;
using PayWall.NetCore.Services;
using PayWallDemo.Models;

namespace PayWallDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPaywallService _paywallService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPaywallService paywallService, ILogger<HomeController> logger)
        {
            _paywallService = paywallService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View(new PaymentModel());
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Payment", model);
                }

                // Direct Payment Request (2D)
                var paymentRequest = new DirectPaymentRequest
                {
                    CardHolderName = model.CardHolderName,
                    CardNumber = model.CardNumber,
                    ExpireMonth = model.ExpiryMonth,
                    ExpireYear = model.ExpiryYear,
                    Cvv = model.Cvv,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    Installment = model.Installment,
                    OrderId = DateTime.Now.Ticks.ToString(), // Generate a unique order ID
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    ReturnUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme),
                    Description = "Test Payment"
                };

                var response = await _paywallService.DirectPaymentAsync(paymentRequest);

                if (response.IsSuccess)
                {
                    return View("Success", response);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Payment failed");
                    return View("Payment", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your payment.");
                return View("Payment", model);
            }
        }

        public async Task<IActionResult> CheckInstallments(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 6)
            {
                return Json(new { success = false, message = "Invalid card number" });
            }

            try
            {
                var binRequest = new BinLookupRequest
                {
                    CardNumber = cardNumber
                };

                var binResponse = await _paywallService.BinLookupAsync(binRequest);

                if (binResponse.IsSuccess)
                {
                    var installmentRequest = new InstallmentLookupRequest
                    {
                        CardNumber = cardNumber,
                        Amount = 100 // Sample amount
                    };

                    var installmentResponse = await _paywallService.InstallmentLookupAsync(installmentRequest);

                    return Json(new
                    {
                        success = true,
                        binInfo = binResponse.Result,
                        installments = installmentResponse.Result
                    });
                }
                else
                {
                    return Json(new { success = false, message = binResponse.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking installments");
                return Json(new { success = false, message = "An error occurred while checking installments." });
            }
        }

        public IActionResult PaymentResult(string paymentId, string status)
        {
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

                // 3D Secure Payment Request
                var paymentRequest = new SecurePaymentRequest
                {
                    CardHolderName = model.CardHolderName,
                    CardNumber = model.CardNumber,
                    ExpireMonth = model.ExpiryMonth,
                    ExpireYear = model.ExpiryYear,
                    Cvv = model.Cvv,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    Installment = model.Installment,
                    OrderId = DateTime.Now.Ticks.ToString(), // Generate a unique order ID
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    ReturnUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme),
                    Description = "Test 3D Payment"
                };

                var response = await _paywallService.SecurePaymentAsync(paymentRequest);

                if (response.IsSuccess && !string.IsNullOrEmpty(response.Result?.HtmlContent))
                {
                    // Return the 3D Secure HTML content to redirect the user to the bank's verification page
                    return Content(response.Result.HtmlContent, "text/html");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "3D payment initialization failed");
                    return View("Payment", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing 3D payment");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your 3D secure payment.");
                return View("Payment", model);
            }
        }
    }
}