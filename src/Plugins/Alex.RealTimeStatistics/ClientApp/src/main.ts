import Vue from 'vue'
import './plugins/axios'
import vuetify from './plugins/vuetify'
import App from './App.vue'
import router from './router'
import store from '@/store/index'
import './registerServiceWorker'
import dateFilter from '@/filters/date.filter'

Vue.config.productionTip = false

Vue.filter('date', dateFilter)

new Vue({
  vuetify,
  router,
  store,
  render: (h) => h(App)
}).$mount('#app')
