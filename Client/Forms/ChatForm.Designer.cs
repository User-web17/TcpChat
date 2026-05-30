namespace Client
{
    partial class ChatForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rtxtChatHistory = new RichTextBox();
            txtMessage = new TextBox();
            btnSend = new Button();
            lstChats = new ListBox();
            btnAddContact = new Button();
            btnCreateGroup = new Button();
            btnAddToGroup = new Button();
            SuspendLayout();
            // 
            // rtxtChatHistory
            // 
            rtxtChatHistory.Location = new Point(20, 21);
            rtxtChatHistory.Name = "rtxtChatHistory";
            rtxtChatHistory.ReadOnly = true;
            rtxtChatHistory.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtxtChatHistory.Size = new Size(432, 262);
            rtxtChatHistory.TabIndex = 0;
            rtxtChatHistory.Text = "";
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(20, 289);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(364, 27);
            txtMessage.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(390, 289);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(62, 29);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // lstChats
            // 
            lstChats.FormattingEnabled = true;
            lstChats.Location = new Point(482, 21);
            lstChats.Name = "lstChats";
            lstChats.Size = new Size(163, 304);
            lstChats.TabIndex = 3;
            lstChats.SelectedIndexChanged += lstChats_SelectedIndexChanged;
            // 
            // btnAddContact
            // 
            btnAddContact.Location = new Point(513, 366);
            btnAddContact.Name = "btnAddContact";
            btnAddContact.Size = new Size(109, 29);
            btnAddContact.TabIndex = 4;
            btnAddContact.Text = "Add Contact";
            btnAddContact.UseVisualStyleBackColor = true;
            btnAddContact.Click += btnAddContact_Click;
            // 
            // btnCreateGroup
            // 
            btnCreateGroup.Location = new Point(493, 331);
            btnCreateGroup.Name = "btnCreateGroup";
            btnCreateGroup.Size = new Size(68, 29);
            btnCreateGroup.TabIndex = 5;
            btnCreateGroup.Text = "Create";
            btnCreateGroup.UseVisualStyleBackColor = true;
            btnCreateGroup.Click += btnCreateGroup_Click;
            // 
            // btnAddToGroup
            // 
            btnAddToGroup.Location = new Point(567, 331);
            btnAddToGroup.Name = "btnAddToGroup";
            btnAddToGroup.Size = new Size(68, 29);
            btnAddToGroup.TabIndex = 6;
            btnAddToGroup.Text = "Add";
            btnAddToGroup.UseVisualStyleBackColor = true;
            btnAddToGroup.Click += btnAddToGroup_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(667, 438);
            Controls.Add(btnAddToGroup);
            Controls.Add(btnCreateGroup);
            Controls.Add(btnAddContact);
            Controls.Add(lstChats);
            Controls.Add(btnSend);
            Controls.Add(txtMessage);
            Controls.Add(rtxtChatHistory);
            Name = "ChatForm";
            Text = "ChatForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtxtChatHistory;
        private TextBox txtMessage;
        private Button btnSend;
        private ListBox lstChats;
        private Button btnAddContact;
        private Button btnCreateGroup;
        private Button btnAddToGroup;
    }
}