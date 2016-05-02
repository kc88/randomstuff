class LargeNumberLowPrecision { //playing loose and fast with the rules of mathematics
	int exponent { public get; private set; }
	double value { public get; private set; }
	
	private static final int precision = 10;
	
	public LNLP (e, v){
		exponent, value = e, v;
		while value > 10 {
			value /= 10;
			exponent++;
		}
		while value < 1 {
			value *= 10;
			exponent--;
		}
		//above scales value to 1.00 thru 9.99 and changes exponent to fit
		value = Math.Round(value, precision-1); //round to precision-1 decimal places. docs https://msdn.microsoft.com/en-us/library/75ks3aby(v=vs.110).aspx
	}
	
	public static Multiply (LNLP a, LNLP b) {
		return new LNLP (a.exponent + b.exponent, a.value * b.value); //constructor clamps value between 1 and 9.99 if product of values is higher
	}
	
	public static Divide (a, b) {
		if (b.exponent - precision > a.exponent) return new LNLP (0, 0); //dividing a small number by a very large number is approximately zero, check value before continuing with arithmetic
		return new LNLP(a.exponent - b.exponent, a.value / b.value); //constructor will scale if the value is less than one
	}
	
	public static Add (LNLP a, b) {
		LNLP high = a.exponent > b.exponent ? a : b;
		LNLP low = a.exponent > b.exponent ? b : a;
		
		if (high.exponent - precision > low.exponent)
			return new LNLP(high.exponent, high.value);
		
		int magnitude_dif = high.exponent - low.exponent;
		LNLP low_scaled = new LNLP(high.exponent, low.value / pow(10, magnitude_dif));
	}
	
	public static Subtract (a, b) {
		return Add(a, new LNLP(b.exponent, -1*b.value));
	}
	
	public static Pow(a, b) {
		product = a;
		for (pow_iterator = 0; pow_iterator < b; pow_iterator++) { //i name all my iterators uniquely, not just i, because of bad experiences with javascript's shitty scope
			product = LNLP.Multiply(product, b); //the equation to do this non-iteratively is hard. i assume you wont ever be doing this anyways because this returns absolutely gigantic results
			//ie: (2 * 10^2) ^ (3 * 10^1) = 1.073 * 10^69, and this only gets more ridiculous
			//be careful of the exponent overflowing int, high roller
		}
		return product;
		
		//return new LNLP(a.exponent*b.value*pow(10, b.exponent), pow(a.value, b.value * pow(10, b.exponent)); 
		//shortform that may or may not be right?? this will possibly take even longer due to the constructor clamping
	}
	
}