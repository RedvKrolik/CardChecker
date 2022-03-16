using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CardChecker
{
    class Program
    {
        private string cardNumber;
        private byte counter;
        private int help;
        // public int error;

        private int iD1;
        private int iD2;
        static void Main(string[] args)
        {
            Program program = new Program();

            Console.WriteLine("Welcome to CardCheck!\nHere you can check your bank card specifications.\n");

        Retry:
            Console.WriteLine("Enter the card number");
            program.cardNumber = Console.ReadLine().Replace(" ", "");

            // the following code is the card check based in the Luhn method
            if (program.cardNumber.Length == 13 || program.cardNumber.Length == 16 || program.cardNumber.Length == 19 || program.cardNumber.Length == 20) //checking if card number fits
            {
                if ((program.cardNumber.Length - 1) % 2 != 0) //card number even or odd -- to use the Luhn method correctly
                {

                    for (int i = 0; i < program.cardNumber.Length; i++)
                    {
                        program.help = Convert.ToInt32(Char.GetNumericValue((program.cardNumber[i])));//storing the index of a digit in card number to a variable 
                        //when stored just into int just as program.cardNumber[i], returns the decimal code of the index, hence getting numeric value of it first
                        //after that, convert into int and store to tthe variable
                        
                        if ((i + 1) % 2 != 0 && program.help * 2 > 9) //checking if the index is odd and if, when multiplied by 2, greater than 9
                        {                                            
                            program.counter += (Byte)(program.help * 2 - 9); //if greater -- subtract 9 from the multiplication and add to the counter variable
                        }
                        else if ((i + 1) % 2 != 0 && program.help * 2 < 9) //checking if the index is odd and if, when multiplied by 2, less than 9
                        {
                            program.counter += (byte)(program.help * 2);//if less add the multiplication to the counter variable
                        }
                        else
                        {
                            program.counter += (Byte)program.help;//otherwise -- just add the index to the counter variable
                        }


                    }
                }
                else if ((program.cardNumber.Length - 1) % 2 == 0) // same as the previous fragment, but for even indexes
                {
                    for (int i = 1; i < program.cardNumber.Length; i += 2)
                    {
                        program.help = Convert.ToInt32(Char.GetNumericValue((program.cardNumber[i])));

                        if ((i + 1) % 2 == 0 && program.help * 2 < 9)  
                        {                                           
                            program.counter += (Byte)(program.help * 2);
                        }
                        else
                        {
                            program.counter += (Byte)program.help;
                        }
                    }
                }


                if (program.counter % 10 == 0)//checking the card number validity
                {
                    Console.WriteLine("\nValidation passed. Approximate card capacity is equal 99%.");

                }
                else
                {
                    Console.WriteLine("\nThe card is invalid. Try again!");

                }
            }
            else //if the card number doesn't fit 13, 16, 19, 20, a message is shown; retuenung to the beginning 
            {
                Console.WriteLine("You have entered an invalid number!");
                goto Retry;
            }

            string ID = program.cardNumber.Remove(7); // declaring a variable that contains data requitred for the task method to recieve the whole card data
            Task<int> task = Program.TaskAsync(ID);//calling the task method
            task.Wait();//setthe delay before the task method has performed



            int error = task.Result;//result of the method is an error code -- 0 or 24

            if (error == 24)//once that code is received, console cleares up and returns to the beginning
            {
                Console.ReadKey();

                Console.Clear();
                goto Retry;
            }


            Console.WriteLine("\nWould you like to check another card?\nType y to agree, any other key to quit.");//after successfull check can quit or continue
            string retry = Console.ReadLine().ToLower();

            if (retry.Trim() == "y")
            {
                Console.Clear();
                goto Retry;
            }

        }


        static async Task<int> TaskAsync(string ID)//the task method to get all the card data
        {
            int error = 0;

            Program CCV = new Program();
            int ID1 = CCV.iD1;
            int ID2 = CCV.iD2;

            string[] NamesOfFields = { "scheme", "currency", "country", "bank" }; // string array with the values' names to check 
            string[,] CardData = { //2-dimensional string array to fill with the data

                { "Scheme","blah blah"},
                {"Country", "blah blah" },
                { "Bank", "blah blah"},
                { "Currency", "blah blah"}
            };

            using var client = new HttpClient(); //creating HTTP client example
            var ValidationCheck = await client.GetAsync("https://lookup.binlist.net/" + ID);//get query to check if the Cardnumber is valide

            if (!Convert.ToString(ValidationCheck).Contains("404"))//checking if the number is valid
            {

                var DataRequest = await client.GetStringAsync("https://lookup.binlist.net/" + ID);//creating a requiest for data


                foreach (string name in NamesOfFields)
                {
                    switch (name)
                    {
                        case "scheme":
                        case "currency":
                            {
                                ID1 = DataRequest.IndexOf('"', DataRequest.IndexOf(':', DataRequest.IndexOf(name))) + 1; 
                                ID2 = DataRequest.IndexOf('"', ID1) - 1;                                                    
                                /*
                                 after "scheme" and "currency" their names follow, e.g. "scheme":"visa", so it looks for the index of the fieldnames
                                 then it finds the '"' after the ':' and stores the index value with 1 added to indicate beginning of the field content
                                 by the folloing '"' index of the content's ending found and stored
                                 */




                                for (int i = 0; i < CardData.GetLength(0); i++)// looks for the correlating strings in the CardData array 
                                {                                              // and fills in the values                  
                                    for (int j = 0; j < CardData.GetLength(1); j++)
                                    {
                                        if (CardData[i, j].ToLower() == name.ToLower()) 
                                        { CardData[i, j + 1] = DataRequest.Substring(ID1, (ID2 + 1) - (ID1)); }
                                    }

                                }

                                break;
                            }
                        case "country":
                        case "bank":
                            {
                                if (DataRequest.Contains(name) && DataRequest.Substring(DataRequest.IndexOf(name), DataRequest.Length - DataRequest.IndexOf(name)).Contains("name"))
                                {
                                    /*
                                     * fields "country" and "bank" contain the "name" value the where the name is stored
                                     * the 1st step is checking whether that field exists
                                     * using the same way, content of the fiend name found 
                                     */

                                    ID1 = DataRequest.IndexOf('"', DataRequest.IndexOf(':', DataRequest.IndexOf("name", DataRequest.IndexOf(name)))) + 1; 

                                    ID2 = DataRequest.IndexOf('"', ID1) - 1;


                                    for (int i = 0; i < CardData.GetLength(0); i++) // looks for the correlating strings in the CardData array 
                                                                                    // and fills in the values 
                                    {
                                        for (int j = 0; j < CardData.GetLength(1); j++)
                                        {
                                            if (CardData[i, j].ToLower() == name.ToLower())
                                            { CardData[i, j + 1] = DataRequest.Substring(ID1, (ID2 + 1) - (ID1)); }
                                        }

                                    }


                                    break;
                                }
                                else
                                {
                                    for (int i = 0; i < CardData.GetLength(0); i++)// if the "name" field check is failed, the correlating string is filled  with "no stated"
                                    {
                                        for (int j = 0; j < CardData.GetLength(1); j++)
                                        {
                                            if (CardData[i, j].ToLower() == name.ToLower())
                                            { CardData[i, j + 1] = "not stated"; }
                                        }

                                    }
                                    break;
                                }
                            }
                    }
                }

              

                for (int i = 0; i < CardData.GetLength(0); i++)//changing string names in the CardData array to Russian
                {
                    for (int j = 0; j < CardData.GetLength(1); j++)
                    {
                        switch (CardData[i, j])
                        {
                            case "Scheme":
                                {
                                    CardData[i, j] = "Платёжная система: ";
                                    break;
                                }
                            case "Country":
                                {
                                    CardData[i, j] = "Название страны: ";
                                    break;
                                }
                            case "Bank":
                                {
                                    CardData[i, j] = "Название банка: ";
                                    break;
                                }
                            case "Currency":
                                {
                                    CardData[i, j] = "Валюта: ";
                                    break;
                                }
                        }
                     


                    }
                  
                } 
                Console.WriteLine();

                for (int i = 0; i < CardData.GetLength(0); i++)// output of filled array with card data
                {
                    for (int j = 0; j < CardData.GetLength(1); j++)
                    {
                        Console.Write(CardData[i, j] + " ");



                    }
                    Console.WriteLine();
                }
                return error;

            }
            else
            {
                Console.WriteLine("Card data check failed!\nReview the number and re-enter it.\nPress any key to continue.");// if card check is failed, error 24 returned
                error = 24;
                return error;
            }
        }

    }
}