<template>
  <div>
    <p>
      Current count (Vuex):
      <strong>{{ currentCount }}</strong>
    </p>
    <v-btn class="ma-2" color="info" @click.prevent="increment">Increment</v-btn>
    <v-btn class="ma-2" color="info" @click.prevent="reset">Reset</v-btn>
  </div>
</template>

<script lang="ts">
import { Action, Getter } from 'vuex-class'
import { Component, Vue } from 'vue-property-decorator'
const namespace = 'counter'

@Component
export default class Counter extends Vue {
  @Getter('currentCount', { namespace })
  private currentCount!: number

  @Action('increment', { namespace })
  private incrementCounter!: () => void

  @Action('reset', { namespace })
  private resetCounter!: () => void

  private increment() {
    this.incrementCounter()
  }

  private reset() {
    this.resetCounter()
  }
}
</script>
