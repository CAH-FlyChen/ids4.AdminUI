import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

import tabstore from './components/TabStore'

export default new Vuex.Store({
  state: {
    CurTabIndex: 3,
    Tabs: [{
      title: '首页',
      path:'index',
      closable:false
    }, {
      title: 'Tab 2',
      path:'other',
      closable:true
    }]
  },
  mutations: {

  },
  actions: {

  },
  modules:{
    tab:tabstore
  }
})
