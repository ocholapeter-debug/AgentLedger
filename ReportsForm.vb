Imports Microsoft.Data.Sqlite
Imports System.Data

Public Class ReportsForm

    Private lblOpening As Label
    Private lblTransactions As Label
    Private lblExpenses As Label
    Private lblCheckpoint As Label
    Private grid As DataGridView

    Public Sub New()
        InitializeComponent()
        BuildForm()
        LoadReport()
    End Sub

    Private Sub BuildForm()
        Me.Text = "Agent Ledger - Daily Report"
        Me.Width = 900
        Me.Height = 650
        Me.StartPosition = FormStartPosition.CenterScreen

        Dim heading As New Label With {
            .Text = "DAILY BUSINESS REPORT",
            .Left = 20,
            .Top = 15,
            .Width = 500,
            .Height = 35,
            .Font = New Font("Arial", 16, FontStyle.Bold)
        }

        lblOpening = MakeLabel(20, 70, 800)
        lblTransactions = MakeLabel(20, 110, 800)
        lblExpenses = MakeLabel(20, 150, 800)
        lblCheckpoint = MakeLabel(20, 190, 800)

        Dim btnRefresh As New Button With {
            .Text = "Refresh Report",
            .Left = 20,
            .Top = 235,
            .Width = 150,
            .Height = 35
        }

        grid = New DataGridView With {
            .Left = 20,
            .Top = 290,
            .Width = 830,
            .Height = 280,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill
        }

        Me.Controls.AddRange(New Control() {
            heading, lblOpening, lblTransactions,
            lblExpenses, lblCheckpoint, btnRefresh, grid
        })

        AddHandler btnRefresh.Click, AddressOf RefreshReport
    End Sub

    Private Function MakeLabel(x As Integer,
                               y As Integer,
                               w As Integer) As Label

        Dim lbl As New Label With {
            .Left = x,
            .Top = y,
            .Width = w,
            .Height = 30,
            .Font = New Font("Arial", 10, FontStyle.Regular)
        }

        Return lbl
    End Function

    Private Function GetOpenDayId(
        connection As SqliteConnection) As Integer

        Using command As New SqliteCommand(
            "SELECT Id FROM BusinessDays " &
            "WHERE Status = 'Open' " &
            "ORDER BY Id DESC LIMIT 1", connection)

            Dim result = command.ExecuteScalar()

            If result Is Nothing OrElse result Is DBNull.Value Then
                Return 0
            End If

            Return Convert.ToInt32(result)
        End Using
    End Function

    Private Function GetTotal(
        connection As SqliteConnection,
        sql As String,
        dayId As Integer) As Decimal

        Using command As New SqliteCommand(sql, connection)

            command.Parameters.AddWithValue("@day", dayId)

            Dim result = command.ExecuteScalar()

            If result Is Nothing OrElse result Is DBNull.Value Then
                Return 0D
            End If

            Return Convert.ToDecimal(result)
        End Using
    End Function

    Private Sub LoadReport()

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim dayId As Integer = GetOpenDayId(connection)

            If dayId = 0 Then
                lblOpening.Text = "Opening balances: No open business day."
                lblTransactions.Text = "Transactions: N/A"
                lblExpenses.Text = "Expenses: N/A"
                lblCheckpoint.Text = "Latest checkpoint: N/A"
                grid.DataSource = Nothing

                MessageBox.Show(
                    "Open a business day before viewing its report.")
                Return
            End If

            Dim openingCash As Decimal = 0D
            Dim openingMTN As Decimal = 0D
            Dim openingAirtel As Decimal = 0D

            Using command As New SqliteCommand(
                "SELECT OpeningCash, MTNFloat, AirtelFloat " &
                "FROM BusinessDays WHERE Id = @day", connection)

                command.Parameters.AddWithValue("@day", dayId)

                Using reader = command.ExecuteReader()
                    If reader.Read() Then
                        openingCash = Convert.ToDecimal(reader("OpeningCash"))
                        openingMTN = Convert.ToDecimal(reader("MTNFloat"))
                        openingAirtel = Convert.ToDecimal(reader("AirtelFloat"))
                    End If
                End Using
            End Using

            lblOpening.Text =
                $"Opening balances — Cash: UGX {openingCash:N0} | " &
                $"MTN: UGX {openingMTN:N0} | " &
                $"Airtel: UGX {openingAirtel:N0}"

            Dim transactionCount As Decimal = GetTotal(
                connection,
                "SELECT COUNT(*) FROM Transactions " &
                "WHERE BusinessDayId = @day",
                dayId)

            Dim transactionAmount As Decimal = GetTotal(
                connection,
                "SELECT COALESCE(SUM(Amount), 0) FROM Transactions " &
                "WHERE BusinessDayId = @day",
                dayId)

            lblTransactions.Text =
                $"Transactions: {transactionCount:N0} recorded | " &
                $"Total recorded amount: UGX {transactionAmount:N0}"

            Dim expenseTotal As Decimal = GetTotal(
                connection,
                "SELECT COALESCE(SUM(Amount), 0) FROM Expenses " &
                "WHERE BusinessDayId = @day",
                dayId)

            lblExpenses.Text =
                $"Total expenses: UGX {expenseTotal:N0}"

            Using command As New SqliteCommand(
                "SELECT CashBalance, MTNFloat, AirtelFloat, " &
                "CheckpointTime FROM Checkpoints " &
                "WHERE BusinessDayId = @day " &
                "ORDER BY Id DESC LIMIT 1", connection)

                command.Parameters.AddWithValue("@day", dayId)

                Using reader = command.ExecuteReader()
                    If reader.Read() Then
                        lblCheckpoint.Text =
                            $"Latest checkpoint — Cash: UGX " &
                            $"{Convert.ToDecimal(reader("CashBalance")):N0} | " &
                            $"MTN: UGX " &
                            $"{Convert.ToDecimal(reader("MTNFloat")):N0} | " &
                            $"Airtel: UGX " &
                            $"{Convert.ToDecimal(reader("AirtelFloat")):N0} | " &
                            $"{reader("CheckpointTime")}"
                    Else
                        lblCheckpoint.Text =
                            "Latest checkpoint: No checkpoint recorded."
                    End If
                End Using
            End Using

            Dim table As New DataTable()

            Using command As New SqliteCommand(
                "SELECT Network, TransactionType, Amount, " &
                "CustomerPhone, ReferenceNumber, TransactionTime " &
                "FROM Transactions WHERE BusinessDayId = @day " &
                "ORDER BY Id DESC", connection)

                command.Parameters.AddWithValue("@day", dayId)

                Using reader = command.ExecuteReader()
                    table.Load(reader)
                End Using
            End Using

            grid.DataSource = table
        End Using
    End Sub

    Private Sub RefreshReport(sender As Object, e As EventArgs)
        LoadReport()
    End Sub

End Class