<template>
  <v-container fluid>
    <v-slide-y-transition mode="out-in">
      <v-row>
        <v-col>
          <h1>Counter</h1>
          <p>This is a simple example of a Vue.js component test integrated with Vuex</p>
          <Counter />
        </v-col>
      </v-row>
    </v-slide-y-transition>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator'
import Counter from '@/components/Counter.vue'

@Component({
  components: { Counter }
})
export default class CounterView extends Vue {}
</script>
