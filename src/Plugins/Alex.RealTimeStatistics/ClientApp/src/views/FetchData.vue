<template>
  <v-container fluid>
    <v-slide-y-transition mode="out-in">
      <v-row>
        <v-col>
          <h1>Weather forecast</h1>
          <p>This component demonstrates fetching data from the server.</p>

          <v-data-table
            :headers="headers"
            :items="forecasts"
            hide-default-footer
            :loading="loading"
            class="elevation-1"
          >
            <template v-slot:progress>
              <v-progress-linear color="blue" indeterminate></v-progress-linear>
            </template>
            <template v-slot:[`item.date`]="{ item }">
              <td>{{ item.date | date }}</td>
            </template>
            <template v-slot:[`item.temperatureC`]="{ item }">
              <v-chip :color="getColor(item.temperatureC)" dark>{{ item.temperatureC }}</v-chip>
            </template>
          </v-data-table>
        </v-col>
      </v-row>
    </v-slide-y-transition>

    <v-alert :value="showError" type="error" v-text="errorMessage">
      This is an error alert.
    </v-alert>

    <v-alert :value="showError" type="warning">
      Are you sure you're using ASP.NET Core endpoint? (default at
      <a href="http://localhost:5000/fetch-data">http://localhost:5000</a>)
      <br />
      API call would fail with status code 404 when calling from Vue app (default at
      <a href="http://localhost:8080/fetch-data">http://localhost:8080</a>) without devServer proxy
      settings in vue.config.js file.
    </v-alert>
  </v-container>
</template>

<script lang="ts">
// an example of a Vue Typescript component using Vue.extend
import Vue from 'vue'
import { Forecast } from '../models/Forecast'

export default Vue.extend({
  data() {
    return {
      loading: true,
      showError: false,
      errorMessage: 'Error while loading weather forecast.',
      forecasts: [] as Forecast[],
      headers: [
        { text: 'Date', value: 'date' },
        { text: 'Temp. (C)', value: 'temperatureC' },
        { text: 'Temp. (F)', value: 'temperatureF' },
        { text: 'Summary', value: 'summary' }
      ]
    }
  },
  methods: {
    getColor(temperature: number) {
      if (temperature < 0) {
        return 'blue'
      } else if (temperature >= 0 && temperature < 30) {
        return 'green'
      } else {
        return 'red'
      }
    },
    async fetchWeatherForecasts() {
      try {
        const response = await this.$axios.get<Forecast[]>('api/WeatherForecast')
        this.forecasts = response.data
      } catch (e) {
        this.showError = true
        this.errorMessage = `Error while loading weather forecast: ${e.message}.`
      }
      this.loading = false
    }
  },
  async created() {
    await this.fetchWeatherForecasts()
  }
})
</script>
