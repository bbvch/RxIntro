namespace Bbv.Rx.Destruction
{
    public class BallisticParameters
    {
        public BallisticParameters(double power, double elevation)
        {
            this.Power = power;
            this.Elevation = elevation;
        }

        public double Power { get; }

        public double Elevation { get; }
    }
}