using System;
using System.Net.Mail;
using NSubstitute;
using NUnit.Framework;

namespace ConferenceRegistration.NSubstitute.Tests
{
	[TestFixture]
	class RegistrationServiceTests
	{
		private IFeeCalculator _feeCalculator;
		private IEmailSender _emailSender;
		private IPaymentProcessor _paymentProcessor;
        private IRegistrationRepository _repository;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			//called once before any tests are ran
		}

		[SetUp]
		public void Setup()
		{
			//called before every test is ran

			_feeCalculator = Substitute.For<IFeeCalculator>();
			_feeCalculator.CalculateFee().Returns(100);

			_emailSender = Substitute.For<IEmailSender>();

			//we will define return values in each test for this stub.
			_paymentProcessor = Substitute.For<IPaymentProcessor>();

            _repository = Substitute.For<IRegistrationRepository>();
        }

        public IRegistrationService GetSubjectUnderTest()
        {
			return new RegistrationService(_feeCalculator, _paymentProcessor,_repository, _emailSender);
        }

		[Test]
		public void MethodName_StateUnderTest_ExpectedBehavior()
		{
			IPaymentProcessor paymentProcessor = Substitute.For<IPaymentProcessor>();

			paymentProcessor.Process().Returns(true, false);
			bool first = paymentProcessor.Process();
			Assert.IsTrue(first);
			bool second = paymentProcessor.Process();
			Assert.IsFalse(second);
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved()
		{
			//setup the paymentProcessor to return true
			_paymentProcessor.Process().Returns(true);


            var registrationService = GetSubjectUnderTest();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			_repository.ReceivedWithAnyArgs().SaveRegistration(null);
		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException()
		{
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Process().Returns(false);

            var registrationService = GetSubjectUnderTest();
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_VerifyRepositoryAndEmailServiceCalled()
		{
			//This shows creating more than one mock and establishing more than one expectation that will be verified
			_paymentProcessor.Process().Returns(true);


            var registrationService = GetSubjectUnderTest();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//change the order of the statements below to see the effects on the test
			Received.InOrder(() =>
			{
				_repository.SaveRegistration(Arg.Any<RegistrationEntity>());
				_emailSender.SendMail(Arg.Any<MailMessage>());
			});
		}
	}
}