using System.Net.Sockets;
using DAL.DataModels;
using DAL.DBHandlers;

namespace Server;

[CollectionDefinition("NonParallelCollection", DisableParallelization = true)]
public class NonParallelCollectionDefinition {}

[Collection("NonParallelCollection")]
[TestCaseOrderer(typeof(PriorityOrderer))]
public class ServerTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region InitializeDB
    [Fact, TestPriority(1)]
    public async Task ValidInitDB()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("localhost", "root", "");
        Assert.True(success, error);
    }

    [Fact, TestPriority(2)]
    public async Task ValidInitDBRadmin()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("26.244.97.115", "root", "");
        Assert.True(success, error);
    }

    [Fact, TestPriority(3)]
    public async Task InvalidInitDB1()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("localhost", "invaliduser", "");
        _output.WriteLine($"Invalid DB username error: {error}");
        Assert.False(success, error);
    }

    [Fact, TestPriority(4)]
    public async Task InvalidInitDB2()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("localhost", "root", "invalidpassword");
        _output.WriteLine($"Invalid DB password error: {error}");
        Assert.False(success, error);
    }
    #endregion

    #region DB Operations
    [Fact, TestPriority(11)]
    public async Task UserDBOperations()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("localhost", "root", "");
        Assert.True(success, error);

        (success, error) = await UserDB.CheckUsername("testusername");
        Assert.True(success, error);

        User mockUser = new()
        {
            Username = "testusername",
            PwdSet = BLL.Utilities.ServerUtilities.Security.HashPassword("password")
        };
        (success, error) = await UserDB.Add(mockUser);
        Assert.True(success, error);

        (success, error) = await UserDB.Add(mockUser);
        _output.WriteLine($"Adding duplicate user error: {error}");
        Assert.False(success, error);

        (success, error) = await UserDB.CheckUsername("testusername");
        _output.WriteLine($"Checking existing username error: {error}");
        Assert.False(success, error);

        (User? getUser, error) = await UserDB.Get("testusername");
        if (error != "")
            _output.WriteLine($"Getting user error: {error}");
        Assert.NotNull(getUser);
        Assert.Equal("testusername", getUser!.Username);
        Assert.True(BLL.Utilities.ServerUtilities.Security.VerifyPassword("password", getUser!.PwdSet!));

        (success, error) = await UserDB.Delete(getUser.UserID);
        Assert.True(success, error);

        (success, error) = await UserDB.CheckUsername("testusername");
        Assert.True(success, error);
    }
    #endregion

    #region TcpListener
    [Fact, TestPriority(21)]
    public async Task ValidListener()
    {
        BLL.Server server = new();

        var (success, error) = await server.InitializeDB("localhost", "root", "");
        Assert.True(success, error);

        await server.Start(null, 12345);

        TcpClient testClient = new();
        testClient.Connect("127.0.0.1", 12345);
        Assert.True(testClient.Connected);

        server.Stop();
    }
    #endregion
}
