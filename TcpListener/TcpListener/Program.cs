// Online C# Editor for free
// Write, Edit and Run your C# code using C# Online Compiler

using Amazon;
using Amazon.DynamoDBv2;
using System;
using System.Net;
using System.Net.Sockets;

public class HelloWorld
{
    public static void Main(string[] args)
    {
        var accessKeyId = File.ReadAllText("..//..//..//..//..//AccessKeyid.txt");
        var secretAccessKey = File.ReadAllText("..//..//..//..//..//SecretAccessKey.txt");

        var dynamoDbClient = new AmazonDynamoDBClient(accessKeyId, secretAccessKey, RegionEndpoint.USEast1);


        var tcpClients = new List<TcpClient>();

        var ipAddress = IPAddress.Any;
        var ipEndpoint = new IPEndPoint(ipAddress, 2203);
        var tcpListener = new TcpListener(ipEndpoint);
        tcpListener.Start();

        Task.Run(() =>
        {
            while (true)
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                tcpClients.Add(tcpClient);
                Task.Run(async () =>
                {
                    var getChars = new char[1024];
                    while (true)
                    {
                        var getStream = tcpClient.GetStream();
                        var streamReader = new StreamReader(getStream);
                        streamReader.Read(getChars, 0, getChars.Length);
                        var getMessage = String.Join("", getChars);

                        foreach (var client in tcpClients)
                        {
                            if (client != tcpClient)
                            {
                                var streamWriter = new StreamWriter(client.GetStream());
                                streamWriter.AutoFlush = true;
                                streamWriter.Write(getMessage);
                                streamWriter.Flush();
                                
                            }
                        }

                        var putItemRequest = new Amazon.DynamoDBv2.Model.PutItemRequest
                        {
                            TableName = "Logging",
                            Item = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
                                    {
                                        {
                                            "Id",
                                            new Amazon.DynamoDBv2.Model.AttributeValue
                                            {
                                                S = Guid.NewGuid().ToString()
                                            }
                                        },
                                        {
                                            "Status",
                                            new Amazon.DynamoDBv2.Model.AttributeValue
                                            {
                                                S = "Receiving Successful"
                                            }
                                        },
                                        {
                                            "Message",
                                            new Amazon.DynamoDBv2.Model.AttributeValue
                                            {
                                                S = getMessage
                                            }
                                        }
                                    }
                        };
                        await dynamoDbClient.PutItemAsync(putItemRequest);

                        Array.Clear(getChars);
                    }
                });
            }
        });

        while (true)
        {

        }

    }
}