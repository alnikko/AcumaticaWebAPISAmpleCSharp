using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcumaticaWebServiceSample.TEST;

namespace AcumaticaWebServiceSample
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateCustomer();
        }

        static void CreateCustomer()
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
                        Value = "TEST1",
                        LinkedCommand = schema.CustomerSummary.CustomerID
                    },
                    new Value
                    {
                        Value = "Test customer",
                        LinkedCommand = schema.CustomerSummary.CustomerName
                    },
                    new Value
                    {
                        Value = "test@email.com",
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

    }
}
