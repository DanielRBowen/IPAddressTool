using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;

namespace Learning
{
    class Program
    {
        static void Main(string[] args)
        {
            string addressCIDR = "192.168.2.76/28";

            Console.WriteLine("Address in CIDR notation: {0}", Regex.Match(addressCIDR, @"(\b\d{1,3}\.){3}\d{1,3}(\/\d{1,2}$)").ToString());

            //Check if the CIDR address is correct
            if (!Regex.IsMatch(addressCIDR, @"(\b\d{1,3}\.){3}\d{1,3}(\/\d{1,2}$)"))
            {
                throw new Exception("CIDR Address is wrong format");
            }

            string[] addressPrefix = Regex.Split(addressCIDR, @"\/");
            string[] addressArr = Regex.Split(addressPrefix[0], @"\.");

            //convert the addresses to binary strings
            List<int> addressListInt = new List<int>();
            for (int index = 0; index < addressArr.Length; index++)
            {
                int number;
                if (int.TryParse(addressArr[index], out number))
                {
                    addressListInt.Add(number);
                }
            }
            string[] addressDottedBinary = ConvertToBinaryStrings(addressListInt.ToArray());

            //Add zeros to each address in the dotted binary.
            for (int index = 0; index < addressDottedBinary.Length; index++)
            {
                string address = addressDottedBinary[index];
                if (address.Length == 0 || address.Length < 8)
                {
                    address = AddZerosMakeByte(address);
                    addressDottedBinary[index] = address;
                }
            }

            // Get the Address Prefix
            int prefix = 0;
            int.TryParse(addressPrefix[1], out prefix);

            //The whole address in binary
            string addressBinary = addressDottedBinary[0] + addressDottedBinary[1] + addressDottedBinary[2] + addressDottedBinary[3];

            // The network address is n bits where n is the prefix of CIDR notation
            var networkAddress = addressBinary.Substring(0, prefix);

            // The host address is (32 - n)
            var hostAddress = addressBinary.Substring(prefix, 32 - prefix);

            // The network mask is 1's for each bit n times with a trail of zeros at the end
            var networkMask = "";
            for (int index = 0; index < prefix; index++)
            {
                networkMask += "1";
            }
            for (int index = prefix; index < 32; index++)
            {
                networkMask += "0";
            }

            // Broadcast address is (32-n) all 1's
            var broadcastAddress = "";
            for (int index = 0; index < prefix; index++)
            {
                broadcastAddress += "0";
            }
            for (int index = prefix; index < 32; index++)
            {
                broadcastAddress += "1";
            }

            // The First Address is (any address) (bitwise AND) (network mask)
            int firstAddressInt = BinaryStringToInt(addressBinary) & BinaryStringToInt(networkMask);
            string firstAddressBinary = Convert.ToString(firstAddressInt, 2);

            // The Last Address is (any address) (bitwise OR) [NOT(network mask)]
            int lastAddressInt = BinaryStringToInt(addressBinary) | ~BinaryStringToInt(networkMask);
            string lastAddressBinary = Convert.ToString(lastAddressInt, 2);

            // The total addresses is 2 ^ (32-n)
            int totalAddresses = (int)Math.Pow(2, (32 - prefix));

            // The number of usable addresses is 
            int usableAddresses = (32 - prefix) + 1 - 2;

            //Total addresses granted is (32-n in decimal) + 1 - 2

            //prefix length for number of subnets is ?
            // subnet prefix = prefix + log base 2 (number of subnets (power of 2))
            // Read for subnets
            Console.Write("Enter the number of subnets: ");
            int subnets = 0;
            int.TryParse(Console.ReadLine(), out subnets);
            int subnetPrefixBits = 0;
            if (subnets > 0)
            {
                subnetPrefixBits = (int)(prefix + Math.Log(subnets, 2));
            }
            else if (subnets != 0)
            {
                throw new Exception("subnets is not greater than 0 or not zero");
            }

            //Maximum number of subnets with given subnet prefix length? Total addresses within the subnets (machines?)
            // http://www.sosmath.com/algebra/logs/log4/log47/log47.html
            // subnetPrefix = prefix + log base 2 (number of subnets)
            // subnetPrefix - prefix = log base 2 (subnets)
            // 2 ^ (subnetPrefix - prefix) = subnets
            int subnetPrefixLength = 0;
            Console.Write("Enter the subnet prefix length: ");
            int.TryParse(Console.ReadLine(), out subnetPrefixLength);
            int maximumSubnets = (int)Math.Pow(2, (subnetPrefixLength - prefix));

            // The subnet broadcast address (32-n) bits to decimal + the zero (1) - 2 (first bit and last bit for special addresses)

            Console.WriteLine("Prefix: {0}", prefix.ToString());
            Console.WriteLine("Address in Binary: {0}", addressBinary);
            Console.WriteLine("Network Address: {0}", networkAddress);
            Console.WriteLine("Host Address: {0}", hostAddress);
            Console.WriteLine("Network Mask: {0}", networkMask);
            Console.WriteLine("Broadcast Address: {0}", broadcastAddress);
            Console.WriteLine("First Address: {0}", firstAddressBinary);
            Console.WriteLine("Last Address: {0}", lastAddressBinary);
            Console.WriteLine("Total Addresses: {0}", totalAddresses);
            Console.WriteLine("Usable Addresses: {0}", usableAddresses);
            Console.WriteLine("Subnet Prefix Length: {0}", subnetPrefixBits);

            Console.ReadKey();
        }


        /// <summary>
        /// Converts an array of integers to binary strings
        /// </summary>
        /// <param name="array">An array of integers.</param>
        /// <returns>Array of strings that are in binary.</returns>
        public static string[] ConvertToBinaryStrings(int[] array)
        {
            int arrayLength = array.Length;
            string[] result = new string[arrayLength];
            for (int index = 0; index < arrayLength; ++index)
            {
                result[index] = Convert.ToString(array[index], 2);
            }
            return result;
        }

        /// <summary>
        /// Converts a binary represented as a string to a Decimal represented as an int
        /// </summary>
        /// <param name="binaryString">A binary number as a string</param>
        /// <returns>A decimal number as an int converted from the binary</returns>
        public static int BinaryStringToInt(string binaryString)
        {
            int result = 0;
            for (int index = 0; index < binaryString.Length; index++)
            {
                char binaryStringIndex = binaryString[index];
                if (binaryStringIndex == '1')
                {
                    int intAdd = 0;
                    int.TryParse(Math.Pow(2, index).ToString(), out intAdd);
                    result += intAdd;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds zeros to a binary number in a string.
        /// </summary>
        /// <param name="binaryNumber">A binary number in string format</param>
        /// <returns>A byte binary number with leading zeros</returns>
        public static string AddZerosMakeByte(string binaryNumber)
        {
            //https://msdn.microsoft.com/en-us/library/system.collections.bitarray.aspx
            //int i = int.Parse("FFFFFF", System.Globalization.NumberStyles.HexNumber);
            //string s = i.ToString("x");
            //int i = 0xFFFFFF;

            //Check if it fits within 0xFF, 255
            if (binaryNumber.Length == 0 || binaryNumber.Length > 8 || !Regex.IsMatch(binaryNumber, @"\b[0-1]{1,8}"))
            {
                throw new Exception("invalid binaryNumber");
            }
            else
            {
                for (int index = binaryNumber.Length; index < 8; index++)
                {
                    binaryNumber = binaryNumber.Insert(0, "0");
                }
            }

            return binaryNumber;
        }
        public static void AppendZerosToBeginningOfEachWord(string[] array, int finalWordLength)
        {
            int arrayLength = array.Length;
            for (int index = 0; index < arrayLength; index++)
            {
                string word = array[index];
                int wordLength = word.Length;
                if (wordLength > finalWordLength)
                {
                    throw new Exception("The finalWordLength must be greater than the wordLength of each word in the array.");
                }
                while (wordLength != finalWordLength)
                {
                    word = word.Insert(0, "0");
                    wordLength++;
                }
                array[index] = word;
            }
        }

        /// <summary>
        /// Do Something        
        /// </summary>
        /// <returns></returns>
        static public int DoSomething()
        {
            return 0;
        }
    }
}
