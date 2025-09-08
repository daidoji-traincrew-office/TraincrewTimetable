using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TraincrewTimetable
{
    // 既存のImageDisplayFormをこのまま残し、クラス名は変更しない
    public class ImageDisplayForm : Form
    {
        private PictureBox pictureBox;
        private Image originalImage;
        private bool overlayEnabled; // ← 追加（必要なら）

        // フォーム移動用
        private Point mouseDownLocation;

        // リサイズ用
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCAPTION = 2;
        private const int WM_NCHITTEST = 0x84;
        private const int RESIZE_HANDLE_SIZE = 10;

        // 3引数コンストラクタを追加
        public ImageDisplayForm(Image image, int percent, bool overlay)
            : this(image, percent)
        {
            overlayEnabled = overlay;
            // overlayEnabledを使った処理が必要ならここに追加
        }

        public ImageDisplayForm(Image image, int percent = 100)
        {
            this.Text = "画像表示";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // ボーダーレス
            this.AllowTransparency = true;                // 透過を有効に
            this.BackColor = Color.Magenta;               // 透過色を指定
            this.TransparencyKey = Color.Magenta;         // 透過色を指定
            this.MinimumSize = new Size(100, 100);

            // オリジナル画像を保持
            originalImage = (Image)image.Clone();

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage, // ★ここをCenterImage→StretchImageに変更
                Margin = Padding.Empty, // ★余白を消す
                Padding = Padding.Empty, // ★余白を消す
                BackColor = Color.Magenta // PictureBoxの背景も透過色に
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
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.Clear(Color.Transparent); // 透過でクリア

                // 画像を描画（アンチエイリアス部分も透過になる）
                g.DrawImage(originalImage, 0, 0, w, h);
            }
            var old = pictureBox.Image;
            pictureBox.Image = bmp;
            if (old != null && old != originalImage) old.Dispose();

            // 画像サイズ=UIサイズにする
            this.ClientSize = new Size(w, h);
            pictureBox.Dock = DockStyle.None;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Size = new Size(w, h);
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
        }

        // フォーム移動
        private void ImageDisplayForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownLocation = e.Location;
            }
        }

        private void ImageDisplayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // リサイズ領域以外でのみ移動
                if (!IsInResizeArea(e.Location))
                {
                    Point clientPoint = (sender is PictureBox) ? pictureBox.PointToScreen(e.Location) : this.PointToScreen(e.Location);
                    Point formPoint = this.PointToClient(clientPoint);
                    this.Left += formPoint.X - mouseDownLocation.X;
                    this.Top += formPoint.Y - mouseDownLocation.Y;
                }
            }
        }

        private bool IsInResizeArea(Point p)
        {
            return
                p.X < RESIZE_HANDLE_SIZE ||
                p.X > this.Width - RESIZE_HANDLE_SIZE ||
                p.Y < RESIZE_HANDLE_SIZE ||
                p.Y > this.Height - RESIZE_HANDLE_SIZE;
        }

        private void InitializeComponent()
        {

        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                if (pos.X < RESIZE_HANDLE_SIZE && pos.Y < RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOPLEFT;
                    return;
                }
                if (pos.X > Width - RESIZE_HANDLE_SIZE && pos.Y < RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOPRIGHT;
                    return;
                }
                if (pos.X < RESIZE_HANDLE_SIZE && pos.Y > Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                    return;
                }
                if (pos.X > Width - RESIZE_HANDLE_SIZE && pos.Y > Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }
                if (pos.X < RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTLEFT;
                    return;
                }
                if (pos.X > Width - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTRIGHT;
                    return;
                }
                if (pos.Y < RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOP;
                    return;
                }
                if (pos.Y > Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                    return;
                }
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
            base.WndProc(ref m);
        }
    }
}