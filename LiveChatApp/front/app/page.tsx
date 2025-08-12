"use client";

import { useRef, useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useRouter, useSearchParams } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";

interface MessageDto {
  user: string;
  message: string;
  room: string;
}

export default function ChatPage() {
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [message, setMessage] = useState("");
  const [userName, setUserName] = useState<string | null>(null);
  const [availableRooms] = useState(["General", "Sports", "Coding", "Gaming"]);
  const [currentRoom, setCurrentRoom] = useState<string | null>(null);
  const [unreadRooms, setUnreadRooms] = useState<string[]>([]);

  const connRef = useRef<signalR.HubConnection | null>(null);
  const isConnected = useRef(false);

  const router = useRouter();
  const searchParams = useSearchParams();
  const initialRoom = searchParams.get("room");

  useEffect(() => {
    setCurrentRoom(initialRoom || "General");
  }, [initialRoom]);

  useEffect(() => {
    const token = localStorage.getItem("token");
    const storedUser = localStorage.getItem("userName");
    if (!token || !storedUser) {
      router.push("/login");
      return;
    }
    setUserName(storedUser);
    if (isConnected.current || !currentRoom) return;

    const start = async () => {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(`http://localhost:5250/chatHub`, {
          transport: signalR.HttpTransportType.WebSockets,
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .build();

      connection.on("ReceiveMessage", (msg: MessageDto) => {
        setMessages((prev) => [...prev, msg]);
      });

      connection.on("RoomActivity", (room: string) => {
        if (room !== currentRoom) {
          setUnreadRooms((prev) =>
            prev.includes(room) ? prev : [...prev, room]
          );
        }
      });

      try {
        await connection.start();
        connRef.current = connection;
        isConnected.current = true;
        await joinRoom(connection, currentRoom);
      } catch (err) {
        console.error("Connection failed:", err);
      }
    };

    start();

    return () => {
      if (connRef.current) {
        connRef.current.off("ReceiveMessage");
        connRef.current.off("RoomActivity");
        connRef.current.stop();
        connRef.current = null;
        isConnected.current = false;
      }
    };
  }, [currentRoom, router]);

  const joinRoom = async (connection: signalR.HubConnection, room: string) => {
    const token = localStorage.getItem("token");
    if (!token) return;

    if (currentRoom) {
      await connection.invoke("LeaveRoom", currentRoom);
    }
    await connection.invoke("JoinRoom", room);

    try {
      const res = await fetch(
        `http://localhost:5250/api/messages/${encodeURIComponent(room)}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      if (!res.ok) throw new Error("Failed to fetch history");
      const history: MessageDto[] = await res.json();
      setMessages(history);
    } catch (err) {
      console.error("Error loading history:", err);
    }
  };

  const switchRoom = async (newRoom: string) => {
    if (!connRef.current || !userName || newRoom === currentRoom) return;
    await joinRoom(connRef.current, newRoom);
    setMessages([]);
    setCurrentRoom(newRoom);
    setUnreadRooms((prev) => prev.filter((r) => r !== newRoom));
  };

  const handleSend = async () => {
    if (connRef.current && message.trim() && userName && currentRoom) {
      const msg: MessageDto = {
        user: userName,
        message: message.trim(),
        room: currentRoom,
      };
      try {
        await connRef.current.invoke("SendMessage", msg);
        setMessage("");
      } catch (err) {
        console.error("Error sending:", err);
      }
    }
  };

  if (!currentRoom) {
    return <p className="p-4 text-red-500">⚠ Please select a room first.</p>;
  }

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-gray-50 to-gray-200">
      <header className="bg-blue-600 text-white py-3 px-6 shadow-lg flex justify-between items-center">
        <h1 className="text-lg font-semibold">
          💬 Chat –{" "}
          <span className="opacity-80">{userName}</span> in{" "}
          <span className="font-bold">{currentRoom}</span>
        </h1>
        <div className="flex items-center gap-2">
          {availableRooms.map((room) => (
            <button
              key={room}
              onClick={() => switchRoom(room)}
              className={`relative px-3 py-1 rounded-lg transition-all duration-300 ${
                room === currentRoom
                  ? "bg-green-500 text-white shadow-md"
                  : "bg-white text-black hover:bg-green-100"
              }`}
            >
              {room}
              {unreadRooms.includes(room) && room !== currentRoom && (
                <span className="absolute top-0 right-0 w-2 h-2 bg-red-500 rounded-full"></span>
              )}
            </button>
          ))}
        </div>
        <button
          onClick={() => {
            localStorage.removeItem("token");
            localStorage.removeItem("userName");
            router.push("/login");
          }}
          className="bg-red-500 hover:bg-red-600 px-3 py-1 rounded-lg text-white transition-all"
        >
          Logout
        </button>
      </header>

      <motion.div
        layout
        className="flex-1 overflow-y-auto p-4 space-y-3"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        <AnimatePresence>
          {messages.map((m, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.2 }}
              className={`flex ${
                m.user === userName ? "justify-end" : "justify-start"
              }`}
            >
              <div
                className={`px-4 py-2 rounded-xl max-w-xs break-words shadow ${
                  m.user === userName
                    ? "bg-blue-500 text-white rounded-br-none"
                    : "bg-gray-200 text-gray-800 rounded-bl-none"
                }`}
              >
                <p className="text-xs font-semibold opacity-75 mb-1">{m.user}</p>
                <p>{m.message}</p>
              </div>
            </motion.div>
          ))}
        </AnimatePresence>
      </motion.div>

      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSend();
        }}
        className="p-4 bg-white shadow flex items-center gap-2 border-t border-gray-200"
      >
        <input
          type="text"
          placeholder="Type a message..."
          className="text-gray-700 border border-gray-300 rounded-lg px-3 py-2 flex-1 focus:outline-none focus:ring-2 focus:ring-blue-300"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
        />
        <button
          type="submit"
          disabled={!message.trim()}
          className={`px-5 py-2 rounded-lg text-white font-medium transition-all ${
            !message.trim()
              ? "bg-gray-400 cursor-not-allowed"
              : "bg-green-500 hover:bg-green-600 shadow-md"
          }`}
        >
          Send
        </button>
      </form>
    </div>
  );
}
