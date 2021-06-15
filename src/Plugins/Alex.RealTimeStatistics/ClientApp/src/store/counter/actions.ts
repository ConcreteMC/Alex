import { ActionTree } from 'vuex'
import { CounterState } from './types'
import { RootState } from '../types'

export const actions: ActionTree<CounterState, RootState> = {
  increment({ commit }) {
    commit('incrementCounter')
  },
  reset({ commit }) {
    commit('resetCounter')
  }
}
