﻿namespace ACT.SpecialSpellTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;

    using ACT.SpecialSpellTimer.Properties;
    using ACT.SpecialSpellTimer.Sound;
    using ACT.SpecialSpellTimer.Utility;

    /// <summary>
    /// ワンポイントテレロップ Controller
    /// </summary>
    public class OnePointTelopController
    {
        /// <summary>
        /// テロップWindowのリスト
        /// </summary>
        private static List<OnePointTelopWindow> telopWindowList = new List<OnePointTelopWindow>();

        /// <summary>
        /// テロップを閉じる
        /// </summary>
        public static void CloseTelops()
        {
            if (telopWindowList != null)
            {
                foreach (var telop in telopWindowList)
                {
                    telop.DataSource.Left = telop.Left;
                    telop.DataSource.Top = telop.Top;

                    ActInvoker.Invoke(() =>
                    {
                        telop.Close();
                    });
                }

                OnePointTelopTable.Default.Save();

                telopWindowList.Clear();
            }
        }

        /// <summary>
        /// ログとマッチングする
        /// </summary>
        /// <param name="logLines">ログ行</param>
        public static void Match(
            string[] logLines)
        {
            var telops = OnePointTelopTable.Default.EnabledTable;
            var player = FF14PluginHelper.GetPlayer();

            foreach (var telop in telops)
            {
                telop.BeginEdit();

                var regex = telop.Regex as Regex;
                var regexToHide = telop.RegexToHide as Regex;
                var isForceHide = false;

                foreach (var log in logLines)
                {
                    // 通常マッチ
                    if (regex == null)
                    {
                        var keyword = player != null ?
                            telop.Keyword.Trim().Replace("<me>", player.Name) :
                            telop.Keyword.Trim();

                        if (log.ToUpper().Contains(
                            keyword.ToUpper()))
                        {
                            telop.MessageReplaced = telop.Message;
                            telop.MatchDateTime = DateTime.Now;
                            telop.Delayed = false;
                            telop.MatchedLog = log;

                            SoundController.Default.Play(telop.MatchSound);
                            SoundController.Default.Play(telop.MatchTextToSpeak);
                        }
                    }

                    // 正規表現マッチ
                    if (regex != null)
                    {
                        if (regex.IsMatch(log))
                        {
                            telop.MessageReplaced = regex.Replace(log, telop.Message);
                            telop.MatchDateTime = DateTime.Now;
                            telop.Delayed = false;
                            telop.MatchedLog = log;

                            SoundController.Default.Play(telop.MatchSound);
                            if (!string.IsNullOrWhiteSpace(telop.MatchTextToSpeak))
                            {
                                var tts = regex.Replace(log, telop.MatchTextToSpeak);
                                SoundController.Default.Play(tts);
                            }
                        }
                    }

                    // 通常マッチ(強制非表示)
                    if (regexToHide == null)
                    {
                        var keyword = player != null ?
                            telop.KeywordToHide.Trim().Replace("<me>", player.Name) :
                            telop.KeywordToHide.Trim();

                        if (log.ToUpper().Contains(
                            keyword.ToUpper()))
                        {
                            isForceHide = true;
                        }
                    }

                    // 正規表現マッチ(強制非表示)
                    if (regexToHide != null)
                    {
                        if (regexToHide.IsMatch(log))
                        {
                            isForceHide = true;
                        }
                    }
                }   // end loop logLines

                // ディレイ時間が経過した？
                if (!telop.Delayed &&
                    telop.MatchDateTime > DateTime.MinValue &&
                    telop.Delay > 0)
                {
                    var delayed = telop.MatchDateTime.AddSeconds(telop.Delay);
                    if (DateTime.Now >= delayed)
                    {
                        telop.Delayed = true;
                        SoundController.Default.Play(telop.DelaySound);
                        var tts = regex != null && !string.IsNullOrWhiteSpace(telop.DelayTextToSpeak) ?
                            regex.Replace(telop.MatchedLog, telop.MatchTextToSpeak) :
                            telop.DelayTextToSpeak;
                        SoundController.Default.Play(tts);
                    }
                }

                var w = telopWindowList.Where(x => x.DataSource.ID == telop.ID).FirstOrDefault();
                if (w == null)
                {
                    w = new OnePointTelopWindow()
                    {
                        DataSource = telop
                    };

                    if (Settings.Default.ClickThroughEnabled)
                    {
                        w.ToTransparentWindow();
                    }

                    w.Opacity = 0;
                    w.Show();
                    w.Visibility = Visibility.Hidden;

                    telopWindowList.Add(w);
                }

                w.Refresh();

                if (Settings.Default.TelopAlwaysVisible)
                {
                    w.Visibility = Visibility.Visible;
                    continue;
                }

                if (telop.MatchDateTime > DateTime.MinValue)
                {
                    if (telop.MatchDateTime.AddSeconds(telop.Delay) <= DateTime.Now &&
                        DateTime.Now <= telop.MatchDateTime.AddSeconds(telop.Delay + telop.DisplayTime))
                    {
                        w.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(telop.KeywordToHide))
                        {
                            w.Visibility = Visibility.Hidden;
                            telop.MatchDateTime = DateTime.MinValue;
                        }
                    }

                    if (isForceHide)
                    {
                        w.Visibility = Visibility.Hidden;
                        telop.MatchDateTime = DateTime.MinValue;
                    }
                }
                else
                {
                    w.Visibility = Visibility.Hidden;
                }

                telop.EndEdit();
            }   // end loop telops
        }
    }
}