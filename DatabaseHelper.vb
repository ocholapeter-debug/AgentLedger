Imports Microsoft.Data.Sqlite
Imports System.IO
Imports System.Security.Cryptography

Public Class DatabaseHelper

    Private Shared ReadOnly DbFolder As String =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "AgentLedger")

    Private Shared ReadOnly ConnectionString As String =
        "Data Source=" &
        Path.Combine(DbFolder, "agent_ledger.db")

    Public Shared Sub InitializeDatabase()

        Directory.CreateDirectory(DbFolder)

        Using con As New SqliteConnection(ConnectionString)
            con.Open()

            Dim sql As String =
                "CREATE TABLE IF NOT EXISTS Users (" &
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                "Username TEXT NOT NULL UNIQUE, " &
                "PasswordHash TEXT NOT NULL, " &
                "PasswordSalt TEXT NOT NULL, " &
                "Role TEXT NOT NULL CHECK(Role IN ('Owner','Attendant')), " &
                "IsActive INTEGER NOT NULL DEFAULT 1)"

            Using cmd As New SqliteCommand(sql, con)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ' Create a demonstration Owner only when no users exist.
        Using con As New SqliteConnection(ConnectionString)
            con.Open()

            Using check As New SqliteCommand(
                "SELECT COUNT(*) FROM Users", con)

                If Convert.ToInt32(check.ExecuteScalar()) = 0 Then
                    AddUser("owner", "ChangeMe@123", "Owner")
                End If
            End Using
        End Using

    End Sub

    Public Shared Sub AddUser(username As String,
                              password As String,
                              role As String)

        If String.IsNullOrWhiteSpace(username) OrElse
           password.Length < 10 OrElse
           (role <> "Owner" AndAlso role <> "Attendant") Then

            Throw New ArgumentException(
                "Enter a username, a password of at least 10 characters, and a valid role.")
        End If

        Dim salt(15) As Byte

        Using rng As RandomNumberGenerator =
            RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Using

        Dim hash As Byte() =
            Rfc2898DeriveBytes.Pbkdf2(
                password, salt, 100000,
                HashAlgorithmName.SHA256, 32)

        Using con As New SqliteConnection(ConnectionString)
            con.Open()

            Dim sql As String =
                "INSERT INTO Users " &
                "(Username, PasswordHash, PasswordSalt, Role) " &
                "VALUES ($username, $hash, $salt, $role)"

            Using cmd As New SqliteCommand(sql, con)
                cmd.Parameters.AddWithValue("$username", username.Trim())
                cmd.Parameters.AddWithValue(
                    "$hash", Convert.ToBase64String(hash))
                cmd.Parameters.AddWithValue(
                    "$salt", Convert.ToBase64String(salt))
                cmd.Parameters.AddWithValue("$role", role)
                cmd.ExecuteNonQuery()
            End Using
        End Using

    End Sub

    Public Shared Function ValidateUser(
        username As String,
        password As String) As String

        Using con As New SqliteConnection(ConnectionString)
            con.Open()

            Dim sql As String =
                "SELECT PasswordHash, PasswordSalt, Role " &
                "FROM Users WHERE Username = $username " &
                "AND IsActive = 1"

            Using cmd As New SqliteCommand(sql, con)
                cmd.Parameters.AddWithValue("$username", username.Trim())

                Using reader = cmd.ExecuteReader()
                    If Not reader.Read() Then Return Nothing

                    Dim savedHash As Byte() =
                        Convert.FromBase64String(reader.GetString(0))

                    Dim salt As Byte() =
                        Convert.FromBase64String(reader.GetString(1))

                    Dim role As String = reader.GetString(2)

                    Dim enteredHash As Byte() =
                        Rfc2898DeriveBytes.Pbkdf2(
                            password, salt, 100000,
                            HashAlgorithmName.SHA256, 32)

                    If CryptographicOperations.FixedTimeEquals(
                        savedHash, enteredHash) Then
                        Return role
                    End If
                End Using
            End Using
        End Using

        Return Nothing
    End Function

    Public Shared Function GetConnectionString() As String
        Directory.CreateDirectory(DbFolder)
        Return ConnectionString
    End Function
    Public Shared Sub CreateExpensesTable()
        Using connection As New Microsoft.Data.Sqlite.SqliteConnection(GetConnectionString())
            connection.Open()

            Dim sql As String =
                "CREATE TABLE IF NOT EXISTS Expenses (" &
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                "BusinessDayId INTEGER NOT NULL, " &
                "Category TEXT NOT NULL, " &
                "Amount REAL NOT NULL, " &
                "Description TEXT, " &
                "ExpenseTime TEXT NOT NULL)"

            Using command As New Microsoft.Data.Sqlite.SqliteCommand(sql, connection)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub CreateCheckpointsTable()
        Using connection As New Microsoft.Data.Sqlite.SqliteConnection(
            GetConnectionString())

            connection.Open()

            Dim sql As String =
                "CREATE TABLE IF NOT EXISTS Checkpoints (" &
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                "BusinessDayId INTEGER NOT NULL, " &
                "CashBalance REAL NOT NULL, " &
                "MTNFloat REAL NOT NULL, " &
                "AirtelFloat REAL NOT NULL, " &
                "Notes TEXT, " &
                "CheckpointTime TEXT NOT NULL)"

            Using command As New Microsoft.Data.Sqlite.SqliteCommand(
                sql, connection)

                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub



End Class
