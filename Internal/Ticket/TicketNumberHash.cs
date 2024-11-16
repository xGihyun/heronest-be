using System.Security.Cryptography;
using System.Text;
namespace Heronest.Internal.Ticket;


public class TicketNumberHash
{
   
   public String ConvertSHA256Hash(string input)
   {
       
    using (SHA256 sha256Hash = SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        var Builder = new StringBuilder();

        foreach(byte b in bytes)
        {
            Builder.Append(b.ToString("x2"));
        }   

        return Builder.ToString().Substring(0,16);


    }
   }
}

/*
   string input = "Hello, world!";
        string hash = ComputeSHA256Hash(input);
        
        // Example: Truncate to the first 16 characters
        string truncatedHash = hash.Substring(0, 16);
*/
