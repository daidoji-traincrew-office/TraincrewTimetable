using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.Json; // 追加

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
        private ImageDisplayForm? displayForm; // 画像表示フォームの参照
        private ImageDisplayForm? sliderForm; // スライダー表示用フォームの参照を追加
        private Dictionary<string, string>? imageMap;
        private Label errorLabel; // エラーメッセージ表示用ラベル
        private CheckBox overlayCheckBox; // フィールド追加
        private Button showSliderButton; // 検索UIに「スライダーを表示」ボタンを追加
        private Button changeSliderButton; // スライダー画像変更ボタン

        // スライダー画像のパスをフィールドとして保持
        private string sliderImagePath = "";

        public Form1()
        {
            InitializeComponent();

            // 画像マップの読み込み
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
                // 画像マップ読み込み失敗時も同様
                MessageBox.Show(this, "画像マップの読み込みに失敗しました: " + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                imageMap = new Dictionary<string, string>();
            }

            // UIの初期化
            searchBox = new TextBox { Left = 10, Top = 10, Width = 100 };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchButton = new Button { Left = 220, Top = 8, Width = 60, Text = "表示" };
            searchComboBox = new ComboBox { Left = 115, Top = 10, Width = 100, Height = 20 };
            // 画像マップの読み込み後、searchComboBoxに全Keyを追加
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
                Top = 70, // ここを40→80など、希望の位置に調整
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
                Top = 90, // scaleBar.Topと揃える
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
                Text = "スタフを閉じる"
            };
            closeButton.Click += CloseButton_Click;

            // スライダーを表示ボタンを「スタフを閉じる」の真下に配置
            showSliderButton = new Button
            {
                Left = closeButton.Left,
                Top = closeButton.Top + closeButton.Height + 5, // 5px下に配置
                Width = 120,
                Height = 30,
                Text = "スライダーを表示"
            };
            showSliderButton.Click += ShowSliderButton_Click;
            Controls.Add(showSliderButton);

            // 「スライダーを変える」ボタンを追加
            changeSliderButton = new Button
            {
                Left = showSliderButton.Left + showSliderButton.Width + 10,
                Top = showSliderButton.Top,
                Width = 120,
                Height = 30,
                Text = "スライダーを変える"
            };
            changeSliderButton.Click += ChangeSliderButton_Click;
            Controls.Add(changeSliderButton);

            overlayCheckBox = new CheckBox();

            errorLabel = new Label
            {
                Left = searchBox.Left,
                Top = searchBox.Top + searchBox.Height + 5, // 検索ボックスの下に5px余白
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

            // Enterキーで表示を実行
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
            errorLabel.Text = ""; // エラー表示をクリア
            string searchText = searchBox.Text.Trim();
            string comboText = searchComboBox.Text.Trim();
            string? matchedKey = null;

            if (string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(comboText))
            {
                errorLabel.Text = $"列車番号を入力してください。";
                return;
            }

            // imageMapのキーのうち、searchBoxまたはsearchComboBoxのどちらかと一致するものを探す
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
                errorLabel.Text = $"{searchText} または {comboText} に対応するスタフが見つかりません。";
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
                    errorLabel.Text = $"{relativePath} が見つかりません。";
                }
            }
            else
            {
                errorLabel.Text = $"{matchedKey} に対応するスタフが見つかりません。";
            }
        }

        private void SearchBox_TextChanged(object? sender, EventArgs e)
        {
            // 入力に応じてComboBoxの要素を絞り込み
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
            // 既に表示中なら閉じる（トグル動作）
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
                errorLabel.Text = "スライダー画像が見つかりません。";
                return;
            }

            using (var img = Image.FromFile(path))
            {
                var bmp = new Bitmap(img);
                bmp.MakeTransparent(Color.White);

                sliderForm = new ImageDisplayForm(bmp, scaleBar.Value, false);
                sliderForm.Text = "スライダー";
                sliderForm.ClientSize = bmp.Size;
                sliderForm.FormClosed += (s, args) => sliderForm = null;
                sliderForm.Show();
            }
        }

        private void ChangeSliderButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "スライダー画像を選択";
                dialog.Filter = "画像ファイル (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                dialog.InitialDirectory = Path.Combine(Application.StartupPath, "image", "slider");
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sliderImagePath = dialog.FileName;
                    errorLabel.Text = $"スライダー画像を変更しました: {Path.GetFileName(sliderImagePath)}";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}


