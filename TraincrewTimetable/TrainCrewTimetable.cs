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
        private SimpleImageDisplayForm displayForm; // 画像表示フォームの参照

        // searchBox.KeyDown += (s, e) => ... の重複を削除し、正しくイベントハンドラを登録
        public TrainCrewTimetable()
        {
            InitializeComponent();

            // UIの初期化
            searchBox = new TextBox { Left = 10, Top = 10, Width = 150 };
            searchBox.KeyDown += SearchBox_KeyDown; // ここで正しく登録
            searchButton = new Button { Left = 170, Top = 8, Width = 60, Text = "表示" };
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
                Text = "スタフを閉じる"
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
                MessageBox.Show($"{fileName}.png が見つかりません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    public class SimpleImageDisplayForm : Form // ← クラス名を変更
    {
        private Image originalImage;
        private PictureBox pictureBox;

        public SimpleImageDisplayForm(Image image, int percent = 100)
        {
            this.Text = "画像表示";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // ボーダーレス
            this.MinimumSize = new Size(100, 100);
            this.BackColor = Color.Transparent; // フォーム自体の背景を透明に

            // 透過を有効にする
            this.AllowTransparency = true;
            this.TransparencyKey = Color.Magenta; // 透過色を指定

            // オリジナル画像を保持
            originalImage = (Image)image.Clone();

            pictureBox = new PictureBox
            {
                BackColor = Color.Magenta, // PictureBoxの余白も透過色で塗る
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Normal
            };
            Controls.Add(pictureBox);

            // マウスイベント（フォームとPictureBox両方に適用）
            this.MouseDown += ImageDisplayForm_MouseDown;
            this.MouseMove += ImageDisplayForm_MouseMove;
            pictureBox.MouseDown += ImageDisplayForm_MouseDown;
            pictureBox.MouseMove += ImageDisplayForm_MouseMove;

            // 初期倍率で表示
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
                g.Clear(Color.Magenta); // 透過色で塗る
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
            // マウスダウンイベントの処理
        }

        private void ImageDisplayForm_MouseMove(object sender, MouseEventArgs e)
        {
            // マウスムーブイベントの処理
        }
    }
}