using System;
using System.Net.Mail;
using Moq;
using NUnit.Framework;

namespace ConferenceRegistration.Moq.Tests
{
	[TestFixture]
	class RegistrationServiceTests
	{
		Mock<IFeeCalculator> _feeCalculator;
		Mock<IEmailSender> _emailSender;
		Mock<IPaymentProcessor> _paymentProcessor;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			//called once before any tests are ran
		}

		[SetUp]
		public void Setup()
		{
			//called before every test is ran

			_feeCalculator = new Mock<IFeeCalculator>();
			_feeCalculator.Setup(f => f.CalculateFee()).Returns(100);

			//for the emailSender stub, we only need the object created, we don't need to return a specific value, because the method is a void
			//a mock object is used for the emailSender in one of the tests.
			_emailSender = new Mock<IEmailSender>();

			//we will define return values in each test for this stub.
			_paymentProcessor = new Mock<IPaymentProcessor>();
		}

		[Test]
		public void MethodName_StateUnderTest_ExpectedBehavior()
		{
			var paymentProcessor = new Mock<IPaymentProcessor>();

			//The SetupSequence is used to establish different return values in order for multiple calls to the Mock
			paymentProcessor.SetupSequence(f => f.Process())
				.Returns(true)
				.Returns(false);

			bool first = paymentProcessor.Object.Process();
			Assert.IsTrue(first);
			bool second = paymentProcessor.Object.Process();
			Assert.IsFalse(second);
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved()
		{
			//setup the paymentProcessor to return true
			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			//create the mock for the repository.  After we need to setup what we expect to be called on the mock.
			var repository = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			repository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

			RegistrationService registrationService = new RegistrationService(_feeCalculator.Object, _paymentProcessor.Object, repository.Object, _emailSender.Object);
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			repository.VerifyAll();
		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException()
		{
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Setup(f => f.Process()).Returns(false);

			var repo = new Mock<IRegistrationRepository>();

			RegistrationService registrationService = new RegistrationService(_feeCalculator.Object, _paymentProcessor.Object, repo.Object, _emailSender.Object);
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_VerifyRepositoryAndEmailServiceCalled()
		{
			//This shows creating more than one mock and establishing more than one expection that will be verified
			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			//create a repository so that a batch record and verify can be done
			MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

			//create mocks for both repository and emailSender
			var repository = mockRepository.Create<IRegistrationRepository>();
			var emailSender = mockRepository.Create<IEmailSender>();

			//Moq doesn't have the concept of ordered expectations
			emailSender.Setup(e => e.SendMail(It.IsAny<MailMessage>()));
			repository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

			RegistrationService registrationService = new RegistrationService(_feeCalculator.Object, _paymentProcessor.Object, repository.Object, emailSender.Object);
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//this verifies that the recorded expectations were called.
			//comment out one of the expectations in the using statement above and see how it will fail this test because a method was called on a mock that wasn't expected.
			mockRepository.VerifyAll();
		}
	}
}