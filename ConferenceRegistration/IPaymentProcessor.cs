﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConferenceRegistration {
	public interface IPaymentProcessor {
		bool Process();
	}
}
