Public Class LoginForm
    Public Sub New()
        InitializeComponent()
        DatabaseHelper.InitializeDatabase()

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles lblTitle.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles lblUsername.Click

    End Sub

    Private Sub btnLogin_Click(sender As Object,
                           e As EventArgs) Handles btnLogin.Click

        Try
            Dim role As String =
            DatabaseHelper.ValidateUser(
                txtUsername.Text,
                txtPassword.Text)

            If role Is Nothing Then
                lblMessage.Text =
                "Invalid username or password, or account inactive."
                txtPassword.Clear()
                Return
            End If

            Dim dashboard As New DashboardForm(role)
            dashboard.Show()
            Me.Hide()

        Catch ex As Exception
            MessageBox.Show(
            "Login error: " & ex.Message,
            "Agent Ledger")
        End Try

    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class