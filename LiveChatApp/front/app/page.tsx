"use client";

import { useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

async function startConnection(
  onReceive: (user: string, message: string) => void
) {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5250/chat", {
      transport: signalR.HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveMessage", onReceive);

  try {
    await connection.start();
    console.log("SignalR Connected.");
  } catch (err) {
    console.error("Connection failed:", err);
  }

  return connection;
}

export default function ChatPage() {
  const [messages, setMessages] = useState<{ user: string; message: string }[]>(
    []
  );
  const [user, setUser] = useState("");
  const [message, setMessage] = useState("");
  const connRef = useRef<signalR.HubConnection | null>(null);

  const handleConnect = async () => {
    connRef.current = await startConnection((user, message) => {
      setMessages((prev) => [...prev, { user, message }]);
    });
  };

  const handleSend = async () => {
    if (connRef.current && message.trim()) {
      await connRef.current.invoke("SendMessage", user, message);
      setMessage("");
    }
  };

  return (
    <div className="p-4 max-w-xl mx-auto">
      <h1 className="text-2xl font-bold mb-4">Chat</h1>

      <div className="mb-2">
        <input
          type="text"
          placeholder="Your name"
          className="border px-2 py-1 mr-2"
          value={user}
          onChange={(e) => setUser(e.target.value)}
        />
        <button
          className="bg-blue-500 text-white px-4 py-1 rounded"
          onClick={handleConnect}
          disabled={!user || connRef.current != null}
        >
          Connect
        </button>
      </div>

      <div className="mb-2">
        <input
          type="text"
          placeholder="Type a message"
          className="border px-2 py-1 w-full"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
      </div>

      <button
        onClick={handleSend}
        className="bg-green-500 text-white px-4 py-1 rounded mt-1"
        disabled={!message.trim() || !connRef.current}
      >
        Send
      </button>

      <div className="mt-4 border-t pt-2">
        {messages.map((m, i) => (
          <div key={i} className="mb-1">
            <span className="font-semibold">{m.user}:</span> {m.message}
          </div>
        ))}
      </div>
    </div>
  );
}
