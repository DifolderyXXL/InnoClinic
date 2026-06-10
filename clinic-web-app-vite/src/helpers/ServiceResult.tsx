export type ServiceResult<T> = { ok: true; data: T } | { ok: false; error: any }

export async function catchResult<T>(
  promise: Promise<{ data: T }>,
): Promise<ServiceResult<T>> {
  try {
    const response = await promise
    return { ok: true, data: response.data }
  } catch (error) {
    return { ok: false, error }
  }
}
