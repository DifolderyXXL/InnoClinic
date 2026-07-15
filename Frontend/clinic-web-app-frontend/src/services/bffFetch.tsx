export default async function bffFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const mergedHeaders = new Headers(options.headers);
  mergedHeaders.set("X-CSRF", "1");
  
  return await fetch(url, {
    ...options,
    headers: mergedHeaders,
    credentials: "include"
  });
}
