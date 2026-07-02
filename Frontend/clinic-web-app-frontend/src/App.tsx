import "./App.css";
import { SilentLoginElement } from "./services/silentLoginElement";
import { useAuth } from "./services/states/userState";
import { Link } from "react-router-dom";

function App() {
  return (
    <>
      <UserProfile />
      <SilentLoginElement />
    </>
  );
}

export default App;

export const UserProfile: React.FC = () => {
  const { state } = useAuth();

  const simplify = (data: unknown) => {
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
      return jsonObject;
    }
  };

  const formatDisplayData = (data: unknown) => {
    if (Array.isArray(data)) {
      const jsonObject = simplify(data);
      return JSON.stringify(jsonObject, null, 2);
    }
    return JSON.stringify(data, null, 2);
  };

  if (state.status === "loading") {
    return <div className="spinner">Loading user profile claims...</div>;
  }

  if (state.status === "unauthorized") {
    return <div className="error-msg"> Unautorized </div>;
  }

  const userClaims = simplify(state.data.claims);

  const mail = userClaims["email"];
  const role = userClaims["role"];

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        transition: "opacity 0.2s ease",
      }}
    >
      <UserBlock mail={mail} role={role} />
      <div style={{ flex: 1, display: "flex", flexDirection: "row" }}></div>
      <Link
        to="/debug"
        style={{ padding: "10px", background: "#333", color: "#fff" }}
      >
        Debug
      </Link>
      <Link
        to="/swagger/index.html"
        style={{ padding: "10px", background: "#889a7e", color: "#fff" }}
      >
        Swagger
      </Link>

      <div
        style={{
          flex: 1,
          background: "#222",
          padding: "10px",
          textAlign: "left",
        }}
      >
        <h4>Данные текущего пользователя:</h4>
        <pre>
          {userClaims ? formatDisplayData(userClaims) : "Не авторизован"}
        </pre>
      </div>
    </div>
  );
};
interface UserBlockProps {
  mail: string;
  role: string;
}

function UserBlock({ mail, role }: UserBlockProps) {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        gap: "32px",
      }}
    >
      <div>
        <strong style={{ fontSize: "1.2em" }}>Email: </strong>
        <span>{mail}</span>
      </div>

      <div
        style={{ width: "1px", height: "24px", backgroundColor: "#d1d5db" }}
      />

      <div>
        <strong style={{ fontSize: "1.2em" }}>Role: </strong>
        <span>{role}</span>
      </div>
    </div>
  );
}
