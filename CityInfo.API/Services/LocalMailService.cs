namespace CityInfo.API.Services
{
    //to add an interface, hghlight the LocalMailService, then right-click & choose Quick Actions &Refactorings-->extract to Interface
    public class LocalMailService : IMailService
    {
        //Initialize these fields, to be sure the values are coming from the appsettings.json
        //Make them readonly to prevent them from being changed
        private readonly string _mailTo = string.Empty;
        private readonly string _mailFrom = string.Empty;

        //inject the IConfig service to access it from LocalMailService
        public LocalMailService(IConfiguration configuration)
        {
            //read the 2 values from appsettings.json and assign them through the indexer and pass the key:value pair
            //access is done by heirarchy: mailSettings:...
            //The keys are case-insensitive
            _mailTo = configuration["mailSettings:mailToAddress"];
            _mailFrom = configuration["mailSettings:mailFromAddress"];
        }

        public void Send(string subject, string message)
        {
            // To send mail - output to console window
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(LocalMailService)}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
