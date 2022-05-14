namespace PartialMockExample; 

public class FooBar {
	private int GetValue(int a, int b) {
		return a + b;
	}


	public int BusinessLogicMethod(int inValue) {
		var x = inValue * 2;
		var temp = GetValue(inValue, x);
		return temp;
	}
}