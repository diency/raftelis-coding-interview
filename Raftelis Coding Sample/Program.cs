using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//--- OVERVIEW ---
//this script takes data from a .txt file formatted like this:
// PIN	ADDRESS	OWNER	MARKET_VALUE	SALE_DATE	SALE_PRICE	LINK
//then puts it into a matrix (list of string[])
//then sorts the rows of the matrix by street name (if two entries have same street name, it sorts by street number)
//prints result to console
//then it sorts the rows of the matrix by first name of address owner
//prints result to console

//script originally by Carter, email: [pretend my company email is here]

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

//RECORD CLASS
//used for storing entries from the .txt
public class Record
{
    public float PIN { get; set; }
    public string Address { get; set; }
    public string Owner { get; set; }
    public float MarketValue { get; set; }
    public DateTime SaleDate { get; set; }
    public float SalePrice { get; set; }
    public string Link { get; set; }

    //constructor
    public Record(float pin, string address, string owner, float marketValue, DateTime saleDate, float salePrice, string link)
    {
        PIN = pin;
        Address = address;
        Owner = owner;
        MarketValue = marketValue;
        SaleDate = saleDate;
        SalePrice = salePrice;
        Link = link;
    }

    //TO STRING - converts this entry into one formatted string for printing
    public override string ToString()
    {
        //the negative numbers in each {} is determining how many characters each column takes up
        //this can cut off some absurdly long entries but it looks waaaaay nicer
        //if you want it to just print raw for debug purposes you can use this:
        //return $"{PIN}, {Address}, {Owner}, {MarketValue}, {SaleDate}, {SalePrice}, {Link}";

        //also the :F0 for PIN is just to make sure it doesnt print the pin in scientific notation

        return string.Format("{0,-11:F0} {1,-30} {2,-45} {3,-12} {4,-12} {5,-15} {6,-100}",
                         PIN, Address, Owner, MarketValue, SaleDate.ToString("MM/dd/yyyy"), SalePrice, Link);
    }
}

class Program
{
    //MAIN
    //explained in overview section at the top
    static void Main()
    {
        string filePath = "Parcels.txt"; // Provide the path to your text file
        List<Record> records = ParseRecordsFromFile(filePath);

        //sort by address name/number
        records.Sort((x,y) => {
            (string, int) xTuple = GetAddressNameAndNumber(x.Address);
            string xRecordName = xTuple.Item1;

            (string, int) yTuple = GetAddressNameAndNumber(y.Address);
            string yRecordName = yTuple.Item1;
            

            int nameComparison = string.Compare(xRecordName,yRecordName);
            if(nameComparison != 0){
                return nameComparison;
            }

            //if names are the same, compare street numbers
            int xRecordNum = xTuple.Item2;
            int yRecordNum = yTuple.Item2;
            return xRecordNum.CompareTo(yRecordNum);
        });

        //print records
        PrintRecords(records);

        //print a little bumper between the databases
        Console.WriteLine("----------------------------");
        Console.WriteLine("First records done printing!");
        Console.WriteLine("----------------------------");

        //sort by firstname
        records.Sort((x,y) => {
            String xOwner = GetFirstNameOrBusiness(x.Owner);
            String yOwner = GetFirstNameOrBusiness(y.Owner);
            return string.Compare(xOwner,yOwner);
        });

        //print again
        PrintRecords(records);

    }

    //GET FIRST NAME OR BUSINESS
    //owner in the db can either be formatted like (lastname, firstname) eg
    //STALEY, CARTER
    //or the full name of a business eg
    //RAFTELIS LLC
    //if its a last/first name we want to cut it up and only return the first name,
    //if its a business we just return it raw
    static String GetFirstNameOrBusiness(String rawName){

        //check if name contains a comma
        //if it does, its a person(s)
        if (rawName.Contains(","))
        {
            //split the string and return the second part
            string[] parts = rawName.Split(',');
            string firstName = parts[1].Trim(); //trim to remove leading/trailing whitespace

            //NOTE: some first names are actually more than one person eg
            //STALEY, LAURA & TOBY
            //currently this function would just return "LAURA & TOBY"
            //but if you only wanted the first name of the first person
            //you'd put the line of code to cut off the second person here

            return firstName;
        }
        else
        {
            //if there's no comma, its a business, so just return the whole thing
            return rawName;
        }
    }

    //GET ADDRESS NAME AND NUMBER
    //given a string for the raw address, returns a tuple containing a string which contains the name of the address and
    //an integer which is the street number.
    //regarding houses with units (eg: 45 B EXAMPLE RD) currently the algorithm ignores unit letters and prioritizes
    //base name + number but this function can be modified to account for unit letters as well
    //by appending the unit letter to the end of the address name
    static (string, int) GetAddressNameAndNumber(string rawAddress){

        //split it into two parts using regular expressions
        //the \d is looking for numbers,
        //the \s is looking for whitespace,
        //and then the .+ is looking for the remaining characters
        Match match = Regex.Match(rawAddress, @"^(\d+)\s+(.+)");

        if(match.Success){
            //store the split string into variables
            //converting num into an int
            int num = int.Parse(match.Groups[1].Value);
            String name = match.Groups[2].Value;

            //some addresses are formatted like "642 - LOST RIVER RD"
            //we want to get rid of the - and whitespace at the front of the string if it exists so we do that here
            if(name.StartsWith("- ") && name.Length > 2){
                name = name.Substring(2);
            }else if(name.StartsWith("-") && name.Length > 1){
                name = name.Substring(1);
            }

            //some addresses have a unit char at the front of them
            //eg "B FROST BLVD"
            //currently we just remove these here
            
            if(name.Length > 2 && char.IsLetter(name[0]) && name[1] == ' '){
                name = name.Substring(2).Trim();
            }

            //alternatively, you can append the unit number to the end of the name string by
            //replacing "name = name.Substring(2).Trim();" with the following:
            /*
                string[] nameParts = name.Split(new[] { ' ' }, 2);
                name = nameParts[1] + " " + nameParts[0];
            */
            //this will cause it to sort the unit letters alphabetically, prioritizing them over unit number

            //return it
            return(name, num);
        }else{
            //oops, it didnt work, throw an error
            throw new ArgumentException("Invalid address format: {0}", rawAddress);
        }
        
    }

    //PRINT RECORDS
    //given a list of records, prints them with a fixed column width
    //column widths can be manually edited at the top of the function
    public static void PrintRecords(List<Record> records)
    {
        foreach (var record in records)
        {
            Console.WriteLine(record.ToString());
        }
    }

    //PARSE RECORDS FROM FILE
    //given a filepath, it opens that file then goes row by row turning each line into
    //a record object, then adding them to a list and returning that list
    public static List<Record> ParseRecordsFromFile(string filePath)
    {

        List<Record> records = new List<Record>();

        //read lines from file
        string[] lines = File.ReadAllLines(filePath);

        //start indexing at 1 - the first entry is literally just PIN|ADDRESS|OWNER|MARKET_VALUE|SALE_DATE|SALE_PRICE|LINK
        //so we skip it here
        for (int i = 1; i < lines.Length; i++)
        {
            //split line into string array using |
            string[] parts = lines[i].Split('|');

            if(parts.Length >= 6){
                //parse data into record object
                //strings can just be put into the object as is but datetime/floats need a little extra code
                //just in case the parsing doesnt work correctly
                
                float pin;
                //the if statement here both tries to parse pin and detects if it doesnt work correctly
                if(!float.TryParse(parts[0], out pin)){
                    //this will not run if pin is parsed correctly
                    pin = 0.0f;
                }

                string address = parts[1];

                string owner = parts[2];

                float marketValue;
                if(!float.TryParse(parts[3], out marketValue)){
                    marketValue = 0.0f;
                }

                DateTime saleDate;
                if(!DateTime.TryParse(parts[4], out saleDate)){
                    saleDate = DateTime.MinValue;
                }

                float salePrice;
                if(!float.TryParse(parts[5], out salePrice)){
                    salePrice = 0.0f;
                }
                
                string link = parts[6];

                //create new record object with parsed values and add it to list
                Record record = new Record(pin, address, owner, marketValue, saleDate, salePrice, link);
                records.Add(record);
            }else{
                Console.WriteLine("ERROR: one of the entries in parcels is missing a section");
            }
        }

        return records;
    }
    
}