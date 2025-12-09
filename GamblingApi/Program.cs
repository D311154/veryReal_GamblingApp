using System.Net;
using System.Text;
using System.Text.Json;
using GamblingApi.Data;

namespace GamblingApi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var db = new AppDbContext();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/accounts/");
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET")
                {
                    var usersList = db.Users.ToList();

                    string responseString = JsonSerializer.Serialize(usersList);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentType = "application/json";

                    response.ContentLength64 = bytes.Length;
                    var output = response.OutputStream;
                    output.Write(bytes, 0, bytes.Length);
                    output.Close();
                }

                else if (request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    string body = reader.ReadToEnd();

                    var newUser = JsonSerializer.Deserialize<User>(body);

                    if (newUser != null)
                    {
                        db.Users.Add(newUser);
                        db.SaveChanges();

                        string responseString = JsonSerializer.Serialize(newUser);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.Created;

                        response.ContentLength64 = bytes.Length;
                        var output = response.OutputStream;
                        output.Write(bytes, 0, bytes.Length);
                        output.Close();
                    }
                }
            }
        }
    }
}
