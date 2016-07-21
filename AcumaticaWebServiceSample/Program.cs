using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using AcumaticaWebServiceSample.TEST;

namespace AcumaticaWebServiceSample
{
    class Program
    {

        static bool invalid = false;
        static void Main()
        {
            int option;
            Console.WriteLine("Select an option you want to do:");
            Console.WriteLine("1. Create Customer");
            Console.WriteLine("2. Get Customer Info");
            Console.Write("Enter option: ");
            int.TryParse(Console.ReadLine(), out option);
            switch (option)
            {
                case 1:
                    string custId, custName, email, address1, address2, city;
                    Console.Write("Customer ID: ");
                    custId = Console.ReadLine();
                    Console.Write("Customer Name: ");
                    custName = Console.ReadLine();
                    Console.Write("Email: ");
                    email = Console.ReadLine();
                    Console.Write("Address 1: ");
                    address1 = Console.ReadLine();
                    Console.Write("Address 2: ");
                    address2 = Console.ReadLine();
                    Console.Write("City: ");
                    city = Console.ReadLine();
                    if (custId.Contains(" "))
                    {
                        Console.WriteLine("Customer ID should have no spaces.");
                    }
                    else if (!IsValidEmail(email))
                    {
                        Console.WriteLine("Email is in invalid format.");
                    }
                    else
                    {
                        CreateCustomer(custId, custName, email, address1, address2, city);
                    }
                    Console.WriteLine(System.Environment.NewLine);
                    Main();
                    break;
                case 2:
                    custId = "";
                    Console.Write("Customer ID: ");
                    custId = Console.ReadLine();
                    Console.WriteLine("Getting information...");
                    Console.WriteLine("================================");
                    GetCustomer(custId);
                    Console.WriteLine(System.Environment.NewLine);
                    Main();
                    break;
                case 3:
                    Environment.Exit(0);
                    break;
                case 0:
                    Console.WriteLine("Value entered not in options.");
                    Console.WriteLine(System.Environment.NewLine);
                    Main();
                    break;
            }
        }    
        

        #region Utilities
        static public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper);
            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                   RegexOptions.IgnoreCase);
        }

        static private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
        #endregion

        #region WebServiceConn

        static void CreateCustomer(string CustomerID, string customerName, string email, string address1, string address2, string city)
        {
            TEST.Screen context = new TEST.Screen();

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/APITEST.asmx";
                LoginResult result = context.Login("admin", "123");

                AR303000Content schema = context.AR303000GetSchema();
                //Create series of commands
                var commands = new Command[]
                {
                    new Value
                    {
                        Value = CustomerID,
                        LinkedCommand = schema.CustomerSummary.CustomerID
                    },
                    new Value
                    {
                        Value = customerName,
                        LinkedCommand = schema.CustomerSummary.CustomerName
                    },
                    new Value
                    {
                        Value = email,
                        LinkedCommand = schema.GeneralInfoMainContact.Email
                    },
                    new Value
                    {
                        Value = "Address 1",
                        LinkedCommand = schema.GeneralInfoMainAddress.AddressLine1
                    },
                    new Value
                    {
                        Value = "Address 2",
                        LinkedCommand = schema.GeneralInfoMainAddress.AddressLine2
                    },
                    new Value
                    {
                        Value = "New York",
                        LinkedCommand = schema.GeneralInfoMainAddress.City
                    },
                    schema.Actions.Save, //Saves Changes
                    schema.CustomerSummary.CustomerID, //Gets Generated Customer ID
                    schema.GeneralInfoFinancialSettings.CustomerClass //Gets generated customer Class
                };
                schema = context.AR303000Submit(commands)[0];
                Console.WriteLine("Created Customer:" + schema.CustomerSummary.CustomerID.Value.ToString());
                Console.WriteLine("Under Customer Class:" + schema.GeneralInfoFinancialSettings.CustomerClass.Value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            finally
            {
                //declare logout to terminate cookie session
                context.Logout();
            }
        }

        static void GetCustomer(string customerID)
        {
            TEST.Screen context = new Screen();

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/APITEST.asmx";
                LoginResult result = context.Login("admin", "123");

                AR303000Content custSchema = context.AR303000GetSchema();

                var commands = new Command[]
                {
                    //Select the needed customer
                    new Value
                    {
                        Value = customerID,
                        LinkedCommand = custSchema.CustomerSummary.CustomerID
                    },
                    //Get the values of the needed elements
                    //customer summary
                    custSchema.CustomerSummary.CustomerID,
                    custSchema.CustomerSummary.CustomerName,
                    //General Info tab, Financial Settings
                    custSchema.GeneralInfoFinancialSettings.CustomerClass,
                    //General Info tab, Main Contact
                    custSchema.GeneralInfoMainContact.Email,
                    custSchema.GeneralInfoMainContact.Phone1,
                    //General Info tab, Main Address
                    custSchema.GeneralInfoMainAddress.AddressLine1,
                    custSchema.GeneralInfoMainAddress.AddressLine2,
                    custSchema.GeneralInfoMainAddress.City,
                    custSchema.GeneralInfoMainAddress.State,
                    custSchema.GeneralInfoMainAddress.PostalCode
                };

                string[][] customerData = context.AR303000Export(commands, null, 0, true, false);

                for (int i = 0; i < customerData.Length; i++)
                {
                    for (int x = 0; x < customerData[i].Length; x++)
                    {
                        Console.Write(customerData[i][x] + ": ");
                        Console.Write(customerData[i + 1][x]);
                        Console.WriteLine();
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                context.Logout();
            }
        }


        #endregion

    }
}
