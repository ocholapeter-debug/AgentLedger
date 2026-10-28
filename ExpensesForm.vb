Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Globalization

Public Class ExpensesForm

    Private cboCategory As ComboBox
    Private txtAmount As TextBox
    Private txtDescription As TextBox
    Private grid As DataGridView
    Private btnSave As Button

    Public Sub New()
        InitializeComponent()
        BuildForm()
        DatabaseHelper.CreateExpensesTable()
        LoadExpenses()
    End Sub

    Private Sub BuildForm()
        Me.Text = "Agent Ledger - Expenses"
        Me.Width = 850
        Me.Height = 600
        Me.StartPosition = FormStartPosition.CenterScreen

        Dim lblCategory As New Label With {
            .Text = "Expense category",
            .Left = 20, .Top = 20, .Width = 130
        }

        cboCategory = New ComboBox With {
            .Left = 160, .Top = 18, .Width = 220,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }

        cboCategory.Items.AddRange(New Object() {
            "Rent", "Transport", "Electricity", "Internet",
            "Salaries", "Bank Charges", "Other"
        })
        cboCategory.SelectedIndex = 0

        Dim lblAmount As New Label With {
            .Text = "Amount (UGX)",
            .Left = 20, .Top = 60, .Width = 130
        }

        txtAmount = New TextBox With {
            .Left = 160, .Top = 58, .Width = 220
        }

        Dim lblDescription As New Label With {
            .Text = "Description",
            .Left = 20, .Top = 100, .Width = 130
        }

        txtDescription = New TextBox With {
            .Left = 160, .Top = 98, .Width = 350
        }

        btnSave = New Button With {
            .Text = "Save Expense",
            .Left = 160, .Top = 140, .Width = 150, .Height = 35
        }

        grid = New DataGridView With {
            .Left = 20, .Top = 200, .Width = 790, .Height = 330,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
        }

        Me.Controls.AddRange(New Control() {
            lblCategory, cboCategory,
            lblAmount, txtAmount,
            lblDescription, txtDescription,
            btnSave, grid
        })

        AddHandler btnSave.Click, AddressOf SaveExpense
    End Sub

    Private Function GetOpenBusinessDayId() As Integer
        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "SELECT Id FROM BusinessDays " &
                "WHERE Status = 'Open' ORDER BY Id DESC LIMIT 1"

            Using command As New SqliteCommand(sql, connection)
                Dim result = command.ExecuteScalar()

                If result Is Nothing OrElse result Is DBNull.Value Then
                    Return 0
                End If

                Return Convert.ToInt32(result)
            End Using
        End Using
    End Function

    Private Sub SaveExpense(sender As Object, e As EventArgs)
        Dim amount As Decimal

        If Not Decimal.TryParse(
            txtAmount.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            amount) OrElse amount <= 0 Then

            MessageBox.Show("Enter a valid amount greater than zero.")
            txtAmount.Focus()
            Return
        End If

        Dim businessDayId As Integer = GetOpenBusinessDayId()

        If businessDayId = 0 Then
            MessageBox.Show(
                "No business day is open. Open a business day first.")
            Return
        End If

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "INSERT INTO Expenses " &
                "(BusinessDayId, Category, Amount, Description, ExpenseTime) " &
                "VALUES (@dayId, @category, @amount, @description, @time)"

            Using command As New SqliteCommand(sql, connection)
                command.Parameters.AddWithValue("@dayId", businessDayId)
                command.Parameters.AddWithValue("@category",
                                                  cboCategory.Text)
                command.Parameters.AddWithValue("@amount",
                                                  Convert.ToDouble(amount))
                command.Parameters.AddWithValue("@description",
                                                  txtDescription.Text.Trim())
                command.Parameters.AddWithValue("@time",
                                                  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                command.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("Expense saved successfully.")

        txtAmount.Clear()
        txtDescription.Clear()
        cboCategory.SelectedIndex = 0

        LoadExpenses()
    End Sub

    Private Sub LoadExpenses()
        Dim table As New DataTable()

        Using connection As New SqliteConnection(
            DatabaseHelper.GetConnectionString())

            connection.Open()

            Dim sql As String =
                "SELECT Category, Amount, Description, ExpenseTime " &
                "FROM Expenses ORDER BY Id DESC"

            Using command As New SqliteCommand(sql, connection)
                Using reader = command.ExecuteReader()
                    table.Load(reader)
                End Using
            End Using
        End Using

        grid.DataSource = table
    End Sub

End Class