Imports System.Data.SQLite
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.Sqlite

Public Class AlertsForm
    Inherits Form

    Private ReadOnly lstAlerts As New ListBox()

    Public Sub New()
        Me.Text = "Agent Ledger - Alerts"
        Me.Width = 650
        Me.Height = 430
        Me.StartPosition = FormStartPosition.CenterScreen

        Dim heading As New Label With {
            .Text = "BUSINESS ALERTS",
            .Dock = DockStyle.Top,
            .Height = 45,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Arial", 14, FontStyle.Bold)
        }
        Me.Controls.Add(heading)

        lstAlerts.Left = 20
        lstAlerts.Top = 60
        lstAlerts.Width = 590
        lstAlerts.Height = 270
        lstAlerts.Font = New Font("Arial", 11)
        Me.Controls.Add(lstAlerts)

        Dim btnRefresh As New Button With {
            .Text = "Refresh Alerts",
            .Left = 20,
            .Top = 345,
            .Width = 140,
            .Height = 35
        }
        AddHandler btnRefresh.Click, AddressOf LoadAlerts
        Me.Controls.Add(btnRefresh)

        AddHandler Me.Load, AddressOf LoadAlerts
    End Sub

    Private Sub LoadAlerts(sender As Object, e As EventArgs)
        lstAlerts.Items.Clear()

        Try
            Using con As New SQLiteConnection(
                DatabaseHelper.GetConnectionString())

                con.Open()

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
                            lstAlerts.Items.Add(
                                "ALERT: No business day is currently open.")
                            Return
                        End If

                        dayId = Convert.ToInt32(r("Id"))
                        openingCash = Convert.ToDecimal(r("OpeningCash"))
                        openingMTN = Convert.ToDecimal(r("MTNFloat"))
                        openingAirtel = Convert.ToDecimal(r("AirtelFloat"))
                    End Using
                End Using

                Dim deposits As Decimal = 0D
                Dim withdrawals As Decimal = 0D
                Dim mtnDeposits As Decimal = 0D
                Dim mtnWithdrawals As Decimal = 0D
                Dim airtelDeposits As Decimal = 0D
                Dim airtelWithdrawals As Decimal = 0D

                Dim sql As String =
                    "SELECT " &
                    "COALESCE(SUM(CASE WHEN lower(TransactionType)='deposit' THEN Amount ELSE 0 END),0) AS D, " &
                    "COALESCE(SUM(CASE WHEN lower(TransactionType)='withdrawal' THEN Amount ELSE 0 END),0) AS W, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='mtn' AND lower(TransactionType)='deposit' THEN Amount ELSE 0 END),0) AS MD, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='mtn' AND lower(TransactionType)='withdrawal' THEN Amount ELSE 0 END),0) AS MW, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='airtel' AND lower(TransactionType)='deposit' THEN Amount ELSE 0 END),0) AS AD, " &
                    "COALESCE(SUM(CASE WHEN lower(Network)='airtel' AND lower(TransactionType)='withdrawal' THEN Amount ELSE 0 END),0) AS AW " &
                    "FROM Transactions WHERE BusinessDayId=@id"

                Using cmd As New SQLiteCommand(sql, con)
                    cmd.Parameters.AddWithValue("@id", dayId)

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            deposits = Convert.ToDecimal(r("D"))
                            withdrawals = Convert.ToDecimal(r("W"))
                            mtnDeposits = Convert.ToDecimal(r("MD"))
                            mtnWithdrawals = Convert.ToDecimal(r("MW"))
                            airtelDeposits = Convert.ToDecimal(r("AD"))
                            airtelWithdrawals = Convert.ToDecimal(r("AW"))
                        End If
                    End Using
                End Using

                Dim expenses As Decimal

                Using cmd As New SQLiteCommand(
                    "SELECT COALESCE(SUM(Amount),0) FROM Expenses " &
                    "WHERE BusinessDayId=@id", con)

                    cmd.Parameters.AddWithValue("@id", dayId)
                    expenses = Convert.ToDecimal(cmd.ExecuteScalar())
                End Using

                Dim expectedCash As Decimal =
                    openingCash + deposits - withdrawals - expenses

                Dim expectedMTN As Decimal =
                    openingMTN - mtnDeposits + mtnWithdrawals

                Dim expectedAirtel As Decimal =
                    openingAirtel - airtelDeposits + airtelWithdrawals

                ' Check the latest actual checkpoint.
                Dim hasCheckpoint As Boolean = False
                Dim actualCash As Decimal
                Dim actualMTN As Decimal
                Dim actualAirtel As Decimal

                Using cmd As New SQLiteCommand(
                    "SELECT CashBalance, MTNFloat, AirtelFloat " &
                    "FROM Checkpoints WHERE BusinessDayId=@id " &
                    "ORDER BY CheckpointTime DESC, Id DESC LIMIT 1", con)

                    cmd.Parameters.AddWithValue("@id", dayId)

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            hasCheckpoint = True
                            actualCash = Convert.ToDecimal(r("CashBalance"))
                            actualMTN = Convert.ToDecimal(r("MTNFloat"))
                            actualAirtel = Convert.ToDecimal(r("AirtelFloat"))
                        End If
                    End Using
                End Using

                If Not hasCheckpoint Then
                    lstAlerts.Items.Add(
                        "WARNING: Record a checkpoint for this business day.")
                Else
                    CheckVariance("Cash", actualCash, expectedCash)
                    CheckVariance("MTN float", actualMTN, expectedMTN)
                    CheckVariance("Airtel float", actualAirtel, expectedAirtel)
                End If

                ' Example minimum float threshold: UGX 50,000.
                If expectedMTN < 50000D Then
                    lstAlerts.Items.Add(
                        "LOW FLOAT: MTN expected float is below UGX 50,000.")
                End If

                If expectedAirtel < 50000D Then
                    lstAlerts.Items.Add(
                        "LOW FLOAT: Airtel expected float is below UGX 50,000.")
                End If

                If lstAlerts.Items.Count = 0 Then
                    lstAlerts.Items.Add(
                        "No alerts. Your balances pass the current checks.")
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Could not load alerts: " & ex.Message)
        End Try
    End Sub

    Private Sub CheckVariance(
        name As String,
        actual As Decimal,
        expected As Decimal)

        Dim difference As Decimal = actual - expected

        If difference < 0D Then
            lstAlerts.Items.Add(
                "SHORTAGE: " & name & " is UGX " &
                Math.Abs(difference).ToString("N0") & " below expected.")
        ElseIf difference > 0D Then
            lstAlerts.Items.Add(
                "SURPLUS: " & name & " is UGX " &
                difference.ToString("N0") & " above expected.")
        End If
    End Sub

End Class