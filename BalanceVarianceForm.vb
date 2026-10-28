Imports System.Data
Imports System.Data.SQLite
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.Sqlite

Public Class BalanceVarianceForm
    Inherits Form

    Private ReadOnly grid As New DataGridView()
    Private ReadOnly lblStatus As New Label()

    Public Sub New()
        Me.Text = "Balance Calculation and Variance"
        Me.Width = 850
        Me.Height = 450
        Me.StartPosition = FormStartPosition.CenterScreen

        Dim heading As New Label With {
            .Text = "EXPECTED BALANCES VS ACTUAL BALANCES",
            .Dock = DockStyle.Top,
            .Height = 45,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Arial", 12, FontStyle.Bold)
        }
        Me.Controls.Add(heading)

        Dim btnRefresh As New Button With {
            .Text = "Calculate Balances",
            .Width = 160,
            .Height = 35,
            .Left = 20,
            .Top = 55
        }
        AddHandler btnRefresh.Click, AddressOf LoadBalances
        Me.Controls.Add(btnRefresh)

        lblStatus.Left = 200
        lblStatus.Top = 60
        lblStatus.Width = 600
        lblStatus.Height = 30
        Me.Controls.Add(lblStatus)

        grid.Left = 20
        grid.Top = 105
        grid.Width = 790
        grid.Height = 280
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill
        grid.RowHeadersVisible = False
        Me.Controls.Add(grid)

        AddHandler Me.Load, AddressOf LoadBalances
    End Sub

    Private Sub LoadBalances(sender As Object, e As EventArgs)
        Try
            Using con As New SQLiteConnection(
                DatabaseHelper.GetConnectionString())

                con.Open()

                ' Find today's open business day.
                Dim dayId As Integer
                Dim openingCash As Decimal
                Dim openingMTN As Decimal
                Dim openingAirtel As Decimal

                Using cmd As New SQLiteCommand(
                    "SELECT Id, OpeningCash, MTNFloat, AirtelFloat " &
                    "FROM BusinessDays WHERE Status='Open' " &
                    "ORDER BY Id DESC LIMIT 1", con)

                    Using r = cmd.ExecuteReader()
                        If Not r.Read() Then
                            MessageBox.Show(
                                "No open business day. Open a business day first.")
                            Return
                        End If

                        dayId = Convert.ToInt32(r("Id"))
                        openingCash = Convert.ToDecimal(r("OpeningCash"))
                        openingMTN = Convert.ToDecimal(r("MTNFloat"))
                        openingAirtel = Convert.ToDecimal(r("AirtelFloat"))
                    End Using
                End Using

                ' Calculate transaction totals.
                Dim cashDeposits As Decimal = 0D
                Dim cashWithdrawals As Decimal = 0D
                Dim mtnDeposits As Decimal = 0D
                Dim mtnWithdrawals As Decimal = 0D
                Dim airtelDeposits As Decimal = 0D
                Dim airtelWithdrawals As Decimal = 0D

                Dim transactionSQL As String =
                    "SELECT " &
                    "COALESCE(SUM(CASE WHEN lower(TransactionType)='deposit' " &
                    "THEN Amount ELSE 0 END),0) AS Deposits, " &
                    "COALESCE(SUM(CASE WHEN lower(TransactionType)='withdrawal' " &
                    "THEN Amount ELSE 0 END),0) AS Withdrawals, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='mtn' AND " &
                    "lower(TransactionType)='deposit' THEN Amount ELSE 0 END),0) AS MTNDeposits, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='mtn' AND " &
                    "lower(TransactionType)='withdrawal' THEN Amount ELSE 0 END),0) AS MTNWithdrawals, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='airtel' AND " &
                    "lower(TransactionType)='deposit' THEN Amount ELSE 0 END),0) AS AirtelDeposits, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='airtel' AND " &
                    "lower(TransactionType)='withdrawal' THEN Amount ELSE 0 END),0) AS AirtelWithdrawals " &
                    "FROM Transactions WHERE BusinessDayId=@dayId"

                Using cmd As New SQLiteCommand(transactionSQL, con)
                    cmd.Parameters.AddWithValue("@dayId", dayId)

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            cashDeposits = Convert.ToDecimal(r("Deposits"))
                            cashWithdrawals = Convert.ToDecimal(r("Withdrawals"))
                            mtnDeposits = Convert.ToDecimal(r("MTNDeposits"))
                            mtnWithdrawals = Convert.ToDecimal(r("MTNWithdrawals"))
                            airtelDeposits = Convert.ToDecimal(r("AirtelDeposits"))
                            airtelWithdrawals = Convert.ToDecimal(r("AirtelWithdrawals"))
                        End If
                    End Using
                End Using

                ' All recorded expenses are assumed to be paid in cash.
                Dim expenses As Decimal = 0D

                Using cmd As New SQLiteCommand(
                    "SELECT COALESCE(SUM(Amount),0) FROM Expenses " &
                    "WHERE BusinessDayId=@dayId", con)

                    cmd.Parameters.AddWithValue("@dayId", dayId)
                    expenses = Convert.ToDecimal(cmd.ExecuteScalar())
                End Using

                Dim expectedCash As Decimal =
                    openingCash + cashDeposits - cashWithdrawals - expenses

                Dim expectedMTN As Decimal =
                    openingMTN - mtnDeposits + mtnWithdrawals

                Dim expectedAirtel As Decimal =
                    openingAirtel - airtelDeposits + airtelWithdrawals

                ' Get the latest actual checkpoint.
                Dim hasCheckpoint As Boolean = False
                Dim actualCash As Decimal = 0D
                Dim actualMTN As Decimal = 0D
                Dim actualAirtel As Decimal = 0D

                Using cmd As New SQLiteCommand(
                    "SELECT CashBalance, MTNFloat, AirtelFloat " &
                    "FROM Checkpoints WHERE BusinessDayId=@dayId " &
                    "ORDER BY CheckpointTime DESC, Id DESC LIMIT 1", con)

                    cmd.Parameters.AddWithValue("@dayId", dayId)

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            hasCheckpoint = True
                            actualCash = Convert.ToDecimal(r("CashBalance"))
                            actualMTN = Convert.ToDecimal(r("MTNFloat"))
                            actualAirtel = Convert.ToDecimal(r("AirtelFloat"))
                        End If
                    End Using
                End Using

                Dim result As New DataTable()
                result.Columns.Add("Balance")
                result.Columns.Add("Expected (UGX)")
                result.Columns.Add("Actual (UGX)")
                result.Columns.Add("Variance (UGX)")
                result.Columns.Add("Status")

                AddBalanceRow(result, "Cash", expectedCash,
                              actualCash, hasCheckpoint)

                AddBalanceRow(result, "MTN Float", expectedMTN,
                              actualMTN, hasCheckpoint)

                AddBalanceRow(result, "Airtel Float", expectedAirtel,
                              actualAirtel, hasCheckpoint)

                grid.DataSource = result
                lblStatus.Text = If(
                    hasCheckpoint,
                    "Compared with your latest checkpoint.",
                    "No checkpoint yet. Actual balances will appear after you record one.")
            End Using

        Catch ex As Exception
            MessageBox.Show("Could not calculate balances: " &
                            ex.Message)
        End Try
    End Sub

    Private Sub AddBalanceRow(
        result As DataTable,
        name As String,
        expected As Decimal,
        actual As Decimal,
        hasCheckpoint As Boolean)

        Dim actualText As String = "Not recorded"
        Dim varianceText As String = "Not available"
        Dim status As String = "Awaiting checkpoint"

        If hasCheckpoint Then
            Dim variance As Decimal = actual - expected

            actualText = actual.ToString("N0")
            varianceText = variance.ToString("N0")

            If variance < 0D Then
                status = "SHORTAGE"
            ElseIf variance > 0D Then
                status = "SURPLUS"
            Else
                status = "BALANCED"
            End If
        End If

        result.Rows.Add(
            name,
            expected.ToString("N0"),
            actualText,
            varianceText,
            status)
    End Sub

End Class