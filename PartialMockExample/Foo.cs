using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PartialMockExample
{
	public class Foo
	{

		public virtual int MyProp
		{
			get { return 5; }
		}

		public virtual void DoSomething()
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
		}


		public virtual bool IsValid()
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			return false;
		}


		public int GetValue(int a, int b)
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			return a + b;
		}

		internal virtual int GetValue2(int a, int b)
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			return a + b;
		}



		public virtual int BusinessLogicMethod(int inValue)
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			var x = inValue * 2;
			var temp = GetValue2(inValue, x);
			return  temp;
		}


		public virtual bool TryParse(string input, out int value)
		{

			return int.TryParse(input, out value);

		}

		public virtual void Bar(out bool cancel)
		{
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			cancel = false;
		}


		public virtual bool MultiPartial(out int count)
		{
			//Console.WriteLine("Value of Count {0}", count);
			Console.WriteLine("{0}-REAL CODE", MethodBase.GetCurrentMethod().Name);
			count = 3;
			return true;
		}

	}
}
