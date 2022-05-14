using System;
using System.Net.Mail;
using NSubstitute;
using NUnit.Framework;
using Unity;

namespace ConferenceRegistration.NSubstitute.Tests {
	[TestFixture]
	public class RegistrationServiceTestsWithUnity {
		[OneTimeSetUp]
		public void FixtureSetup() {
			//called once before any tests are ran

			//these object will only be created once for the entire test class

			//we will be using all run time configuration with code in this class, so we will not need to load a configuration file
			_container = new UnityContainer();

			var feeCalculator = Substitute.For<IFeeCalculator>();
			feeCalculator.CalculateFee().Returns(100);
			_container.RegisterInstance(feeCalculator);
		}

		[SetUp]
		public void Setup() {
			//called before every test is ran

			//notice that we are creating a new payment processor stub for every test.  If we didn't, each call to the Stub extension method in each
			//test would just add to playback list of the stub object.  Since each test only makes one call to the object, we want to reset that list each time, 
			//so we generate a Substitute.For.
			_paymentProcessor = Substitute.For<IPaymentProcessor>();
			_container.RegisterInstance(_paymentProcessor);
		}

		private IUnityContainer _container;
		private IPaymentProcessor _paymentProcessor;

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved() {
			//setup the paymentProcessor to return true
			_paymentProcessor.Process().Returns(true);

			_container.RegisterInstance(Substitute.For<IEmailSender>());

			//create the mock for the repository.  After we need to setup what we expect to be called on the mock.
			var repository = Substitute.For<IRegistrationRepository>();

			_container.RegisterInstance(repository);

			var registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			repository.Received().SaveRegistration(Arg.Any<RegistrationEntity>());
		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException() {
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Process().Returns(false);

			_container.RegisterInstance(Substitute.For<IEmailSender>());
			_container.RegisterInstance(Substitute.For<IRegistrationRepository>());

			var registrationService = _container.Resolve<RegistrationService>();
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSuccedes_VerifyRepositoryAndEmailServiceCalled() {
			//This shows creating more than one mock and establishing more than one expectation that will be verified

			_paymentProcessor.Process().Returns(true);

			var repository = Substitute.For<IRegistrationRepository>();
			var emailSender = Substitute.For<IEmailSender>();

			_container.RegisterInstance(repository);
			_container.RegisterInstance(emailSender);

			var registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//change the order of the statements below to see the effects on the test
			Received.InOrder(() => {
				                 repository.SaveRegistration(Arg.Any<RegistrationEntity>());
				                 emailSender.SendMail(Arg.Any<MailMessage>());
			                 });
		}
	}
}