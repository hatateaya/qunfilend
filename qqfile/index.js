const account =YOUR_QQ_ID
const bot = require("oicq").createClient(account)
bot.on("system.login.slider", function (event) { //监听滑动验证码事件
  process.stdin.once("data", (input) => {
    this.sliderLogin(input); //输入ticket
  });
}).on("system.login.device", function (event) { //监听登录保护验证事件
  process.stdin.once("data", () => {
    this.login(); //验证完成后按回车登录
  });
}).login("PWDPWDPWDPWDPWDPWD"); //需要填写密码或md5后的密码
exports.bot = bot
console.log(bot.bkn)
console.log(bot.cookies)
console.log(bot.uin)