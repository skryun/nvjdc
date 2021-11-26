<template>
  <el-row :span="24" v-show="isSHowAnnouncement">
    <el-col :span="24">
      <el-card style="max-width: 510px;justify-content: center;margin: auto;">
     <el-tag  style="font-size: var(--el-font-size-large)"
          ><i class="el-icon-chat-round"></i> 公告</el-tag>
        <p>
          <span
            style="font-size: var(--el-font-size-small)"
            v-html="announcement"
          ></span>
        </p>
      </el-card>
    </el-col>
  </el-row>
  <el-row :span="24" v-show="ck !== ''">
    <el-col :span="24">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>CK</span>
          </div>
        </template>
        <div class="card-body" style="word-break: break-all">
          <p>{{ ck }}</p>
        </div>
        <div class="card-footer">
          <el-button
            id="btncopy"
            type="primary"
            size="small"
            :data-clipboard-text="ck"
            auto
            @click="CopyCK"
            >复制CK</el-button
          >
        </div>
      </el-card>
    </el-col>
  </el-row>
  <el-row :span="24" v-show="ShowCaptcha" @touchend="remove">
    <el-col :span="24">
      <el-card style="max-width: 510px;justify-content: center;margin: auto;" >
        <template #header>
          <div class="card-header">
            <span>请完成安全验证</span>
          </div>
        </template>
        <div class="card-body">
          <el-row class="demo-autocomplete">
            <el-col>
              <el-tag type="success">资源余量：{{ tabcount }}</el-tag>
              <el-tag>当前占用:{{ marginCount }}</el-tag>
              <el-tag type="danger">{{ times }}秒后释放资源</el-tag>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <div id="captcha"></div>
            </el-col>
          </el-row>
        </div>
      </el-card>
    </el-col>
  </el-row>
  <el-row :span="24" v-show="!ShowCaptcha">
    <el-col :span="24">
     
        <el-card style="max-width: 510px;justify-content: center;margin: auto;">
        <div class="card-body" >
           <el-tabs v-model="activeName"   >
          <el-tab-pane label="短信登陆" name="first"> 
              <el-row class="demo-autocomplete" v-show="uploadtype === 'ql'">
            <el-col  style="font-size: 14px">
              服务器余量：{{ ckcount }} <br>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <el-tag type="success">资源余量：{{ tabcount }}</el-tag>
              <el-tag>当前占用:{{ marginCount }}</el-tag>
              <el-tag type="danger">{{ times }}秒后释放资源</el-tag>
            </el-col>
          </el-row>
        
          <el-row class="demo-autocomplete" v-show="uploadtype === 'xdd'">
            <el-col>
              <span class="elabe">你的QQ</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete" v-show="uploadtype === 'xdd'">
            <el-col>
              <el-input
                style="max-width: 260px"
                v-model="QQ"
                placeholder="QQ"
                prefix-icon="el-icon-chat-round"
              />
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <span class="elabe">你的手机号码</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <el-input
                style="max-width: 260px"
                v-model="phone"
                placeholder="Phone"
                prefix-icon="el-icon-mobile-phone"
              />
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <span class="elabe">验证码</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <el-input
                v-model="code"
                style="max-width: 150px"
                placeholder="Code"
                prefix-icon="el-icon-lock"
              />
              <el-button
                type="success"
                v-show="isShow"
                @click="GetSMSCode"
                plain
                style="width: 110px"
                >获取验证码</el-button
              >
              <el-button
                type="success"
                v-show="!isShow"
                plain
                style="width: 110px"
                disabled
                >{{ Codetime }}秒后重发</el-button
              >
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete" v-show="uploadtype === 'ql'">
            <el-col>
              <span class="elabel">服务器</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete" v-show="uploadtype === 'ql'">
            <el-col>
              <el-select
                v-model="optionsvlue"
                placeholder="Select"
                style="width: 100%; max-width: 260px"
                @change="valuechange"
              >
                <el-option
                  v-for="item in options"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <span style="float: left">{{ item.label }}</span>
                  <span
                    style="
                      float: right;
                      color: var(--el-text-color-secondary);
                      font-size: 13px;
                    "
                    >{{ item.count }}</span
                  >
                </el-option>
              </el-select>
            </el-col>
          </el-row>
          <el-button type="primary" @click="Login">登录 </el-button>
           </el-tab-pane>
          <el-tab-pane label="WSKEY" name="WSKEY" :disabled="uploadtype !=='ql'">
            <el-row class="demo-autocomplete">
            <el-col>
              <el-tag>WSKEY容量:{{ WSKEYCount }}</el-tag>
            </el-col>
          </el-row>
              <el-row class="demo-autocomplete">
            <el-col>
              <span class="elabe">WSKEY</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <el-input
                style="max-width: 260px"
                v-model="wskey"
                placeholder="wskey"
                prefix-icon="el-icon-edit"
              />
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <span class="elabe">备注</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete">
            <el-col>
              <el-input
                v-model="remarks"
                style="max-width: 260px"
                placeholder="remarks"
                prefix-icon="el-icon-lock"
              />
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete" v-show="uploadtype === 'ql'">
            <el-col>
              <span class="elabel">服务器</span>
            </el-col>
          </el-row>
          <el-row class="demo-autocomplete" v-show="uploadtype === 'ql'">
            <el-col>
              <el-select
                v-model="optionsvlue"
                placeholder="Select"
                style="width: 100%; max-width: 260px"
                @change="valuechange"
              >
                <el-option
                  v-for="item in options"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <span style="float: left">{{ item.label }}</span>
                  <span
                    style="
                      float: right;
                      color: var(--el-text-color-secondary);
                      font-size: 13px;
                    "
                    >{{ item.count }}</span
                  >
                </el-option>
              </el-select>
            </el-col>
          </el-row>
          <el-button type="primary" @click="UploadWSKEYS">上传 </el-button>
          </el-tab-pane>
          </el-tabs>
         
        </div>
       </el-card>
      
       
    </el-col>
  </el-row>
</template>
<style>
.elabe {
  font-size: var(--el-font-size-base);
}
.demo-autocomplete {
  margin-bottom: 10px;
}
.esuccess {
  color: var(--el-color-success) !important;
}
.block {
  position: absolute;
  left: 0;
  top: 0;
}

.sliderContainer {
  position: relative;
  text-align: center;
  line-height: 40px;
  background: #f7f9fa;
  color: #45494c;
  border-radius: 2px;
  margin-bottom: 50px;
}

.sliderbg {
  position: absolute;
  left: 0;
  right: 0;
  top: 0;
  background-color: #f7f9fa;
  height: 40px;
  border-radius: 2px;
  border: 1px solid #e6e8eb;
}

.sliderContainer_active .slider {
  top: -1px;
  border: 1px solid #1991fa;
}

.sliderContainer_active .sliderMask {
  border-width: 1px 0 1px 1px;
}

.sliderContainer_success .slider {
  top: -1px;
  border: 1px solid #52ccba;
  background-color: #52ccba !important;
}

.sliderContainer_success .sliderMask {
  border: 1px solid #52ccba;
  border-width: 1px 0 1px 1px;
  background-color: #d2f4ef;
}

.sliderContainer_success .sliderIcon:before {
  content: "\f00c";
}

.sliderContainer_fail .slider {
  top: -1px;
  border: 1px solid #f57a7a;
  background-color: #f57a7a !important;
}

.sliderContainer_fail .sliderMask {
  border: 1px solid #f57a7a;
  background-color: #fce1e1;
  border-width: 1px 0 1px 1px;
}

.sliderContainer_fail .sliderIcon:before {
  content: "\f00d";
}

.sliderContainer_active .sliderText,
.sliderContainer_success .sliderText,
.sliderContainer_fail .sliderText {
  display: none;
}

.sliderMask {
  position: absolute;
  left: 0;
  top: 0;
  height: 40px;
  border: 0 solid #1991fa;
  background: #d1e9fe;
  border-radius: 2px;
}

.slider {
  position: absolute;
  top: 0;
  left: 0;
  width: 40px;
  height: 40px;
  background: #fff;
  box-shadow: 0 0 3px rgba(0, 0, 0, 0.3);
  cursor: pointer;
  transition: background 0.2s linear;
  border-radius: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.slider:hover {
  background: #1991fa;
}

.slider:hover .sliderIcon {
  background-position: 0 -13px;
}

.sliderText {
  position: relative;
}

.refreshIcon {
  position: absolute;
  right: 0;
  top: 0;
  cursor: pointer;
  margin: 6px;
  color: rgba(0, 0, 0, 0.25);
  font-size: 1rem;
  z-index: 5;
  transition: color 0.3s linear;
}

.refreshIcon:hover {
  color: #6c757d;
}
</style>
<script>
import { onMounted, reactive, toRefs } from "vue";
import { useRouter } from "vue-router";
import { ElLoading } from "element-plus";
import $ from "jquery";
import { ElMessage, ElNotification } from "element-plus";
import "@/assets/slidercaptch.js";
import {
  getConfigMain,
  getQLConfig,
  SendSMS,
  VerifyCode,
  AutoCaptcha,
  VerifyCaptcha,
  UploadWSKEY
} from "@/api/index";
import Clipboard from "clipboard";
export default {
  setup() {
    const router = useRouter();
    let data = reactive({
      marginCount: 0,
       WSKEYCount: 0,
      ckcount: 0,
      ctime:180,
      tabcount: 0,
      allowAdd: true,
      First: true,
      isShow: true,
      autocount:0,
      ShowCaptcha: false,
      code: "",
      ck: "",
      QQ: "",
      wskey:"",
      remarks:"",
      activeName: 'first',
      times: 0,
      Codetime: 60,
      isSHowAnnouncement: false,
      announcement: "",
      centerDialogVisible: true,
      getCode: false,
      options: [],
      optionsvlue: 0,
      uploadtype: "none",
      phone: "",
      oldphone: "",
      VsliderCaptcha: null,
      form: {
        code: "",
        phone: "",
      },
    });

    const CopyCK = async () => {
      var clipboard = new Clipboard("#btncopy"); // 这里可以理解为选择器，选择上面的复制按钮
      clipboard.on("success", () => {
        console.log("复制成功");
        ElMessage.success("复制成功");

        clipboard.destroy();
      });
      clipboard.on("error", () => {
        console.log("复制失败");
        ElMessage.error("复制失败");
        // 释放内存
        clipboard.destroy();
      });
    };
    const getmainConfig = async () => {
      const cphone = localStorage.getItem("phone");
      if (cphone) {
        data.phone = cphone;
        localStorage.removeItem("phone");
      }
      data.VsliderCaptcha = $("#captcha").sliderCaptcha({
        id: "captcha",
        width: 275,
        height: 170,
        sliderL: 51,
        barText: "向右滑动填充拼图",
        remoteUrl: "123",
        verify: async function (arr, url) {
          console.log(url);
          const loading = ElLoading.service({
          lock: true,
          text: "验证中请稍候..",
        });
          
          var VerifyCaptchadata = await VerifyCaptcha({
            Phone: data.phone,
            point: arr,
          });
          loading.close();
          if (VerifyCaptchadata.success) {
            data.isShow = false;
            data.getCode = true;
            let timer2 = setInterval(function () {
              data.Codetime--;
              if (data.Codetime <= 0) {
                clearInterval(timer2);
                data.isShow = true;
              }
            }, 1000);
            data.ShowCaptcha=false;
            ElMessage.success("认证成功");
            return true;
          } else {
            if (VerifyCaptchadata.data.status == 666) {
              ElMessage.error("验证失败,请重试");
              data.VsliderCaptcha.reset(
                VerifyCaptchadata.data.big,
                VerifyCaptchadata.data.small
              );
            } else {
              data.ShowCaptcha = false;
            }
          }
        },
        onSuccess: function () {},
      });
      const configdata = await getConfigMain();
      console.log(configdata);
      data.uploadtype = configdata.data.type;
      data.ctime=configdata.data.closetime;
      data.announcement = configdata.data.announcement;
      if (configdata.data.announcement != "") {
        data.isSHowAnnouncement = true;
      }
      ElNotification.success({
        title: "提示",
        message: "获取验证码后"+data.ctime+"分钟后会释放资源,请在"+ data.ctime+"分钟内完成登录",
        iconClass: "esuccess",
      });
      if (!configdata.success) {
        ElMessage.error(configdata.message);
        return;
      }
     if (configdata.data.autocount)data.autocount=configdata.data.autocount;
      if (configdata.data.ckcount) data.ckcount = configdata.data.ckcount;
      if(configdata.data.wskeycount)data.WSKEYCount=configdata.data.wskeycount;
      data.tabcount = configdata.data.tabcount;
      if (configdata.data.list.length > 0) {
        configdata.data.list.forEach((element) => {
          if (data.optionsvlue == 0) data.optionsvlue = element.qLkey;
          data.options.push({
            value: element.qLkey,
            label: element.qlName,
            count: "容量:" + element.qL_CAPACITY,
          });
        });
      }
    };
    const valuechange = async (value) => {
      const configdata = await getQLConfig(value);
      data.ckcount = configdata.data.ckcount;
      data.tabcount = configdata.data.tabcount;
      data.WSKEYCount=configdata.data.wskeycount;
      if (!configdata.success) {
        ElMessage.error(configdata.message);
        return;
      }
      ElMessage.success("切换成功");
    };
    onMounted(getmainConfig);
    const UploadWSKEYS = async () => {
        if(!data.wskey)ElMessage.error("请先输入wskey");
        if(!data.remarks)ElMessage.error("请先输入备注");
        const loading = ElLoading.service({
        lock: true,
        text: "正在上传",
      });
      const body = await UploadWSKEY({
        wskey: data.wskey,
        remarks: data.remarks,
        qlkey: data.optionsvlue,
      });
       loading.close();
       if (!body.success) {
          ElMessage.error(body.message);
      } else {

         localStorage.setItem("qlid", body.data.qlid);
          localStorage.setItem("qlkey", body.data.qlkey);
          router.push("/");
      }
    }
    const Login = async () => {
      console.log(data.getCode);
      if (!data.code) ElMessage.error("请先输入验证码");
      if (!data.phone) ElMessage.error("请先输入手机");
      if (!data.getCode) ElMessage.error("请先获取验证码");
      if (!data.QQ && data.uploadtype == "xdd") ElMessage.error("请先获取QQ");
      const loading = ElLoading.service({
        lock: true,
        text: "正在登陆",
      });
      const body = await VerifyCode({
        Phone: data.phone,
        QQ: data.QQ,
        qlkey: data.optionsvlue,
        Code: data.code,
      });
      loading.close();
      if (!body.success) {
        ElMessage.error(body.message);
      } else {
        data.marginCount = 0;
        if (body.data.tabcount) data.tabcount = body.data.tabcount;
        console.log(body);
        if (data.uploadtype == "ql") {
          ElNotification.success({
            title: "提示",
            message: "登录成功",
            iconClass: "esuccess",
          });
          localStorage.setItem("qlid", body.data.qlid);
          localStorage.setItem("qlkey", body.data.qlkey);
          router.push("/");
        } else {
          if (data.uploadtype == "none") data.ck = body.data.ck;
          var message = "登陆成功";
          if (data.uploadtype == "xdd") {
            message = "xdd推送成功！";
            data.ck = "";
          }
          ElNotification.success({
            title: "提示",
            message: message,
            iconClass: "esuccess",
          });
        }
      }
    };
    const SMSTIME = (body) => {
      if (body.success == false && body.data.status != 666) {
        ElMessage.error(body.message);
        return false;
      }
      data.tabcount = body.data.tabcount;
      data.ckcount = body.data.ckcount;
      data.isShow = false;
      data.getCode = true;
      let timer2 = setInterval(function () {
        data.Codetime--;
        if (data.Codetime <= 0) {
          clearInterval(timer2);
          data.isShow = true;
        }
      }, 1000);
    };
    const GetSMSCode = async () => {
      if (data.phone == "") {
        ElMessage.error("请输入手机号码");
        return false;
      }
      var re = /^1\d{10}$/;
      var res = re.test(data.phone);
      if (!res) {
        ElMessage.error("手机号错误");
        return false;
      }
      if (data.oldphone != "" && data.phone != data.oldphone) {
        data.First = true;
      }
      if (data.oldphone != "" && data.phone == data.oldphone) {
        data.First = false;
      }

      if (data.First) {
        const loading = ElLoading.service({
          lock: true,
          text: "正在获取验证码",
        });

        data.marginCount = 1;
        data.times = data.ctime*60;
        data.Codetime = 60;
        data.oldphone = data.phone;
        data.First = false;

        const body = await SendSMS({
          Phone: data.phone,
          qlkey: data.optionsvlue,
        });
        if (body.success == true || body.data.status == 666) {
          let timer = setInterval(function () {
            data.times--;
            if (data.times <= 0) {
              clearInterval(timer);
            }
          }, 1000);
        }

        if (body.success == false && body.data.status == 666) {
          loading.setText("正在安全验证");
          console.log("安全验证");
          var AutoCaptchadata = null;
          for (let index = 0; index < data.autocount; index++) {
            let count = index + 1;
            loading.setText("第" + count + "次安全验证");
            AutoCaptchadata = await AutoCaptcha({ Phone: data.phone });

            if (AutoCaptchadata.success) break;
            if (
              !AutoCaptchadata.success &&
              AutoCaptchadata.data.status != 666
            ) {
              break;
            }
          }
          if(data.autocount==0) {
            AutoCaptchadata=body;
          }
          if (AutoCaptchadata.success) body.success = true;
          else {
             loading.close();
            if(AutoCaptchadata.data.status != 666){
               ElMessage.error(AutoCaptchadata.message);
               return false;
            }
           
            data.VsliderCaptcha.reset(
              AutoCaptchadata.data.big,
              AutoCaptchadata.data.small
            );

            data.ShowCaptcha = true;
            return false;
          }
        }
        loading.close();
        SMSTIME(body);
      } else {
        localStorage.setItem("phone", data.phone);
        router.go(0);
      }
    };
    return {
      ...toRefs(data),
      CopyCK,
      Login,
      UploadWSKEYS,
      GetSMSCode,
      valuechange,
    };
  },
};
</script>