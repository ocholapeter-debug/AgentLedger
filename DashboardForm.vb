Public Class DashboardForm

    Private userRole As String

    Public Sub New(role As String)
        InitializeComponent()

        userRole = role
        lblWelcome.Text = "Agent Ledger - " & userRole
    End Sub

    Private Sub btnOpenDay_Click(
        sender As Object, e As EventArgs
    ) Handles btnOpenDay.Click

        Form1.ShowDialog()
    End Sub



    ''  Private Sub btnExpenses_Click(sender As Object, e As EventArgs) Handles btnExpenses.Click

    ''    MessageBox.Show(
    ''      "Expense management will be added in a future step.")
    ''End Sub

    ''Private Sub btnCheckpoints_Click(
    ''  sender As Object, e As EventArgs
    '') Handles btnCheckpoints.Click

    ''  MessageBox.Show(
    ''    "Checkpoint recording will be added in a future step.")
    ''End Sub

    Private Sub btnCloseDay_Click(
        sender As Object, e As EventArgs
    ) Handles btnCloseDay.Click

        MessageBox.Show(
            "Day closing will be implemented in a future step.")
    End Sub

    ''Private Sub btnReports_Click(
    ''  sender As Object, e As EventArgs
    '') Handles btnReports.Click

    ''  MessageBox.Show(
    ''    "Reports will be implemented in a future step.")
    '' End Sub

    Private Sub btnLogout_Click(
        sender As Object, e As EventArgs
    ) Handles btnLogout.Click

        Dim login As New LoginForm()
        login.Show()
        Me.Close()
    End Sub

    Private Sub btnTransactions_Click_1(sender As Object, e As EventArgs) Handles btnTransactions.Click
        ''  MessageBox.Show(
        '' "Transaction recording will be added in Day Four.")
        Using frm As New TransactionForm()
            frm.ShowDialog()


        End Using

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnTransactions.Click

    End Sub

    Private Sub btnExpenses_Click(sender As Object, e As EventArgs) Handles btnExpenses.Click
        Using frm As New ExpensesForm()
            frm.ShowDialog()
        End Using
    End Sub


    Private Sub btnCheckpoints_Click(
    sender As Object, e As EventArgs
) Handles btnCheckpoints.Click

        Using frm As New CheckpointsForm()
            frm.ShowDialog()
        End Using

    End Sub


    Private Sub btnReports_Click(
    sender As Object, e As EventArgs
) Handles btnReports.Click

        Using frm As New ReportsForm()
            frm.ShowDialog()
        End Using

    End Sub


    Private Sub btnBalanceVariance_Click(sender As Object, e As EventArgs) Handles btnBalanceVariance.Click

        Using frm As New BalanceVarianceForm()
            frm.ShowDialog()
        End Using

    End Sub

    Private Sub btnAlerts_Click(
    sender As Object,
    e As EventArgs) Handles btnAlerts.Click

        Using frm As New AlertsForm()
            frm.ShowDialog()
        End Using

    End Sub

    Private Sub btnSearch_Click(
    sender As Object,
    e As EventArgs) Handles btnSearch.Click

        Using frm As New SearchForm()
            frm.ShowDialog()
        End Using

    End Sub



End Class