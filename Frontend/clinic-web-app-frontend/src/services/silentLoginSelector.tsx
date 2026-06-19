import { silentLoginUrl } from "./bffEndpoints";

export function silentLogin(
  iframeSelector = "#bff-silent-login",
): Promise<boolean> {
  const timeout = 5000;

  return new Promise((resolve) => {
    function onMessage(e: MessageEvent) {
      if (e.data && e.data["source"] === "bff-silent-login") {
        window.removeEventListener("message", onMessage);
        resolve(!!e.data.isLoggedIn);
      }
    }

    window.addEventListener("message", onMessage);

    const timer = setTimeout(() => {
      window.removeEventListener("message", onMessage);
      resolve(false);
    }, timeout);

    const iframe = document.querySelector(
      iframeSelector,
    ) as HTMLIFrameElement | null;
    if (iframe) {
      iframe.src = silentLoginUrl;
    } else {
      clearTimeout(timer);
      window.removeEventListener("message", onMessage);
      resolve(false);
    }
  });
}
