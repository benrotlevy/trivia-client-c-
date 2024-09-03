using System.Net.Http;

namespace trivia_client
{
    public partial class LoginForm : Form
    {
        private TcpClientHandler tcpClient;
        private TextBox txtUsername;
        private TextBox txtPassword;

        public LoginForm()
        {
            InitializeComponent();
            tcpClient = new TcpClientHandler();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form properties
            this.Text = "Chat Client Login";
            this.Size = new Size(300, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            this.Controls.Add(mainLayout);

            // Title
            Label lblTitle = new Label
            {
                Text = "Chat Client",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // Username
            txtUsername = CreateTextBox("Username");
            mainLayout.Controls.Add(txtUsername, 0, 1);

            // Password
            txtPassword = CreateTextBox("Password");
            txtPassword.UseSystemPasswordChar = true;
            mainLayout.Controls.Add(txtPassword, 0, 2);

            // Login button
            Button btnLogin = new Button
            {
                Text = "Login",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White
            };
            btnLogin.Click += btnLogin_Click;
            mainLayout.Controls.Add(btnLogin, 0, 3);
        }

        private TextBox CreateTextBox(string placeholder)
        {
            TextBox textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Gray,
                Text = placeholder
            };
            textBox.Enter += (sender, e) => {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };
            textBox.Leave += (sender, e) => {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
            return textBox;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                await tcpClient.ConnectAsync("localhost", 8000); // Replace with actual server details

                var loginData = new
                {
                    username = txtUsername.Text,
                    password = txtPassword.Text
                };

                var (actionCode, jsonContent) = MessageProcessor.CreateMessage(
                    MessageProcessor.ActionCode.Login,
                    loginData
                );

                await tcpClient.SendMessageAsync(actionCode, jsonContent);

                var (responseActionCode, responseJsonContent) = await tcpClient.ReceiveMessageAsync();
                var (responseAction, responseContent) = MessageProcessor.ProcessMessage(responseActionCode, responseJsonContent);

                if (responseAction == MessageProcessor.ActionCode.LoginResponse)
                {
                    if (responseContent["status"].ToString() == "100")
                    {
                        MainChatForm mainForm = new MainChatForm(tcpClient);
                        this.Hide();
                        mainForm.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Login failed: " + responseContent["message"].ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Unexpected server response.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
