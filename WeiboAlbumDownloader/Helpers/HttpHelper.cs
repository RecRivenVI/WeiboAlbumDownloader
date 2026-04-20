using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WeiboAlbumDownloader.Enums;
using WeiboAlbumDownloader.Models;

namespace WeiboAlbumDownloader.Helpers
{
    public class HttpHelper
    {
        static HttpClient client;
        
        /// <summary>
        /// 验证码验证回调委托
        /// </summary>
        /// <param name="captchaUrl">验证码URL</param>
        /// <returns>用户是否完成验证</returns>
        public delegate Task<bool> CaptchaValidationHandler(string captchaUrl);
        
        /// <summary>
        /// 全局验证码验证处理器
        /// </summary>
        public static CaptchaValidationHandler? GlobalCaptchaHandler { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="fileName">不为空的时候，表示存图</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string url, WeiboDataSource dataSource, string cookie, string fileName = "", Action<string, MessageEnum> logAction = null)
        {
            return await GetAsyncWithRetry<T>(url, dataSource, cookie, fileName, logAction, maxRetries: 1);
        }
        
        /// <summary>
        /// 带重试和验证码处理的请求方法
        /// </summary>
        private static async Task<T> GetAsyncWithRetry<T>(string url, WeiboDataSource dataSource, string cookie, string fileName = "", Action<string, MessageEnum> logAction = null, int maxRetries = 1)
        {
            string responseBody = null;
            int retryCount = 0;
            
            while (retryCount <= maxRetries)
            {
                try
                {
                    //logAction?.Invoke($"[Request URL]: {url}", MessageEnum.Info);
                    //logAction?.Invoke($"[Request Cookie]: {cookie}", MessageEnum.Info);

                    var handler = new HttpClientHandler()
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    };
                    client = new HttpClient(handler);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Referer", "https://m.weibo.cn/");
                    if (string.IsNullOrEmpty(fileName))
                    {
                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
                        request.Headers.Add("Accept", "application/json, text/plain, */*");
                        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                        request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                        request.Headers.Add("Cookie", cookie);
                    }

                    var response = await client.SendAsync(request);

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

                    logAction?.Invoke($"[Response Status Code]: {response.StatusCode}", MessageEnum.Info);
                    //logAction?.Invoke($"[Response Body]: {responseBody}", MessageEnum.Info);

                    response.EnsureSuccessStatusCode();

                    // 检查是否是验证码错误（仅对非文件下载请求）
                    if (string.IsNullOrEmpty(fileName) && (responseBody.Contains("\"ok\":-100") || responseBody.Contains("\"ok\": -100")))
                    {
                        logAction?.Invoke($"⚠️ 检测到验证码验证要求（ok=-100）", MessageEnum.Warning);
                        
                        // 尝试解析 JSON 提取验证码 URL
                        try
                        {
                            var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                            if (errorResponse != null && errorResponse.url != null)
                            {
                                string captchaUrl = errorResponse.url.ToString();
                                logAction?.Invoke($"🔗 验证码链接: {captchaUrl}", MessageEnum.Warning);
                                
                                // 调用全局验证码处理器
                                if (GlobalCaptchaHandler != null)
                                {
                                    logAction?.Invoke($"⏳ 等待用户完成验证码验证...", MessageEnum.Info);
                                    bool validationSuccess = await GlobalCaptchaHandler(captchaUrl);
                                    
                                    if (validationSuccess)
                                    {
                                        logAction?.Invoke($"✅ 验证码验证完成，重试请求...", MessageEnum.Success);
                                        retryCount++;
                                        continue; // 重试请求
                                    }
                                    else
                                    {
                                        logAction?.Invoke($"❌ 验证码验证失败或取消", MessageEnum.Error);
                                        throw new Exception("验证码验证失败");
                                    }
                                }
                                else
                                {
                                    logAction?.Invoke($"❌ 未配置验证码处理器，无法自动处理验证码", MessageEnum.Error);
                                    throw new Exception("需要手动处理验证码");
                                }
                            }
                        }
                        catch (Exception ex) when (!(ex is Exception) || ex.Message == "验证码验证失败" || ex.Message == "需要手动处理验证码")
                        {
                            throw;
                        }
                        catch
                        {
                            logAction?.Invoke($"⚠️ 无法解析验证码URL，原始响应: {responseBody}", MessageEnum.Warning);
                            throw new Exception("检测到验证码但无法解析URL");
                        }
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string directory = Path.GetDirectoryName(fileName);
                        string fileNameOnly = Path.GetFileName(fileName);

                        var invalidChar = Path.GetInvalidFileNameChars();
                        var cleanedFileNameOnly = invalidChar.Aggregate(fileNameOnly, (o, r) => (o.Replace(r.ToString(), string.Empty)));

                        string finalFullPath;
                        if (cleanedFileNameOnly.Length > 200)
                        {
                            // Truncate the file name part, then recombine with the original directory
                            string truncatedFileName = cleanedFileNameOnly.Substring(0, 200) + Path.GetExtension(cleanedFileNameOnly);
                            finalFullPath = Path.Combine(directory, truncatedFileName);
                        }
                        else
                        {
                            // Recombine the original directory with the cleaned file name
                            finalFullPath = Path.Combine(directory, cleanedFileNameOnly);
                        }

                        string uniquePath = GetUniqueFileName(finalFullPath);

                        var stream = response.Content.ReadAsStream();
                        FileStream lxFS = File.Create(uniquePath);
                        await stream.CopyToAsync(lxFS);
                        lxFS.Close();
                        lxFS.Dispose();
                        stream.Dispose();

                        return default(T);
                    }

                    Type type = typeof(T);
                    if (type == typeof(string))
                        return (T)Convert.ChangeType(responseBody, typeof(T));
                    else
                        return JsonConvert.DeserializeObject<T>(responseBody);
                }
                catch (Exception ex)
                {
                    // 如果是验证码相关的异常且还有重试次数，不记录错误日志
                    if (ex.Message.Contains("验证码") && retryCount < maxRetries)
                    {
                        retryCount++;
                        continue;
                    }
                    
                    logAction?.Invoke($"[Request Failed]: URL: {url}", MessageEnum.Error);
                    if (responseBody != null && responseBody.Contains("<title>登录 - 微博</title>"))
                    {
                        logAction?.Invoke($"[Cookie可能失效]: {responseBody}", MessageEnum.Error);
                    }
                    logAction?.Invoke($"[Exception]: {ex.ToString()}", MessageEnum.Error);
                    throw;
                }
            }
            
            throw new Exception("达到最大重试次数，请求失败");
        }

        public static string GetUniqueFileName(string filePath)
        {
            // 获取文件目录和扩展名
            string directory = Path.GetDirectoryName(filePath)!;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int count = 1;

            // 初始文件路径
            string uniqueFilePath = filePath;

            // 检查文件是否存在，如果存在则循环添加编号直到找到一个不存在的文件名
            while (File.Exists(uniqueFilePath))
            {
                string newFileName = $"{fileNameWithoutExtension}({count}){extension}";
                uniqueFilePath = Path.Combine(directory, newFileName);
                count++;
            }

            return uniqueFilePath;
        }
    }
}
