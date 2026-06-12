import { authorizationApi } from './api'
import { catchResult } from '#/helpers/ServiceResult'

export const weatherService = {
  weatherforecast: async () => {
    return catchResult(authorizationApi.get('/weatherforecast'))
  },
}
