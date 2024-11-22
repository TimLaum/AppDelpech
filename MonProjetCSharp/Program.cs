using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using System.Collections.Generic; // Pour la liste
using Newtonsoft.Json.Linq; // Pour le JSON

public class CalculData
{
    public string Calcul { get; set; }
    public string Resultat { get; set; }
    public bool Parite { get; set; }
    public bool Premier { get; set; }
    public bool Parfait { get; set; }

    public static CalculData FromJson(string json)
    {
        JObject jsonObject = JObject.Parse(json);
        return new CalculData
        {
            Calcul = jsonObject["calcul"]?.ToString(),
            Resultat = jsonObject["resultat"]?.ToString(),
            Parite = jsonObject["est_pair"]?.ToObject<bool>() ?? false,
            Premier = jsonObject["est_premier"]?.ToObject<bool>() ?? false,
            Parfait = jsonObject["est_parfait"]?.ToObject<bool>() ?? false
        };
    }
}



class Program
{
    static void Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5001/");
        listener.Start();
        Console.WriteLine("Serveur C# en écoute sur http://localhost:5001/");
        CreerBaseDeDonneesEtTable();
        AfficherDonneesBDD();

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            if (request.HttpMethod == "POST")
            {
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string json = reader.ReadToEnd();
                    Console.WriteLine("JSON reçu : " + json);
                    CalculData data = CalculData.FromJson(json);
                    Console.WriteLine($"Calcul reçu : {data.Calcul}, Résultat : {data.Resultat}, Parité : {data.Parite}, Premier : {data.Premier}, Parfait : {data.Parfait}");
                    EnregistrerEnBDD(data.Calcul, data.Resultat, data.Parite, data.Premier, data.Parfait);

                    HttpListenerResponse response = context.Response;
                    string responseString = data.Resultat;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }

            if (request.HttpMethod == "GET")
            {
                List<CalculData> calculs = RecupererTousLesCalculs();
                string jsonResponse = JsonConvert.SerializeObject(calculs);
                HttpListenerResponse response = context.Response;
                response.ContentType = "application/json";
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }
    }

    private static List<CalculData> RecupererTousLesCalculs()
    {
        string connectionString = "Data Source=calculs.db";
        var calculs = new List<CalculData>();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectQuery = "SELECT Calcul, Resultat,Parite,Premier,Parfait FROM Calculs";

            using (var command = new SqliteCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        calculs.Add(new CalculData
                        {
                            Calcul = reader["Calcul"].ToString(),
                            Resultat = reader["Resultat"].ToString(),
                            Parite = Convert.ToBoolean(reader["Parite"]),
                            Premier = Convert.ToBoolean(reader["Premier"]),
                            Parfait = Convert.ToBoolean(reader["Parfait"])
                        });
                    }
                }
            }
            return calculs;
        }
    }

    private static void AfficherDonneesBDD()
        {
            string connectionString = "Data Source=calculs.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT Id, Calcul, Resultat, Parite, Premier, Parfait, DateCreation FROM Calculs";

                using (var command = new SqliteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Données actuelles dans la base de données:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"Id: {reader["Id"]}, Calcul: {reader["Calcul"]}, Résultat: {reader["Resultat"]}, Parite: {Convert.ToBoolean(reader["Parite"])}, Premier: {Convert.ToBoolean(reader["Premier"])}, Parfait: {Convert.ToBoolean(reader["Parfait"])}, Date: {reader["DateCreation"]}");
                        }
                    }
                }
            }
        }
    private static void CreerBaseDeDonneesEtTable()
    {
        string connectionString = "Data Source=calculs.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Calculs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Calcul TEXT,
                    Resultat TEXT,
                    Parite INTEGER,
                    Premier INTEGER,
                    Parfait INTEGER,
                    DateCreation TEXT
                )";

            using (var command = new SqliteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Table 'Calculs' vérifiée/créée avec succès.");

            }
        }
    }

    private static void EnregistrerEnBDD(string calcul, string resultat, bool parite, bool premier, bool parfait)
    {
        string connectionString = "Data Source=calculs.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO Calculs (Calcul, Resultat, Parite, Premier, Parfait, DateCreation) VALUES (@calcul, @resultat, @Parite, @Premier, @Parfait, @dateCreation)";
            using (var command = new SqliteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@calcul", calcul);
                command.Parameters.AddWithValue("@resultat", resultat);
                command.Parameters.AddWithValue("@Parite", parite ? 1 : 0);
                command.Parameters.AddWithValue("@Premier", premier ? 1 : 0);
                command.Parameters.AddWithValue("@Parfait", parfait ? 1 : 0);
                command.Parameters.AddWithValue("@dateCreation", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }
        Console.WriteLine("Données enregistrées avec succès !");
    }
}
