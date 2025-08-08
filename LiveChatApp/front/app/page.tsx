"use client";

import { useRef, useState, useEffect } from "react";
import { useSession, signIn } from "next-auth/react";
import * as signalR from "@microsoft/signalr";
import { useRouter } from "next/navigation";

let connection: signalR.HubConnection | null = null;

async function startConnection(
  token: string,
  onReceive: (user: string, message: string) => void
) {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`http://localhost:5250/chatHub?access_token=${token}`, {
      transport: signalR.HttpTransportType.WebSockets,
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveMessage", onReceive);

  try {
    await connection.start();
    console.log("✅ SignalR Connected.");
  } catch (err) {
    console.error("❌ Connection failed:", err);
  }

  return connection;
}

export default function ChatPage() {
  const { data: session, status } = useSession();
  const [messages, setMessages] = useState<{ user: string; message: string }[]>(
    []
  );
  const [message, setMessage] = useState("");
  const connRef = useRef<signalR.HubConnection | null>(null);
  const isConnected = useRef(false);
  const router = useRouter();

  useEffect(() => {
    if (isConnected.current) return;

    const start = async () => {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5250/chatHub", {
          transport: signalR.HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect()
        .build();

      connection.on("ReceiveMessage", (user, message) => {
        setMessages((prev) => [...prev, { user, message }]);
      });

      try {
        await connection.start();
        console.log("Connected");
        connRef.current = connection;
        isConnected.current = true;
      } catch (err) {
        console.error(err);
      }

      

      const fetchHistory = async () => {
        try {
          const res = await fetch("http://localhost:5250/api/messages/room");
          if (!res.ok) throw new Error("Failed to fetch chat history");

          const history = await res.json();
          console.log("Chat history:", history);
          setMessages(history);
        } catch (err) {
          console.error("Error loading chat history:", err);
        }
      };

      fetchHistory();
    };

    start();

    return () => {
      if (connRef.current) {
        connRef.current.stop();
        connRef.current = null;
        isConnected.current = false;
        console.log("Disconnected");
      }
    };
  }, []);

  const handleSend = async () => {
    if (connRef.current && message.trim()) {
      await connRef.current.invoke("SendMessage", session?.user?.name, message);
      setMessage("");
    }
  };

  if (status === "loading") {
    return <p className="p-4">Loading...</p>;
  }

  if (status === "unauthenticated") {
    return (
      <div className="p-4">
        <p className="mb-2">You must be logged in to join the chat.</p>
        <button
          onClick={() => signIn()}
          className="bg-blue-500 text-white px-4 py-2 rounded"
        >
          Login
        </button>
        <button onClick={() => router.push("/register")} className="bg-green-500 text-white px-4 py-2 rounded">Register</button>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen bg-gray-100">
      <header className="bg-blue-600 text-white py-3 px-6 shadow-lg">
        <h1 className="text-xl font-semibold">
          💬 Chat – Logged in as {session?.user?.name}
        </h1>
      </header>

      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.map((m, i) => (
          <div
            key={i}
            className={`flex ${m.user === session?.user?.name ? "justify-end" : "justify-start"
              }`}
          >
            <div
              className={`px-4 py-2 rounded-xl max-w-xs break-words shadow ${m.user === session?.user?.name
                ? "bg-blue-500 text-white rounded-br-none"
                : "bg-gray-200 text-gray-800 rounded-bl-none"
                }`}
            >
              <p className="text-xs font-semibold opacity-75 mb-1">{m.user}</p>
              <p>{m.message}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="p-4 bg-white shadow flex items-center gap-2">
        <input
          type="text"
          placeholder="Type a message..."
          className="border border-gray-300 rounded-lg px-3 py-2 flex-1 focus:outline-none focus:ring focus:ring-green-200"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <button
          onClick={handleSend}
          className={`px-5 py-2 rounded-lg text-white font-medium transition ${!message.trim()
            ? "bg-gray-400 cursor-not-allowed"
            : "bg-green-500 hover:bg-green-600"
            }`}
          disabled={!message.trim()}
        >
          Send
        </button>
      </div>
    </div>
  );
}
