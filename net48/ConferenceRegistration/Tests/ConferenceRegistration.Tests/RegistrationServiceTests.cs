using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConferenceRegistration.RhinoMocks.Tests
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

			//Stubs are used to return canned responses, we do not need to verify methods have been called with specific parameters.
			_feeCalculator = MockRepository.GenerateStub<IFeeCalculator>();
			_feeCalculator.Stub(f => f.CalculateFee()).Return(100);

			//for the emailSender stub, we only need the object created, we don't need to return a specific value, because the method is a void
			//a mock object is used for the emailSender in one of the tests.
			_emailSender = MockRepository.GenerateStub<IEmailSender>();


			//we will define return values in each test for this stub.
			_paymentProcessor = MockRepository.GenerateStub<IPaymentProcessor>();

            _repository = MockRepository.GenerateMock<IRegistrationRepository>();

        }

        private IRegistrationService GetSubjectUnderTest()
        {
			return new RegistrationService(_feeCalculator,_paymentProcessor,_repository,_emailSender);
        }

		[Test]
		public void MethodName_StateUnderTest_ExpectedBehavior()
		{
			IPaymentProcessor paymentProcessor = MockRepository.GenerateStub<IPaymentProcessor>();

			paymentProcessor.Stub(f => f.Process()).Return(true).Repeat.Once();
			paymentProcessor.Stub(f => f.Process()).Return(false).Repeat.Once();

			bool first = paymentProcessor.Process();
			Assert.IsTrue(first);
			bool second = paymentProcessor.Process();
			Assert.IsFalse(second);
		}


		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved()
		{
			//setup the paymentProcessor to return true
			_paymentProcessor.Stub(f => f.Process()).Return(true);

			//create the mock for the repository.  After we need to setup what we expect to be called on the mock.
			IRegistrationRepository repository = MockRepository.GenerateStrictMock<IRegistrationRepository>();
			repository.Expect(e => e.SaveRegistration(null)).IgnoreArguments();

			RegistrationService registrationService = new RegistrationService(_feeCalculator, _paymentProcessor, repository, _emailSender);
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			repository.VerifyAllExpectations();

		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException()
		{
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			_paymentProcessor.Stub(f => f.Process()).Return(false);

			//generating a stub instead of a mock because we don't expect to actually call the repo
			IRegistrationRepository repo = MockRepository.GenerateStub<IRegistrationRepository>();

			RegistrationService registrationService = new RegistrationService(_feeCalculator, _paymentProcessor, repo, _emailSender);
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_VerifyRepositoryAndEmailServiceCalled()
		{
			//This shows creating more than one mock and establishing more than one expection that will be verified

			_paymentProcessor.Stub(f => f.Process()).Return(true);

			//create a repository so that a batch record and verify can be done
			MockRepository mockRepository = new MockRepository();

			//create mocks for both repository and emailSender
			IRegistrationRepository repository = mockRepository.StrictMock<IRegistrationRepository>();
			IEmailSender emailSender = mockRepository.StrictMock<IEmailSender>();

			//record the expectations  Placing the record in a using statement automatically places the Mock Repository and all objects in replay mode
			//after the using block

			//Notice the order of the expectations is not the same as the order the statements are called in the RegisterForConference method.  
			//by default mocks are in an unordered state.  It is possible to record expectations that will fail if not called in the correct order.
			using(mockRepository.Record())
			{
				emailSender.Expect(e => e.SendMail(null)).IgnoreArguments();
				repository.Expect(e => e.SaveRegistration(null)).IgnoreArguments();
			}


			RegistrationService registrationService = new RegistrationService(_feeCalculator, _paymentProcessor, repository, emailSender);
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//this verifies that the recorded expectations were called.
			//comment out one of the expectations in the using statement above and see how it will fail this test because a method was called on a mock that wasn't expected.
			mockRepository.VerifyAll();

		}
	}
}
