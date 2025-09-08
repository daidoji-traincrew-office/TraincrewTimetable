using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TraincrewTimetable
{
    public partial class SearchImageForm : Form
    {
        private TextBox searchBox;
        private Button searchButton;
        private TrackBar scaleBar;
        private Label scaleLabel;
        private Button closeButton;
        private ImageDisplayForm displayForm; // �摜�\���t�H�[���̎Q��

        public SearchImageForm()
        {
            InitializeComponent();

            // UI�̏�����
            searchBox = new TextBox { Left = 10, Top = 10, Width = 150 };
            searchButton = new Button { Left = 170, Top = 8, Width = 60, Text = "����" };
            scaleBar = new TrackBar
            {
                Left = 10,
                Top = 40,
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
                Top = 40,
                Width = 50,
                Height = 20,
                Text = "100%"
            };
            closeButton = new Button
            {
                Left = 10,
                Top = 140, // ������80��140�ɕύX���ĉ��Ɉړ�
                Width = 100,
                Height = 30,
                Text = "�摜�����"
            };
            closeButton.Click += CloseButton_Click;

            scaleBar.ValueChanged += (s, e) =>
            {
                scaleLabel.Text = $"{scaleBar.Value}%";
                if (displayForm != null && !displayForm.IsDisposed)
                {
                    displayForm.SetScalePercent(scaleBar.Value);
                }
            };

            searchButton.Click += SearchButton_Click;

            Controls.Add(searchBox);
            Controls.Add(searchButton);
            Controls.Add(scaleBar);
            Controls.Add(scaleLabel);
            Controls.Add(closeButton);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string fileName = searchBox.Text.Trim();
            if (string.IsNullOrEmpty(fileName)) return;

            string imagePath = Path.Combine(Application.StartupPath, "image", $"{fileName}.png");
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

                    displayForm = new ImageDisplayForm(bmp, percent);
                    displayForm.Show();
                }
            }
            else
            {
                MessageBox.Show($"{fileName}.png ��������܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, System.EventArgs e)
        {
            if (displayForm != null && !displayForm.IsDisposed)
            {
                displayForm.Close();
                displayForm = null;
            }
        }
    }
}