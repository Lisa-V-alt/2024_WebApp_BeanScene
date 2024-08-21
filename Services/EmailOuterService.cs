using MailKit.Security;
using MimeKit.Text;
using MimeKit;

namespace BeanScene.Services
{
    public class EmailOuterService
    {
        public EmailInnerService Inner { get; set; }

        // Constructor to establish link between
        // instance of Outer_class to its
        // instance of the Inner_class
        public EmailOuterService()
        {
            this.Inner = new EmailInnerService(this);
        }

        public class EmailInnerService
        {
            private EmailOuterService obj;
            public EmailInnerService()
            {
                //????? i don't know what this does
            }
            public EmailInnerService(EmailOuterService outer)
            {

                this.obj = outer;
            }

            public bool SendEmailConfirmation(string emailTo, string lastName, string userName, string password, string confirmationLink)
            {
                var email = new MimeMessage();
                // Using Gmail (this is my gmail account..... haha, add your own)
                email.From.Add(MailboxAddress.Parse("bubbbblerat@gmail.com"));

                email.To.Add(MailboxAddress.Parse(emailTo));
                email.Subject = "Registration Confirmation";
                email.Body = new TextPart(TextFormat.Plain) { Text = "Dear " + lastName + ",\n" + "You have successfully registered to the RetailWebApp. \n Your user name: " + userName + " and password: " + password + "\n Kind Regards\n RetailWebApp Service Team" };

                // send email
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                // Using Gmail -- this is MY ACCOUNT! aDD YOUR OWN HERE 
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("bubbbblerat@gmail.com", "cfotdcggnvvyexmn");

                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
        }

        public bool SendEmailConfirmation(string emailTo, string lastName, string userName, string password, string confirmationLink)
        {
            return Inner.SendEmailConfirmation(emailTo, lastName, userName, password, confirmationLink);
        }
    }
}
