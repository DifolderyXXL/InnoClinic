import { profilesApi } from './api'
import { catchResult } from '#/helpers/ServiceResult'

export const profileService = {
  myProfile: async () => {
    return catchResult(profilesApi.get('/my-profile'))
  },
}
