using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestWeiboMobileApi.Helpers;
using TestWeiboMobileApi.Models;

namespace TestWeiboMobileApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  m.weibo.cn API 数据获取测试脚本");
            Console.WriteLine("========================================\n");

            // 配置参数
            Console.Write("请输入微博用户 UID (例如: 7523917567): ");
            string userId = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ UID 不能为空！");
                return;
            }

            Console.Write("\n请输入微博 Cookie (从 weibo.cn 或 m.weibo.cn 获取): ");
            string cookie = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(cookie))
            {
                Console.WriteLine("❌ Cookie 不能为空！");
                Console.WriteLine("\n💡 提示: 请按以下步骤获取 Cookie:");
                Console.WriteLine("   1. 浏览器访问 https://m.weibo.cn/");
                Console.WriteLine("   2. 按 F12 打开开发者工具");
                Console.WriteLine("   3. 切换到 Network (网络) 标签");
                Console.WriteLine("   4. 刷新页面");
                Console.WriteLine("   5. 点击任意请求，复制 Request Headers 中的 Cookie 值");
                return;
            }

            Console.Write("\n请输入要测试的页数 (默认 3): ");
            string pageInput = Console.ReadLine()?.Trim();
            int maxPages = 3;
            if (!string.IsNullOrEmpty(pageInput) && int.TryParse(pageInput, out int pages))
            {
                maxPages = pages;
            }

            Console.WriteLine($"\n🚀 开始测试...");
            Console.WriteLine($"   用户 UID: {userId}");
            Console.WriteLine($"   测试页数: {maxPages}");
            Console.WriteLine($"   容器 ID: 107603{userId}\n");

            // 执行测试
            await TestWeiboMobileApi(userId, cookie, maxPages);

            Console.WriteLine("\n✅ 测试完成！");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 测试 m.weibo.cn API 数据获取
        /// </summary>
        static async Task TestWeiboMobileApi(string userId, string cookie, int maxPages)
        {
            long sinceId = 0;
            int page = 1;
            int totalWeibos = 0;
            int totalImages = 0;
            int totalVideos = 0;
            bool userInfoCached = false;
            string nickName = "";

            while (page <= maxPages)
            {
                Console.WriteLine($"\n{'=',-60}");
                Console.WriteLine($"📄 正在获取第 {page} 页数据...");
                
                // 构建 API URL
                string url = $"https://m.weibo.cn/api/container/getIndex?type=uid&value={userId}&containerid=107603{userId}&since_id={sinceId}&page={page}";
                
                try
                {
                    var response = await HttpHelper.GetAsync<WeiboCnMobileModel>(url, cookie);
                    
                    if (response == null)
                    {
                        Console.WriteLine("❌ 响应为空，可能是网络错误或 Cookie 失效");
                        break;
                    }

                    if (response.Ok != 1)
                    {
                        Console.WriteLine($"❌ API 返回错误，ok={response.Ok}");
                        break;
                    }

                    if (response.Data == null || response.Data.Cards == null || response.Data.Cards.Count == 0)
                    {
                        Console.WriteLine("⚠️  没有更多数据了");
                        break;
                    }

                    // 更新 since_id
                    if (response.Data.CardlistInfo?.SinceId != null)
                    {
                        sinceId = response.Data.CardlistInfo.SinceId.Value;
                    }

                    // 显示总数据量
                    if (!userInfoCached && response.Data.CardlistInfo?.Total != null)
                    {
                        Console.WriteLine($"📊 该用户共有 {response.Data.CardlistInfo.Total} 条微博");
                    }

                    Console.WriteLine($"📋 当前页卡片数: {response.Data.Cards.Count}");

                    // 处理每张卡片（每条微博）
                    foreach (var card in response.Data.Cards)
                    {
                        // 只处理微博类型 (card_type=9)，跳过转发
                        if (card?.CardType != 9 || card?.Mblog?.RetweetedStatus != null)
                            continue;

                        totalWeibos++;

                        // 获取用户信息（仅第一次）
                        if (!userInfoCached && card.Mblog?.User != null)
                        {
                            nickName = card.Mblog.User.ScreenName ?? "未知用户";
                            Console.WriteLine($"\n👤 用户信息:");
                            Console.WriteLine($"   昵称: {nickName}");
                            Console.WriteLine($"   UID: {card.Mblog.User.Id}");
                            Console.WriteLine($"   粉丝数: {card.Mblog.User.FollowersCount}");
                            Console.WriteLine($"   简介: {card.Mblog.User.Description}");
                            Console.WriteLine($"   头像: {card.Mblog.User.AvatarHd}");
                            userInfoCached = true;
                        }

                        // 解析微博内容
                        string weiboContent = ParseWeiboContent(card.Mblog?.Text);
                        string createdAt = card.Mblog?.CreatedAt ?? "未知时间";
                        
                        Console.WriteLine($"\n📝 微博 #{totalWeibos}");
                        Console.WriteLine($"   时间: {createdAt}");
                        Console.WriteLine($"   MID: {card.Mblog?.Mid}");
                        Console.WriteLine($"   内容: {weiboContent}");

                        // 统计图片
                        List<string> imageUrls = new List<string>();
                        if (card.Mblog?.PicIds != null && card.Mblog.PicIds.Any())
                        {
                            foreach (var picId in card.Mblog.PicIds)
                            {
                                string imageUrl = $"https://wx4.sinaimg.cn/large/{System.IO.Path.GetFileName(picId)}.jpg";
                                imageUrls.Add(imageUrl);
                            }
                            totalImages += imageUrls.Count;
                            Console.WriteLine($"   🖼️  图片数: {imageUrls.Count}");
                            Console.WriteLine($"   示例URL: {imageUrls[0]}");
                        }

                        // 统计视频
                        List<string> videoUrls = new List<string>();
                        if (card.Mblog?.PageInfo?.Urls != null)
                        {
                            var urls = card.Mblog.PageInfo.Urls;
                            if (!string.IsNullOrEmpty(urls.Mp48kMp4)) videoUrls.Add(urls.Mp48kMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp44kMp4)) videoUrls.Add(urls.Mp44kMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp42kMp4)) videoUrls.Add(urls.Mp42kMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp41080pMp4)) videoUrls.Add(urls.Mp41080pMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp4720pMp4)) videoUrls.Add(urls.Mp4720pMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp4HDMp4)) videoUrls.Add(urls.Mp4HDMp4);
                            else if (!string.IsNullOrEmpty(urls.Mp4LDMp4)) videoUrls.Add(urls.Mp4LDMp4);

                            if (videoUrls.Any())
                            {
                                totalVideos++;
                                Console.WriteLine($"   🎬 视频: 有");
                                Console.WriteLine($"   示例URL: {videoUrls[0]}");
                            }
                        }

                        // 统计 LivePhoto
                        if (card.Mblog?.LivePhoto != null && card.Mblog.LivePhoto.Any())
                        {
                            Console.WriteLine($"   📹 LivePhoto: {card.Mblog.LivePhoto.Count} 个");
                        }

                        // 互动数据
                        Console.WriteLine($"   👍 点赞: {card.Mblog?.AttitudesCount}, 💬 评论: {card.Mblog?.CommentsCount}, 🔄 转发: {card.Mblog?.RepostsCount}");
                    }

                    // 已移除随机延时，以便快速触发验证测试
                    // Random rd = new Random();
                    // int delay = rd.Next(2000, 4000);
                    // Console.WriteLine($"\n⏱️  等待 {delay}ms 后继续下一页...");
                    // await Task.Delay(delay);
                    
                    Console.WriteLine($"\n⚡ 无延时，继续下一页...");

                    page++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 处理第 {page} 页时出错: {ex.Message}");
                    
                    // 检查是否是验证码错误
                    if (ex.Message.Contains("captcha") || ex.Message.Contains("-100"))
                    {
                        Console.WriteLine("\n⚠️  检测到验证码验证！");
                        Console.WriteLine("建议：等待 30-60 分钟后重试，或更换 Cookie");
                        break;
                    }
                    
                    break;
                }
            }

            // 输出统计摘要
            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine("📊 测试统计摘要:");
            Console.WriteLine($"   总微博数: {totalWeibos}");
            Console.WriteLine($"   总图片数: {totalImages}");
            Console.WriteLine($"   总视频数: {totalVideos}");
            Console.WriteLine($"   实际获取页数: {page - 1}");
        }

        /// <summary>
        /// 解析微博文本内容，去除 HTML 标签
        /// </summary>
        static string ParseWeiboContent(string? htmlText)
        {
            if (string.IsNullOrEmpty(htmlText))
                return "";

            // 去除 <a> 和 <span> 标签及其内容
            string result = Regex.Replace(htmlText, @"<a.*?>.*?</a>|<span.*?>.*?</span>", string.Empty);
            // 去除其他 HTML 标签
            result = Regex.Replace(result, @"<.*?>", string.Empty);
            // 去除多余空白
            result = Regex.Replace(result, @"\s+", " ").Trim();

            // 限制显示长度
            if (result.Length > 100)
                return result.Substring(0, 100) + "...";

            return result;
        }
    }
}
