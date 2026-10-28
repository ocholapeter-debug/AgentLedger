Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Globalization

Public Class CheckpointsForm

    Private txtCash As TextBox
    Private txtMTN As TextBox
    Private txtAirtel As TextBox
    Private txtNotes As TextBox
    Private grid As DataGridView

    Public Sub New()
        InitializeComponent()
        BuildForm()
        DatabaseHelper.CreateCheckpointsTable()
        LoadCheckpoints()
    End Sub

    Private Sub BuildForm()
        Me.Text = "Agent Ledger - Cash and Float Checkpoints"
        Me.Width = 900
        Me.Height = 650
        Me.StartPosition = FormStartPosition.CenterScreen

        AddFieldLabel("Physical Cash (UGX)", 20, 20)
        txtCash = AddTextBox(190, 18, 220)

        AddFieldLabel("MTN Float (UGX)", 20, 60)
        txtMTN = AddTextBox(190, 58, 220)

        AddFieldLabel("Airtel Float (UGX)", 20, 100)
        txtAirtel = AddTextBox(190, 98, 220)

        AddFieldLabel("Notes", 20, 140)
        txtNotes = AddTextBox(190, 138, 400)

        txtNotes.Multiline = True
        txtNotes.Height = 50

        Dim btnSave As New Button With {
            .Text = "Save Checkpoint",
            .Left = 190,
            .Top = 205,
            .Width = 160,
            .Height = 35
        }

        grid = New DataGridView With {
            .Left = 20,
            .Top = 260,
            .Width = 830,
            .Height = 320,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill
        }

        Me.Controls.Add(btnSave)
        Me.Controls.Add(grid)

        AddHandler btnSave.Click, AddressOf SaveCheckpoint
    End Sub

    Private Sub AddFieldLabel(caption As String,
                              x As Integer, y As Integer)

        Dim lbl As New Label With {
            .Text = caption,
            .Left = x,
            .Top = y,
            .Width = 165,
            .Height = 25
        }

        Me.Controls.Add(lbl)
    End Sub

    Private Function AddTextBox(x As Integer, y As Integer,
                                boxWidth As Integer) As TextBox

        Dim txt As New TextBox With {
            .Left = x,
            .Top = y,
            .Width = boxWidth
        }

        Me.Controls.Add(txt)
        Return txt
    End Function

    Private Function GetOpenBusinessDayId() As Integer

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "SELECT Id FROM BusinessDays " &
                "WHERE Status = 'Open' " &
                "ORDER BY Id DESC LIMIT 1"

            Using command As New SqliteCommand(sql, connection)

                Dim result = command.ExecuteScalar()

                If result Is Nothing OrElse result Is DBNull.Value Then
                    Return 0
                End If

                Return Convert.ToInt32(result)
            End Using
        End Using
    End Function

    Private Function ReadAmount(txt As TextBox,
                                fieldName As String,
                                ByRef amount As Decimal) As Boolean

        If Not Decimal.TryParse(
            txt.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            amount) OrElse amount < 0 Then

            MessageBox.Show(
                fieldName & " must be a valid amount of zero or more.")

            txt.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub SaveCheckpoint(sender As Object, e As EventArgs)

        Dim cash As Decimal
        Dim mtn As Decimal
        Dim airtel As Decimal

        If Not ReadAmount(txtCash, "Physical cash", cash) Then Return
        If Not ReadAmount(txtMTN, "MTN float", mtn) Then Return
        If Not ReadAmount(txtAirtel, "Airtel float", airtel) Then Return

        Dim dayId As Integer = GetOpenBusinessDayId()

        If dayId = 0 Then
            MessageBox.Show(
                "No business day is open. Open a business day first.")
            Return
        End If

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "INSERT INTO Checkpoints " &
                "(BusinessDayId, CashBalance, MTNFloat, " &
                "AirtelFloat, Notes, CheckpointTime) " &
                "VALUES (@day, @cash, @mtn, @airtel, @notes, @time)"

            Using command As New SqliteCommand(sql, connection)

                command.Parameters.AddWithValue("@day", dayId)
                command.Parameters.AddWithValue("@cash", CDbl(cash))
                command.Parameters.AddWithValue("@mtn", CDbl(mtn))
                command.Parameters.AddWithValue("@airtel", CDbl(airtel))
                command.Parameters.AddWithValue(
                    "@notes", txtNotes.Text.Trim())
                command.Parameters.AddWithValue(
                    "@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                command.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("Checkpoint saved successfully!")

        txtCash.Clear()
        txtMTN.Clear()
        txtAirtel.Clear()
        txtNotes.Clear()

        LoadCheckpoints()
    End Sub

    Private Sub LoadCheckpoints()

        Dim table As New DataTable()

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "SELECT CashBalance AS [Cash (UGX)], " &
                "MTNFloat AS [MTN Float (UGX)], " &
                "AirtelFloat AS [Airtel Float (UGX)], " &
                "Notes, CheckpointTime AS [Recorded At] " &
                "FROM Checkpoints ORDER BY Id DESC"

            Using command As New SqliteCommand(sql, connection)
                Using reader = command.ExecuteReader()
                    table.Load(reader)
                End Using
            End Using
        End Using

        grid.DataSource = table
    End Sub

End Class