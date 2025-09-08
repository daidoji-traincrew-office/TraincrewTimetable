using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TraincrewTimetable
{
    public partial class TrainCrewTimetable : Form
    {
        private TextBox searchBox;
        private Button searchButton;
        private TrackBar scaleBar;
        private Label scaleLabel;
        private Button closeButton;
        private SimpleImageDisplayForm displayForm; // �摜�\���t�H�[���̎Q��

        // searchBox.KeyDown += (s, e) => ... �̏d�����폜���A�������C�x���g�n���h����o�^
        public TrainCrewTimetable()
        {
            InitializeComponent();

            // UI�̏�����
            searchBox = new TextBox { Left = 10, Top = 10, Width = 150 };
            searchBox.KeyDown += SearchBox_KeyDown; // �����Ő������o�^
            searchButton = new Button { Left = 170, Top = 8, Width = 60, Text = "�\��" };
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
                Top = 90,
                Width = 100,
                Height = 30,
                Text = "�X�^�t�����"
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

                    displayForm = new SimpleImageDisplayForm(bmp, percent);
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

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButton_Click(searchButton, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }

    public class SimpleImageDisplayForm : Form // �� �N���X����ύX
    {
        private Image originalImage;
        private PictureBox pictureBox;

        public SimpleImageDisplayForm(Image image, int percent = 100)
        {
            this.Text = "�摜�\��";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // �{�[�_�[���X
            this.MinimumSize = new Size(100, 100);
            this.BackColor = Color.Transparent; // �t�H�[�����̂̔w�i�𓧖���

            // ���߂�L���ɂ���
            this.AllowTransparency = true;
            this.TransparencyKey = Color.Magenta; // ���ߐF���w��

            // �I���W�i���摜��ێ�
            originalImage = (Image)image.Clone();

            pictureBox = new PictureBox
            {
                BackColor = Color.Magenta, // PictureBox�̗]�������ߐF�œh��
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Normal
            };
            Controls.Add(pictureBox);

            // �}�E�X�C�x���g�i�t�H�[����PictureBox�����ɓK�p�j
            this.MouseDown += ImageDisplayForm_MouseDown;
            this.MouseMove += ImageDisplayForm_MouseMove;
            pictureBox.MouseDown += ImageDisplayForm_MouseDown;
            pictureBox.MouseMove += ImageDisplayForm_MouseMove;

            // �����{���ŕ\��
            SetScalePercent(percent);
        }

        public void SetScalePercent(int percent)
        {
            if (originalImage == null) return;
            if (percent == 0)
            {
                pictureBox.Image = null;
                return;
            }
            int w = originalImage.Width * percent / 100;
            int h = originalImage.Height * percent / 100;
            if (w < 1) w = 1;
            if (h < 1) h = 1;

            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Magenta); // ���ߐF�œh��
                g.DrawImage(originalImage, 0, 0, w, h);
            }
            var old = pictureBox.Image;
            pictureBox.Image = bmp;
            if (old != null && old != originalImage) old.Dispose();

            this.ClientSize = new Size(w, h);
            pictureBox.Dock = DockStyle.None;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Size = new Size(w, h);
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void ImageDisplayForm_MouseDown(object sender, MouseEventArgs e)
        {
            // �}�E�X�_�E���C�x���g�̏���
        }

        private void ImageDisplayForm_MouseMove(object sender, MouseEventArgs e)
        {
            // �}�E�X���[�u�C�x���g�̏���
        }
    }
}