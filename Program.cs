using Neo4j.Driver;
using System;
using System.Threading.Tasks;


class Program
{
    static async Task Main()
    {
       using  var driver = GraphDatabase.Driver(
            "neo4j://127.0.0.1:7687",
            AuthTokens.Basic("neo4j","123456789")
            );

        //var session = driver.AsyncSession();

        //await using var session = driver.AsyncSession(); // this was commented due to failing to reuse same session
        //moving inside the while loop which gives new session for new operation only when needed

        while (true)
        {
            try
            {
                Console.WriteLine("1. Add Person and Company");
                Console.WriteLine("2. View Data");
                Console.WriteLine("3. Exit");

                var choice = Console.ReadLine();

                if (choice == "3") break;

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Enter Person Name");
                        var person = Console.ReadLine();

                        Console.WriteLine("Enter Company Name");
                        var company = Console.ReadLine();

                        await using(var session = driver.AsyncSession()) //each loop gets fresh session 
                        {
                            await session.RunAsync($@"
                        MERGE (p:Person {{name:'{person}'}})
                        MERGE (c:Company {{name:'{company}'}})
                        MERGE (p)-[:WORKS_AT]->(c)
                    ",new {person,company});
                        }

                        Console.WriteLine("Inserted");
                        break;

                    case "2":
                        await using (var session = driver.AsyncSession())
                        {
                            var result = await session.RunAsync(@"
                                MATCH (p:Person)-[:WORKS_AT]->(c:Company)
                                RETURN p.name as person, c.name as company
                            ");


                            await result.ForEachAsync(record =>
                            {
                                Console.WriteLine($"{record["person"]} works at {record["company"]}");
                            });
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid Option");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            //finally
            //{
            //    await session.CloseAsync();
            //    await driver.DisposeAsync();
            //}  because used 'using'  which does automatically 

            
        }
    }
}