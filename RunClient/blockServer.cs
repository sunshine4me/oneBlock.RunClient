using Block.Interface;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace RunClient {
    /// <summary>
    /// 用来接收oneBlock 案例的类
    /// </summary>
    public class blockServer {
        //定义delegate
        public delegate ICaseResult runDelegate(OneBlockCase tc);



        /// <summary>
        /// debug事件
        /// </summary>
        public event runDelegate debugEvent;


        private int RunCount;

        /// <summary>
        /// 启动端口监听服务
        /// </summary>
        /// <param name="port">端口</param>
        public void StartListener(int port) {
            using (HttpListener listerner = new HttpListener()) {
                listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证 Anonymous匿名访问
                listerner.Prefixes.Add($"http://+:{port}/testRun/");
                var logger = LogManager.GetLogger(GetType());
                logger.Debug($"开始监听{port},准备接收测试案例");

                listerner.Start();
                //listerner.BeginGetContext();//异步调用(允许并发处理)


               
                while (true) {
                    //等待请求连接
                    HttpListenerContext ctx = listerner.GetContext();
                    ctx.Response.StatusCode = 200;//设置返回给客服端http状态代码
                    ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");


                    StreamReader reader = new StreamReader(ctx.Request.InputStream);

                    try {
                        logger.Debug("接收案例数据,数据初始化...");
                        var str = reader.ReadToEnd();
                        var testcase = JsonConvert.DeserializeObject<OneBlockCase>(str);
                        logger.Debug("数据初始化完成!");


                        if (RunCount <= 0) {
                            if (debugEvent != null) { // 如果有对象注册  
                                RunCount = debugEvent.GetInvocationList().Count();
                                foreach (runDelegate de in debugEvent.GetInvocationList()) {
                                    de.BeginInvoke(testcase, new AsyncCallback(runCallBack), "Debug");
                                }
                            } else {
                                logger.Warn("未注册Debug事件,执行中止!");
                            }

                        } else {
                            ctx.Response.StatusCode = 503;//设置返回给客服端http状态代码
                            System.IO.Stream output = ctx.Response.OutputStream;
                            System.IO.StreamWriter writer = new System.IO.StreamWriter(output);

                            logger.Debug("Debug模式正在执行其他案例,请稍后再试!");
                            // 必须关闭输出流
                            writer.Close();
                        }
                    } catch (Exception e) {
                        logger.Error($"http请求错误:{e.Message} \r\n{e.StackTrace}");

                        ctx.Response.StatusCode = 500;//设置返回给客服端http状态代码

                        System.IO.Stream output = ctx.Response.OutputStream;
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(output);
                        writer.Write(e.Message);
                        // 必须关闭输出流
                        writer.Close();

                    }

                    ctx.Response.Close();

                }
            }
        }

        /// <summary>
        /// 回调函数(Todo something)
        /// </summary>
        private void runCallBack(IAsyncResult result) {
            RunCount--;
            var logger = LogManager.GetLogger(GetType());
            logger.Info($"{result.AsyncState}执行完成.");
            //runDelegate handler = (runDelegate)((AsyncResult)result).AsyncDelegate;
            //var re = handler.EndInvoke(result);
        }

        
    }



}
