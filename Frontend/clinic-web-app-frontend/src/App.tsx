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



  const formatDisplayData = (data: unknown) => {
    return JSON.stringify(data, null, 2);
  };

  if (state.status === "loading") {
    return <div className="spinner">Loading user profile claims...</div>;
  }

  if (state.status === "unauthorized") {
    return <div className="error-msg"> Unautorized </div>;
  }

  const userClaims = state.data.profile;

  const mail = state.data.getEmail();
  const role = state.data.getRoles().join(", ");
  

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
      <a 
        href="/swagger/index.html"
        style={{ padding: "10px", background: "#889a7e", color: "#fff" }}
      >
        Swagger
      </a>

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
  mail: string | object;
  role: string | object;
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
        <span>{mail.toString()}</span>
      </div>

      <div
        style={{ width: "1px", height: "24px", backgroundColor: "#d1d5db" }}
      />

      <div>
        <strong style={{ fontSize: "1.2em" }}>Role: </strong>
        <span>{role.toString()}</span>
      </div>
    </div>
  );
}
