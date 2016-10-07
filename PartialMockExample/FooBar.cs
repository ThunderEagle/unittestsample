using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PartialMockExample
{
	public class FooBar
	{
		private int GetValue(int a, int b)
		{
			return a + b;
		}


		public int BusinessLogicMethod(int inValue)
		{
			var x = inValue * 2;
			var temp = GetValue(inValue, x);
			return temp;
		}
	}
}
