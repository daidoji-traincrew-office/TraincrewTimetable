using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TraincrewTimetable
{
    public class ImageDisplayForm : Form
    {
        private PictureBox pictureBox;
        private Image originalImage;

        // �t�H�[���ړ��p
        private Point mouseDownLocation;

        // ���T�C�Y�p
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

        public ImageDisplayForm(Image image, int percent = 100)
        {
            this.Text = "�摜�\��";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // �{�[�_�[���X
            this.MinimumSize = new Size(100, 100);

            // �I���W�i���摜��ێ�
            originalImage = (Image)image.Clone();

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage
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
                g.Clear(Color.Transparent);
                g.DrawImage(originalImage, 0, 0, w, h);
            }
            var old = pictureBox.Image;
            pictureBox.Image = bmp;
            if (old != null && old != originalImage) old.Dispose();
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

            // �E�B���h�E�T�C�Y���摜�T�C�Y�ɍ��킹�ĕύX
            this.ClientSize = new Size(w, h);
        }

        // �t�H�[���ړ�
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
                // ���T�C�Y�̈�ȊO�ł݈̂ړ�
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