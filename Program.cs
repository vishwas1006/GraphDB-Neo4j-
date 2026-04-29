using Neo4j.Driver;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var driver = GraphDatabase.Driver(
            "neo4j://127.0.0.1:7687",
            AuthTokens.Basic("neo4j", "123456789")
            );

        while (true)
        {
            try
            {
                Console.WriteLine("1. Add Person and Company");
                Console.WriteLine("2. View Data");
                Console.WriteLine("3. Update Company");
                Console.WriteLine("4. Delete Person");
                Console.WriteLine("5. Exit");
                Console.WriteLine();
                Console.WriteLine("Choose Option:");


                var choice = Console.ReadLine();

                if (choice == "5") break;

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Enter Person Name:");
                        var person = Console.ReadLine();

                        Console.WriteLine("Enter Company Name:");
                        var company = Console.ReadLine();

                       await using(var session = driver.AsyncSession()) //session coming from driver so using it 
                        {
                            await session.RunAsync($@"
                                MERGE (p:Person {{name:'{person}'}})
                                MERGE (c:Company {{name:'{company}'}})
                                MERGE (p)-[:WORKS_AT]->(c)
                            ");
                        }
                        Console.WriteLine("Record inserted");
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


                    //Updating 
                    case "3":
                        Console.WriteLine("Enter Person name:");
                        var name = Console.ReadLine();

                        Console.WriteLine("Enter New Company name");
                        var newCompany = Console.ReadLine();

                        await using(var session = driver.AsyncSession())
                        {
                            await session.RunAsync($@"
                                MATCH (p:Person {{name:'{name}'}})
                                MERGE (c:Company {{name:'{newCompany}'}})
                                MERGE (p)-[:WORKS_AT]->(c)
                            ");
                        }
                        Console.WriteLine("Updated company record");
                        break;

                    case "4":
                        Console.WriteLine("Enter name to be deleted");
                        var delName = Console.ReadLine();

                        await using(var session = driver.AsyncSession())
                        {
                            await session.RunAsync($@"
                                MATCH (p:Person {{name:'{delName}'}})
                                DETACH DELETE p
                            ");
                        }

                        Console.WriteLine("Deleted Record");
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
        }
    }
}