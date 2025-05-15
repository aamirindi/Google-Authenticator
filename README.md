# ASP.NET Core MVC - Google Authenticator Integration 🔐

This repository contains a simple implementation of **Google Authenticator-based two-factor authentication (2FA)** using **ASP.NET Core MVC**. The project demonstrates how to generate and verify Time-based One-Time Passwords (TOTP) for secure user login.

## 📸 Screenshots

| Login Page | OTP Generation | OTP Verification |
|------------|----------------|------------------|
| ![Login](wwwroot/login.png) | ![Generate OTP](wwwroot/generateOtp.png) | ![Verify OTP](wwwroot/verify.png) |

## 📽️ Demo Video

A full walkthrough video of the implementation will be uploaded soon. Stay tuned!

## 🛠️ Features

- User login with Google Authenticator-based OTP verification
- QR code generation for Google Authenticator setup
- TOTP generation and validation using `OATH` standard
- Secure and simple 2FA mechanism

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio Code or any C#-compatible IDE
- Google Authenticator app (iOS/Android)

### Clone the repository

```bash
git clone https://github.com/aamirindi/Google-Authenticator.git
cd Google-Authenticator
dotnet watch run
