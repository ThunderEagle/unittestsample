using System.Net.Mail;

namespace ConferenceRegistration {
	public interface IEmailSender {
		void SendMail(MailMessage message);
	}
}