using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NationalParkWebApp_1003.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;

namespace NationalParkWebApp_1003.Utility
{
    public class NotificationSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _config;

        public NotificationSender(IOptions<EmailSettings> emailSettings, IConfiguration config)
        {
            _emailSettings = emailSettings.Value;
            _config = config;
        }

        // 1. Booking Confirmed Email
        public async System.Threading.Tasks.Task SendBookingEmailAsync(string toEmail, string customerName, string parkName, string visitDate, decimal amount, string transactionId)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, "National Park Booking"),
                Subject = $"Booking Confirmed - {parkName}",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #28a745;'>Booking Confirmed!</h2>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        <p>Aapki National Park visit appointment successfully book ho gayi hai.</p>
                        <hr/>
                        <p><strong>National Park:</strong> {parkName}</p>
                        <p><strong>Visit Date:</strong> {visitDate}</p>
                        <p><strong>Total Amount:</strong> ₹{amount}</p>
                        <p><strong>Payment Transaction ID:</strong> {transactionId}</p>
                        <hr/>
                        <p style='color: #666; font-size: 12px;'>Thank you for choosing National Park Services.</p>
                    </div>",
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort)
            {
                Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        // 2. Cancellation OTP Email
        public async System.Threading.Tasks.Task SendOtpEmailAsync(string toEmail, string customerName, string otp)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, "National Park Booking Support"),
                Subject = "Cancellation Request - Verification OTP",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #dc3545;'>Booking Cancellation Verification</h2>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        <p>Aapne apni booking cancel karne ke liye request ki hai. Verification ke liye aapka OTP niche diya gaya hai:</p>
                        <div style='background: #f8f9fa; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #333;'>
                            {otp}
                        </div>
                        <p style='color: #888; font-size: 13px; margin-top: 15px;'>Yeh OTP agle 5 minute tak valid hai. Agar yeh request aapne nahi ki hai toh is email ko ignore karein.</p>
                    </div>",
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort)
            {
                Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        // 3. Cancellation Success Email
        public async System.Threading.Tasks.Task SendCancellationSuccessEmailAsync(string toEmail, string customerName, int bookingId)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, "National Park Booking Support"),
                Subject = $"Booking Cancelled - #{bookingId}",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #6c757d;'>Booking Cancelled Successfully</h2>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        <p>Aapki Booking ID <strong>#{bookingId}</strong> successfully cancel kar di gayi hai.</p>
                        <p>Agar aapka refund banta hai, toh woh aapke source account me 5-7 working days me credit ho jayega.</p>
                    </div>",
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort)
            {
                Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        // 4. SMS & Call (Twilio)
        public void SendBookingSms(string toPhone, string customerName, string parkName, string visitDate)
        {
            InitTwilio();

            string fromPhone = _config["TwilioSettings:PhoneNumber"]!;
            string formattedPhone = FormatPhoneNumber(toPhone);

            MessageResource.Create(
                body: $"Hello {customerName}, your National Park booking for {parkName} on {visitDate} is CONFIRMED. Thank you!",
                from: new PhoneNumber(fromPhone),
                to: new PhoneNumber(formattedPhone)
            );
        }

        public void SendCancellationOtpSms(string toPhone, string customerName, string otp)
        {
            InitTwilio();

            string fromPhone = _config["TwilioSettings:PhoneNumber"]!;
            string formattedPhone = FormatPhoneNumber(toPhone);

            MessageResource.Create(
                body: $"Hello {customerName}, your OTP for booking cancellation is: {otp}. Valid for 5 minutes.",
                from: new PhoneNumber(fromPhone),
                to: new PhoneNumber(formattedPhone)
            );
        }

        public void SendCancellationSms(string toPhone, string customerName, int bookingId, string parkName)
        {
            InitTwilio();

            string fromPhone = _config["TwilioSettings:PhoneNumber"]!;
            string formattedPhone = FormatPhoneNumber(toPhone);

            MessageResource.Create(
                body: $"Hello {customerName}, your order successfully canceled. Booking ID: #{bookingId}, National Park: {parkName}.",
                from: new PhoneNumber(fromPhone),
                to: new PhoneNumber(formattedPhone)
            );
        }

        public void MakeBookingCall(string toPhone, string customerName, string parkName, string visitDate)
        {
            InitTwilio();

            string fromPhone = _config["TwilioSettings:PhoneNumber"]!;
            string formattedPhone = FormatPhoneNumber(toPhone);

            var voiceResponse = new VoiceResponse();
            voiceResponse.Say(
                $"Hello {customerName}. Namaste. Aapka National Park appointment for {parkName} on {visitDate} successfully confirm ho gaya hai. Have a wonderful visit. Thank you!",
                language: Twilio.TwiML.Voice.Say.LanguageEnum.EnIn,
                voice: Twilio.TwiML.Voice.Say.VoiceEnum.PollyAditi
            );

            CallResource.Create(
                twiml: new Twiml(voiceResponse.ToString()),
                from: new PhoneNumber(fromPhone),
                to: new PhoneNumber(formattedPhone)
            );
        }

        private void InitTwilio()
        {
            var sid = _config["TwilioSettings:AccountSid"];
            var token = _config["TwilioSettings:AuthToken"];
            TwilioClient.Init(sid, token);
        }

        private string FormatPhoneNumber(string phone)
        {
            phone = phone.Trim().Replace(" ", "").Replace("-", "");
            if (!phone.StartsWith("+"))
            {
                return "+91" + phone;
            }
            return phone;
        }
    }
}