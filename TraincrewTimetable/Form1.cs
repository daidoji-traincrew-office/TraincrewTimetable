using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.Json; // �ǉ�

namespace TraincrewTimetable
{
    public partial class Form1 : Form
    {
        private TextBox searchBox;
        private Button searchButton;
        private ComboBox searchComboBox;
        private TrackBar scaleBar;
        private Label scaleLabel;
        private Button closeButton;
        private ImageDisplayForm? displayForm; // �摜�\���t�H�[���̎Q��
        private ImageDisplayForm? sliderForm; // �X���C�_�[�\���p�t�H�[���̎Q�Ƃ�ǉ�
        private Dictionary<string, string>? imageMap;
        private Label errorLabel; // �G���[���b�Z�[�W�\���p���x��
        private CheckBox overlayCheckBox; // �t�B�[���h�ǉ�
        private Button showSliderButton; // ����UI�Ɂu�X���C�_�[��\���v�{�^����ǉ�
        private Button changeSliderButton; // �X���C�_�[�摜�ύX�{�^��

        // �X���C�_�[�摜�̃p�X���t�B�[���h�Ƃ��ĕێ�
        private string sliderImagePath = "";

        public Form1()
        {
            InitializeComponent();

            // �摜�}�b�v�̓ǂݍ���
            string mapPath = Path.Combine(Application.StartupPath, "image", "image_map.json");
            try
            {
                if (File.Exists(mapPath))
                {
                    string json = File.ReadAllText(mapPath);
                    imageMap = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    }) ?? new Dictionary<string, string>();
                }
                else
                {
                    imageMap = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                // �摜�}�b�v�ǂݍ��ݎ��s�������l
                MessageBox.Show(this, "�摜�}�b�v�̓ǂݍ��݂Ɏ��s���܂���: " + ex.Message, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                imageMap = new Dictionary<string, string>();
            }

            // UI�̏�����
            searchBox = new TextBox { Left = 10, Top = 10, Width = 100 };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchButton = new Button { Left = 220, Top = 8, Width = 60, Text = "�\��" };
            searchComboBox = new ComboBox { Left = 115, Top = 10, Width = 100, Height = 20 };
            // �摜�}�b�v�̓ǂݍ��݌�AsearchComboBox�ɑSKey��ǉ�
            if (imageMap != null)
            {
                searchComboBox.Items.Clear();
                foreach (var key in imageMap.Keys)
                {
                    searchComboBox.Items.Add(key);
                }
            }
            searchComboBox.SelectedIndex = 0;

            scaleBar = new TrackBar
            {
                Left = 10,
                Top = 70, // ������40��80�ȂǁA��]�̈ʒu�ɒ���
                Width = 220,
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10,
                SmallChange = 1,
                LargeChange = 10
            };
            scaleLabel = new Label
            {
                Left = 240,
                Top = 90, // scaleBar.Top�Ƒ�����
                Width = 50,
                Height = 20,
                Text = "100%"
            };
            closeButton = new Button
            {
                Left = 10,
                Top = 115,
                Width = 100,
                Height = 30,
                Text = "�X�^�t�����"
            };
            closeButton.Click += CloseButton_Click;

            // �X���C�_�[��\���{�^�����u�X�^�t�����v�̐^���ɔz�u
            showSliderButton = new Button
            {
                Left = closeButton.Left,
                Top = closeButton.Top + closeButton.Height + 5, // 5px���ɔz�u
                Width = 120,
                Height = 30,
                Text = "�X���C�_�[��\��"
            };
            showSliderButton.Click += ShowSliderButton_Click;
            Controls.Add(showSliderButton);

            // �u�X���C�_�[��ς���v�{�^����ǉ�
            changeSliderButton = new Button
            {
                Left = showSliderButton.Left + showSliderButton.Width + 10,
                Top = showSliderButton.Top,
                Width = 120,
                Height = 30,
                Text = "�X���C�_�[��ς���"
            };
            changeSliderButton.Click += ChangeSliderButton_Click;
            Controls.Add(changeSliderButton);

            overlayCheckBox = new CheckBox();

            errorLabel = new Label
            {
                Left = searchBox.Left,
                Top = searchBox.Top + searchBox.Height + 5, // �����{�b�N�X�̉���5px�]��
                Width = 300,
                Height = 20,
                ForeColor = Color.Red
            };

            scaleBar.ValueChanged += (s, e) =>
            {
                scaleLabel.Text = $"{scaleBar.Value}%";
                if (displayForm != null && !displayForm.IsDisposed)
                {
                    displayForm.SetScalePercent(scaleBar.Value);
                }
                if (sliderForm != null && !sliderForm.IsDisposed)
                {
                    sliderForm.SetScalePercent(scaleBar.Value);
                }
            };

            searchButton.Click += SearchButton_Click;

            // Enter�L�[�ŕ\�������s
            searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SearchButton_Click(searchButton, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            searchComboBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SearchButton_Click(searchButton, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            Controls.Add(searchBox);
            Controls.Add(searchButton);
            Controls.Add(searchComboBox);
            Controls.Add(scaleBar);
            Controls.Add(scaleLabel);
            Controls.Add(closeButton);
            Controls.Add(errorLabel);
        }

        private void SearchButton_Click(object? sender, EventArgs e)
        {
            errorLabel.Text = ""; // �G���[�\�����N���A
            string searchText = searchBox.Text.Trim();
            string comboText = searchComboBox.Text.Trim();
            string? matchedKey = null;

            if (string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(comboText))
            {
                errorLabel.Text = $"��Ԕԍ�����͂��Ă��������B";
                return;
            }

            // imageMap�̃L�[�̂����AsearchBox�܂���searchComboBox�̂ǂ��炩�ƈ�v������̂�T��
            if (imageMap != null)
            {
                foreach (var key in imageMap.Keys)
                {
                    if (key.Equals(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                        key.Equals(comboText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        matchedKey = key;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(matchedKey))
            {
                errorLabel.Text = $"{searchText} �܂��� {comboText} �ɑΉ�����X�^�t��������܂���B";
                return;
            }

            if (imageMap != null && imageMap.TryGetValue(matchedKey, out var relativePath) && !string.IsNullOrEmpty(relativePath))
            {
                string imagePath = Path.Combine(Application.StartupPath, "image", relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(imagePath))
                {
                    using (var img = Image.FromFile(imagePath))
                    {
                        var bmp = new Bitmap(img);
                        int percent = scaleBar.Value;

                        if (displayForm != null && !displayForm.IsDisposed)
                        {
                            displayForm.Close();
                            displayForm = null;
                        }

                        displayForm = new ImageDisplayForm(bmp, percent, overlayCheckBox.Checked);
                        displayForm.Show();
                    }
                }
                else
                {
                    errorLabel.Text = $"{relativePath} ��������܂���B";
                }
            }
            else
            {
                errorLabel.Text = $"{matchedKey} �ɑΉ�����X�^�t��������܂���B";
            }
        }

        private void SearchBox_TextChanged(object? sender, EventArgs e)
        {
            // ���͂ɉ�����ComboBox�̗v�f���i�荞��
            string text = searchBox.Text;
            if (imageMap != null)
            {
                searchComboBox.Items.Clear();
                foreach (var key in imageMap.Keys)
                {
                    if (string.IsNullOrEmpty(text) || key.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        searchComboBox.Items.Add(key);
                    }
                }
                if (searchComboBox.Items.Count > 0)
                {
                    searchComboBox.SelectedIndex = 0;
                }
            }
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            if (displayForm != null && !displayForm.IsDisposed)
            {
                displayForm.Close();
                displayForm = null;
            }
            if (sliderForm != null && !sliderForm.IsDisposed)
            {
                sliderForm.Close();
                sliderForm = null;
            }
        }

        private void ShowSliderButton_Click(object? sender, EventArgs e)
        {
            // ���ɕ\�����Ȃ����i�g�O������j
            if (sliderForm != null && !sliderForm.IsDisposed)
            {
                sliderForm.Close();
                sliderForm = null;
                return;
            }

            string path = sliderImagePath;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Application.StartupPath, "image", "slider", "slider1.png");
            }
            if (!File.Exists(path))
            {
                errorLabel.Text = "�X���C�_�[�摜��������܂���B";
                return;
            }

            using (var img = Image.FromFile(path))
            {
                var bmp = new Bitmap(img);
                bmp.MakeTransparent(Color.White);

                sliderForm = new ImageDisplayForm(bmp, scaleBar.Value, false);
                sliderForm.Text = "�X���C�_�[";
                sliderForm.ClientSize = bmp.Size;
                sliderForm.FormClosed += (s, args) => sliderForm = null;
                sliderForm.Show();
            }
        }

        private void ChangeSliderButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "�X���C�_�[�摜��I��";
                dialog.Filter = "�摜�t�@�C�� (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                dialog.InitialDirectory = Path.Combine(Application.StartupPath, "image", "slider");
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sliderImagePath = dialog.FileName;
                    errorLabel.Text = $"�X���C�_�[�摜��ύX���܂���: {Path.GetFileName(sliderImagePath)}";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}


