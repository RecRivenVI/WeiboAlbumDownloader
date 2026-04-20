using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestWeiboMobileApi.Helpers
{
    public class HttpHelper
    {
        /// <summary>
        /// 发送 GET 请求并解析 JSON 响应
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求 URL</param>
        /// <param name="cookie">微博 Cookie</param>
        /// <returns>解析后的对象</returns>
        public static async Task<T?> GetAsync<T>(string url, string cookie)
        {
            try
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Referer", "https://m.weibo.cn/");
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
                    request.Headers.Add("Accept", "application/json, text/plain, */*");
                    request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                    request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    request.Headers.Add("Cookie", cookie);

                    var response = await client.SendAsync(request);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"❌ HTTP 请求失败: {response.StatusCode}");
                        return default;
                    }

                    string responseBody;
                    var contentEncoding = response.Content.Headers.ContentEncoding.ToString();

                    if (contentEncoding.Contains("gzip"))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var decompressedStream = new GZipStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressedStream))
                        {
                            responseBody = await reader.ReadToEndAsync();
                        }
                    }
                    else
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                    }

                    // 检查是否是验证码错误
                    if (responseBody.Contains("captcha") || responseBody.Contains("-100"))
                    {
                        Console.WriteLine("\n⚠️  检测到验证码验证或访问限制！");
                        
                        // 尝试解析 JSON 提取验证码 URL
                        try
                        {
                            var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                            if (errorResponse != null && errorResponse.url != null)
                            {
                                string captchaUrl = errorResponse.url.ToString();
                                Console.WriteLine($"\n🔗 验证码链接:");
                                Console.WriteLine(captchaUrl);
                                Console.WriteLine($"\n💡 请在浏览器中打开上述链接完成验证，然后重新运行测试脚本");
                            }
                            else
                            {
                                Console.WriteLine($"响应内容: {responseBody}");
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"响应内容: {responseBody}");
                        }
                        
                        Console.WriteLine("\n可能原因：");
                        Console.WriteLine("   1. Cookie 已失效或被标记");
                        Console.WriteLine("   2. 请求频率过高");
                        Console.WriteLine("   3. IP 被临时限制");
                        Console.WriteLine("\n建议解决方案：");
                        Console.WriteLine("   1. 在浏览器中打开验证码链接完成验证");
                        Console.WriteLine("   2. 等待 30-60 分钟后重试");
                        Console.WriteLine("   3. 重新获取新的 Cookie");
                        Console.WriteLine("   4. 更换网络环境（如使用代理）");
                        return default;
                    }

                    var result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HTTP 请求异常: {ex.Message}");
                return default;
            }
        }
    }
}
