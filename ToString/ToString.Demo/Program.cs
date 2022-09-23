using System;

namespace ToString.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Address address = new Address() { City = "Žilina", Street = "A. Rudnaya" };
            Console.WriteLine(address);
        }
    }

    //[ToString]
    public partial class Address
    {
        public string City { get; set; }

        public string Street { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        //public override string ToString() => $"City = {City}, Street = {Street}, ZipCode = {ZipCode}, Country = {Country}";
    }
}
