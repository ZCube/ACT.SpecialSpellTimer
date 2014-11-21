﻿namespace ACT.SpecialSpellTimer
{
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using ACT.SpecialSpellTimer.Properties;
    using ACT.SpecialSpellTimer.Utility;

    /// <summary>
    /// SpellTimerControl
    /// </summary>
    public partial class SpellTimerControl : UserControl
    {
        /// <summary>
        /// Control間の縦方向のMargin
        /// </summary>
        private const double ControlVerticalMargin = 4.0d;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SpellTimerControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// スペルのTitle
        /// </summary>
        public string SpellTitle { get; set; }

        /// <summary>
        /// 残りリキャストTime(秒数)
        /// </summary>
        public double RecastTime { get; set; }

        /// <summary>
        /// リキャストの進捗率
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// 描画を更新する
        /// </summary>
        public void Refresh()
        {
            this.Width = Settings.Default.ProgressBarSize.Width;

            var tb = default(TextBlock);
            var font = Settings.Default.Font;
            var fontBrush = new SolidColorBrush(Settings.Default.FontColor.ToWPF());

            // Titleを描画する
            tb = this.SpellTitleTextBlock;
            tb.Text = string.IsNullOrWhiteSpace(this.SpellTitle) ? "　" : this.SpellTitle;
            tb.FontFamily = font.ToFontFamilyWPF();
            tb.FontSize = font.ToFontSizeWPF();
            tb.FontStyle = font.ToFontStyleWPF();
            tb.FontWeight = font.ToFontWeightWPF();
            tb.Foreground = fontBrush;

            // リキャスト時間を描画する
            tb = this.RecastTimeTextBlock;
            tb.Text = this.RecastTime.ToString("N1");
            tb.FontFamily = font.ToFontFamilyWPF();
            tb.FontSize = font.ToFontSizeWPF();
            tb.FontStyle = font.ToFontStyleWPF();
            tb.FontWeight = font.ToFontWeightWPF();
            tb.Foreground = fontBrush;

            // ProgressBarを描画する
            var foreBrush = new SolidColorBrush(Settings.Default.ProgressBarColor.ToWPF());
            var backBrush = new SolidColorBrush(Settings.Default.ProgressBarColor.ToWPF().ChangeBrightness(0.4d));

            var foreRect = new Rectangle();
            foreRect.Stroke = foreBrush;
            foreRect.Fill = foreBrush;
            foreRect.Width = (double)(Settings.Default.ProgressBarSize.Width * this.Progress);
            foreRect.Height = Settings.Default.ProgressBarSize.Height;
            foreRect.RadiusX = 2.0d;
            foreRect.RadiusY = 2.0d;
            Canvas.SetLeft(foreRect, 0);
            Canvas.SetTop(foreRect, 0);

            var backRect = new Rectangle();
            backRect.Stroke = backBrush;
            backRect.Fill = backBrush;
            backRect.Width = Settings.Default.ProgressBarSize.Width;
            backRect.Height = foreRect.Height;
            backRect.RadiusX = 2.0d;
            backRect.RadiusY = 2.0d;
            Canvas.SetLeft(backRect, 0);
            Canvas.SetTop(backRect, 0);

            this.ProgressBarCanvas.Width = backRect.Width;
            this.ProgressBarCanvas.Height = backRect.Height;

            this.ProgressBarCanvas.Children.Clear();
            this.ProgressBarCanvas.Children.Add(backRect);
            this.ProgressBarCanvas.Children.Add(foreRect);
        }
    }
}