using System;
using System.Text.RegularExpressions;


[Serializable]
class LNLP :IComparable<LNLP>
{ //playing loose and fast with the rules of mathematics //LongNumberLowPrecision, you may wish to refactor this
  //fyi does not support negative numbers - things will crash
    public int exponent { get; private set; }
    public double val { get; private set; }

    private const int precision = 10;

    public LNLP(double v) : this(0, v)
    {
    }

    public LNLP(int e, double v)
    {
        if (v < 0) throw new System.ArgumentException("LNLP value cannot be negative", "v");
        exponent = e;
        val = v;
        while (val > 10)
        {
            val /= 10;
            exponent++;
        }
        while (val < 1)
        {
            val *= 10;
            exponent--;
        }
        //might have rounding errors for numbers that are very far off from the 1-10 scale before clamping
        //above scales value to 1.00 thru 9.99 and changes exponent to fit
        //val = Math.Round(value, precision-1); //round to precision-1 decimal places. docs https://msdn.microsoft.com/en-us/library/75ks3aby(v=vs.110).aspx

        val *= Math.Pow(10, precision - 1);
        val = Math.Round(val);
        val /= Math.Pow(10, precision - 1);
    }

    public static LNLP Multiply(LNLP a, LNLP b)
    {
        return new LNLP(a.exponent + b.exponent, a.val * b.val); //constructor clamps value between 1 and 9.99 if product of values is higher
    }

    public static LNLP Divide(LNLP a, LNLP b)
    {
        if (b.exponent - precision > a.exponent) return new LNLP(0, 0); //dividing a small number by a very large number is approximately zero, check value before continuing with arithmetic
        return new LNLP(a.exponent - b.exponent, a.val / b.val); //constructor will scale if the value is less than one
    }

    public static LNLP Add(LNLP a, LNLP b)
    {
        LNLP high = a.exponent > b.exponent ? a : b;
        LNLP low = a.exponent > b.exponent ? b : a;

        if (high.exponent - precision > low.exponent)
            return new LNLP(high.exponent, high.val); //adding a tiny number to a large number is approx the large number

        int magnitude_dif = high.exponent - low.exponent;
        return new LNLP(high.exponent, high.val + low.val / Math.Pow(10, magnitude_dif));
    }

    public static LNLP Subtract(LNLP a, LNLP b)
    {
        if (b.exponent > a.exponent) return new LNLP(0, 0.0);//no negative numbers please

        LNLP high = a.exponent > b.exponent ? a : b;
        LNLP low = a.exponent > b.exponent ? b : a;

        if (high.exponent - precision > low.exponent)
            return new LNLP(high.exponent, high.val);

        int magnitude_dif = high.exponent - low.exponent;
        return new LNLP(high.exponent, high.val - low.val / Math.Pow(10, magnitude_dif));
    }
    /*
	public static LNLP Pow(LNLP a, LNLP b) {
		LNLP product = a;
		for (int pow_iterator = 0; pow_iterator < b; pow_iterator++) { //i name all my iterators uniquely, not just i, because of bad experiences with javascript's shitty scope
			product = LNLP.Multiply(product, b); //the equation to do this non-iteratively is hard. i assume you wont ever be doing this anyways because this returns absolutely gigantic results
			//ie: (2 * 10^2) ^ (3 * 10^1) = 1.073 * 10^69, and this only gets more ridiculous
			//be careful of the exponent overflowing int, high roller
		}
		return product;
		
		//return new LNLP(a.exponent*b.value*pow(10, b.exponent), pow(a.value, b.value * pow(10, b.exponent)); 
		//shortform that may or may not be right?? this will possibly take even longer due to the constructor clamping
	}*/
    public static LNLP Pow(LNLP a, double b)
    {
        double decExponent = b * a.exponent; //get the exponent, ill send you the math on this
        int expIpart = (int)Math.Floor(decExponent); //integral part of exponent
        double exponentFpart = decExponent - expIpart; //fractional part of exponent

        return new LNLP(expIpart, Math.Pow((double)a.val, b) * Math.Pow(10, exponentFpart));
    }
    public new string ToString()
    {
        return String.Format("{0:f} * 10 ^ {1:d}", val, exponent);
    }

    public static LNLP FromString(string s)
    {
        var reg = Regex.Match(s, "(?<v>\A\d\.\d*) \* 10 \^ (?<e>\d*\z)");
        return new LNLP(Int32.Parse(reg.Groups["e"].Value), Double.Parse(reg.Groups["v"].Value));
    }
    
    public override bool Equals(Object o)
    {
        if (!(o is LNLP))
        {
            return false;
        }
        LNLP b = (LNLP)o;
        return val == b.val && exponent == b.exponent;
    }

    public static LNLP operator +(LNLP a, LNLP b)
    {
        return Add(a, b);
    }
    public static LNLP operator -(LNLP a, LNLP b)
    {
        return Subtract(a, b);
    }
    public static LNLP operator *(LNLP a, LNLP b)
    {
        return Multiply(a, b);
    }
    public static LNLP operator /(LNLP a, LNLP b)
    {
        return Divide(a, b);
    }
    public static bool operator ==(LNLP a, LNLP b)
    {
        return a.Equals(b);
    }
    public static bool operator !=(LNLP a, LNLP b)
    {
        return !a.Equals(b);
    }

    public int CompareTo(LNLP b)
    {
        int valCompare = val.CompareTo(b.val);
        int expCompare = exponent.CompareTo(b.exponent);
        if (expCompare != 0) return expCompare;
        return valCompare;
    }

    public static bool operator >(LNLP a, LNLP b)
    {
        return a.CompareTo(b) > 0;
    }
    public static bool operator <(LNLP a, LNLP b)
    {
        return a.CompareTo(b) < 0;
    }

    public override int GetHashCode()
    {
        return exponent.GetHashCode() * val.GetHashCode();
    }

    public static implicit operator LNLP(int v)
    {
        return new LNLP((double)v);
    }
    public static implicit operator LNLP(float v)
    {
        return new LNLP(v);
    }
    public static explicit operator int(LNLP a)
    {
        return (int)a.val * (int)Math.Pow(10, a.exponent);
    }
    public static explicit operator float(LNLP a)
    {
        return (float)a.val * (float)Math.Pow(10, a.exponent);
    }
}
