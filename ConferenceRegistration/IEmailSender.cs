using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace ConferenceRegistration {
	public interface IEmailSender {
		void SendMail(MailMessage message);
	}
}
