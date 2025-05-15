using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GoogleAuthenticator.Models;
using OtpNet;
using QRCoder;

namespace GoogleAuthenticator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    // 1. install 2 packages -- dotnet add package Otp.NET --version 1.4.0 & dotnet add package QRCoder --version 1.6.0 (these are stable versions)
    // 2. create action of Login
    // login
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(IFormCollection fc)
    {
        // setting a deafault email and pass for testing
        if (fc["email"].Equals("aamirindi@gmail.com") && fc["password"].Equals("admin"))
        {
            // setting the session
            HttpContext.Session.SetString("tempId", fc["email"]);

            // Generate a unique secret and store it
            byte[] secretKeyByte = KeyGeneration.GenerateRandomKey(20); // using for random key generation

            // convert it to string format (base32)
            string base32Secret = Base32Encoding.ToString(secretKeyByte);

            // Store it in session to use for otp
            HttpContext.Session.SetString("secretKey", base32Secret);

            // redirect to Generate QR code page
            return RedirectToAction("GenerateQRCode");

        }
        else
        {
            // else return the empty view or show some error using the toastr
            TempData["error"] = "Login Failed";
            return View();
        }
    }

    // 3. GenerateQRCode page
    public IActionResult GenerateQRCode()
    {
        // first we need to get the email and secret key from session
        string tempId = HttpContext.Session.GetString("tempId");
        string base32Secret = HttpContext.Session.GetString("secretKey");

        // if the session is null the return to login page
        if (string.IsNullOrEmpty(tempId) || string.IsNullOrEmpty(base32Secret))
        {
            return RedirectToAction("Login");
        }

        // set the issuer name 
        string issuer = "Calisthenics";

        // then we will use these to make a special URL that works with google authenticator
        string otpPathUrl = $"otpauth://totp/{issuer}:{tempId}?secret={base32Secret}&issuer{issuer}";

        // create a QR code from that URL
        using var qrGenerator = new QRCodeGenerator();

        using var qrCodeData = qrGenerator.CreateQrCode(otpPathUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new SvgQRCode(qrCodeData);

        // turns it into an svg -- for scalable image
        string svgImage = qrCode.GetGraphic(5);

        // send it to the view for display
        ViewBag.QrCodeSvg = svgImage;
        return View();
    }


    // 4. verify page
    public IActionResult Verify()
    {

        // if the session is not there then redirect to Login page
        if (HttpContext.Session.GetString("tempId") == null)
        {
            return RedirectToAction("Login");
        }
        else
        {
            return View();
        }
    }

    // 5. Vertiy the otp
    [HttpPost]
    public IActionResult Verify(IFormCollection fc)
    {
        // first we need to get the email and secret key from session
        string tempId = HttpContext.Session.GetString("tempId");
        string base32Secret = HttpContext.Session.GetString("secretKey");
        var token = fc["passToken"];

        // check if anything is missing. If yes, go back to login
        if (string.IsNullOrEmpty(tempId) || string.IsNullOrEmpty(base32Secret) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        try
        {
            // convert the secret key back to byte array
            byte[] secretKey = Base32Encoding.ToBytes(base32Secret);

            // create a Totp (time-based otp) object
            var totp = new Totp(secretKey);

            // (bool) ---> we are using boolean, because the result will be either true (valid) or false (invalid)
            // (isValid) ---> we are storing that result in a variable called isValid
            // (VerifyTotp) ---> VerifyTotp is a method that checks if the otp Code is correct
            // (token) ---> this is the otp code the use typed in 6 digits, we are passing it into the method to verify it
            // (out long timeStepMatched) ---> out means this variable will get a valuen from the method, it tell you at what time step the token matched
            // (VerificationWindow) ---> google authenticator codes are time based (they change every 30 sec). If the user's phone time is little head or behind, we allow a 'window' of time steps. (2,2) means accepts 2 steps before and 2 steps after the current time (so we are allowing about +60 & -60 sec in either direction)
            bool isValid = totp.VerifyTotp(token, out long timeStepMatched, new VerificationWindow(2, 2));

            if (isValid)
            {
                // true ---> to home page
                HttpContext.Session.SetString("id", tempId);
                return RedirectToAction("Index");
            }
            else
            {
                // false ---> verify page
                TempData["error"] = "Invalid OTP, Please try again";
                return RedirectToAction("Verify");
            }
        }
        catch (Exception ex)
        {
            // logging the error
            _logger.LogError(ex, "Otp verification failed");
            TempData["error"] = "something went wrong.try again";
            return RedirectToAction("verify");
        }
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
