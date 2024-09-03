using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace trivia_client
{
    public partial class MainChatForm : Form
    {
        private TcpClientHandler tcpClient;
        private string currentRoom = "General";
        private RichTextBox txtChat;
        private TextBox txtMessage;
        private ListBox lstUsers;
        private ListBox lstRooms;

        public MainChatForm(TcpClientHandler client)
        {
            InitializeComponent();
            tcpClient = client;
            SetupUI();
            StartListening();
        }

        private void SetupUI()
        {
            // Form properties
            this.Text = "Chat Client";
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            this.Controls.Add(mainLayout);

            // Left panel (Rooms)
            Panel leftPanel = CreatePanel("Rooms", Color.FromArgb(230, 230, 230));
            mainLayout.Controls.Add(leftPanel, 0, 0);

            lstRooms = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None
            };
            leftPanel.Controls.Add(lstRooms);

            Button btnCreateRoom = CreateButton("Create Room", Color.FromArgb(0, 150, 136));
            btnCreateRoom.Click += btnCreateRoom_Click;
            leftPanel.Controls.Add(btnCreateRoom);

            // Center panel (Chat)
            Panel centerPanel = CreatePanel("Chat", Color.White);
            mainLayout.Controls.Add(centerPanel, 1, 0);

            txtChat = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            centerPanel.Controls.Add(txtChat);

            TableLayoutPanel bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                ColumnCount = 2
            };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            centerPanel.Controls.Add(bottomLayout);

            txtMessage = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };
            bottomLayout.Controls.Add(txtMessage, 0, 0);

            Button btnSend = CreateButton("Send", Color.FromArgb(0, 150, 136));
            btnSend.Click += btnSend_Click;
            bottomLayout.Controls.Add(btnSend, 1, 0);

            // Right panel (Users)
            Panel rightPanel = CreatePanel("Users", Color.FromArgb(230, 230, 230));
            mainLayout.Controls.Add(rightPanel, 2, 0);

            lstUsers = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None
            };
            rightPanel.Controls.Add(lstUsers);
        }

        private Panel CreatePanel(string title, Color backColor)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = backColor
            };

            Label lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Height = 30
            };
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Bottom,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Height = 40,
                Margin = new Padding(0, 10, 0, 0)
            };
        }

        private async void StartListening()
        {
            try
            {
                while (true)
                {
                    var (actionCode, jsonContent) = await tcpClient.ReceiveMessageAsync();
                    var (action, content) = MessageProcessor.ProcessMessage(actionCode, jsonContent);
                    ProcessIncomingMessage(action, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }



        private void ProcessIncomingMessage(MessageProcessor.ActionCode action, JToken content)
        {
            switch (action)
            {
                case MessageProcessor.ActionCode.ChatMessage:
                    UpdateChatWindow(content["sender"].ToString(), content["message"].ToString());
                    break;
                case MessageProcessor.ActionCode.UserList:
                    UpdateUserList(content["users"].ToObject<string[]>());
                    break;
                case MessageProcessor.ActionCode.RoomList:
                    UpdateRoomList(content["rooms"].ToObject<string[]>());
                    break;
                    // Add more cases as needed
            }
        }

        private void UpdateChatWindow(string sender, string message)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke(new Action<string, string>(UpdateChatWindow), sender, message);
            }
            else
            {
                txtChat.SelectionColor = Color.FromArgb(0, 150, 136);
                txtChat.AppendText($"{sender}: ");
                txtChat.SelectionColor = txtChat.ForeColor;
                txtChat.AppendText($"{message}{Environment.NewLine}");
                txtChat.ScrollToCaret();
            }
        }

        private void UpdateUserList(string[] users)
        {
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(new Action<string[]>(UpdateUserList), users);
            }
            else
            {
                lstUsers.Items.Clear();
                lstUsers.Items.AddRange(users);
            }
        }

        private void UpdateRoomList(string[] rooms)
        {
            if (lstRooms.InvokeRequired)
            {
                lstRooms.Invoke(new Action<string[]>(UpdateRoomList), rooms);
            }
            else
            {
                lstRooms.Items.Clear();
                lstRooms.Items.AddRange(rooms);
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageData = new
                {
                    room = currentRoom,
                    message = message
                };

                var (actionCode, jsonContent) = MessageProcessor.CreateMessage(
                    MessageProcessor.ActionCode.ChatMessage,
                    messageData
                );

                await tcpClient.SendMessageAsync(actionCode, jsonContent);
                txtMessage.Clear();
            }
        }

        private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            string roomName = Microsoft.VisualBasic.Interaction.InputBox("Enter room name:", "Create Room", "");
            if (!string.IsNullOrWhiteSpace(roomName))
            {
                var roomData = new
                {
                    name = roomName
                };

                var (actionCode, jsonContent) = MessageProcessor.CreateMessage(
                    MessageProcessor.ActionCode.CreateRoom,
                    roomData
                );

                await tcpClient.SendMessageAsync(actionCode, jsonContent);
            }
        }

        private void lstRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem != null)
            {
                JoinRoom(lstRooms.SelectedItem.ToString());
            }
        }

        private async void JoinRoom(string roomName)
        {
            var roomData = new
            {
                name = roomName
            };

            var (actionCode, jsonContent) = MessageProcessor.CreateMessage(
                MessageProcessor.ActionCode.JoinRoom,
                roomData
            );

            await tcpClient.SendMessageAsync(actionCode, jsonContent);
            currentRoom = roomName;
            this.Text = $"Chat Client - {currentRoom}";
            txtChat.Clear();
        }

        private void MainChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpClient.Close();
        }
    }
}
