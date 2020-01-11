
import 'babel-polyfill'
import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'

import ElementUI from 'element-ui';
import 'element-ui/lib/theme-chalk/index.css';
import './assets/css/reset.css'
import './assets/css/index.css'
import http from './plugin/http.js'
Vue.config.productionTip = false

Vue.use(ElementUI);

Vue.use(http);

import './mixin'
import mgr from './plugin/oidc'
import base from './plugin/base64'

var main = async ()=>{
  var user = await mgr.getUser();
  if (user&&user.expired===false){
    console.log(user);
    http.http.setheader(user.access_token);
    base.user = user;
    new Vue({
      router,
      store,
      render: h => h(App)
    }).$mount('#app')
  }
  else mgr.signinRedirect();
}

main();


