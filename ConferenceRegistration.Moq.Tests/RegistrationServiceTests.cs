using System;
using System.Net.Mail;
using Moq;
using NUnit.Framework;

namespace ConferenceRegistration.Moq.Tests
{
	[TestFixture]
	class RegistrationServiceTests
	{
		private Mock<IFeeCalculator> _feeCalculator;
		private Mock<IEmailSender> _emailSender;
		private Mock<IPaymentProcessor> _paymentProcessor;
        private Mock<IRegistrationRepository> _registrationRepository;

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
			_registrationRepository = new Mock<IRegistrationRepository>(MockBehavior.Strict);
		}

        private IRegistrationService GetSubjectUnderTest()
        {
			return new RegistrationService(_feeCalculator.Object, _paymentProcessor.Object, _registrationRepository.Object, _emailSender.Object);
        }

		[Test]
		public void MethodName_StateUnderTest_ExpectedBehavior()
		{
			//The SetupSequence is used to establish different return values in order for multiple calls to the Mock
			_paymentProcessor.SetupSequence(f => f.Process())
				.Returns(true)
				.Returns(false);

			bool first = _paymentProcessor.Object.Process();
			Assert.IsTrue(first);
			bool second = _paymentProcessor.Object.Process();
			Assert.IsFalse(second);
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved()
		{
			//setup the paymentProcessor to return true
			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			//we need to setup what we expect to be called on the mock.
			_registrationRepository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

            var registrationService = GetSubjectUnderTest();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

            _registrationRepository.VerifyAll();
		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException()
		{
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Setup(f => f.Process()).Returns(false);
            var registrationService = GetSubjectUnderTest();
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_VerifyRepositoryAndEmailServiceCalled()
		{
			//This shows creating more than one mock and establishing more than one expectation that will be verified
			_paymentProcessor.Setup(f => f.Process()).Returns(true);

			//create a repository so that a batch record and verify can be done
			MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

			//create mocks for both repository and emailSender
			var repository = mockRepository.Create<IRegistrationRepository>();
			var emailSender = mockRepository.Create<IEmailSender>();

			//Moq doesn't have the concept of ordered expectations
			emailSender.Setup(e => e.SendMail(It.IsAny<MailMessage>()));
			repository.Setup(e => e.SaveRegistration(It.IsAny<RegistrationEntity>()));

			var registrationService = new RegistrationService(_feeCalculator.Object, _paymentProcessor.Object, repository.Object, emailSender.Object);
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//this verifies that the recorded expectations were called.
			//comment out one of the expectations above and see how it will fail this test because a method was called on a mock that wasn't expected.
			mockRepository.VerifyAll();
		}
	}
}