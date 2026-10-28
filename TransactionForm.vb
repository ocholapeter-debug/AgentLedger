Imports Microsoft.Data.Sqlite
Imports System.Globalization

Public Class TransactionForm

    Private cboNetwork As New ComboBox()
    Private cboType As New ComboBox()
    Private txtAmount As New TextBox()
    Private txtPhone As New TextBox()
    Private txtReference As New TextBox()
    Private grid As New DataGridView()
    Private lblStatus As New Label()

    Public Sub New()
        InitializeComponent()
        BuildForm()
        CreateTransactionTable()
        LoadTransactions()
    End Sub

    Private Sub BuildForm()
        Me.Text = "Agent Ledger - Transactions"
        Me.Size = New Size(850, 650)
        Me.StartPosition = FormStartPosition.CenterScreen

        AddLabel("Network", 25, 25)
        cboNetwork.Items.AddRange(
            New Object() {"MTN", "Airtel"})
        cboNetwork.SetBounds(180, 25, 150, 25)
        cboNetwork.DropDownStyle = ComboBoxStyle.DropDownList
        cboNetwork.SelectedIndex = -1
        Me.Controls.Add(cboNetwork)

        AddLabel("Transaction Type", 25, 70)
        cboType.Items.AddRange(
            New Object() {"Deposit", "Withdrawal"})
        cboType.SetBounds(180, 70, 150, 25)
        cboType.DropDownStyle = ComboBoxStyle.DropDownList
        cboNetwork.SelectedIndex = -1
        Me.Controls.Add(cboType)

        AddLabel("Amount (UGX)", 25, 115)
        txtAmount.SetBounds(180, 115, 150, 25)
        Me.Controls.Add(txtAmount)

        AddLabel("Customer Phone (optional)", 25, 160)
        txtPhone.SetBounds(230, 160, 150, 25)
        Me.Controls.Add(txtPhone)

        AddLabel("Reference (optional)", 25, 205)
        txtReference.SetBounds(180, 205, 250, 28)
        Me.Controls.Add(txtReference)

        Dim btnSave As New Button()
        btnSave.Text = "Save Transaction"
        btnSave.SetBounds(180, 250, 160, 38)
        AddHandler btnSave.Click, AddressOf SaveTransaction
        Me.Controls.Add(btnSave)

        lblStatus.SetBounds(25, 300, 760, 30)
        Me.Controls.Add(lblStatus)

        grid.SetBounds(25, 340, 770, 240)
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(grid)
    End Sub

    Private Sub AddLabel(caption As String,
                         x As Integer, y As Integer)

        Dim lbl As New Label()
        lbl.Text = caption
        lbl.SetBounds(x, y, 220, 28)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub CreateTransactionTable()
        Using con As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            con.Open()

            Dim sql As String =
                "CREATE TABLE IF NOT EXISTS Transactions (" &
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                "BusinessDayId INTEGER NOT NULL, " &
                "Network TEXT NOT NULL, " &
                "TransactionType TEXT NOT NULL, " &
                "Amount REAL NOT NULL CHECK(Amount > 0), " &
                "CustomerPhone TEXT, " &
                "ReferenceNumber TEXT, " &
                "TransactionTime TEXT NOT NULL)"

            Using cmd As New SqliteCommand(sql, con)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Sub SaveTransaction(sender As Object,
                                e As EventArgs)

        If cboNetwork.SelectedIndex = -1 OrElse
           cboType.SelectedIndex = -1 Then

            MessageBox.Show("Select a network and transaction type.")
            Return
        End If

        Dim amount As Decimal

        If Not Decimal.TryParse(
            txtAmount.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            amount) OrElse amount <= 0D Then

            MessageBox.Show("Enter an amount greater than zero.")
            txtAmount.Focus()
            Return
        End If

        Try
            Using con As New SqliteConnection(
                DatabaseHelper.GetConnectionString())

                con.Open()

                Dim dayId As Integer

                Using check As New SqliteCommand(
                    "SELECT Id FROM BusinessDays " &
                    "WHERE Status = 'Open' LIMIT 1", con)

                    Dim result = check.ExecuteScalar()

                    If result Is Nothing Then
                        MessageBox.Show(
                            "Open a business day before recording transactions.")
                        Return
                    End If

                    dayId = Convert.ToInt32(result)
                End Using

                Dim sql As String =
                    "INSERT INTO Transactions " &
                    "(BusinessDayId, Network, TransactionType, " &
                    "Amount, CustomerPhone, ReferenceNumber, " &
                    "TransactionTime) " &
                    "VALUES ($day, $network, $type, $amount, " &
                    "$phone, $reference, $time)"

                Using cmd As New SqliteCommand(sql, con)
                    cmd.Parameters.AddWithValue("$day", dayId)
                    cmd.Parameters.AddWithValue(
                        "$network", cboNetwork.Text)
                    cmd.Parameters.AddWithValue(
                        "$type", cboType.Text)
                    cmd.Parameters.AddWithValue("$amount", amount)
                    cmd.Parameters.AddWithValue(
                        "$phone", txtPhone.Text.Trim())
                    cmd.Parameters.AddWithValue(
                        "$reference", txtReference.Text.Trim())
                    cmd.Parameters.AddWithValue(
                        "$time", DateTime.Now.ToString("o"))

                    cmd.ExecuteNonQuery()
                End Using
            End Using

            lblStatus.Text = "Transaction saved successfully."
            txtAmount.Clear()
            txtPhone.Clear()
            txtReference.Clear()
            LoadTransactions()

        Catch ex As Exception
            MessageBox.Show(
                "Could not save transaction: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadTransactions()
        Using con As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            con.Open()

            Dim sql As String =
                "SELECT Id, Network, TransactionType, Amount, " &
                "CustomerPhone, ReferenceNumber, TransactionTime " &
                "FROM Transactions ORDER BY Id DESC"

            Using cmd As New SqliteCommand(sql, con)
                Using reader = cmd.ExecuteReader()
                    Dim table As New DataTable()
                    table.Load(reader)
                    grid.DataSource = table
                End Using
            End Using
        End Using
    End Sub

End Class