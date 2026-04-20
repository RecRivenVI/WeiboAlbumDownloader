using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeiboAlbumDownloader.Enums;

namespace WeiboAlbumDownloader.Helpers
{
    internal class SeleniumHelper
    {
        public static string? GetCookie(WeiboDataSource dataSource)
        {
            string loginUrl = "https://passport.weibo.com/sso/signin?entry=wapsso&source=wapssowb&url=https://weibo.cn";
            if (dataSource == WeiboDataSource.WeiboCn)
                loginUrl = "https://passport.weibo.com/sso/signin?entry=wapsso&source=wapssowb&url=https://weibo.cn";
            else if (dataSource == WeiboDataSource.WeiboCnMobile)
                loginUrl = "https://passport.weibo.com/sso/signin?entry=wapsso&source=wapsso&url=https://m.weibo.cn";
            else
                loginUrl = "https://passport.weibo.com/sso/signin?entry=miniblog&source=miniblog&url=https://weibo.com/";

            // 创建 Edge 浏览器选项并配置参数
            var edgeOptions = new EdgeOptions();
            edgeOptions.AddArgument("--no-sandbox");
            edgeOptions.AddArgument("--disable-dev-shm-usage");
            edgeOptions.AddArgument("--window-size=1280,800");
            
            IWebDriver driver = new EdgeDriver(edgeOptions);
            driver.Url = loginUrl;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromHours(8))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500),
            };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            //wait.Until(d => d.FindElement(By.LinkText("title")));

            // 等待页面加载完成并获取页面标题
            wait.Until(d => d.Title.Equals("微博 – 随时随地发现新鲜事") || d.Title.Equals("我的首页") || d.Title.Equals("微博"));

            // 获取页面标题并进行检查
            string pageTitle = driver.Title;
            if (pageTitle.Equals("微博 – 随时随地发现新鲜事") || pageTitle.Equals("我的首页") || pageTitle.Equals("微博"))
            {
                //AppendLog("扫码登陆成功", MessageEnum.Success);
                // 获取所有的 Cookie 对象
                IReadOnlyCollection<Cookie> cookies = driver.Manage().Cookies.AllCookies;

                // 将 Cookie 对象转换为一个字符串，格式类似于 HTTP 请求头的 Cookie 字符串
                string cookie = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));

                // 打印 Cookie 字符串
                Debug.WriteLine(cookie);

                driver.Quit();

                return cookie;
            }
            else
            {
                Debug.WriteLine("未登录");
            }

            // 程序结束时，手动关闭浏览器
            driver.Quit();

            return null;
        }
        
        /// <summary>
        /// 显示验证码等待窗口，使用系统默认浏览器打开验证码页面
        /// </summary>
        /// <param name="captchaUrl">验证码URL</param>
        /// <param name="logAction">日志回调</param>
        /// <returns>是否验证成功</returns>
        public static async Task<bool> ShowCaptchaWindow(string captchaUrl, Action<string, MessageEnum>? logAction = null)
        {
            try
            {
                logAction?.Invoke($"🌐 正在使用系统默认浏览器打开验证码页面...", MessageEnum.Info);
                
                // 使用系统默认浏览器打开验证码链接
                Process.Start(new ProcessStartInfo
                {
                    FileName = captchaUrl,
                    UseShellExecute = true  // 使用系统默认浏览器
                });
                
                logAction?.Invoke($"✅ 验证码页面已在浏览器中打开", MessageEnum.Success);
                
                // 在 UI 线程上显示等待对话框
                bool userConfirmed = false;
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 创建自定义等待窗口
                    var captchaWindow = new Window
                    {
                        Title = "⏳ 等待验证码验证",
                        Width = 450,
                        Height = 280,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Topmost = true,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStyle = WindowStyle.ToolWindow
                    };
                    
                    // 创建主容器
                    var mainPanel = new StackPanel
                    {
                        Margin = new Thickness(20),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    
                    // 标题文本
                    var titleText = new TextBlock
                    {
                        Text = "请在浏览器中完成验证码",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    
                    // 说明文本
                    var infoText = new TextBlock
                    {
                        Text = "1. 在打开的浏览器窗口中完成验证\n2. 验证成功后返回此处\n3. 点击下方按钮继续下载",
                        FontSize = 13,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 20),
                        LineHeight = 22
                    };
                    
                    // 提示文本
                    var hintBox = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(255, 243, 205)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(255, 223, 0)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10),
                        CornerRadius = new CornerRadius(4),
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    
                    var hintText = new TextBlock
                    {
                        Text = "💡 提示：如果浏览器未自动打开，请手动复制以下链接到浏览器：\n\n" + captchaUrl,
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.DarkGoldenrod
                    };
                    hintBox.Child = hintText;
                    
                    // 按钮面板
                    var buttonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    // 完成按钮
                    var completeButton = new Button
                    {
                        Content = "✅ 已完成验证",
                        Width = 120,
                        Height = 35,
                        Margin = new Thickness(0, 0, 10, 0),
                        Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    
                    completeButton.Click += (s, e) =>
                    {
                        userConfirmed = true;
                        captchaWindow.DialogResult = true;
                        captchaWindow.Close();
                    };
                    
                    // 取消按钮
                    var cancelButton = new Button
                    {
                        Content = "❌ 取消下载",
                        Width = 120,
                        Height = 35,
                        Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    
                    cancelButton.Click += (s, e) =>
                    {
                        userConfirmed = false;
                        captchaWindow.DialogResult = false;
                        captchaWindow.Close();
                    };
                    
                    buttonPanel.Children.Add(completeButton);
                    buttonPanel.Children.Add(cancelButton);
                    
                    // 组装界面
                    mainPanel.Children.Add(titleText);
                    mainPanel.Children.Add(infoText);
                    mainPanel.Children.Add(hintBox);
                    mainPanel.Children.Add(buttonPanel);
                    
                    captchaWindow.Content = mainPanel;
                    
                    // 显示模态对话框
                    captchaWindow.ShowDialog();
                });
                
                if (userConfirmed)
                {
                    logAction?.Invoke($"✅ 用户确认验证完成，继续下载...", MessageEnum.Success);
                    
                    // 给用户一些时间让 Cookie 生效
                    await Task.Delay(2000);
                    
                    return true;
                }
                else
                {
                    logAction?.Invoke($"❌ 用户取消了验证码验证", MessageEnum.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"❌ 打开验证码页面失败: {ex.Message}", MessageEnum.Error);
                return false;
            }
        }
    }
}
