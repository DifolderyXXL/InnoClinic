import { useState, useEffect } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "./assets/vite.svg";
import heroImg from "./assets/hero.png";
import "./App.css";

// Константы эндпоинтов
const loginUrl = "/bff/login";
const silentLoginUrl = "/bff/silent-login";
const userUrl = "/bff/user";

interface ClaimItem {
  type: string;
  value: string | object;
  valueType?: string | null;
}

// Базовая функция для запросов с CSRF-заголовком
async function bffFetch(url: string): Promise<Response> {
  return await fetch(url, {
    headers: { "X-CSRF": "1" },
    credentials: "include",
  });
}

// Функция бесшумного логина через iframe (из твоего исходного JS-примера)
function silentLogin(iframeSelector = "#bff-silent-login"): Promise<boolean> {
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

// Компонент кнопки для API запросов
interface ApiButtonProps {
  api: string;
  onDataFetched: (data: unknown) => void;
  onError: (error: unknown) => void;
}

function ApiButton({ api, onDataFetched, onError }: ApiButtonProps) {
  return (
    <button
      onClick={async () => {
        try {
          const response = await bffFetch(api);
          if (response.ok) {
            const data = await response.json();
            onDataFetched(data);
          } else {
            onError(`Ошибка: ${response.status}`);
          }
        } catch (error) {
          console.error(`Fetch failed for ${api}`, error);
          onError(error);
        }
      }}
    >
      Запрос к {api}
    </button>
  );
}

function App() {
  const [count, setCount] = useState(0);
  const [userClaims, setUserClaims] = useState<ClaimItem[] | null>(null);
  const [logoutUrlState, setLogoutUrlState] = useState("/bff/logout");
  const [apiResponse, setApiResponse] = useState<string>("");
  const [logMessage, setLogMessage] = useState<string>("");

  // Аналог функции onLoad из оригинального скрипта
  useEffect(() => {
    async function checkAuth() {
      try {
        const resp = await bffFetch(userUrl);

        if (resp.ok) {
          const claims: ClaimItem[] = await resp.json();
          setUserClaims(claims);
          setLogMessage("User logged in");

          // Ищем динамический logoutUrl в клеймах
          const logoutUrlClaim = claims.find(
            (claim) => claim.type === "bff:logout_url",
          );
          if (logoutUrlClaim) {
            setLogoutUrlState(String(logoutUrlClaim.value));
          }
        } else if (resp.status === 401) {
          setLogMessage("User not logged in, attempting silent login...");

          const silentLoginResult = await silentLogin();
          setLogMessage(`Silent login result: ${silentLoginResult}`);

          if (silentLoginResult) {
            window.location.reload();
          }
        }
      } catch (e) {
        setLogMessage("Error checking user status");
        console.error(e);
      }
    }

    checkAuth();
  }, []);

  const handleLogin = () => window.location.assign(loginUrl);
  const handleLogout = () => window.location.assign(logoutUrlState);

  // Хелпер для красивого вывода клеймов/ответов в UI
  const formatDisplayData = (data: unknown) => {
    if (Array.isArray(data)) {
      // Превращаем массив клеймов [ {type: "sub", value: "123"} ] в красивый объект { sub: "123" }
      const jsonObject = data.reduce(
        (acc, item) => {
          if (item.type && item.value !== undefined) {
            acc[item.type] = item.value;
          }
          return acc;
        },
        {} as Record<string, unknown>,
      );
      return JSON.stringify(jsonObject, null, 2);
    }
    return JSON.stringify(data, null, 2);
  };

  return (
    <>
      {/* Кнопки управления авторизацией */}
      <div
        style={{ padding: "10px", background: "#1a1a1a", borderRadius: "8px" }}
      >
        {userClaims ? (
          <button onClick={handleLogout}>LOGOUT</button>
        ) : (
          <button onClick={handleLogin}>LOGIN</button>
        )}

        <div style={{ marginTop: "10px", gap: "3px", display: "flex" }}>
          <ApiButton
            api="/api/profiles/get-headers"
            onDataFetched={(data) => setApiResponse(formatDisplayData(data))}
            onError={(err) => setApiResponse(String(err))}
          />
          <ApiButton
            api="/api/profiles/my-profile"
            onDataFetched={(data) => setApiResponse(formatDisplayData(data))}
            onError={(err) => setApiResponse(String(err))}
          />
          <ApiButton
            api="/api/profiles/weather"
            onDataFetched={(data) => setApiResponse(formatDisplayData(data))}
            onError={(err) => setApiResponse(String(err))}
          />
          <ApiButton
            api="/api"
            onDataFetched={(data) => setApiResponse(formatDisplayData(data))}
            onError={(err) => setApiResponse(String(err))}
          />
          <ApiButton
            api="/bff/user"
            onDataFetched={(data) => setApiResponse(formatDisplayData(data))}
            onError={(err) => setApiResponse(String(err))}
          />
        </div>
      </div>

      {/* Панель дебага/логирования, как в оригинальном скрипте */}
      <div
        style={{
          display: "flex",
          gap: "20px",
          margin: "20px 0",
          textAlign: "left",
        }}
      >
        <div style={{ flex: 1, background: "#222", padding: "10px" }}>
          <h4>Лог системы:</h4>
          <pre>{logMessage}</pre>
        </div>
        <div style={{ flex: 1, background: "#222", padding: "10px" }}>
          <h4>Данные текущего пользователя:</h4>
          <pre>
            {userClaims ? formatDisplayData(userClaims) : "Не авторизован"}
          </pre>
        </div>
        <div style={{ flex: 1, background: "#222", padding: "10px" }}>
          <h4>Результат последнего запроса:</h4>
          <pre>{apiResponse || "Нет данных"}</pre>
        </div>
      </div>

      {/* Скрытый iframe для silent login, обязателен для работы скрипта */}
      <iframe
        id="bff-silent-login"
        style={{ display: "none" }}
        title="bff-silent-login"
      />

      <hr />

      {/* Твой стандартный контент лендинга */}
      <section id="center">
        <div className="hero">
          <img src={heroImg} className="base" width="170" height="179" alt="" />
          <img src={reactLogo} className="framework" alt="React logo" />
          <img src={viteLogo} className="vite" alt="Vite logo" />
        </div>
        <div>
          <h1>Get started</h1>
          <p>
            Edit <code>src/App.tsx</code> and save to test <code>HMR</code>
          </p>
        </div>
        <button
          type="button"
          className="counter"
          onClick={() => setCount((count) => count + 1)}
        >
          Count is {count}
        </button>
      </section>

      <div className="ticks"></div>

      <section id="next-steps">
        <div id="docs">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#documentation-icon"></use>
          </svg>
          <h2>Documentation</h2>
          <p>Your questions, answered</p>
          <ul>
            <li>
              <a href="https://vite.dev/" target="_blank" rel="noreferrer">
                <img className="logo" src={viteLogo} alt="" />
                Explore Vite
              </a>
            </li>
            <li>
              <a href="https://react.dev/" target="_blank" rel="noreferrer">
                <img className="button-icon" src={reactLogo} alt="" />
                Learn more
              </a>
            </li>
          </ul>
        </div>

        <div id="social">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#social-icon"></use>
          </svg>
          <h2>Connect with us</h2>
          <p>Join the Vite community</p>
          <ul>
            <li>
              <a
                href="https://github.com/vitejs/vite"
                target="_blank"
                rel="noreferrer"
              >
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#github-icon"></use>
                </svg>
                GitHub
              </a>
            </li>
            <li>
              <a href="https://chat.vite.dev/" target="_blank" rel="noreferrer">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#discord-icon"></use>
                </svg>
                Discord
              </a>
            </li>
          </ul>
        </div>
      </section>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  );
}

export default App;
