Imports System.Data
Imports Microsoft.Data.Sqlite
Imports System.Drawing
Imports System.Windows.Forms
''Imports Microsoft.Data.Sqlite

Public Class SearchForm
    Inherits Form

    Private ReadOnly txtSearch As New TextBox()
    Private ReadOnly cboNetwork As New ComboBox()
    Private ReadOnly cboType As New ComboBox()
    Private ReadOnly grid As New DataGridView()
    Private ReadOnly lblResults As New Label()

    Public Sub New()
        Me.Text = "Agent Ledger - Search Transactions"
        Me.Width = 1000
        Me.Height = 600
        Me.StartPosition = FormStartPosition.CenterScreen

        Dim heading As New Label With {
            .Text = "SEARCH AND VIEW TRANSACTIONS",
            .Left = 20,
            .Top = 15,
            .Width = 900,
            .Height = 35,
            .Font = New Font("Arial", 14, FontStyle.Bold)
        }
        Me.Controls.Add(heading)

        Dim lblSearch As New Label With {
            .Text = "Phone or Reference:",
            .Left = 20,
            .Top = 70,
            .Width = 140
        }
        Me.Controls.Add(lblSearch)

        txtSearch.Left = 165
        txtSearch.Top = 65
        txtSearch.Width = 200
        Me.Controls.Add(txtSearch)

        Dim lblNetwork As New Label With {
            .Text = "Network:",
            .Left = 385,
            .Top = 70,
            .Width = 70
        }
        Me.Controls.Add(lblNetwork)

        cboNetwork.Left = 455
        cboNetwork.Top = 65
        cboNetwork.Width = 130
        cboNetwork.DropDownStyle = ComboBoxStyle.DropDownList
        cboNetwork.Items.AddRange(
            New Object() {"All", "MTN", "Airtel"})
        cboNetwork.SelectedIndex = 0
        Me.Controls.Add(cboNetwork)

        Dim lblType As New Label With {
            .Text = "Type:",
            .Left = 605,
            .Top = 70,
            .Width = 45
        }
        Me.Controls.Add(lblType)

        cboType.Left = 650
        cboType.Top = 65
        cboType.Width = 130
        cboType.DropDownStyle = ComboBoxStyle.DropDownList
        cboType.Items.AddRange(
            New Object() {"All", "Deposit", "Withdrawal"})
        cboType.SelectedIndex = 0
        Me.Controls.Add(cboType)

        Dim btnSearch As New Button With {
            .Text = "Search",
            .Left = 800,
            .Top = 63,
            .Width = 100,
            .Height = 30
        }
        AddHandler btnSearch.Click, AddressOf SearchTransactions
        Me.Controls.Add(btnSearch)

        Dim btnClear As New Button With {
            .Text = "Clear",
            .Left = 900,
            .Top = 63,
            .Width = 70,
            .Height = 30
        }
        AddHandler btnClear.Click, AddressOf ClearSearch
        Me.Controls.Add(btnClear)

        lblResults.Left = 20
        lblResults.Top = 110
        lblResults.Width = 900
        lblResults.Height = 25
        Me.Controls.Add(lblResults)

        grid.Left = 20
        grid.Top = 145
        grid.Width = 940
        grid.Height = 380
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect
        grid.RowHeadersVisible = False
        Me.Controls.Add(grid)

        AddHandler Me.Load, AddressOf SearchTransactions
    End Sub

    Private Sub SearchTransactions(
        sender As Object,
        e As EventArgs)

        Try
            Using con As New SqliteConnection(
                DatabaseHelper.GetConnectionString())

                con.Open()

                Dim sql As String =
                    "SELECT Id, Network, TransactionType, Amount, " &
                    "CustomerPhone, ReferenceNumber, TransactionTime " &
                    "FROM Transactions WHERE 1=1 "

                If txtSearch.Text.Trim() <> "" Then
                    sql &= "AND (CustomerPhone LIKE @search " &
                           "OR ReferenceNumber LIKE @search) "
                End If

                If cboNetwork.Text <> "All" Then
                    sql &= "AND lower(Network)=lower(@network) "
                End If

                If cboType.Text <> "All" Then
                    sql &= "AND lower(TransactionType)=lower(@type) "
                End If

                sql &= "ORDER BY TransactionTime DESC, Id DESC"

                Dim results As New DataTable()

                Using cmd As New SqliteCommand(sql, con)

                    If txtSearch.Text.Trim() <> "" Then
                        cmd.Parameters.AddWithValue(
                            "@search", "%" & txtSearch.Text.Trim() & "%")
                    End If

                    If cboNetwork.Text <> "All" Then
                        cmd.Parameters.AddWithValue(
                            "@network", cboNetwork.Text)
                    End If

                    If cboType.Text <> "All" Then
                        cmd.Parameters.AddWithValue(
                            "@type", cboType.Text)
                    End If

                    Using reader = cmd.ExecuteReader()
                        results.Load(reader)

                    End Using
                End Using

                grid.DataSource = results
                lblResults.Text =
                    "Transactions found: " & results.Rows.Count.ToString()

                If results.Rows.Count = 0 Then
                    lblResults.Text =
                        "No matching transactions were found."
                End If

                If grid.Columns.Contains("Id") Then
                    grid.Columns("Id").HeaderText = "Transaction ID"
                End If

                If grid.Columns.Contains("Network") Then
                    grid.Columns("Network").HeaderText = "Network"
                End If

                If grid.Columns.Contains("TransactionType") Then
                    grid.Columns("TransactionType").HeaderText = "Type"
                End If

                If grid.Columns.Contains("Amount") Then
                    grid.Columns("Amount").HeaderText = "Amount (UGX)"
                    grid.Columns("Amount").DefaultCellStyle.Format = "N0"
                End If

                If grid.Columns.Contains("CustomerPhone") Then
                    grid.Columns("CustomerPhone").HeaderText = "Customer Phone"
                End If

                If grid.Columns.Contains("ReferenceNumber") Then
                    grid.Columns("ReferenceNumber").HeaderText = "Reference"
                End If

                If grid.Columns.Contains("TransactionTime") Then
                    grid.Columns("TransactionTime").HeaderText = "Date and Time"
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show(
                "Unable to search transactions: " & ex.Message,
                "Search Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClearSearch(
        sender As Object,
        e As EventArgs)

        txtSearch.Clear()
        cboNetwork.SelectedIndex = 0
        cboType.SelectedIndex = 0
        SearchTransactions(sender, e)
    End Sub

End Class