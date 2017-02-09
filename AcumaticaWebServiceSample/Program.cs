using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using AcumaticaWebServiceSample.TEST;
using System.ServiceProcess;
using System.Diagnostics;

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
            Console.WriteLine("3. Create Sales Invoice");
            Console.WriteLine("4. Create Invoice to Payments");
            Console.WriteLine("5. Get List of all stock items.");
            Console.WriteLine("6. Get specific item via barcode.");
            
            Console.Write("Enter option: ");
            int.TryParse(Console.ReadLine(), out option);
            switch (option)
            {
                case 1:
                    string custId, custName, email, address1, address2, city, barcode;
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
                    CreateSalesOrder();
                    break;
                case 4:
                    Program foo = new Program();
                    foo.CreateSalesInvoiceToPayment();
                    break;
                case 5:
                    Console.WriteLine("Getting information...");
                    Console.WriteLine("================================");
                    GetAllStockItem();
                    Console.WriteLine(System.Environment.NewLine);
                    Main();
                    break;
                case 6:
                    barcode = "";
                    Console.Write("Barcode: ");
                    barcode = Console.ReadLine();
                    Console.WriteLine("Getting information...");
                    Console.WriteLine("================================");
                    findStockItem(barcode);
                    Console.WriteLine(System.Environment.NewLine);
                    Main();
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
            bool loginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin@SKYFIINTERNETSOLUTIONS", "Password@123");

                loginSuccess = true;

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
                if (loginSuccess)
                    context.Logout(); //declare logout to terminate cookie session
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
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
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

        static void CreateSalesOrder()
        {
            TEST.Screen context = new Screen();
            bool loginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin", "123");

                //If login is successful
                loginSuccess = true;

                SO301000Content schema = context.SO301000GetSchema();

                var commands = new Command[]
                {
                    //Assign Values
                    new Value
                    {
                        Value = "SO",
                        LinkedCommand = schema.OrderSummary.OrderType
                    },
                    new Value
                    {
                        Value = "<NEW>",
                        LinkedCommand = schema.OrderSummary.OrderNbr
                    },
                    new Value
                    {
                        Value = "TEST",
                        LinkedCommand = schema.OrderSummary.Customer
                    },
                    new Value
                    {
                        Value = "Test Sales Order",
                        LinkedCommand = schema.OrderSummary.Description
                    },
                    new Value
                    {
                        Value = "OPTIONAL",
                        LinkedCommand = schema.OrderSummary.CustomerOrder
                    },

                    //add new item in document details tab of sales order screen

                    //first item
                    schema.DocumentDetails.ServiceCommands.NewRow,
                    new Value
                    {
                        Value = "AALEGO500",
                        LinkedCommand = schema.DocumentDetails.InventoryID
                    },
                    new Value
                    {
                        Value = "4",
                        LinkedCommand = schema.DocumentDetails.Quantity
                    },
                    new Value
                    {
                        Value = "EA",
                        LinkedCommand = schema.DocumentDetails.UOM
                    },
                    new Value
                    {
                        Value = "120",
                        LinkedCommand = schema.DocumentDetails.UnitPrice,
                        Commit = true
                    },
                    new Value
                    {
                        Value = "2",
                        LinkedCommand = schema.DocumentDetails.DiscountPercent,
                        Commit = true
                    },

                    //second item
                    schema.DocumentDetails.ServiceCommands.NewRow,
                    new Value
                    {
                        Value = "CONGRILL",
                        LinkedCommand = schema.DocumentDetails.InventoryID
                    },
                    new Value
                    {
                        Value = "2",
                        LinkedCommand = schema.DocumentDetails.Quantity
                    },
                    new Value
                    {
                        Value = "EA",
                        LinkedCommand = schema.DocumentDetails.UOM
                    },

                    // Save Action
                    schema.Actions.Save,

                    // Request data to return after save
                    schema.OrderSummary.OrderType,
                    schema.OrderSummary.OrderNbr,
                    schema.OrderSummary.OrderedQty,
                    schema.OrderSummary.OrderTotal

                };

                schema = context.SO301000Submit(commands)[0];
                Console.WriteLine("Order Type: " + schema.OrderSummary.OrderType.Value.ToString());
                Console.WriteLine("Order Nbr: " + schema.OrderSummary.OrderNbr.Value.ToString());
                Console.WriteLine("Ordered Qty: " + schema.OrderSummary.OrderedQty.Value.ToString());
                Console.WriteLine("Order Total: " + schema.OrderSummary.OrderTotal.Value.ToString());
                Console.Read();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            finally
            {
                if (loginSuccess)
                    context.Logout();
            }
        }

        void CreateSalesInvoiceToPayment()
        {
            TEST.Screen context = new Screen();
            bool loginSuccess = false;
            string yesNo, invoiceType, invoiceNbr;
            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin", "123");

                //If login is successful
                loginSuccess = true;

                SO301000Content schema = context.SO301000GetSchema();

                var commands = new Command[]
                {
                    //Assign Values
                    new Value
                    {
                        Value = "IN",
                        LinkedCommand = schema.OrderSummary.OrderType
                    },
                    new Value
                    {
                        Value = "<NEW>",
                        LinkedCommand = schema.OrderSummary.OrderNbr
                    },
                    new Value
                    {
                        Value = "TEST",
                        LinkedCommand = schema.OrderSummary.Customer
                    },
                    new Value
                    {
                        Value = "Test Sales Order",
                        LinkedCommand = schema.OrderSummary.Description
                    },
                    new Value
                    {
                        Value = "OPTIONAL",
                        LinkedCommand = schema.OrderSummary.CustomerOrder
                    },
                    new Value
                    {
                        Value = "095135123",
                        LinkedCommand = schema.OrderSummary.ExternalReference
                    },

                    //add new item in document details tab of sales order screen

                    //first item
                    schema.DocumentDetails.ServiceCommands.NewRow,
                    new Value
                    {
                        Value = "AALEGO500",
                        LinkedCommand = schema.DocumentDetails.InventoryID
                    },
                    new Value
                    {
                        Value = "4",
                        LinkedCommand = schema.DocumentDetails.Quantity
                    },
                    new Value
                    {
                        Value = "EA",
                        LinkedCommand = schema.DocumentDetails.UOM
                    },
                    new Value
                    {
                        Value = "120",
                        LinkedCommand = schema.DocumentDetails.UnitPrice,
                        Commit = true
                    },
                    new Value
                    {
                        Value = "2",
                        LinkedCommand = schema.DocumentDetails.DiscountPercent,
                        Commit = true
                    },

                    //second item
                    schema.DocumentDetails.ServiceCommands.NewRow,
                    new Value
                    {
                        Value = "CONGRILL",
                        LinkedCommand = schema.DocumentDetails.InventoryID
                    },
                    new Value
                    {
                        Value = "2",
                        LinkedCommand = schema.DocumentDetails.Quantity
                    },
                    new Value
                    {
                        Value = "EA",
                        LinkedCommand = schema.DocumentDetails.UOM
                    },

                    // Save Action
                    //schema.Actions.Save,
                    schema.Actions.PrepareInvoice

                };

                context.SO301000Submit(commands);

                var status = context.SO301000GetProcessStatus();
                while (status.Status == TEST.ProcessStatus.InProcess)
                {
                    status = context.SO301000GetProcessStatus();
                }

                if (status.Status == TEST.ProcessStatus.Completed)
                {
                    commands = new Command[]
                    {
                        schema.Shipments.InvoiceType,
                        schema.Shipments.InvoiceNbr
                    };
                    var invoice = context.SO301000Submit(commands)[0];
                    invoiceType = invoice.Shipments.InvoiceType.Value.ToString();
                    invoiceNbr = invoice.Shipments.InvoiceNbr.Value.ToString();

                    Console.WriteLine("Creating Invoice...");
                    Console.Write(invoiceType);
                    Console.WriteLine(" : " + invoiceNbr);
                    Console.WriteLine("Releasing Invoice...");
                    //ReleaseSOInvoice(invoiceNbr, invoiceType);
                    
                    // Create Payment
                    Console.WriteLine("Auto Create Payment?");
                    Console.Write("Create Payment? (y/n)");
                    yesNo = Console.ReadLine();
                    if (yesNo == "y")
                    {
                        Console.WriteLine("Creating Payment...");
                        CreateReleasePayment(invoiceType, invoiceNbr);
                        Console.WriteLine("Payment Released");
                        Console.Write(" Want to Void Payment? (y/n)");
                        yesNo = Console.ReadLine();
                        if (yesNo == "y")
                        {
                            Console.WriteLine("Voiding Payment...");
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                EventLog.WriteEntry("Acumatica Web Service", ex.Message);
                Console.Read();
            }
            finally
            {
                if (loginSuccess)
                    context.Logout();
            }
        }

        //void ReleaseSOInvoice(string invoiceNbr, string invoiceType)
        //{
        //    TEST.Screen context = new TEST.Screen();
        //    bool loginSucess = false;

        //    try
        //    {
        //        context.CookieContainer = new System.Net.CookieContainer();
        //        context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
        //        context.Timeout = 10000;
        //        LoginResult result = context.Login("admin", "123");

        //        SO303000Content schema = context.SO303000GetSchema();
        //        var commands = new Command[]
        //        {
        //            new Value
        //            {
        //                Value = invoiceType,
        //                LinkedCommand = schema.InvoiceSummary.Type
        //            },
        //            new Value
        //            {
        //                Value = invoiceNbr,
        //                LinkedCommand = schema.InvoiceSummary.ReferenceNbr
        //            },
        //            schema.Actions.ReleaseAction
        //        };
        //        context.SO303000Submit(commands);

        //        var status = context.SO303000GetProcessStatus();
        //        while (status.Status == ProcessStatus.InProcess)
        //        {
        //            status = context.SO303000GetProcessStatus();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Console.Read();
        //    }
        //    finally
        //    {
        //        if (loginSucess)
        //            context.Logout();
        //    }
        //}

        void CreateReleasePayment(string invoiceType, string invoiceNbr)
        {
            TEST.Screen context = new Screen();
            bool isLoginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer();
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                context.Login("admin", "123");

                AR302000Content schema = context.AR302000GetSchema();
                var commands = new List<Command>();
                commands.Add(
                    new Value
                    {
                        Value = "Payment",
                        LinkedCommand = schema.PaymentSummary.Type
                    });
                commands.Add(schema.Actions.Insert);
                commands.Add(
                    new Value
                    {
                        Value = "TEST",
                        LinkedCommand = schema.PaymentSummary.Customer
                    });
                commands.Add(
                    new Value
                    {
                        Value = "CASH",
                        LinkedCommand = schema.PaymentSummary.PaymentMethod
                    });
                commands.Add(
                    new Value
                    {
                        Value = "0941231",
                        LinkedCommand = schema.PaymentSummary.PaymentRef
                    });
                commands.Add(
                    new Value
                    {
                        Value = "False",
                        LinkedCommand = schema.PaymentSummary.Hold,
                        Commit = true
                    });

                //add invoice
                commands.Add(schema.DocumentsToApply.ServiceCommands.NewRow);
                commands.Add(
                    new Value
                    {
                        Value = invoiceNbr,
                        LinkedCommand = schema.DocumentsToApply.ReferenceNbr,
                        Commit = true
                    });
                commands.Add(schema.PaymentSummary.AppliedToDocuments);
                var payment = context.AR302000Submit(commands.ToArray());
                string AppliedDoc = payment[0].PaymentSummary.AppliedToDocuments.Value;

                commands = new List<Command>();
                commands.Add(
                    new Value
                    {
                        Value = AppliedDoc,
                        LinkedCommand = schema.PaymentSummary.PaymentAmount
                    });
                commands.Add(schema.PaymentSummary.Type);
                commands.Add(schema.PaymentSummary.ReferenceNbr);
                commands.Add(schema.Actions.Save);
                commands.Add(schema.Actions.Release);
                payment = context.AR302000Submit(commands.ToArray());

                Console.WriteLine("Payment Type: " + payment[0].PaymentSummary.Type.Value.ToString());
                Console.WriteLine("Payment Ref.: " + payment[0].PaymentSummary.ReferenceNbr.Value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            finally
            {
                if (isLoginSuccess)
                    context.Logout();
            }
        }

        void VoidPayment(string refNbr)
        {
            TEST.Screen context = new Screen();
            bool loginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin", "123");

                //If login is successful
                loginSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (loginSuccess)
                    context.Logout();
            }
        }

        static void GetAllStockItem()
        {
            TEST.Screen context = new Screen();
            bool loginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin", "123");

                //If login is successful
                loginSuccess = true;

                IN202500Content stockItemsSchema = context.IN202500GetSchema();

                var commands = new Command[]
                {
                    stockItemsSchema.StockItemSummary.ServiceCommands.EveryInventoryID,
                    stockItemsSchema.StockItemSummary.InventoryID,
                    stockItemsSchema.StockItemSummary.Description,
                    //stockItemsSchema.GeneralSettingsItemDefaults.ItemClass,
                    //stockItemsSchema.GeneralSettingsUnitOfMeasureBaseUnit.BaseUnit
                    //,
                    //new Field
                    //{
                    //    ObjectName = stockItemsSchema.StockItemSummary.InventoryID.ObjectName, FieldName = "LastModifiedDateTime"
                    //}
                };

                string[][] stockItemList = context.IN202500Export(commands, null, 0, true, false);
                Console.WriteLine("Inventory ID \t Description");
                for (int i = 0; i < stockItemList.Length; i++)
                {
                    for (int x = 0; x < stockItemList[i].Length; x++)
                    {
                        //Console.Write(stockItemList[i][x] + ": ");
                        Console.Write(stockItemList[i + 1][x]);
                        Console.Write("\t");
                    }
                    Console.WriteLine();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (loginSuccess)
                    context.Logout();
            }
        }

        static void findStockItem(string barcode)
        {
            TEST.Screen context = new Screen();
            bool loginSuccess = false;

            try
            {
                context.CookieContainer = new System.Net.CookieContainer(); //stores cookie session
                context.EnableDecompression = true;
                context.Timeout = 100000000;
                context.Url = "http://localhost/AcumaticaERP/Soap/TESTAPI.asmx";
                LoginResult result = context.Login("admin", "123");

                //If login is successful
                loginSuccess = true;

                IN202500Content stockItemsSchema = context.IN202500GetSchema();

                var commands = new Command[]
                {
                    stockItemsSchema.StockItemSummary.ServiceCommands.EveryInventoryID,
                    stockItemsSchema.StockItemSummary.InventoryID,
                    stockItemsSchema.StockItemSummary.Description,
                    stockItemsSchema.GeneralSettingsItemDefaults.ItemClass,
                    stockItemsSchema.GeneralSettingsUnitOfMeasureBaseUnit.BaseUnit
                    //,
                    //new Field
                    //{
                    //    ObjectName = stockItemsSchema.StockItemSummary.InventoryID.ObjectName, FieldName = "LastModifiedDateTime"
                    //}
                };

                var filter = new Filter[]
                {
                    new Filter
                    {
                        Field = stockItemsSchema.CrossReference.AlternateID,
                        Condition = FilterCondition.Equals,
                        Value = "Barcode",
                        Operator = FilterOperator.Or
                    },
                    new Filter
                    {
                        OpenBrackets = 1,
                        Field = stockItemsSchema.CrossReference.AlternateID,
                        Condition = FilterCondition.Equals,
                        Value = "Global",
                        Operator = FilterOperator.And,
                    },
                    new Filter
                    {
                    Field = stockItemsSchema.CrossReference.AlternateID,
                    Condition = FilterCondition.Equals,
                    Value = barcode
                    }
                };

                string[][] stockItemList = context.IN202500Export(commands, null, 0, true, false);

                for (int i = 0; i < stockItemList.Length; i++)
                {
                    for (int x = 0; x < stockItemList[i].Length; x++)
                    {
                        Console.Write(stockItemList[i][x] + ": ");
                        Console.Write(stockItemList[i + 1][x]);
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
                if (loginSuccess)
                    context.Logout();
            }
        }

        #endregion

    }
}
