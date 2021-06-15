<template>
  <v-app>
    <v-navigation-drawer
      persistent
      :mini-variant="miniVariant"
      :clipped="clipped"
      v-model="drawer"
      enable-resize-watcher
      fixed
      app
    >
      <v-list>
        <v-list-item value="true" v-for="(item, i) in items" :key="i" :to="item.link">
          <v-list-item-action>
            <v-icon v-html="item.icon"></v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title v-text="item.title"></v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar app :clipped-left="clipped" color="info" dark>
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
      <v-btn class="d-none d-lg-flex" icon @click.stop="miniVariant = !miniVariant">
        <v-icon v-html="miniVariant ? 'mdi-chevron-right' : 'mdi-chevron-left'"></v-icon>
      </v-btn>
      <v-btn class="d-none d-lg-flex" icon @click.stop="clipped = !clipped">
        <v-icon>mdi-web</v-icon>
      </v-btn>
      <v-toolbar-title v-text="title"></v-toolbar-title>
      <v-spacer></v-spacer>
    </v-app-bar>

    <v-main>
      <router-view />
    </v-main>

    <v-footer app>
      <span>&nbsp;Software Ateliers&nbsp;&copy;&nbsp;2021</span>
    </v-footer>
  </v-app>
</template>

<script lang="ts">
import HelloWorld from '@/components/HelloWorld.vue'
import { Component, Vue } from 'vue-property-decorator'

@Component({
  components: { HelloWorld }
})
export default class App extends Vue {
  private clipped = true
  private drawer = true
  private miniVariant = false
  private right = true
  private title = 'ASP.NET Core Vue Starter'
  private items = [
    { title: 'Home', icon: 'mdi-home', link: '/' },
    { title: 'Counter', icon: 'mdi-gesture-tap', link: '/counter' },
    { title: 'Fetch data', icon: 'mdi-download', link: '/fetch-data' }
  ]
}
</script>
