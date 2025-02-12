using BLL.Game.Components.Entity;
using BLL.Game.Components.Others;
using BLL.Server;
using BLL.Server.DataModels;
using DAL;

namespace Tests;

[Collection("NonParallelCollection")]
[TestCaseOrderer(typeof(PriorityOrderer))]
public class ClientTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region 1.a.1 FileManager
    [Fact]
    public void FileManagerTest()
    {
        string testFolder = "TestFolder", testFile = "test1";
        Player mockPlayer = Player.DefaultPlayer();

        FileManager.WriteJson(testFolder, testFile, mockPlayer);

        Player? readObject = FileManager.ReadJson<Player>(testFolder, testFile);
        Assert.NotNull(readObject);
        Assert.Equal(mockPlayer.Name, readObject.Name);
    }
    #endregion

    #region 1.b.1 Connect & Assets
    [Fact]
    public void ValidConnect()
    {
        ServerHandler serverHandler = new();

        bool connectResult = serverHandler.Connect(out string error);
        Assert.True(connectResult, error);
    }

    [Fact(Skip = "One-time test, to check correct error message when server isn't running, and server must be running for other tests")]
    // [Fact]
    public void InvalidConnect()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool connectResult = serverHandler.Connect(out string error);
        if (error != "")
            _output.WriteLine($"Connect error: {error}");
        Assert.False(connectResult);
    }

    [Fact]
    public void UpdateAssets()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool updateResult = serverHandler.UpdateAssets(out string error);
        Assert.True(updateResult, error);
    }
    #endregion

    #region 1.b.2 Authentication
    [Fact, TestPriority(1)]
    public void ValidRegister()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool registerResult = serverHandler.Register("username", "nickname", "password", "email", out string error);
        Assert.True(registerResult, error);
    }

    [Fact, TestPriority(2)]
    public void InvalidRegister()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool registerResult = serverHandler.Register("username", "nickname", "password", "email", out string error);
        if (error != "")
            _output.WriteLine($"Register error: {error}");
        Assert.False(registerResult);
    }

    [Fact, TestPriority(3)]
    public void ValidLogin()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);
    }

    [Fact, TestPriority(3)]
    public void InvalidLogin1()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("invalidusername", "password", out string error);
        if (error != "")
            _output.WriteLine($"Login error: {error}");
        Assert.False(loginResult, error);
    }

    [Fact, TestPriority(3)]
    public void InvalidLogin2()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "invalidpassword", out string error);
        if (error != "")
            _output.WriteLine($"Login error: {error}");
        Assert.False(loginResult);
    }

    [Fact, TestPriority(4)]
    public void ResetPassword()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool resetResult = serverHandler.ResetPassword("username", "email", "password", out string error);
        Assert.True(resetResult, error);
    }

    [Fact, TestPriority(4)]
    public void InvalidResetPassword1()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool resetResult = serverHandler.ResetPassword("invalidusername", "email", "password", out string error);
        if (error != "")
            _output.WriteLine($"Reset password error: {error}");
        Assert.False(resetResult);
    }

    [Fact, TestPriority(4)]
    public void InvalidResetPassword2()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool resetResult = serverHandler.ResetPassword("username", "invalidemail", "password", out string error);
        if (error != "")
            _output.WriteLine($"Reset password error: {error}");
        Assert.False(resetResult);
    }
    #endregion

    #region 1.b.3 Modify user
    [Fact, TestPriority(5)]
    public void ValidGetUser()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        User? user = serverHandler.GetUser("username", out string error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.NotNull(user);

        Assert.Equal("username", user?.Username);
    }

    [Fact, TestPriority(5)]
    public void InvalidGetUser()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        User? user = serverHandler.GetUser("invalidusername", out string error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.Null(user);
    }

    [Fact, TestPriority(6)]
    public void ValidUpdateUser()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool changeResult = serverHandler.UpdateMainUser("newnickname", "newemail", "newpassword", out error);
        Assert.True(changeResult, error);

        User? user = serverHandler.GetUser("username", out error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.NotNull(user);

        Assert.True(serverHandler.VerifyPassword("newpassword"));
        Assert.Equal("newnickname", user?.Nickname);
        Assert.Equal("newemail", user?.Email);

        changeResult = serverHandler.UpdateMainUser("nickname", "email", "password", out error);
        Assert.True(changeResult, error);
    }

    [Fact, TestPriority(6)]
    public void InvalidUpdateUser1()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "invalidpassword", out string error);
        if (error != "")
            _output.WriteLine($"Login error: {error}");
        Assert.False(loginResult, error);

        bool changeResult = serverHandler.UpdateMainUser("newnickname", "newemail", "newpassword", out error);
        if (error != "")
            _output.WriteLine($"Change password error: {error}");
        Assert.False(changeResult);

        User? user = serverHandler.GetUser("username", out error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.NotNull(user);

        bool validatePassword = BLL.Utilities.NetworkUtilities.Security.VerifyPassword("newpassword", user?.PwdSet!);
        if (error != "")
            _output.WriteLine($"Change password error: {error}");
        Assert.False(validatePassword);
    }

    [Fact, TestPriority(6)]
    public void InvalidUpdateUser2()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("invalidusername", "password", out string error);
        if (error != "")
            _output.WriteLine($"Login error: {error}");
        Assert.False(loginResult, error);

        bool changeResult = serverHandler.UpdateMainUser("newnickname", "newemail", "newpassword", out error);
        if (error != "")
            _output.WriteLine($"Change password error: {error}");
        Assert.False(changeResult);

        User? user = serverHandler.GetUser("invalidusername", out error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.Null(user);
    }

    [Fact, TestPriority(100)]
    public void DeleteUser()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool deleteResult = serverHandler.DeleteAccount(out error);
        Assert.True(deleteResult, error);

        User? user = serverHandler.GetUser("username", out error);
        if (error != "")
            _output.WriteLine($"Get user error: {error}");
        Assert.Null(user);
    }
    #endregion

    #region 1.c.1 Local save
    [Fact, TestPriority(7)]
    public void LocalSaveTest()
    {
        GameSave writeSave = new(new(), "TestSave");

        FileManager.WriteJson(FileManager.FolderNames.Saves, writeSave.Name, writeSave);

        GameSave? readSave = FileManager.ReadJson<GameSave>(FileManager.FolderNames.Saves, writeSave.Name);
        Assert.NotNull(readSave);
        Assert.Equal(writeSave.Name, readSave.Name);
        Assert.Equal(writeSave.RunData.RunID, readSave.RunData.RunID);
    }
    #endregion

    #region 1.d.1 Online save
    [Fact, TestPriority(9)]
    public void UploadSave()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool uploadResult = serverHandler.UploadSave(new(new(), "testSave"), out error);
        Assert.True(uploadResult, error);
    }

    [Fact, TestPriority(10)]
    public void DownloadSave()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool downloadSave = serverHandler.DownloadSave(out error);
        Assert.True(downloadSave, error);
    }
    #endregion

    #region 1.d.2 Online score
    [Fact, TestPriority(11)]
    public void UploadScore()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool uploadResult = serverHandler.UploadScore(0, new(0), out error);
        Assert.True(uploadResult, error);
    }

    [Fact, TestPriority(12)]
    public void DownloadScores()
    {
        ServerHandler serverHandler = new();

        serverHandler.Connect(out _);

        bool loginResult = serverHandler.Login("username", "password", out string error);
        Assert.True(loginResult, error);

        bool downloadScores = serverHandler.GetAllScores(out List<string> personal, out List<string> monthly, out List<string> allTime, out error);
        Assert.True(downloadScores, error);
        Assert.True(personal.Count > 0);
        Assert.True(monthly.Count > 0);
        Assert.True(allTime.Count > 0);
    }
    #endregion
}