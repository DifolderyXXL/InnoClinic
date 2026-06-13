export async function throwResponseErrors(response: any) {
  if (!response.ok) {
    const responseTargetErrors = response.error?.response?.data

    console.log(responseTargetErrors)
    if (responseTargetErrors && Array.isArray(responseTargetErrors)) {
      const targetErrors = responseTargetErrors.flatMap((x) => x.description)

      console.log(targetErrors)

      if (targetErrors.length > 0) {
        throw new Error(targetErrors.join(';\n'))
      }
    }

    const errorData = await response.error.json().catch(() => ({}))
    throw new Error(errorData.message || 'Something went wrong')
  }
}

export function fieldSpecificErrors(meta: any) {
  return meta.isTouched && meta.errors.length ? (
    <em style={{ color: 'red' }}>
      {meta.errors.map((err: any) => err?.message || err).join(', ')}
    </em>
  ) : null
}
