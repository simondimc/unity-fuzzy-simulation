using System.Collections.Generic;

public class Sample {

    public float[] x;
    public float[] y;

    public Sample(float[] x, float[] y) {
        this.x = x;
        this.y = y;
    }

    public Sample Copy() {
        float[] a = new float[this.x.Length];
        float[] b = new float[this.y.Length];
        this.x.CopyTo(a , 0);
        this.y.CopyTo(b , 0);
        return new Sample(a, b);
    }

}