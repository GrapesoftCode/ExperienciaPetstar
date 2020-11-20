using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using GP.Petstar.Web.Models;
using System.Net;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using GP.Petstar.Web.Util;

namespace GP.Petstar.Web
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            //return Task.FromResult(0);
            return ConfigSendGridAsync(message);
        }

        private Task ConfigSendGridAsync(IdentityMessage message)
        {
            #region formatter
            var link = message.Body;

            link = link.Replace("Please confirm your account by clicking <a href=\"", "");
            link = link.Replace("\">here</a>", "");

            Uri myUri = new Uri(link);
            //string host = myUri.Host;  // host is "www.contoso.com"

            var url = (myUri.IsDefaultPort) ? myUri.Host : myUri.Host + ":" + myUri.Port;

            String Msg_HTMLx = @"<html>
	                                <body>
                                        <a href='" + link + @"'>
                                            <img src = 'http://" + url + @"/images/1.jpg' class='image' style='max-width: 100%;' />
			                            </a>
	                                </body>
                                </html>";
            #endregion


            var emailControl = new EmailControl();

            var result = emailControl.SendMail(Properties.Settings.Default.Email_From, message.Destination, "Confirmación de email petstar", true, Msg_HTMLx, Properties.Settings.Default.Email_Host, Convert.ToInt32(Properties.Settings.Default.Email_Port), Properties.Settings.Default.Email_User, Properties.Settings.Default.Email_Pass, true, true);

            if (result.code != 0)
                throw new Exception("Error al enviar email.");
            
            //<host>smtp.gmail.com</host>
            //  <port>587</port>
            //  <user>tucuenta@gmail.com</user>
            //  <password>contraseñagmail</password>
            //  <enableSsl>true</enableSsl>
            //</configuration>

            //MailMessage msg = new MailMessage();
            //msg.From = new MailAddress("mercadomugauli@gmail.com");
            //msg.To.Add(new MailAddress(message.Destination));
            //msg.Subject = message.Subject;
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            //SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("mercadomugauli@gmail.com", "Mugauli160187.");
            //smtpClient.Credentials = credentials;
            //smtpClient.EnableSsl = true;
            //smtpClient.Send(msg);

            //MailMessage msg = new MailMessage();
            //msg.From = new MailAddress(Properties.Settings.Default.Email_From);
            //msg.To.Add(new MailAddress(message.Destination));
            //msg.Subject = message.Subject;
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            //SmtpClient smtpClient = new SmtpClient(Properties.Settings.Default.Email_Host, Convert.ToInt32(Properties.Settings.Default.Email_Port));
            //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("redessociales@petstar.mx", "PSTcomunicacion1");
            //smtpClient.Credentials = credentials;
            //smtpClient.EnableSsl = true;
            //smtpClient.Send(msg);

            //// Send the email.
            //if (transportWeb != null)
            //{
            //    return transportWeb.DeliverAsync(myMessage);
            //}
            //else
            //{
            return Task.FromResult(0);
            //}
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager which is used in this application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.  
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
