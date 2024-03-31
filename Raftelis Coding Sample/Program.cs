using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//--- OVERVIEW ---
//this code takes data from a .txt file formatted like this:
// PIN	ADDRESS	OWNER	MARKET_VALUE	SALE_DATE	SALE_PRICE	LINK
//then puts it into a matrix (list of string[])
//then sorts the rows of the matrix by street name (if two entries have same street name, it sorts by street number)
//prints result to console
//then it sorts the rows of the matrix by first name of address owner
//prints result to console

//code originally by Carter, email: [pretend my company email is here]

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
        //also the :F0 for PIN is just to make sure it doesnt print the pin in scientific notation

        return string.Format("{0,-11:F0} {1,-30} {2,-45} {3,-12} {4,-12} {5,-15} {6,-100}",
                         PIN, Address, Owner, MarketValue, SaleDate.ToString("MM/dd/yyyy"), SalePrice, Link);
    }

    //GET FIRSTNAME
    //returns a string that contains either the first name of owner (eg STALEY, CARTER => CARTER)
    //or the full name of a business that owns the property (eg RAFTELIS LLC => RAFTELIS LLC)
    public string GetFirstName()
    {
        //check if name contains a comma
        if (Owner.Contains(","))
        {
            //split the string and return the second part (first name)
            string[] parts = Owner.Split(',');
            string firstName = parts[1].Trim(); //trim whitespace

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
            return Owner;
        }
    }

    //GET ADDRESS STRING
    //returns a string with the name of the address, excluding the address number, unit number, and any "-" characters.
    public string GetAddressString()
    {
        //split it into two parts using regular expressions
        Match match = Regex.Match(Address, @"^(\d+)\s+(.+)");

        if(match.Success){
            //store the split string into variable
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
            return name;
        }else{
            throw new ArgumentException("Invalid address format: {0}", Address);
        }
    }

    public int GetAddressNumber(){
        //split it into two parts using regular expressions
        Match match = Regex.Match(Address, @"^(\d+)\s+(.+)");

        if(match.Success){
            //converting num string into an int
            int num = int.Parse(match.Groups[1].Value);

            //return it
            return num;
        }else{
            throw new ArgumentException("Invalid address format: {0}", Address);
        }
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
        records = SortByColumn(records,1);

        PrintRecords(records);

        Console.WriteLine("----------------------------");
        Console.WriteLine("First records done printing!");
        Console.WriteLine("----------------------------");

        //sort by firstname
        records = SortByColumn(records,2);

        PrintRecords(records);

    }

    //SORT BY COLUMN
    //given a matrix of entries and a column, sorts it alphabetically/numerically
    static List<Record> SortByColumn(List<Record> records, int column){
        switch(column)
        {

            case 0: //pin
                records.Sort((x,y) => {
                    return x.PIN.CompareTo(y.PIN);
                });
                break;

            case 1: //address
                records.Sort((x,y) => {

                    int nameComparison = string.Compare(x.GetAddressString(),y.GetAddressString());

                    if(nameComparison != 0){
                        return nameComparison;
                    }

                    //if names are the same, compare street numbers
                    return x.GetAddressNumber().CompareTo(y.GetAddressNumber());
                });
                break;

            case 2: //firstname
                records.Sort((x,y) => {
                    return string.Compare(x.GetFirstName(),y.GetFirstName());
                });
                break;

            case 3: //market value
                records.Sort((x,y) => {
                    return x.MarketValue.CompareTo(y.MarketValue);
                });
                break;

            case 4: //sale date
                records.Sort((x,y) => {
                    return x.SaleDate.CompareTo(y.SaleDate);
                });
                break;

            case 5: //sale price
                records.Sort((x,y) => {
                    return x.SalePrice.CompareTo(y.SalePrice);
                });
                break;

            case 6: //link - note: this one is not parsed at all right now, so its pretty useless as is
                records.Sort((x,y) => {
                    return string.Compare(x.Link,y.Link);
                });
                break;

            default:
                Console.WriteLine("Error: Column not supported/out of bounds");
                break;
        }

        return records;
    }

    //PRINT RECORDS
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

        //split files into lines
        string[] lines = File.ReadAllLines(filePath);

        //start indexing at line 1 - the first entry is literally just PIN|ADDRESS|OWNER|MARKET_VALUE|SALE_DATE|SALE_PRICE|LINK
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
                //the if statement here is also actually parsing the pin (assuming it works)
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