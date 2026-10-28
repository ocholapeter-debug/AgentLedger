<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DashboardForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        lblWelcome = New Label()
        btnOpenDay = New Button()
        btnTransactions = New Button()
        btnExpenses = New Button()
        btnCheckpoints = New Button()
        btnCloseDay = New Button()
        btnReports = New Button()
        btnLogout = New Button()
        btnBalanceVariance = New Button()
        btnAlerts = New Button()
        btnSearch = New Button()
        SuspendLayout()
        ' 
        ' lblWelcome
        ' 
        lblWelcome.AutoSize = True
        lblWelcome.Location = New Point(105, 36)
        lblWelcome.Name = "lblWelcome"
        lblWelcome.Size = New Size(85, 25)
        lblWelcome.TabIndex = 0
        lblWelcome.Text = "Welcome"
        ' 
        ' btnOpenDay
        ' 
        btnOpenDay.Location = New Point(106, 77)
        btnOpenDay.Name = "btnOpenDay"
        btnOpenDay.Size = New Size(112, 34)
        btnOpenDay.TabIndex = 1
        btnOpenDay.Text = "Open"
        btnOpenDay.UseVisualStyleBackColor = True
        ' 
        ' btnTransactions
        ' 
        btnTransactions.Location = New Point(106, 130)
        btnTransactions.Name = "btnTransactions"
        btnTransactions.Size = New Size(124, 34)
        btnTransactions.TabIndex = 2
        btnTransactions.Text = "Transactions"
        btnTransactions.UseVisualStyleBackColor = True
        ' 
        ' btnExpenses
        ' 
        btnExpenses.Location = New Point(118, 187)
        btnExpenses.Name = "btnExpenses"
        btnExpenses.Size = New Size(112, 34)
        btnExpenses.TabIndex = 3
        btnExpenses.Text = "Expenses"
        btnExpenses.UseVisualStyleBackColor = True
        ' 
        ' btnCheckpoints
        ' 
        btnCheckpoints.Location = New Point(340, 77)
        btnCheckpoints.Name = "btnCheckpoints"
        btnCheckpoints.Size = New Size(112, 34)
        btnCheckpoints.TabIndex = 4
        btnCheckpoints.Text = "Checkpoints"
        btnCheckpoints.UseVisualStyleBackColor = True
        ' 
        ' btnCloseDay
        ' 
        btnCloseDay.Location = New Point(340, 130)
        btnCloseDay.Name = "btnCloseDay"
        btnCloseDay.Size = New Size(112, 34)
        btnCloseDay.TabIndex = 5
        btnCloseDay.Text = "CloseDay"
        btnCloseDay.UseVisualStyleBackColor = True
        ' 
        ' btnReports
        ' 
        btnReports.Location = New Point(353, 188)
        btnReports.Name = "btnReports"
        btnReports.Size = New Size(112, 34)
        btnReports.TabIndex = 6
        btnReports.Text = "Reports"
        btnReports.UseVisualStyleBackColor = True
        ' 
        ' btnLogout
        ' 
        btnLogout.Location = New Point(246, 297)
        btnLogout.Name = "btnLogout"
        btnLogout.Size = New Size(112, 34)
        btnLogout.TabIndex = 7
        btnLogout.Text = "Logout"
        btnLogout.UseVisualStyleBackColor = True
        ' 
        ' btnBalanceVariance
        ' 
        btnBalanceVariance.Location = New Point(542, 84)
        btnBalanceVariance.Name = "btnBalanceVariance"
        btnBalanceVariance.Size = New Size(220, 34)
        btnBalanceVariance.TabIndex = 8
        btnBalanceVariance.Text = "Balance && Variance"
        btnBalanceVariance.UseVisualStyleBackColor = True
        ' 
        ' btnAlerts
        ' 
        btnAlerts.Location = New Point(560, 137)
        btnAlerts.Name = "btnAlerts"
        btnAlerts.Size = New Size(112, 34)
        btnAlerts.TabIndex = 9
        btnAlerts.Text = "Alerts"
        btnAlerts.UseVisualStyleBackColor = True
        ' 
        ' btnSearch
        ' 
        btnSearch.Location = New Point(560, 188)
        btnSearch.Name = "btnSearch"
        btnSearch.Size = New Size(191, 34)
        btnSearch.TabIndex = 10
        btnSearch.Text = "Search Transactions"
        btnSearch.UseVisualStyleBackColor = True
        ' 
        ' DashboardForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnSearch)
        Controls.Add(btnAlerts)
        Controls.Add(btnBalanceVariance)
        Controls.Add(btnLogout)
        Controls.Add(btnReports)
        Controls.Add(btnCloseDay)
        Controls.Add(btnCheckpoints)
        Controls.Add(btnExpenses)
        Controls.Add(btnTransactions)
        Controls.Add(btnOpenDay)
        Controls.Add(lblWelcome)
        Name = "DashboardForm"
        Text = "DashboardForm"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblWelcome As Label
    Friend WithEvents btnOpenDay As Button
    Friend WithEvents btnTransactions As Button
    Friend WithEvents btnExpenses As Button
    Friend WithEvents btnCheckpoints As Button
    Friend WithEvents btnCloseDay As Button
    Friend WithEvents btnReports As Button
    Friend WithEvents btnLogout As Button
    Friend WithEvents btnBalanceVariance As Button
    Friend WithEvents btnAlerts As Button
    Friend WithEvents btnSearch As Button
End Class
