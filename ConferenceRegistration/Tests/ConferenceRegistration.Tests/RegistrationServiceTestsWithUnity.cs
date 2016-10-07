using System;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConferenceRegistration.RhinoMocks.Tests {
	[TestFixture]
	public class RegistrationServiceTestsWithUnity {

		IUnityContainer _container;
		IPaymentProcessor _paymentProcessor;

		[OneTimeSetUp]
		public void FixtureSetup() {
			//called once before any tests are ran

			//these object will only be created once for the entire test class

			//we will be using all run time configuration with code in this class, so we will not need to load a configuration file
			_container = new UnityContainer();

			IFeeCalculator feeCalculator = MockRepository.GenerateStub<IFeeCalculator>();
			feeCalculator.Stub(f => f.CalculateFee()).Return(100);
			_container.RegisterInstance<IFeeCalculator>(feeCalculator);


		}

		[SetUp]
		public void Setup() {
			//called before every test is ran

			//notice that we are creating a new payment processor stub for every test.  If we didn't, each call to the Stub extension method in each
			//test would just add to playback list of the stub object.  Since each test only makes one call to the object, we want to reset that list each time, 
			//so we generate a new mock.
			_paymentProcessor = MockRepository.GenerateStub<IPaymentProcessor>();
			_container.RegisterInstance<IPaymentProcessor>(_paymentProcessor);

		}


		[Test]
		public void RegisterForConference_PaymentProcessingSucceeds_EntitySaved() {
			//setup the paymentProcessor to return true
			_paymentProcessor.Stub(f => f.Process()).Return(true);

			_container.RegisterInstance<IEmailSender>(MockRepository.GenerateStub<IEmailSender>());

			//create the mock for the repository.  After we need to setup what we expect to be called on the mock.
			IRegistrationRepository repository = MockRepository.GenerateStrictMock<IRegistrationRepository>();
			repository.Expect(e => e.SaveRegistration(null)).IgnoreArguments();
			//must put the mock into replay mode  The mock cannot be used by the object under test until this is done.
			repository.Replay();

			_container.RegisterInstance<IRegistrationRepository>(repository);

			RegistrationService registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");


			repository.VerifyAllExpectations();

		}

		[Test]
		public void RegisterForConference_PaymentProcessingFails_ThrowsException() {
			//setup payment processor to return false so that we can test that an exception is thrown when processing fails 
			//IPaymentProcessor paymentProcessor = MockRepository.GenerateStub<IPaymentProcessor>();
			_paymentProcessor.Stub(f => f.Process()).Return(false);

			_container.RegisterInstance<IEmailSender>(MockRepository.GenerateStub<IEmailSender>());

			//generating a stub instead of a mock because we don't expect to actually call the repo
			_container.RegisterInstance<IRegistrationRepository>(MockRepository.GenerateStub<IRegistrationRepository>());

			RegistrationService registrationService = _container.Resolve<RegistrationService>();
			Assert.Throws<ApplicationException>(() => registrationService.RegisterForConference("Joe", "smith", "joe.smith@abc.com"));
		}

		[Test]
		public void RegisterForConference_PaymentProcessingSuccedes_VerifyRepositoryAndEmailServiceCalled() {
			//This shows creating more than one mock and establishing more than one expection that will be verified

			_paymentProcessor.Stub(f => f.Process()).Return(true);

			//create a repository so that a batch record and verify can be done
			MockRepository mockRepository = new MockRepository();

			//create mocks for both repository and emailSender
			IRegistrationRepository repository = mockRepository.StrictMock<IRegistrationRepository>();
			IEmailSender emailSender = mockRepository.StrictMock<IEmailSender>();

			_container.RegisterInstance(repository);
			_container.RegisterInstance(emailSender);

			//record the expectations  Placing the record in a using statement automatically places the Mock Repository and all objects in replay mode
			//after the using block

			//Notice the order of the expectations is not the same as the order the statements are called in the RegisterForConference method.  
			//by default mocks are in an unordered state.  It is possible to record expectations that will fail if not called in the correct order.
			using (mockRepository.Record()) {
				emailSender.Expect(e => e.SendMail(null)).IgnoreArguments();
				repository.Expect(e => e.SaveRegistration(null)).IgnoreArguments();
			}


			RegistrationService registrationService = _container.Resolve<RegistrationService>();
			registrationService.RegisterForConference("Joe", "Smith", "joe.smith@abc.com");

			//this verifies that the recorded expectations were called.
			//comment out one of the expectations in the using statement above and see how it will fail this test because a method was called on a mock that wasn't expected.
			mockRepository.VerifyAll();

		}

	}
}
