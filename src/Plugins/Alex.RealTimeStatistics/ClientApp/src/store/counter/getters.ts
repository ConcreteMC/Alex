import { GetterTree } from 'vuex'
import { CounterState } from './types'
import { RootState } from '../types'

export const getters: GetterTree<CounterState, RootState> = {
  currentCount(state): number {
    return state.counter
  }
}
