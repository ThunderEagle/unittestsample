using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace ConferenceRegistration {
	public class RegistrationService : IRegistrationService
    {
        private readonly IFeeCalculator _feeCalculator;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IRegistrationRepository _repository;
        private readonly IEmailSender _emailSender;

		public RegistrationService(IFeeCalculator feeCalculator, IPaymentProcessor paymentProcessor, IRegistrationRepository repository, IEmailSender emailSender) {
			_feeCalculator = feeCalculator;
			_paymentProcessor = paymentProcessor;
			_repository = repository;
			_emailSender = emailSender;
		}

		public void RegisterForConference(string firstName, string lastName, string emailAddress) {
			//create new entity
			var entity = new RegistrationEntity() { EmailAddress = emailAddress, FirstName = firstName, LastName = lastName };

			//calculate the fee
			entity.InvoiceAmount = _feeCalculator.CalculateFee();

			//process the payment
			if( _paymentProcessor.Process()) {
				_repository.SaveRegistration(entity);
				MailMessage mail = new MailMessage(@"conference@usergroup.org", entity.EmailAddress, "Conference Registration", "You are now registered");
				_emailSender.SendMail(mail);
			}
			else {
				throw new ApplicationException("Payment processing failed!");
			}

		}
	}
}
