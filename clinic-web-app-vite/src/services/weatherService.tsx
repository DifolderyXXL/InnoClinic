import { api } from './api'
import { catchResult } from '#/helpers/ServiceResult'

export const weatherService = {
  weatherforecast: async () => {
    return catchResult(api.get('/weatherforecast'))
  },
}
