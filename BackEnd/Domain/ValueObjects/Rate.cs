using OnlineShop.Domain.Common;


namespace Domain.ValueObjects
{
    public sealed class RateValue
    {
        public int Value { get; }

        private RateValue(int value)
        {
            Value = value;
        }

        public static RateValue Create(int value)
        {
            if (value < 1 || value > 5)
                throw new DomainException("امتیاز باید بین ۱ تا ۵ باشد.");

            return new RateValue(value);
        }
    }
}
