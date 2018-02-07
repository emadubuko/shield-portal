namespace DQI.DAL.Entity
{
    public class WeeklyResultData
    {
        public virtual string ProcessIndicator { get; set; }
        public virtual Weeks TheWeek { get; set; }
        public virtual int Numerator { get; set; }
        public virtual int Denominator { get; set; }
        public virtual string Concurrence
        {
            get
            {
                if (Denominator == 0) return "";
                return (100 * 1.0 * (Numerator / Denominator)).ToString("N2") + "%";
            }
        }
    }
     

    public enum Weeks
    {
        Week1 = 1,
        Week2 = 2,
        Week3 = 3,
        Week4 = 4,
        Week5 = 5,
        Week6 = 6,
        Week7 = 7,
        Week8 = 8,
        Week9 = 9,
        Week10 = 10,
        Week11 = 11,
        Week12 = 12
    }
}
