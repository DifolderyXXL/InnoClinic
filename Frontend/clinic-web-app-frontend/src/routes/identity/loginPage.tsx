import React, { useState } from "react";

export default function LoginPageObsolet() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [emailTouched, setEmailTouched] = useState(false);
  const [passwordTouched, setPasswordTouched] = useState(false);

  const [showPassword, setShowPassword] = useState(false);

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const isEmailValid = emailRegex.test(email);
  const isPasswordValid = password.length >= 6 && password.length <= 15;

  const showEmailError = emailTouched && !isEmailValid;
  const showPasswordError = passwordTouched && !isPasswordValid;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!isEmailValid || !isPasswordValid) return;

    const urlParams = new URLSearchParams(window.location.search);
    const returnUrl = urlParams.get("returnUrl") || "";

    fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password, returnUrl }),
    })
      .then((res) => res.json())
      .then((data) => {
        if (data.returnUrl) {
          window.location.href = data.returnUrl;
        }
      })
      .catch((err) => console.error(err));
  };

  return (
    <div
      style={{
        maxWidth: "320px",
        margin: "40px auto",
        fontFamily: "sans-serif",
      }}
    >
      <form
        onSubmit={handleSubmit}
        style={{ display: "flex", flexDirection: "column", gap: "16px" }}
      >
        <h2>Вход</h2>

        <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="text"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            onBlur={() => setEmailTouched(true)}
            style={{
              padding: "8px",
              border: `1px solid ${showEmailError ? "red" : "#ccc"}`,
              borderRadius: "4px",
              outline: "none",
            }}
          />
          {showEmailError && (
            <span style={{ color: "red", fontSize: "12px" }}>
              Некорректный формат почты
            </span>
          )}
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
          <label htmlFor="password">Пароль</label>
          <div style={{ position: "relative", display: "flex" }}>
            <input
              id="password"
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              onBlur={() => setPasswordTouched(true)}
              style={{
                padding: "8px",
                paddingRight: "40px",
                border: `1px solid ${showPasswordError ? "red" : "#ccc"}`,
                borderRadius: "4px",
                outline: "none",
                width: "100%",
              }}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              style={{
                position: "absolute",
                right: "4px",
                top: "50%",
                transform: "translateY(-50%)",
                background: "none",
                border: "none",
                cursor: "pointer",
                padding: "4px",
              }}
            ></button>
          </div>
          {showPasswordError && (
            <span style={{ color: "red", fontSize: "12px" }}>
              От 6 до 15 символов
            </span>
          )}
        </div>

        <button
          type="submit"
          disabled={!isEmailValid || !isPasswordValid}
          style={{
            padding: "10px",
            backgroundColor:
              !isEmailValid || !isPasswordValid ? "#ccc" : "#007bff",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: !isEmailValid || !isPasswordValid ? "default" : "pointer",
          }}
        >
          Войти
        </button>
      </form>
    </div>
  );
}
