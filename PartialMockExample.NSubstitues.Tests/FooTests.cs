using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace PartialMockExample.NSubstitue.Tests
{
	[TestFixture]
	public class FooTests
	{

		[Test]
		public void RealCodeExecuting()
		{
			var foo = new Foo();

			foo.DoSomething();
			foo.IsValid();
			foo.GetValue(2, 2);
			foo.GetValue2(2, 2);
			foo.BusinessLogicMethod(2);

		}

		[Test]
		public void PartialMock_StillRealCode()
		{

			var foo = Substitute.ForPartsOf<Foo>();

			foo.DoSomething();
			foo.IsValid();
			foo.GetValue(2, 2);
			foo.GetValue2(2, 2);
			foo.BusinessLogicMethod(2);

		}

		[Test]
		public void StubAVoidMethod()
		{

			var foo = Substitute.ForPartsOf<Foo>();

			foo.When(f => f.DoSomething()).DoNotCallBase();
			foo.DoSomething();

			foo.IsValid();

		}

		[Test]
		public void StubVirual()
		{
			var foo = Substitute.ForPartsOf<Foo>();
			foo.When(f => f.IsValid()).DoNotCallBase();
			foo.IsValid().Returns(true);
			var actual = foo.IsValid();
			Assert.IsTrue(actual);
		}

		[Test]
		public void StubNonVirual()
		{
			//note, this won't work
			var foo = Substitute.ForPartsOf<Foo>();
			foo.GetValue(Arg.Is(2), Arg.Is(2)).Returns(10);
			foo.GetValue(2, 2);
		}

		[Test]
		public void StubInternalMethod()
		{
			var foo = Substitute.ForPartsOf<Foo>();

			//foo.GetValue2(Arg.Is(2), Arg.Is(4)).Returns(10);

			//foo.When(f => f.GetValue2(Arg.Any<int>(), Arg.Any<int>())).DoNotCallBase();
			foo.WhenForAnyArgs(f => f.GetValue2(0, 0)).DoNotCallBase();
			//foo.GetValue2(2, 2).Returns(10);
			foo.GetValue2(2, 4).Returns(10);

			var actual = foo.BusinessLogicMethod(2);
			Assert.AreEqual(10, actual);
		}



		[Test]
		public void TryParseSub()
		{
			var foo = Substitute.ForPartsOf<Foo>();
			int result;
			foo.WhenForAnyArgs(f => f.TryParse(string.Empty, out result)).DoNotCallBase();

			foo.TryParse("3", out result).Returns(x =>
			{
				x[1] = 5;
				return true;
			});
		}

		[Test]
		public void DoIt()
		{

			var foo = Substitute.ForPartsOf<Foo>();
			bool cancel;

			foo.WhenForAnyArgs(f => f.Bar(out cancel)).DoNotCallBase();
			foo.WhenForAnyArgs(f => f.Bar(out cancel)).Do(x => { x[0] = true; });

			foo.Bar(out cancel);
			Assert.IsTrue(cancel);

		}

		[Test]
		public void DoIt2()
		{

			var foo = Substitute.ForPartsOf<Foo>();
			int count;
			

			foo.WhenForAnyArgs(f => f.MultiPartial(out count)).DoNotCallBase();
			//foo.When(f => f.MultiPartial(out count)).DoNotCallBase();
			//foo.When(f => f.MultiPartial(out count)).DoNotCallBase();
			
			foo.MultiPartial(out count).Returns(x =>
			{
				x[0] = 5;
				return false;
			});

			var actual = foo.MultiPartial(out count);
			var actual2 = foo.MultiPartial(out count);
			Assert.IsFalse(actual2);
			Assert.AreEqual(5, count);

		}



		[Test]
		public void ReadOnlyPropertyTest()
		{
			var foo = Substitute.ForPartsOf<Foo>();
			foo.When(x => { var temp = x.MyProp; }).DoNotCallBase();
		}

	}
}
