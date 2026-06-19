export default async function bffFetch(url: string): Promise<Response> {
  return await fetch(url, {
    headers: { "X-CSRF": "1" },
    credentials: "include",
  });
}
