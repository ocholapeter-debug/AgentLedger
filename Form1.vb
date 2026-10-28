Imports Microsoft.Data.Sqlite
Imports System.IO
Imports System.Globalization

Public Class Form1

    '' Private ReadOnly dbPath As String =
    ''  Path.Combine(Application.StartupPath, "agent_ledger.db")

    Private ReadOnly connectionString As String = DatabaseHelper.GetConnectionString()

    Public Sub New()
        InitializeComponent()
        '' connectionString =
        ''   "Data Source=" & dbPath
        BuildForm()
        CreateDatabase()
    End Sub

    Private Sub CreateDatabase()
        Using con As New SqliteConnection(connectionString)
            con.Open()

            Dim sql As String =
                "CREATE TABLE IF NOT EXISTS BusinessDays (" &
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                "OpenTime TEXT NOT NULL, " &
                "OpeningCash REAL NOT NULL, " &
                "MTNFloat REAL NOT NULL, " &
                "AirtelFloat REAL NOT NULL, " &
                "Status TEXT NOT NULL)"

            Using cmd As New SqliteCommand(sql, con)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private txtCash As New TextBox()
    Private txtMTN As New TextBox()
    Private txtAirtel As New TextBox()
    Private lblStatus As New Label()

    Private Sub BuildForm()
        Me.Text = "Agent Ledger - Open Business Day"
        Me.Width = 480
        Me.Height = 420
        Me.StartPosition = FormStartPosition.CenterScreen

        AddField("Opening Cash (UGX)", txtCash, 35)
        AddField("MTN Float (UGX)", txtMTN, 100)
        AddField("Airtel Float (UGX)", txtAirtel, 165)

        Dim btnOpen As New Button()
        btnOpen.Text = "Open Business Day"
        btnOpen.SetBounds(35, 240, 200, 40)
        AddHandler btnOpen.Click, AddressOf OpenDay
        Me.Controls.Add(btnOpen)

        lblStatus.SetBounds(35, 300, 390, 45)
        Me.Controls.Add(lblStatus)
    End Sub

    Private Sub AddField(labelText As String,
                         box As TextBox, top As Integer)

        Dim lbl As New Label()
        lbl.Text = labelText
        lbl.SetBounds(35, top, 180, 25)
        Me.Controls.Add(lbl)

        box.SetBounds(220, top, 190, 25)
        Me.Controls.Add(box)
    End Sub

    Private Sub OpenDay(sender As Object, e As EventArgs)
        Dim cash, mtn, airtel As Decimal

        If Not ReadAmount(txtCash.Text, cash) OrElse
           Not ReadAmount(txtMTN.Text, mtn) OrElse
           Not ReadAmount(txtAirtel.Text, airtel) Then

            MessageBox.Show(
                "Enter valid amounts of zero or more.",
                "Invalid Amount")
            Return
        End If

        Using con As New SqliteConnection(connectionString)
            con.Open()

            Using check As New SqliteCommand(
                "SELECT COUNT(*) FROM BusinessDays " &
                "WHERE Status = 'Open'", con)

                If Convert.ToInt32(check.ExecuteScalar()) > 0 Then
                    MessageBox.Show(
                        "A business day is already open.")
                    Return
                End If
            End Using

            Dim sql As String =
                "INSERT INTO BusinessDays " &
                "(OpenTime, OpeningCash, MTNFloat, " &
                "AirtelFloat, Status) " &
                "VALUES ($time, $cash, $mtn, $airtel, 'Open')"

            Using cmd As New SqliteCommand(sql, con)
                cmd.Parameters.AddWithValue(
                    "$time", DateTime.Now.ToString("o"))
                cmd.Parameters.AddWithValue("$cash", cash)
                cmd.Parameters.AddWithValue("$mtn", mtn)
                cmd.Parameters.AddWithValue("$airtel", airtel)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblStatus.Text = "Business day opened successfully."
        MessageBox.Show("Opening balances saved.")
    End Sub

    Private Function ReadAmount(value As String,
                                ByRef amount As Decimal) As Boolean
        Return Decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            amount) AndAlso amount >= 0D
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class