"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const [login, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const router = useRouter();

  const handleLogin = async (e : any) => {
    e.preventDefault();

    const res = await fetch("http://localhost:5250/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ Login: login, Password: password }),
    });

    if (res.ok) {
      const data = await res.json();
      localStorage.setItem("token", data.tokenStr);
      router.push("/");
    } else if (res.status === 401) {
        console.log(res.status)
      alert("Invalid credentials");
    }
    else if (res.status === 500) {
      alert("Server error, please try again later.");
    }
  };

  return (
    <form onSubmit={handleLogin} className="p-6 max-w-sm mx-auto">
      <input
        type="text"
        placeholder="Username"
        value={login}
        onChange={(e) => setUsername(e.target.value)}
        className="border p-2 mb-2 w-full"
      />
      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        className="border p-2 mb-2 w-full"
      />
      <button type="submit" className="bg-blue-500 text-white px-4 py-2">
        Login
      </button>
    </form>
  );
}
