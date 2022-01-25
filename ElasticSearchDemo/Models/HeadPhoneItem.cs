using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ElasticSearchDemo.Models;

public class HeadPhoneItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
    public string Type { get; set; }
    public float Rating { get; set; }
    public int TotalRatings { get; set; }
    public decimal Price { get; set; }
    public decimal MRP { get; set; }

    public static async Task<List<HeadPhoneItem>> GetData()
    {
        List<HeadPhoneItem> headphones = new();
        using StreamReader reader = new($"{ Directory.GetCurrentDirectory() }/wwwroot/flipkart_headphones.csv");

        bool isHeadRow = true;

        while (!reader.EndOfStream)
        {
            HeadPhoneItem headphoneItem = new();

            string line = await reader.ReadLineAsync();
            string[] values = line.Split(',');

            if (isHeadRow)
            {
                isHeadRow = false;
                continue;
            }

            if (values.Length >= 7)
            {
                headphoneItem.Title = values[0];
                headphoneItem.Color = values[1];
                headphoneItem.Type = values[2];

                _ = float.TryParse(values[3], out float rating);
                headphoneItem.Rating = rating;

                _ = int.TryParse(values[3], out int totalRating);
                headphoneItem.TotalRatings = totalRating;

                _ = decimal.TryParse(values[3], out decimal price);
                headphoneItem.Price = price;

                _ = decimal.TryParse(values[3], out decimal mrp);
                headphoneItem.MRP = mrp;

                headphones.Add(headphoneItem);
            }
        }

        return headphones;
    }
}
