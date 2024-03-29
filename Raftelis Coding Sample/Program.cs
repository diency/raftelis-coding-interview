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
}

class Program
{
    //MAIN
    //explained in overview section at the top
    static void Main()
    {
        string filePath = "Parcels.txt"; // Provide the path to your text file
        List<Record> records = ParseRecordsFromFile(filePath);

        PrintRecords(records);
    }

    //PRINT RECORDS
    //given a list of records, prints them with a fixed column width
    //column widths can be manually edited at the top of the function
    public static void PrintRecords(List<Record> records)
    {
        // Define column widths
        int pinWidth = 11;
        int addressWidth = 30;
        int ownerWidth = 45;
        int marketValueWidth = 12;
        int saleDateWidth = 12;
        int salePriceWidth = 15;
        int linkWidth = 100;

        //print header
        Console.WriteLine($"{FormatColumn("PIN", pinWidth)}{FormatColumn("ADDRESS", addressWidth)}{FormatColumn("OWNER", ownerWidth)}{FormatColumn("MARKET_VALUE", marketValueWidth)}{FormatColumn("SALE_DATE", saleDateWidth)}{FormatColumn("SALE_PRICE", salePriceWidth)}{FormatColumn("LINK", linkWidth)}");

        //print records
        foreach (var record in records)
        {
            Console.WriteLine($"{FormatColumn(record.PIN.ToString("F0"), pinWidth)}{FormatColumn(record.Address, addressWidth)}{FormatColumn(record.Owner, ownerWidth)}{FormatColumn(record.MarketValue.ToString("F2"), marketValueWidth)}{FormatColumn(record.SaleDate.ToString("yyyy-MM-dd"), saleDateWidth)}{FormatColumn(record.SalePrice.ToString("F2"), salePriceWidth)}{FormatColumn(record.Link, linkWidth)}");
        }
    }

    //FORMAT COLUMN
    //helper function for print records that pads/truncates a string based on given width
    private static string FormatColumn(string value, int width)
    {
        // Truncate or pad the string to fit the specified width
        if (value.Length >= width)
        {
            return value.Substring(0, width);
        }
        else
        {
            return value.PadRight(width);
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