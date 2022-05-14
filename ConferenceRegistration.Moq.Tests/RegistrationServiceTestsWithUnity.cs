using System;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using Unity;

namespace ConferenceRegistration.Moq.Tests {
	[TestFixture]
	public class RegistrationServiceTestsWithUnity {
		[OneTimeSetUp]
		public void FixtureSetup() {
			//called once before any tests are ran

			//these object will only be created once for the entire test class

			//we will be using all run time configuration with code in this class, so we will not need to load a configuration file
			_container = new UnityContainer();

			var feeCalculator = new Mock<IFeeCalculator>();
			feeCalculator.Setup(f => f.CalculateFee()).Returns(100);
			_container.RegisterInstance(feeCalculator.Object);
		}

		[SetUp]
		public void Setup() {
			//called before every test is ran

			//notice that we are creating a new payment processor Mock for every test.  If we didn't, each call to the Setup extension method in each
			//test would just add to playback list of the mock object.  Since each test only makes one call to the object, we want to reset that list each time, 
			//so we generate a new mock.
			_paymentProcessor = new Mock<IPaymentProcessor>();
			_container.RegisterInstance(_paymentProcessor.Object);
		}

		private IUnityContainer _container;
		private Mock<IPaymentProcessor> _paymentProcessor;

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved() {
			//setup the paymentProcessor to return true
			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			_container.RegisterInstance(new Mock<IEmailSender>().Object);

			//create the mock for the repository.  After we need to setup what we expect to be called on the mock.
			var repository = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			repository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

			_container.RegisterInstance(repository.Object);

			var registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			repository.VerifyAll();
		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException() {
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Setup(f => f.Process()).Returns(false);

			_container.RegisterInstance(new Mock<IEmailSender>().Object);

			_container.RegisterInstance(new Mock<IRegistrationRepository>().Object);

			var registrationService = _container.Resolve<RegistrationService>();
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSuccedes_VerifyRepositoryAndEmailServiceCalled() {
			//This shows creating more than one mock and establishing more than one expectation that will be verified

			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			//create a repository so that a batch record and verify can be done
			var mockRepository = new MockRepository(MockBehavior.Strict);

			//create mocks for both repository and emailSender
			var repository = mockRepository.Create<IRegistrationRepository>();
			var emailSender = mockRepository.Create<IEmailSender>();

			_container.RegisterInstance(repository.Object);
			_container.RegisterInstance(emailSender.Object);

			emailSender.Setup(e => e.SendMail(It.IsAny<MailMessage>()));
			repository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

			var registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//this verifies that the recorded expectations were called.
			//comment out one of the expectations in the using statement above and see how it will fail this test because a method was called on a mock that wasn't expected.
			mockRepository.VerifyAll();
		}
	}
}